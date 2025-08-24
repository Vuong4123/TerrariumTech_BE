using Microsoft.Extensions.Configuration;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.TerraniumLayout;
using TerrariumGardenTech.Common.RequestModel.Terrarium;
using TerrariumGardenTech.Common.ResponseModel.Base;
using TerrariumGardenTech.Common.ResponseModel.Terrarium;
using TerrariumGardenTech.Common.ResponseModel.TerrariumLayout;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using static TerrariumGardenTech.Common.Enums.CommonData;

namespace TerrariumGardenTech.Service.Service;

public class AITerrariumRequest
{
    public int EnvironmentId { get; set; }
    public int ShapeId { get; set; }
    public int TankMethodId { get; set; }
    public int AccessoryId { get; set; }
}

public class AITerrariumResponse
{
    public string TerrariumName { get; set; }
    public string Description { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; }
    public int Height { get; set; }    // cm
    public int Width { get; set; }     // cm
    public int Depth { get; set; }     // cm
    public List<string> TerrariumImages { get; set; }
    public List<string> Accessories { get; set; }
}

public class TerrariumService : ITerrariumService
{
    private readonly UnitOfWork _unitOfWork;
    private static readonly HttpClient _httpClient = new HttpClient();
    private readonly ICloudinaryService _cloudinaryService;

    public TerrariumService(UnitOfWork unitOfWork, ICloudinaryService cloudinaryService, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
    }
    #region Gen AI Terrarium
    public async Task<AITerrariumResponse> PredictTerrariumAsync(AITerrariumRequest request)
    {
        try
        {
            var environment = await _unitOfWork.Environment.GetByIdAsync(request.EnvironmentId);
            var shape = await _unitOfWork.Shape.GetByIdAsync(request.ShapeId);
            var tankMethod = await _unitOfWork.TankMethod.GetByIdAsync(request.TankMethodId);
            var accessory = await _unitOfWork.Accessory.GetByIdAsync(request.AccessoryId);

            if (environment == null || shape == null || tankMethod == null)
                throw new ArgumentException("Invalid environment, shape, or tank method ID");

            var terrarium = GenerateTerrarium(request, environment.EnvironmentName, shape.ShapeName,
                tankMethod.TankMethodDescription, accessory?.Name ?? "plant");

            var imageUrls = await GenerateTerrariumImages(environment.EnvironmentName, shape.ShapeName,
                accessory?.Name ?? "moss");

            terrarium.TerrariumImages = imageUrls;
            terrarium.ImageUrl = imageUrls.FirstOrDefault();

            return terrarium;
        }
        catch (Exception ex)
        {
            var fallback = await GenerateFallbackTerrarium(request);
            return fallback;
        }
    }

    private async Task<List<string>> GenerateTerrariumImages(string environment, string shape, string accessory)
    {
        var envTranslated = await TranslateEnvironment(environment);
        var shapeTranslated = await TranslateShape(shape);
        var accessoryTranslated = await TranslateAccessory(accessory);

        Console.WriteLine($"🌿 Generating: {environment} -> {envTranslated}, {shape} -> {shapeTranslated}");

        var prompts = new[]
        {
        $"{shapeTranslated} glass terrarium {envTranslated} plants moss stones clear container",
        $"closed {shapeTranslated} terrarium {envTranslated} ecosystem miniature landscape",
        $"{shapeTranslated} terrarium jar {envTranslated} plants {accessoryTranslated} soil layers",
        $"transparent {shapeTranslated} terrarium {envTranslated} garden botanical display"
    };

        var images = new List<string>();

        for (int i = 0; i < prompts.Length; i++)
        {
            var url = await GenerateImage(prompts[i], i + 1);
            if (url != null)
            {
                images.Add(url);
            }
            await Task.Delay(1500);
        }

        if (images.Count == 0)
        {
            images = await GenerateSimpleTerrariumImages(envTranslated, shapeTranslated);
        }

        return images;
    }

    private async Task<string> TranslateEnvironment(string environmentName)
    {
        try
        {
            var allEnvironments = await _unitOfWork.Environment.GetAllAsync();

            foreach (var env in allEnvironments)
            {
                var englishName = env.EnvironmentName.ToLower() switch
                {
                    var name when name.Contains("nhiệt đới") || name.Contains("tropical") => "tropical forest",
                    var name when name.Contains("sa mạc") || name.Contains("desert") => "desert landscape",
                    var name when name.Contains("ôn đới") || name.Contains("temperate") => "temperate forest",
                    var name when name.Contains("alpine") || name.Contains("núi") => "alpine mountain",
                    var name when name.Contains("địa trung hải") || name.Contains("mediterranean") => "mediterranean",
                    var name when name.Contains("rừng mưa") => "rainforest",
                    var name when name.Contains("thảo nguyên") => "grassland",
                    var name when name.Contains("ven biển") => "coastal",
                    _ => "tropical forest" // default
                };

                if (env.EnvironmentName == environmentName)
                    return englishName;
            }

            return "tropical forest"; // fallback
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Environment translation error: {ex.Message}");
            return "tropical forest";
        }
    }

    private async Task<string> TranslateShape(string shapeName)
    {
        try
        {
            var allShapes = await _unitOfWork.Shape.GetAllAsync();

            foreach (var shape in allShapes)
            {
                var englishShape = shape.ShapeName.ToLower() switch
                {
                    var name when name.Contains("cầu") || name.Contains("tròn") || name.Contains("sphere") => "spherical round",
                    var name when name.Contains("vuông") || name.Contains("cube") => "cubic square",
                    var name when name.Contains("chữ nhật") || name.Contains("rectangular") => "rectangular",
                    var name when name.Contains("oval") || name.Contains("ellipse") => "oval elliptical",
                    var name when name.Contains("kim tự tháp") || name.Contains("pyramid") => "pyramid triangular",
                    var name when name.Contains("lục giác") => "hexagonal",
                    var name when name.Contains("trụ") || name.Contains("cylinder") => "cylindrical",
                    _ => "rectangular" // default
                };

                if (shape.ShapeName == shapeName)
                    return englishShape;
            }

            return "rectangular"; // fallback
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Shape translation error: {ex.Message}");
            return "rectangular";
        }
    }

    private async Task<string> TranslateAccessory(string accessoryName)
    {
        if (string.IsNullOrEmpty(accessoryName))
            return "moss";

        try
        {
            var allAccessories = await _unitOfWork.Accessory.GetAllAsync();

            foreach (var acc in allAccessories)
            {
                var englishAccessory = acc.Name.ToLower() switch
                {
                    var name when name.Contains("akadama") => "akadama soil",
                    var name when name.Contains("đất") => "terrarium soil",
                    var name when name.Contains("rêu") || name.Contains("moss") => "moss",
                    var name when name.Contains("cây") => "small plants",
                    var name when name.Contains("đá") || name.Contains("stone") => "decorative stones",
                    var name when name.Contains("sỏi") => "gravel",
                    var name when name.Contains("cát") => "sand",
                    var name when name.Contains("gỗ") => "driftwood",
                    var name when name.Contains("than") => "activated carbon",
                    _ => acc.Name.ToLower() // giữ nguyên nếu không match
                };

                if (acc.Name == accessoryName)
                    return englishAccessory;
            }

            return "moss"; // fallback
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Accessory translation error: {ex.Message}");
            return "moss";
        }
    }

    private async Task<string> GenerateImage(string prompt, int seed)
    {
        try
        {
            var fullPrompt = $"{prompt} terrarium glass botanical container ecosystem";
            var encoded = Uri.EscapeDataString(fullPrompt);
            var randomSeed = seed * 1000 + new Random().Next(100, 999);

            var url = $"https://image.pollinations.ai/prompt/{encoded}?width=768&height=768&seed={randomSeed}&nologo=true&model=flux";

            Console.WriteLine($"🎨 Prompt {seed}: {fullPrompt}");

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(12) };
            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode &&
                response.Content.Headers.ContentType?.MediaType?.StartsWith("image/") == true)
            {
                return url;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Image generation failed: {ex.Message}");
            return null;
        }
    }

    private async Task<List<string>> GenerateSimpleTerrariumImages(string env, string shape)
    {
        var simplePrompts = new[]
        {
        $"{shape} terrarium {env}",
        $"glass terrarium {env} plants",
        $"{shape} bottle garden {env}",
        $"terrarium container {env}"
    };

        var images = new List<string>();

        for (int i = 0; i < simplePrompts.Length; i++)
        {
            var url = await GenerateImage(simplePrompts[i], i + 100);
            if (url != null)
            {
                images.Add(url);
            }
            await Task.Delay(1000);
        }

        return images;
    }

    private AITerrariumResponse GenerateTerrarium(AITerrariumRequest request, string env,
        string shape, string method, string accessory)
    {
        var rnd = new Random();

        return new AITerrariumResponse
        {
            TerrariumName = $"Terrarium {shape} {env}",
            Description = $"Terrarium {shape} phong cách {env}, sử dụng {method}, kết hợp {accessory}. " +
                         $"Kích thước: {rnd.Next(15, 35)}cm(H) x {rnd.Next(15, 30)}cm(W) x {rnd.Next(10, 25)}cm(D).",
            Height = rnd.Next(15, 35),
            Width = rnd.Next(15, 30),
            Depth = rnd.Next(10, 25),
            MinPrice = rnd.Next(180, 320) * 1000,
            MaxPrice = rnd.Next(350, 650) * 1000,
            Stock = rnd.Next(15, 45),
            ImageUrl = "",
            TerrariumImages = new List<string>(),
            Accessories = new List<string> { accessory, "Đất terrarium", "Sỏi trang trí", "Rêu tự nhiên" }
        };
    }

    private async Task<AITerrariumResponse> GenerateFallbackTerrarium(AITerrariumRequest request)
    {
        var rnd = new Random();

        try
        {
            var defaultEnv = await _unitOfWork.Environment.GetByIdAsync(request.EnvironmentId);
            var defaultShape = await _unitOfWork.Shape.GetByIdAsync(request.ShapeId);
            var defaultMethod = await _unitOfWork.TankMethod.GetByIdAsync(request.TankMethodId);

            var images = new List<string>();
            if (defaultEnv != null && defaultShape != null)
            {
                images = await GenerateTerrariumImages(defaultEnv.EnvironmentName,
                    defaultShape.ShapeName, "moss");
            }

            return new AITerrariumResponse
            {
                TerrariumName = $"Terrarium {defaultShape?.ShapeName ?? "Rectangular"} {defaultEnv?.EnvironmentName ?? "Tropical"}",
                Description = $"Terrarium {defaultShape?.ShapeName ?? "Rectangular"} phong cách {defaultEnv?.EnvironmentName ?? "Tropical"}, " +
                             $"sử dụng {defaultMethod?.TankMethodDescription ?? "Closed system"}. " +
                             $"Kích thước: {rnd.Next(15, 35)}cm(H) x {rnd.Next(15, 30)}cm(W) x {rnd.Next(10, 25)}cm(D).",
                Height = rnd.Next(15, 35),
                Width = rnd.Next(15, 30),
                Depth = rnd.Next(10, 25),
                MinPrice = rnd.Next(180, 320) * 1000,
                MaxPrice = rnd.Next(350, 650) * 1000,
                Stock = rnd.Next(15, 45),
                ImageUrl = images.FirstOrDefault() ?? "",
                TerrariumImages = images,
                Accessories = await GetDynamicAccessories()
            };
        }
        catch (Exception ex)
        {
            return new AITerrariumResponse
            {
                TerrariumName = "Terrarium Rectangular Tropical",
                Description = "Basic terrarium with tropical plants in rectangular container.",
                Height = 25,
                Width = 18,
                Depth = 12,
                MinPrice = 200000,
                MaxPrice = 400000,
                Stock = 20,
                ImageUrl = "",
                TerrariumImages = new List<string>(),
                Accessories = new List<string> { "Moss", "Soil", "Small plants" }
            };
        }
    }

    private async Task<List<string>> GetDynamicAccessories()
    {
        try
        {
            var accessories = await _unitOfWork.Accessory.GetAllAsync();
            return accessories.Take(4).Select(a => a.Name).ToList();
        }
        catch
        {
            return new List<string> { "Moss", "Soil", "Small plants", "Decorative stones" };
        }
    }
    #endregion

    #region Mấy hàm get
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
            GeneratedByAI = t.GeneratedByAI,
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

    public async Task<IBusinessResult> GetAllGeneratedByAI(TerrariumGetAllRequest request)
    {
        var tuple = await _unitOfWork.Terrarium.GetGenByAI(request);
        var list = tuple.Item1.ToList();

        if (!list.Any())
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

        var terrariums = list.Select(t => new TerrariumDetailResponse
        {
            TerrariumId = t.TerrariumId,
            TerrariumName = t.TerrariumName,
            Description = t.Description,
            MinPrice = t.MinPrice,
            MaxPrice = t.MaxPrice,
            Stock = t.Stock,
            Status = t.Status,
            GeneratedByAI = t.GeneratedByAI,
            TerrariumImages = t.TerrariumImages?.Select(ti => new TerrariumImageResponse
            {
                TerrariumImageId = ti.TerrariumImageId,
                TerrariumId = ti.TerrariumId,
                ImageUrl = ti.ImageUrl ?? string.Empty
            }).ToList()
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
        var topIds = await _unitOfWork.Terrarium.GetTopRatedTerrariumIdsAsync(topN);
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
    #endregion
    public async Task<IBusinessResult> CreateTerrariumAI(TerrariumCreateRequest terrariumCreateRequest)
    {
        try
        {
            // Lấy danh sách Accessory theo tên
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
                MinPrice = terrariumCreateRequest.MinPrice,
                MaxPrice = terrariumCreateRequest.MaxPrice,
                Stock = terrariumCreateRequest.Stock,

                Description = terrariumCreateRequest.Description,
                Status = terrariumCreateRequest.Status,
                GeneratedByAI = true,

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
                foreach (var accessory in accessories)
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
    public async Task<IBusinessResult> UpdateTerrarium(TerrariumUpdateRequest terrariumUpdateRequest)
    {
        try
        {
            var terra = await _unitOfWork.Terrarium
                .GetByIdAsync(terrariumUpdateRequest.TerrariumId);

            if (terra == null)
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

            // Kiểm tra xem tên Terrarium mới đã tồn tại chưa
            var existingTerrarium = await _unitOfWork.Terrarium
                .FindAsync(t => t.TerrariumName == terrariumUpdateRequest.TerrariumName && t.TerrariumId != terrariumUpdateRequest.TerrariumId);

            if (existingTerrarium.Any())
            {
                return new BusinessResult(Const.WARNING_EXISTING_NAME_CODE, "Tên Terrarium đã tồn tại.");
            }

            // Kiểm tra xem Environment, Shape, và TankMethod có tồn tại không
            var environmentExists = await _unitOfWork.Environment
                .FindAsync(e => e.EnvironmentId == terrariumUpdateRequest.EnvironmentId);
            if (environmentExists == null)
            {
                return new BusinessResult(Const.WARNING_INVALID_CODE, "Môi trường không tồn tại.");
            }

            var shapeExists = await _unitOfWork.Shape
                .FindAsync(s => s.ShapeId == terrariumUpdateRequest.ShapeId);
            if (shapeExists == null)
            {
                return new BusinessResult(Const.WARNING_INVALID_CODE, "Hình dạng không tồn tại.");
            }

            var tankMethodExists = await _unitOfWork.TankMethod
                .FindAsync(tm => tm.TankMethodId == terrariumUpdateRequest.TankMethodId);
            if (tankMethodExists == null)
            {
                return new BusinessResult(Const.WARNING_INVALID_CODE, "Phương pháp bể không tồn tại.");
            }

            // Cập nhật thuộc tính cơ bản
            terra.EnvironmentId = terrariumUpdateRequest.EnvironmentId;
            terra.ShapeId = terrariumUpdateRequest.ShapeId;
            terra.TankMethodId = terrariumUpdateRequest.TankMethodId;
            terra.TerrariumName = terrariumUpdateRequest.TerrariumName;
            terra.Description = terrariumUpdateRequest.Description;
            terra.Status = terrariumUpdateRequest.Status;
            terra.bodyHTML = terrariumUpdateRequest.bodyHTML ?? string.Empty;
            terra.UpdatedAt = DateTime.UtcNow;

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


    public async Task<IBusinessResult> CreateTerrarium(TerrariumCreate terrariumCreateRequest)
    {
        try
        {
            // Kiểm tra xem tên Terrarium đã tồn tại chưa
            var existingTerrarium = await _unitOfWork.Terrarium
                .FindAsync(t => t.TerrariumName == terrariumCreateRequest.TerrariumName);

            if (existingTerrarium.Any())
            {
                return new BusinessResult(Const.WARNING_EXISTING_NAME_CODE, "Tên Terrarium đã tồn tại.");
            }

            // Kiểm tra xem Environment, Shape, và TankMethod có tồn tại không
            var environmentExists = await _unitOfWork.Environment
                .FindAsync(e => e.EnvironmentId == terrariumCreateRequest.EnvironmentId);
            if (environmentExists == null)
            {
                return new BusinessResult(Const.WARNING_INVALID_CODE, "Môi trường không tồn tại.");
            }

            var shapeExists = await _unitOfWork.Shape
                .FindAsync(s => s.ShapeId == terrariumCreateRequest.ShapeId);
            if (shapeExists == null)
            {
                return new BusinessResult(Const.WARNING_INVALID_CODE, "Hình dạng không tồn tại.");
            }

            var tankMethodExists = await _unitOfWork.TankMethod
                .FindAsync(tm => tm.TankMethodId == terrariumCreateRequest.TankMethodId);
            if (tankMethodExists == null)
            {
                return new BusinessResult(Const.WARNING_INVALID_CODE, "Phương pháp bể không tồn tại.");
            }

            // Nếu có AccessoryNames thì mới tìm
            List<Accessory> accessories = new();
            if (terrariumCreateRequest.AccessoryNames != null && terrariumCreateRequest.AccessoryNames.Any())
            {
                accessories = await _unitOfWork.Accessory.GetByName(terrariumCreateRequest.AccessoryNames);
            }

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

    // CREATE - Fixed với proper repository calls
    public async Task<TerrariumLayout> CreateAsync(CreateLayoutRequest request)
    {
        try
        {
            // Kiểm tra terrarium tồn tại
            var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(request.TerrariumId);
            if (terrarium == null)
                throw new ArgumentException("Terrarium not found");

            // Tạo layout đơn giản
            var layout = new TerrariumLayout
            {
                UserId = request.userId,
                LayoutName = request.LayoutName,
                TerrariumId = request.TerrariumId,
                Status = LayoutStatus.Draft,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            await _unitOfWork.TerrariumLayout.CreateAsync(layout);
            return layout;
        }
        catch (Exception e)
        {
            return null;
        }
    }
    // Service method
    public async Task<IBusinessResult> SubmitLayoutAsync(int layoutId, int userId)
    {
        var layout = await _unitOfWork.TerrariumLayout.GetByIdAsync(layoutId);

        if (layout == null)
            return new BusinessResult(Const.FAIL_READ_CODE, "Layout không tồn tại");

        if (layout.UserId != userId)
            return new BusinessResult(Const.FAIL_UPDATE_CODE, "Bạn không có quyền với layout này");

        if (layout.Status != LayoutStatus.Draft)
            return new BusinessResult(Const.FAIL_UPDATE_CODE, "Layout đã được gửi trước đó");

        // Validate layout có đủ thông tin chưa
        if (string.IsNullOrEmpty(layout.LayoutName))
            return new BusinessResult(Const.FAIL_UPDATE_CODE, "Vui lòng nhập tên layout");

        // Chuyển sang Pending
        layout.Status = LayoutStatus.Pending;
        layout.UpdatedDate = DateTime.Now;

        await _unitOfWork.TerrariumLayout.UpdateAsync(layout);
        await _unitOfWork.SaveAsync();

        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Đã gửi yêu cầu thành công", layout);
    }

    // GET BY ID - Simple, no includes
    public async Task<TerrariumLayoutDetailDto?> GetByIdAsync(int id)
    {
        var layout = await _unitOfWork.TerrariumLayout.GetByIdAsync(id);
        if (layout == null) return null;

        return new TerrariumLayoutDetailDto
        {
            LayoutId = layout.LayoutId,
            LayoutName = layout.LayoutName,
            Status = layout.Status.ToString(),
            FinalPrice = layout.FinalPrice,
            CreatedDate = layout.CreatedDate,
            UpdatedDate = layout.UpdatedDate,
            UserId = layout.UserId,
            TerrariumId = layout.TerrariumId,
            ReviewedBy = layout.ReviewedBy,
            ReviewDate = layout.ReviewDate,
            ReviewNotes = layout.ReviewNotes
        };
    }

    // GET ALL - Simple list
    public async Task<List<TerrariumLayoutDto>> GetAllAsync()
    {
        var layouts = await _unitOfWork.TerrariumLayout.GetAllAsync();

        return layouts.Select(l => new TerrariumLayoutDto
        {
            LayoutId = l.LayoutId,
            LayoutName = l.LayoutName,
            Status = l.Status.ToString(),
            FinalPrice = l.FinalPrice,
            CreatedDate = l.CreatedDate,
            UpdatedDate = l.UpdatedDate,
            UserId = l.UserId,
            TerrariumId = l.TerrariumId,
            ReviewedBy = l.ReviewedBy,
            ReviewDate = l.ReviewDate,
            ReviewNotes = l.ReviewNotes
        }).ToList();
    }

    // GET BY USER
    public async Task<List<TerrariumLayoutDto>> GetByUserIdAsync(int userId)
    {
        var allLayouts = await _unitOfWork.TerrariumLayout.GetAllAsync();
        var userLayouts = allLayouts.Where(l => l.UserId == userId).ToList();

        return userLayouts.Select(l => new TerrariumLayoutDto
        {
            LayoutId = l.LayoutId,
            LayoutName = l.LayoutName,
            Status = l.Status.ToString(),
            FinalPrice = l.FinalPrice,
            CreatedDate = l.CreatedDate,
            UpdatedDate = l.UpdatedDate,
            UserId = l.UserId,
            TerrariumId = l.TerrariumId,
            ReviewedBy = l.ReviewedBy,
            ReviewDate = l.ReviewDate,
            ReviewNotes = l.ReviewNotes
        }).ToList();
    }

    // GET PENDING
    public async Task<List<TerrariumLayoutDto>> GetPendingAsync()
    {
        var allLayouts = await _unitOfWork.TerrariumLayout.GetAllAsync();
        var pendingLayouts = allLayouts.Where(l => l.Status == LayoutStatus.Pending).ToList();

        return pendingLayouts.Select(l => new TerrariumLayoutDto
        {
            LayoutId = l.LayoutId,
            LayoutName = l.LayoutName,
            Status = l.Status.ToString(),
            FinalPrice = l.FinalPrice,
            CreatedDate = l.CreatedDate,
            UpdatedDate = l.UpdatedDate,
            UserId = l.UserId,
            TerrariumId = l.TerrariumId,
            ReviewedBy = l.ReviewedBy,
            ReviewDate = l.ReviewDate,
            ReviewNotes = l.ReviewNotes
        }).ToList();
    }

    // SINGLE UPDATE METHOD - Handle all updates
    public async Task<bool> UpdateAsync(int id, UpdateLayoutRequest request)
    {
        var layout = await _unitOfWork.TerrariumLayout.GetByIdAsync(id);
        if (layout == null) return false;

        // Update all provided fields
        if (!string.IsNullOrEmpty(request.LayoutName))
            layout.LayoutName = request.LayoutName;

        if (request.TerrariumId.HasValue)
        {
            // Validate terrarium exists
            var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(request.TerrariumId.Value);
            if (terrarium == null)
                throw new ArgumentException("Terrarium not found");

            layout.TerrariumId = request.TerrariumId.Value;
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            // Validate status
            var validStatuses = new[] { "Pending", "Approved", "Rejected", "Ordered" };
            if (!validStatuses.Contains(request.Status))
                throw new ArgumentException("Invalid status");

            layout.Status = request.Status;

            // Auto set review info when status changes
            if (request.Status != "Pending" && request.ReviewedBy.HasValue)
            {
                layout.ReviewedBy = request.ReviewedBy.Value;
                layout.ReviewDate = DateTime.Now;
            }
        }

        if (request.FinalPrice.HasValue)
            layout.FinalPrice = request.FinalPrice.Value;

        if (request.ReviewedBy.HasValue)
            layout.ReviewedBy = request.ReviewedBy.Value;

        if (!string.IsNullOrEmpty(request.ReviewNotes))
            layout.ReviewNotes = request.ReviewNotes;

        layout.UpdatedDate = DateTime.Now;

        await _unitOfWork.TerrariumLayout.UpdateAsync(layout);
        return true;
    }

    // REVIEW
    public async Task<TerrariumLayout> ReviewAsync(int id, int managerId, string status, decimal? price, string? notes)
    {
        var layout = await _unitOfWork.TerrariumLayout.GetByIdAsync(id);
        if (layout == null) throw new ArgumentException("Layout not found");

        layout.Status = status;
        layout.FinalPrice = price;
        layout.ReviewedBy = managerId;
        layout.ReviewDate = DateTime.Now;
        layout.ReviewNotes = notes;
        layout.UpdatedDate = DateTime.Now;

        await _unitOfWork.TerrariumLayout.UpdateAsync(layout);
        return layout;
    }
}