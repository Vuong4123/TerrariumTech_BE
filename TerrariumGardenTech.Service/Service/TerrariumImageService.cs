using Microsoft.AspNetCore.Http;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.TerrariumImage;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class TerrariumImageService : ITerrariumImageService
{
    private readonly ICloudinaryService _cloudinaryService;
    private readonly UnitOfWork _unitOfWork;

    public TerrariumImageService(UnitOfWork unitOfWork, ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<IBusinessResult> GetAllTerrariumImageAsync()
    {
        var terraImage = await _unitOfWork.TerrariumImage.GetAllAsync();
        if (terraImage == null) return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, terraImage);
    }

    public async Task<IBusinessResult?> GetTerrariumImageByIdAsync(int Id)
    {
        var terraImage = await _unitOfWork.TerrariumImage.GetByIdAsync(Id);
        if (terraImage == null) return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, terraImage);
    }

    public async Task<IBusinessResult> GetByTerrariumId(int terrariumId)
    {
        var terrariumImages = await _unitOfWork.TerrariumImage.GetAllByTerrariumIdAsync(terrariumId);
        if (terrariumImages != null && terrariumImages.Any())
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, terrariumImages);

        return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG,
            "No images found for the given AccessoryId.");
    }

    public async Task<IBusinessResult> UpdateTerrariumImageAsync(TerrariumImageUploadUpdateRequest request)
    {
        try
        {
            var existing = await _unitOfWork.TerrariumImage.GetByIdAsync(request.TerrariumImageId);
            if (existing == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Không tìm thấy ảnh Terrarium.");

            // Cập nhật TerrariumId nếu cần
            existing.TerrariumId = request.TerrariumId;

            // Nếu có ảnh mới thì xử lý upload
            if (request.ImageFile != null)
            {
                // Upload ảnh mới lên Cloudinary
                var uploadResult = await _cloudinaryService.UploadImageAsync(
                    request.ImageFile,
                    $"terrariums/{request.TerrariumId}" // publicId optional
                );

                if (uploadResult.Status == Const.SUCCESS_CREATE_CODE && uploadResult.Data is string uploadedUrl)
                {
                    // Xoá ảnh cũ nếu tồn tại
                    if (!string.IsNullOrEmpty(existing.ImageUrl))
                        await _cloudinaryService.DeleteImageAsync(existing.ImageUrl);

                    existing.ImageUrl = uploadedUrl;
                }
                else
                {
                    return new BusinessResult(Const.FAIL_UPLOAD_CODE, "Upload ảnh mới thất bại.");
                }
            }

            // Cập nhật trong DB
            var result = await _unitOfWork.TerrariumImage.UpdateAsync(existing);
            if (result > 0)
                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, existing);

            return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }


    public async Task<IBusinessResult> CreateTerrariumImageAsync(IFormFile imageFile, int terrariumId)
    {
        if (imageFile == null || imageFile.Length == 0)
            return new BusinessResult(Const.FAIL_CREATE_CODE, "Image file is required.");

        try
        {
            // Upload image to Cloudinary and get the result (UploadResult)
            var res_imageUrl =
                await _cloudinaryService.UploadImageAsync(imageFile, $"terrariums/{terrariumId}",
                    terrariumId.ToString());

            // Ensure the result is a valid URL
            if (res_imageUrl == null || string.IsNullOrEmpty(res_imageUrl.ToString()))
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Cloudinary upload failed.");

            // Create new TerrariumImage object
            var terraImage = new TerrariumImage
            {
                TerrariumId = terrariumId,
                ImageUrl = res_imageUrl.Data.ToString() // Explicitly convert the result to a string (URL)
            };

            // Save the new image into the database
            var result = await _unitOfWork.TerrariumImage.CreateAsync(terraImage);
            if (result > 0)
                return new BusinessResult
                {
                    Status = Const.SUCCESS_CREATE_CODE,
                    Message = "Image created successfully.",
                    Data = terraImage
                };

            return new BusinessResult(Const.FAIL_CREATE_CODE, "Failed to create the image.");
        }
        catch (Exception ex)
        {
            // Return exception message if an error occurred
            return new BusinessResult(Const.FAIL_CREATE_CODE, ex.Message);
        }
    }


    public async Task<IBusinessResult> DeleteTerrariumImageAsync(int imageId)
    {
        var image = await _unitOfWork.TerrariumImage.GetByIdAsync(imageId);
        if (image == null)
            return new BusinessResult(Const.FAIL_READ_CODE, "Terrarium image not found.");

        try
        {
            // Xóa ảnh khỏi cơ sở dữ liệu
            var deleted = await _unitOfWork.TerrariumImage.RemoveAsync(image);
            if (deleted)
            {
                // Xoá ảnh trong Cloudinary (nếu có URL)
                if (!string.IsNullOrEmpty(image.ImageUrl))
                    await _cloudinaryService.DeleteImageAsync(image.ImageUrl);

                return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);
            }

            return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }
}