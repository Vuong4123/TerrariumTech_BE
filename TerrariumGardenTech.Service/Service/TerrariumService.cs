using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Terrarium;
using TerrariumGardenTech.Common.RequestModel.TerrariumVariant;
using TerrariumGardenTech.Common.ResponseModel.Accessory;
using TerrariumGardenTech.Common.ResponseModel.Base;
using TerrariumGardenTech.Common.ResponseModel.Terrarium;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.Mappers;

namespace TerrariumGardenTech.Service.Service;
public class AITerrariumRequest
{
    public int EnvironmentId { get; set; }
    public int ShapeId { get; set; }
    public int TankMethodId { get; set; }
}

public class AITerrariumResponse
{
    public string TerrariumName { get; set; }
    public string Description { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; }
    public List<string> TerrariumImages { get; set; }
}

public class TerrariumService : ITerrariumService
{
    private readonly UnitOfWork _unitOfWork;
    private static readonly HttpClient _httpClient = new HttpClient();
    private readonly string _geminiApiKey = "AIzaSyCxt7zx-cUzvkuroP1uaSp_m0SkoFU_j4A";
    private const string GeminiModel = "gemini-1.5-flash-latest";
    private readonly ICloudinaryService _cloudinaryService;

    public TerrariumService(UnitOfWork unitOfWork, ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
    }
    public async Task<AITerrariumResponse> PredictTerrariumAsync(AITerrariumRequest request)
    {
        try
        {
            var environment = await _unitOfWork.Environment.GetByIdAsync(request.EnvironmentId);
            var shape = await _unitOfWork.Shape.GetByIdAsync(request.ShapeId);
            var tankMethod = await _unitOfWork.TankMethod.GetByIdAsync(request.TankMethodId);

            if (environment == null || shape == null || tankMethod == null)
                throw new ArgumentException("Invalid environment, shape, or tank method ID");

            var prompt = BuildPrompt(request, environment.EnvironmentName, shape.ShapeName, tankMethod.TankMethodDescription);
            var aiText = await CallGeminiAsync(prompt);

            if (string.IsNullOrWhiteSpace(aiText))
                return GenerateFallbackTerrarium(request, environment.EnvironmentName, shape.ShapeName, tankMethod.TankMethodDescription);

            var terrarium = ParseTerrariumJson(aiText, environment.EnvironmentName, shape.ShapeName, tankMethod.TankMethodDescription);
            return terrarium ?? GenerateFallbackTerrarium(request, environment.EnvironmentName, shape.ShapeName, tankMethod.TankMethodDescription);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in PredictTerrariumAsync: {ex.Message}");
            return GenerateFallbackTerrarium(request);
        }
    }

    private string BuildPrompt(AITerrariumRequest req, string environment, string shape, string method)
    {
        return $@"Generate a creative terrarium design based on these specifications:
            - Environment: {environment} (ID: {req.EnvironmentId})
            - Shape: {shape} (ID: {req.ShapeId})
            - Tank Method: {method} (ID: {req.TankMethodId})

            Create a JSON response with:
            1. terrariumName: Creative Vietnamese name
            2. description: Detailed Vietnamese description (50-100 words)
            3. minPrice: 100000-300000 VND
            4. maxPrice: 300000-900000 VND
            5. stock: 5-50
            6. imageKeywords: Array of 5 specific English search terms that would find perfect images for this terrarium (e.g. ['desert succulent terrarium', 'glass geometric planter'])";
    }

    private AITerrariumResponse ParseTerrariumJson(string aiText, string environment, string shape, string method)
    {
        try
        {
            var match = Regex.Match(aiText, @"\{(?:[^{}]|(?<o>\{)|(?<-o>\}))+(?(o)(?!))\}", RegexOptions.Singleline);
            string json = match.Success ? match.Value : aiText;

            dynamic data = JsonConvert.DeserializeObject(json);

            // Extract search keywords for image search
            List<string> imageKeywords = data.imageKeywords?.ToObject<List<string>>();
            if (imageKeywords == null || !imageKeywords.Any())
            {
                imageKeywords = new List<string>
            {
                $"terrarium {environment.ToLower()}",
                $"{shape.ToLower()} planter",
                $"miniature {environment.ToLower()} garden",
                "terrarium plants",
                $"{environment.ToLower()} {shape.ToLower()} plants"
            };
            }

            // Generate dynamic image URLs using the keywords
            var imageUrls = GetDynamicImageUrls(imageKeywords);

            return new AITerrariumResponse
            {
                TerrariumName = data.terrariumName ?? $"{environment} {shape}",
                Description = data.description ?? $"Terrarium {environment} với hình dạng {shape}, phương pháp {method}.",
                MinPrice = (decimal?)data.minPrice ?? 150000m,
                MaxPrice = (decimal?)data.maxPrice ?? 450000m,
                Stock = (int?)data.stock ?? 10,
                ImageUrl = imageUrls.FirstOrDefault(),
                TerrariumImages = imageUrls
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing AI JSON: {ex.Message}");
            return null;
        }
    }

    private List<string> GetDynamicImageUrls(List<string> keywords)
    {
        var results = new List<string>();
        var random = new Random();

        int numImages = random.Next(3, 6);
        var selectedKeywords = keywords.OrderBy(x => random.Next()).Take(numImages).ToList();

        foreach (var keyword in selectedKeywords)
        {
            var encodedKeyword = Uri.EscapeDataString(keyword);

            int provider = random.Next(4);
            string imageUrl;

            switch (provider)
            {
                case 0:
                    imageUrl = $"https://source.unsplash.com/featured/600x400/?{encodedKeyword}&sig={Guid.NewGuid().ToString().Substring(0, 8)}";
                    break;

                case 1:
                    imageUrl = $"https://pixabay.com/api/no_key_required_for_this_endpoint/?q={encodedKeyword}&image_type=photo&orientation=horizontal&per_page=3";
                    break;

                case 2:
                    imageUrl = $"https://images.pexels.com/photos/search?query={encodedKeyword}&per_page=1&orientation=landscape";
                    break;

                default:
                    imageUrl = $"https://source.unsplash.com/random/600x400/?{encodedKeyword}&sig={Guid.NewGuid().ToString().Substring(0, 8)}";
                    break;
            }

            results.Add(imageUrl);
        }

        return results;
    }

    private AITerrariumResponse GenerateFallbackTerrarium(AITerrariumRequest request, string env = null, string shape = null, string method = null)
    {
        env ??= $"Môi trường {request.EnvironmentId}";
        shape ??= $"Hình dạng {request.ShapeId}";
        method ??= $"Phương pháp {request.TankMethodId}";

        var rnd = new Random();
        var terrariumName = $"Bể {shape.ToLower()} {env.ToLower()}";
        var description = $"Tiểu cảnh {shape.ToLower()} mang phong cách {env.ToLower()}, được thiết kế theo phương pháp {method.ToLower()}.";

        decimal minPrice = rnd.Next(100, 301) * 1000;
        decimal maxPrice = rnd.Next(300, 901) * 1000;

        // Create dynamic search terms based on environment and shape
        var searchTerms = new List<string>
    {
        $"terrarium {env.ToLower()}",
        $"{shape.ToLower()} planter",
        "glass terrarium",
        "miniature garden",
        "succulent arrangement"
    };

        // Get dynamic images using the search terms
        var imageUrls = GetDynamicImageUrls(searchTerms);

        return new AITerrariumResponse
        {
            TerrariumName = terrariumName,
            Description = description,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Stock = rnd.Next(5, 51),
            ImageUrl = imageUrls.FirstOrDefault(),
            TerrariumImages = imageUrls
        };
    }


    private async Task<string> CallGeminiAsync(string prompt)
    {
        var requestPayload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new { temperature = 0.7, maxOutputTokens = 1024, topP = 0.95, topK = 40 }
        };

        var jsonRequest = JsonConvert.SerializeObject(requestPayload);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{GeminiModel}:generateContent?key={_geminiApiKey}";
        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Gemini API error: {response.StatusCode}");
            return null;
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        dynamic apiResponse = JsonConvert.DeserializeObject(responseContent);
        return apiResponse?.candidates?[0]?.content?.parts?[0]?.text;
    }

    public async Task<IBusinessResult> GetAll(TerrariumGetAllRequest request)
    {
        var tuple = await _unitOfWork.Terrarium.GetFilterAndPagedAsync(request);
        var list = tuple.Item1;
        var enumerable = list.ToList();

        if (!enumerable.Any())
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

        if (!request.Pagination.IsPagingEnabled)
        {
            var tuple_ = await _unitOfWork.Terrarium.GetFilterAndPagedAsync(request);
            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", tuple_.Item1);
        }

        var terrariumIds = enumerable.Select(t => t.TerrariumId).ToList();
        var ratingStats = await _unitOfWork.Terrarium.GetTerrariumRatingStatsAsync(terrariumIds);
        // ✅ NEW: lấy tổng lượt mua theo terrarium
        var purchaseCounts = await _unitOfWork.Terrarium.GetTerrariumPurchaseCountsAsync(terrariumIds);

        var terrariums = enumerable.Select(t => new TerrariumDetailResponse
        {
            TerrariumId = t.TerrariumId,
            EnvironmentId = t.EnvironmentId,
            ShapeId = t.ShapeId,
            TankMethodId = t.TankMethodId,
            TerrariumName = t.TerrariumName,
            Description = t.Description,
            MinPrice = t.MinPrice,
            MaxPrice = t.MaxPrice,
            Stock = t.Stock,
            Status = t.Status,
            TerrariumImages = t.TerrariumImages?.Select(ti => new TerrariumImageResponse
            {
                TerrariumImageId = ti.TerrariumImageId,
                TerrariumId = ti.TerrariumId,
                ImageUrl = ti.ImageUrl ?? string.Empty
            }).ToList(),
            // Thêm rating
            AverageRating = ratingStats.ContainsKey(t.TerrariumId) ? ratingStats[t.TerrariumId].AverageRating : 0,
            FeedbackCount = ratingStats.ContainsKey(t.TerrariumId) ? ratingStats[t.TerrariumId].FeedbackCount : 0,
            // ✅ map lượt mua
            PurchaseCount = purchaseCounts.TryGetValue(t.TerrariumId, out var pc) ? pc : 0
        }).ToList();

        var tableResponse = new QueryTableResult(request, terrariums, tuple.Item2);

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", tableResponse);
    }

    public async Task<IBusinessResult> GetById(int id)
    {
        try
        {
            // Lấy Terrarium từ repository
            var terrarium = await _unitOfWork.Terrarium.GetTerrariumWithImagesByIdAsync(id);

            if (terrarium == null)
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No data found.");

            // Lấy rating & số feedback cho Terrarium này (dùng repository batch)
            var ratingStats = await _unitOfWork.Terrarium.GetTerrariumRatingStatsAsync(new List<int> { id });
            var avgRating = ratingStats.ContainsKey(id) ? ratingStats[id].AverageRating : 0;
            var feedbackCount = ratingStats.ContainsKey(id) ? ratingStats[id].FeedbackCount : 0;
            // ✅ Lượt mua (từ các order đã Paid)
            var purchaseCount = await _unitOfWork.Terrarium.GetTerrariumPurchaseCountAsync(id);


            // Ánh xạ Terrarium sang TerrariumResponse
            var terrariumResponse = new TerrariumResponse
            {
                TerrariumId = terrarium.TerrariumId,
                EnvironmentId = terrarium.EnvironmentId,
                ShapeId = terrarium.ShapeId,
                TankMethodId = terrarium.TankMethodId,
                TerrariumName = terrarium.TerrariumName,
                Description = terrarium.Description,
                MinPrice = terrarium.MinPrice,
                MaxPrice = terrarium.MaxPrice,
                Stock = terrarium.Stock,
                Status = terrarium.Status,
                Accessories = terrarium.TerrariumAccessory.Select(a => new TerrariumAccessoryResponse
                {
                    AccessoryId = a.Accessory.AccessoryId,
                    Name = a.Accessory.Name,
                    Description = a.Accessory.Description,
                    Price = a.Accessory.Price
                }).ToList(),
                BodyHTML = terrarium.bodyHTML,
                CreatedAt = terrarium.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = terrarium.UpdatedAt ?? DateTime.UtcNow,
                TerrariumImages = terrarium.TerrariumImages.Select(ti => new TerrariumImageResponse
                {
                    TerrariumImageId = ti.TerrariumImageId,
                    TerrariumId = ti.TerrariumId,
                    ImageUrl = ti.ImageUrl
                }).ToList(),
                // Bổ sung hai trường mới
                AverageRating = avgRating,
                FeedbackCount = feedbackCount,
                PurchaseCount = purchaseCount
            };

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", terrariumResponse);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }


    public async Task<IBusinessResult> Save(Terrarium terrarium)
    {
        try
        {
            var result = -1;
            var terrariumEntity = _unitOfWork.Terrarium.GetByIdAsync(terrarium.TerrariumId);
            if (terrariumEntity != null)
            {
                result = await _unitOfWork.Terrarium.UpdateAsync(terrarium);
                if (result > 0)
                    return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, terrarium);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            // Create new terrarium if it does not exist
            result = await _unitOfWork.Terrarium.CreateAsync(terrarium);
            if (result > 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, terrarium);

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }
    public async Task<IBusinessResult> UpdateTerrarium(TerrariumUpdateRequest terrariumUpdateRequest)
    {
        try
        {
            var terra = await _unitOfWork.Terrarium
                .GetByIdAsync(terrariumUpdateRequest.TerrariumId); // Include quan hệ

            if (terra == null)
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

            // Cập nhật thuộc tính cơ bản
            terra.EnvironmentId = terrariumUpdateRequest.EnvironmentId;
            terra.ShapeId = terrariumUpdateRequest.ShapeId;
            terra.TankMethodId = terrariumUpdateRequest.TankMethodId;
            terra.TerrariumName = terrariumUpdateRequest.TerrariumName;
            terra.Description = terrariumUpdateRequest.Description;
            terra.Status = terrariumUpdateRequest.Status;
            terra.bodyHTML = terrariumUpdateRequest.bodyHTML ?? string.Empty;
            terra.UpdatedAt = DateTime.UtcNow;

            // Thiết lập giá trị mặc định nếu không có giá từ variant
            //decimal defaultPrice = 100; // Giá trị mặc định khi không có variant
            //terra.MinPrice = terrariumUpdateRequest.MinPrice ?? defaultPrice;
            //terra.MaxPrice = terrariumUpdateRequest.MaxPrice ?? defaultPrice;

            var ctx = _unitOfWork.Terrarium.Context();

            // ===== XÓA DỮ LIỆU QUAN HỆ CŨ =====
            ctx.TerrariumAccessory.RemoveRange(ctx.TerrariumAccessory.Where(x => x.TerrariumId == terra.TerrariumId));
            await _unitOfWork.Terrarium.SaveChangesAsync(); // Lưu các thay đổi đã xóa


            // ===== THÊM DỮ LIỆU QUAN HỆ MỚI =====
            if (terrariumUpdateRequest.AccessoryNames?.Any() == true)
            {
                var accessories = await _unitOfWork.Accessory
                    .FindAsync(a => terrariumUpdateRequest.AccessoryNames.Contains(a.Name));

                terra.TerrariumAccessory = accessories.Select(a => new TerrariumAccessory
                {
                    TerrariumId = terra.TerrariumId,
                    AccessoryId = a.AccessoryId
                }).ToList();
            }
            else
            {
                // Nếu không có accessories mới, giữ nguyên accessories cũ
                var existingAccessories = await _unitOfWork.TerrariumAccessory
                    .FindAsync(ta => ta.TerrariumId == terra.TerrariumId);

                terra.TerrariumAccessory = existingAccessories.ToList();
            }

            // Cập nhật và lưu lại
            await _unitOfWork.Terrarium.UpdateAsync(terra);


            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    //public async Task<IBusinessResult> GetByAccesname(string name)
    //{
    //    try
    //    {
    //        // Tìm Terrarium theo tên accessory (nếu GetByNameAsync trả về List<Terrarium>)
    //        var terrariumList = await _unitOfWork.Terrarium.GetByNameAsync(name);

    //        if (terrariumList == null || !terrariumList.Any())
    //            return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No accessory found with that name.");

    //        // Lấy tất cả TerrariumId để query rating batch
    //        var terrariumIds = terrariumList.Select(t => t.TerrariumId).ToList();
    //        var ratingStats = await _unitOfWork.Terrarium.GetTerrariumRatingStatsAsync(terrariumIds);

    //        // Mapping dữ liệu từ Terrarium sang response model
    //        var terrariumResponse = terrariumList.Select(t => new TerrariumResponse
    //        {
    //            TerrariumId = t.TerrariumId,
    //            EnvironmentId = t.EnvironmentId,
    //            ShapeId = t.ShapeId,
    //            TankMethodId = t.TankMethodId,
    //            TerrariumName = t.TerrariumName,
    //            Description = t.Description,
    //            //MinPrice = t.MinPrice,
    //            //MaxPrice = t.MaxPrice,
    //            //Stock = t.Stock,
    //            //Status = t.Status,
    //            //BodyHTML = t.bodyHTML ?? string.Empty,
    //            //CreatedAt = t.CreatedAt ?? DateTime.UtcNow,
    //            //UpdatedAt = t.UpdatedAt ?? DateTime.UtcNow,
    //            Accessories = t.TerrariumAccessory.Select(a => new TerrariumAccessoryResponse
    //            {
    //                AccessoryId = a.Accessory.AccessoryId,
    //                Name = a.Accessory.Name,
    //                Description = a.Accessory.Description,
    //                Price = a.Accessory.Price
    //            }).ToList(),
    //            TerrariumImages = t.TerrariumImages.Select(ti => new TerrariumImageResponse
    //            {
    //                TerrariumImageId = ti.TerrariumImageId,
    //                TerrariumId = ti.TerrariumId,
    //                ImageUrl = ti.ImageUrl ?? string.Empty
    //            }).ToList(),
    //            // Bổ sung rating
    //            AverageRating = ratingStats.ContainsKey(t.TerrariumId) ? ratingStats[t.TerrariumId].AverageRating : 0,
    //            FeedbackCount = ratingStats.ContainsKey(t.TerrariumId) ? ratingStats[t.TerrariumId].FeedbackCount : 0
    //        }).ToList();

    //        return new BusinessResult(Const.SUCCESS_READ_CODE, "Accessory found.", terrariumResponse);
    //    }
    //    catch (Exception ex)
    //    {
    //        return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
    //    }
    //}

    public async Task<IBusinessResult> GetTerrariumSuggestions(int userId)
    {
        try
        {
            // Lấy dữ liệu từ bảng Personalize dựa trên userId
            var userPersonalize = await _unitOfWork.Personalize.GetByUserIdAsync(userId);
            if (userPersonalize == null)
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "User personalize data not found.");

            // Lấy dữ liệu Terrariums theo các thuộc tính đã chọn trong Personalize
            var suggestedTerrariums = await _unitOfWork.Terrarium.GetAllAsync(); // Đảm bảo gọi await để lấy List<Terrarium>

            // Sử dụng Where sau khi có danh sách
            var filteredTerrariums = suggestedTerrariums
                .Where(t => t.EnvironmentId == userPersonalize.EnvironmentId &&
                            t.ShapeId == userPersonalize.ShapeId &&
                            t.TankMethodId == userPersonalize.TankMethodId)
                .ToList();

            if (suggestedTerrariums == null || !suggestedTerrariums.Any())
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No matching terrariums found.");

            // Mapping dữ liệu Terrarium sang response
            var terrariumResponses = suggestedTerrariums.Select(t => new TerrariumResponse
            {
                TerrariumId = t.TerrariumId,
                TerrariumName = t.TerrariumName,
                Description = t.Description,
                MinPrice = t.MinPrice,
                MaxPrice = t.MaxPrice,
                Stock = t.Stock,
                Status = t.Status,
                EnvironmentId = t.EnvironmentId,
                ShapeId = t.EnvironmentId,
                TankMethodId = t.EnvironmentId,
                TerrariumImages = t.TerrariumImages.Select(img => new TerrariumImageResponse
                {
                    TerrariumId = img.TerrariumId,
                    TerrariumImageId = img.TerrariumImageId,
                    ImageUrl = img.ImageUrl
                }).ToList() // Ánh xạ đúng kiểu
            }).ToList();

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Terrarium suggestions retrieved successfully.", terrariumResponses);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }
    public async Task<IBusinessResult> CreateTerrariumAI(TerrariumCreateRequest terrariumCreateRequest)
    {
        try
        {
            // Lấy danh sách Accessory theo tên
            var AccessoryNames = await _unitOfWork.Accessory.GetByName(terrariumCreateRequest.AccessoryNames);
            if (AccessoryNames == null || AccessoryNames.Count == 0)
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

            //// Kiểm tra nếu không có variant, hãy gán giá trị mặc định cho MinPrice và MaxPrice
            //decimal defaultPrice = 100; // Giá trị mặc định khi không có variant
            //decimal minPrice = terrariumCreateRequest.MinPrice ?? defaultPrice;
            //decimal maxPrice = terrariumCreateRequest.MaxPrice ?? defaultPrice;

            // Tạo mới Terrarium
            var newTerrarium = new Terrarium
            {
                TerrariumName = terrariumCreateRequest.TerrariumName,
                EnvironmentId = terrariumCreateRequest.EnvironmentId,
                ShapeId = terrariumCreateRequest.ShapeId,
                TankMethodId = terrariumCreateRequest.TankMethodId,
                bodyHTML = terrariumCreateRequest.bodyHTML,
                MinPrice = terrariumCreateRequest.MinPrice,
                MaxPrice = terrariumCreateRequest.MaxPrice,
                Stock = terrariumCreateRequest.Stock,

                Description = terrariumCreateRequest.Description,
                Status = terrariumCreateRequest.Status,


                CreatedAt = DateTime.Now
            };

            // Tạo Terrarium vào cơ sở dữ liệu
            var result = await _unitOfWork.Terrarium.CreateAsync(newTerrarium);
            if (terrariumCreateRequest.TerrariumImages != null)
            {
                foreach (var imageUrl in terrariumCreateRequest.TerrariumImages)
                {
                    var terrariumImage = new TerrariumImage
                    {
                        TerrariumId = newTerrarium.TerrariumId,
                        ImageUrl = imageUrl
                    };
                    _unitOfWork.TerrariumImage.Context().Add(terrariumImage);
                }
                await _unitOfWork.TerrariumImage.SaveChangesAsync();
            }


            if (result > 0)
            {
                // Gán Accessory vào Terrarium
                foreach (var accessory in AccessoryNames)
                {
                    var terrariumAccessory = new TerrariumAccessory
                    {
                        AccessoryId = accessory.AccessoryId,
                        TerrariumId = newTerrarium.TerrariumId
                    };
                    _unitOfWork.TerrariumAccessory.Context().Add(terrariumAccessory);
                    await _unitOfWork.TerrariumAccessory.SaveChangesAsync();
                }

                return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, newTerrarium);
            }

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    public async Task<IBusinessResult> CreateTerrarium(TerrariumCreate terrariumCreateRequest)
    {
        try
        {
            // Nếu có AccessoryNames thì mới tìm
            List<Accessory> accessories = new();
            if (terrariumCreateRequest.AccessoryNames != null && terrariumCreateRequest.AccessoryNames.Any())
            {
                accessories = await _unitOfWork.Accessory.GetByName(terrariumCreateRequest.AccessoryNames);
            }

            //// Kiểm tra nếu không có variant, hãy gán giá trị mặc định cho MinPrice và MaxPrice
            //decimal defaultPrice = 100; // Giá trị mặc định khi không có variant
            //decimal minPrice = terrariumCreateRequest.MinPrice ?? defaultPrice;
            //decimal maxPrice = terrariumCreateRequest.MaxPrice ?? defaultPrice;

            // Tạo mới Terrarium
            var newTerrarium = new Terrarium
            {
                TerrariumName = terrariumCreateRequest.TerrariumName,
                EnvironmentId = terrariumCreateRequest.EnvironmentId,
                ShapeId = terrariumCreateRequest.ShapeId,
                TankMethodId = terrariumCreateRequest.TankMethodId,
                bodyHTML = terrariumCreateRequest.bodyHTML,
                Description = terrariumCreateRequest.Description,
                Status = terrariumCreateRequest.Status,


                CreatedAt = DateTime.Now
            };

            // Tạo Terrarium vào cơ sở dữ liệu
            var result = await _unitOfWork.Terrarium.CreateAsync(newTerrarium);


            if (result > 0)
            {
                // Nếu có accessories thì gán
                if (accessories != null && accessories.Any())
                {
                    foreach (var accessory in accessories)
                    {
                        var terrariumAccessory = new TerrariumAccessory
                        {
                            AccessoryId = accessory.AccessoryId,
                            TerrariumId = newTerrarium.TerrariumId
                        };
                        _unitOfWork.TerrariumAccessory.Context().Add(terrariumAccessory);
                    }
                    await _unitOfWork.TerrariumAccessory.SaveChangesAsync();
                }

                return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, newTerrarium);

            }

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }
    public async Task<IBusinessResult> DeleteById(int id)
    {
        try
        {
            var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(id);
            if (terrarium != null)
            {
                // Xóa các bản ghi liên quan trong bảng TerrariumImage
                var relatedImages = await _unitOfWork.TerrariumImage.GetAllByTerrariumIdAsync(id);
                //.GetAllAsync(x => x.TerrariumId == id);

                foreach (var image in
                         relatedImages)
                    await _unitOfWork.TerrariumImage.RemoveAsync(image); // Gọi RemoveAsync cho từng đối tượng.
                // Xóa các bản ghi liên quan trong bảng TerrariumVariant
                var relatedVariants =
                    await _unitOfWork.TerrariumVariant
                        .GetAllByTerrariumIdAsync(
                            id); // Cần phương thức GetAllByTerrariumIdAsync trong Repository của TerrariumVariant
                foreach (var variant in
                         relatedVariants)
                    await _unitOfWork.TerrariumVariant
                        .RemoveAsync(variant); // Xóa các bản ghi liên quan trong TerrariumVariant
                var result = await _unitOfWork.Terrarium.RemoveAsync(terrarium);
                if (result) return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG, result);

                return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
            }

            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> FilterTerrariumsAsync(int? environmentId, int? shapeId, int? tankMethodId)
    {
        try
        {
            // 1) Lấy danh sách Terrarium đã lọc
            var terrariumList = await _unitOfWork.Terrarium
                .FilterTerrariumsAsync(environmentId, shapeId, tankMethodId);

            if (terrariumList == null || !terrariumList.Any())
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No terrariums match the filter.");

            var terrariumIds = terrariumList.Select(t => t.TerrariumId).ToList();

            // 2) Lấy batch rating stats (trung bình + số feedback)
            var ratingStats = await _unitOfWork.Terrarium.GetTerrariumRatingStatsAsync(terrariumIds);

            // 3) Lấy batch purchase count
            var purchaseCounts = await _unitOfWork.Terrarium.GetTerrariumPurchaseCountsAsync(terrariumIds);

            // 4) Map sang response
            var terrariums = terrariumList.Select(t => new TerrariumDetailResponse
            {
                TerrariumId = t.TerrariumId,
                EnvironmentId = t.EnvironmentId,
                ShapeId = t.ShapeId,
                TankMethodId = t.TankMethodId,
                TerrariumName = t.TerrariumName,
                Description = t.Description,
                MinPrice = (decimal)t.MinPrice,
                MaxPrice = (decimal)t.MaxPrice,
                Stock = t.Stock,
                Status = t.Status,
                TerrariumImages = t.TerrariumImages?.Select(ti => new TerrariumImageResponse
                {
                    TerrariumImageId = ti.TerrariumImageId,
                    TerrariumId = ti.TerrariumId,
                    ImageUrl = ti.ImageUrl
                }).ToList() ?? new List<TerrariumImageResponse>(),

                // ✅ Bổ sung 3 trường mới
                AverageRating = ratingStats.ContainsKey(t.TerrariumId) ? ratingStats[t.TerrariumId].AverageRating : 0,
                FeedbackCount = ratingStats.ContainsKey(t.TerrariumId) ? ratingStats[t.TerrariumId].FeedbackCount : 0,
                PurchaseCount = purchaseCounts.ContainsKey(t.TerrariumId) ? purchaseCounts[t.TerrariumId] : 0
            }).ToList();

            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, terrariums);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> GetTerrariumByNameAsync(string terrariumName)
    {
        try
        {
            // Lấy danh sách Terrarium theo tên
            var terrariumList = await _unitOfWork.Terrarium.GetTerrariumByNameAsync(terrariumName);

            if (terrariumList == null || !terrariumList.Any())
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No terrarium found with that name.");

            // Lấy rating trung bình và feedback count cho từng Terrarium (batch)
            var terrariumIds = terrariumList.Select(t => t.TerrariumId).ToList();
            var ratingStats = await _unitOfWork.Terrarium.GetTerrariumRatingStatsAsync(terrariumIds);

            var terrariumResponseList = terrariumList.Select(t => new TerrariumResponse
            {
                TerrariumId = t.TerrariumId,
                EnvironmentId = t.EnvironmentId,
                ShapeId = t.ShapeId,
                TankMethodId = t.TankMethodId,
                TerrariumName = t.TerrariumName,
                Description = t.Description,
                MinPrice = t.MinPrice,
                MaxPrice = t.MaxPrice,
                Stock = t.Stock,
                Status = t.Status,
                Accessories = t.TerrariumAccessory.Select(a => new TerrariumAccessoryResponse
                {
                    AccessoryId = a.Accessory.AccessoryId,
                    Name = a.Accessory.Name,
                    Description = a.Accessory.Description,
                    Price = a.Accessory.Price
                }).ToList(),
                BodyHTML = t.bodyHTML,
                CreatedAt = t.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = t.UpdatedAt ?? DateTime.UtcNow,
                TerrariumImages = t.TerrariumImages.Select(ti => new TerrariumImageResponse
                {
                    TerrariumImageId = ti.TerrariumImageId,
                    TerrariumId = ti.TerrariumId,
                    ImageUrl = ti.ImageUrl ?? string.Empty
                }).ToList(),
                AverageRating = ratingStats.ContainsKey(t.TerrariumId) ? ratingStats[t.TerrariumId].AverageRating : 0,
                FeedbackCount = ratingStats.ContainsKey(t.TerrariumId) ? ratingStats[t.TerrariumId].FeedbackCount : 0
            }).ToList();

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Terrarium(s) found.", terrariumResponseList);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }

    }
    public async Task<IBusinessResult> GetTopBestSellersAllTimeAsync(int topN)
    {
        // ids theo purchase all-time
        var topIds = await _unitOfWork.Terrarium.GetBestSellerTerrariumIdsAsync(topN);
        return await BuildCardListByTerrariumIds(topIds);
    }

    public async Task<IBusinessResult> GetTopBestSellersLastDaysAsync(int days, int topN)
    {
        var to = DateTime.UtcNow;
        var from = to.AddDays(-days);
        var topIds = await _unitOfWork.Terrarium.GetBestSellerTerrariumIdsInRangeAsync(from, to, topN);
        return await BuildCardListByTerrariumIds(topIds);
    }

    public async Task<IBusinessResult> GetTopRatedAsync(int topN)
    {
        var topIds = await _unitOfWork.Terrarium.GetTopRatedTerrariumIdsAsync(topN, minFeedback: 1);
        return await BuildCardListByTerrariumIds(topIds);
    }

    public async Task<IBusinessResult> GetNewestAsync(int topN)
    {
        var terrariums = await _unitOfWork.Terrarium.GetNewestAsync(topN);
        if (terrariums.Count == 0)
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

        // Bổ sung rating & purchase
        var ids = terrariums.Select(t => t.TerrariumId).ToList();
        var ratingStats = await _unitOfWork.Terrarium.GetTerrariumRatingStatsAsync(ids);
        var purchaseCounts = await _unitOfWork.Terrarium.GetTerrariumPurchaseCountsAsync(ids);

        var list = terrariums.Select(t => new TerrariumCardResponse
        {
            TerrariumId = t.TerrariumId,
            TerrariumName = t.TerrariumName,
            ThumbnailUrl = t.TerrariumImages?.FirstOrDefault()?.ImageUrl,
            MinPrice = t.MinPrice,
            MaxPrice = t.MaxPrice,
            Stock = t.Stock,
            Status = t.Status,
            AverageRating = ratingStats.TryGetValue(t.TerrariumId, out var rs) ? rs.AverageRating : 0,
            FeedbackCount = ratingStats.TryGetValue(t.TerrariumId, out rs) ? rs.FeedbackCount : 0,
            PurchaseCount = purchaseCounts.TryGetValue(t.TerrariumId, out var pc) ? pc : 0
        }).ToList();

        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, list);
    }

    // Helper dùng chung cho 3 API đầu
    private async Task<IBusinessResult> BuildCardListByTerrariumIds(List<int> terrariumIds)
    {
        if (terrariumIds == null || terrariumIds.Count == 0)
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

        // lấy entity
        var terrariums = await _unitOfWork.Terrarium.GetListByIdsAsync(terrariumIds);
        if (terrariums.Count == 0)
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

        var ratingStats = await _unitOfWork.Terrarium.GetTerrariumRatingStatsAsync(terrariumIds);
        var purchaseCounts = await _unitOfWork.Terrarium.GetTerrariumPurchaseCountsAsync(terrariumIds);

        // giữ nguyên thứ tự theo terrariumIds (đã sort từ repo)
        var dict = terrariums.ToDictionary(t => t.TerrariumId);
        var list = new List<TerrariumCardResponse>();
        foreach (var id in terrariumIds)
        {
            if (!dict.TryGetValue(id, out var t)) continue;

            list.Add(new TerrariumCardResponse
            {
                TerrariumId = t.TerrariumId,
                TerrariumName = t.TerrariumName,
                ThumbnailUrl = t.TerrariumImages?.FirstOrDefault()?.ImageUrl,
                MinPrice = t.MinPrice,
                MaxPrice = t.MaxPrice,
                Stock = t.Stock,
                Status = t.Status,
                AverageRating = ratingStats.TryGetValue(t.TerrariumId, out var rs) ? rs.AverageRating : 0,
                FeedbackCount = ratingStats.TryGetValue(t.TerrariumId, out rs) ? rs.FeedbackCount : 0,
                PurchaseCount = purchaseCounts.TryGetValue(t.TerrariumId, out var pc) ? pc : 0
            });
        }

        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, list);
    }
}
