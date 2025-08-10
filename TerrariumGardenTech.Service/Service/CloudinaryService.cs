using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService()
    {
        var cloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");
        var cloudinary = new Cloudinary(cloudinaryUrl);
        cloudinary.Api.Secure = true;
        _cloudinary = cloudinary;
    }

    // Upload ảnh lên Cloudinary
    public async Task<IBusinessResult> UploadImageAsync(IFormFile file, string folder, string publicId = null)
    {
        if (file == null || file.Length == 0)
            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);

        try
        {
            // Tạo đối tượng UploadParams
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = folder,
                PublicId = publicId
            };

            // Gọi Cloudinary để tải lên
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            // Kiểm tra kết quả upload
            if (uploadResult?.StatusCode == HttpStatusCode.OK && uploadResult.SecureUrl != null)
                return new BusinessResult
                {
                    Status = Const.SUCCESS_CREATE_CODE,
                    Message = "Image uploaded successfully",
                    Data = uploadResult.SecureUrl.ToString() // Trả về URL hình ảnh đã upload
                };

            // Nếu upload thất bại, trả về thông báo lỗi chi tiết
            return new BusinessResult(Const.FAIL_CREATE_CODE,
                "Image upload failed. Please check Cloudinary configuration or try again later.");
        }
        catch (Exception ex)
        {
            // Trả về lỗi chi tiết khi xảy ra exception
            return new BusinessResult(Const.FAIL_CREATE_CODE, $"An error occurred: {ex.Message}");
        }
    }

    // Xoá ảnh trên Cloudinary
    public async Task<IBusinessResult> DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return new BusinessResult(Const.FAIL_DELETE_CODE, "Image URL is invalid.");

        try
        {
            if (!TryExtractPublicIdFromUrl(imageUrl, out var publicId))
                return new BusinessResult(Const.FAIL_DELETE_CODE, "Cannot parse Cloudinary publicId from URL.");

            var deletion = new DeletionParams(publicId)
            {
                // Nếu bạn cần xoá video/raw, có thể set:
                // ResourceType = ResourceType.Video / ResourceType.Raw
                // Mặc định sẽ là Image
            };

            var result = await _cloudinary.DestroyAsync(deletion);

            if (result.Result == "ok")
                return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);

            return new BusinessResult(Const.FAIL_DELETE_CODE, result.Error?.Message ?? "Failed to delete image.");
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    private static bool TryExtractPublicIdFromUrl(string url, out string publicId)
    {
        publicId = null!;
        if (string.IsNullOrWhiteSpace(url)) return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;

        // Ví dụ path:
        // /image/upload/v1700000000/terrarium/users/123/avatar.jpg
        // /image/upload/w_300,h_300,c_fill/v1700000000/terrarium/users/123/avatar.webp
        // /video/upload/v1700000000/folder/my-video.mp4
        // /raw/upload/v1700000000/folder/data.json

        var path = uri.AbsolutePath.TrimStart('/');

        // Tìm đoạn sau "/upload/"
        var marker = "/upload/";
        var idx = path.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return false;

        var afterUpload = path[(idx + marker.Length)..]; // có thể bắt đầu bằng "w_...," (transform) hoặc "v123..." hoặc folder

        // Bỏ segment transformation nếu có (nó luôn là 1 segment ngay sau /upload/ và có dấu ',')
        var firstSlash = afterUpload.IndexOf('/');
        if (firstSlash > 0)
        {
            var firstSegment = afterUpload[..firstSlash];
            if (firstSegment.Contains(',')) // có transformation
            {
                afterUpload = afterUpload[(firstSlash + 1)..];
            }
        }

        // Bỏ version nếu có (v + số) – VD: v1700000000
        if (afterUpload.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            var slash = afterUpload.IndexOf('/');
            if (slash > 0 && afterUpload.AsSpan(1, slash - 1).ToString().All(char.IsDigit))
            {
                afterUpload = afterUpload[(slash + 1)..];
            }
        }

        // afterUpload bây giờ là "terrarium/users/123/avatar.jpg"
        // Bỏ phần mở rộng
        var fileName = Path.GetFileName(afterUpload);
        var noExt = Path.GetFileNameWithoutExtension(fileName);
        var dir = Path.GetDirectoryName(afterUpload)?.Replace('\\', '/');

        publicId = string.IsNullOrEmpty(dir) ? noExt : $"{dir}/{noExt}";
        return !string.IsNullOrWhiteSpace(publicId);
    }



}