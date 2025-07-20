using Microsoft.AspNetCore.Http;
using TerrariumGardenTech.Common;
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

    public async Task<IBusinessResult> UpdateTerrariumImageAsync(int terrariumImageId, IFormFile? newImageFile)
    {
        try
        {
            var existing = await _unitOfWork.TerrariumImage.GetByIdAsync(terrariumImageId);
            if (existing == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Terrarium image not found.");

            var newUrl = existing.ImageUrl;

            if (newImageFile != null)
            {
                // Fix for CS0029: Extract the URL from the IBusinessResult returned by UploadImageAsync
                var uploadResult =
                    await _cloudinaryService.UploadImageAsync(newImageFile, $"terrariums/{terrariumImageId}",
                        string.Empty);
                if (uploadResult.Status == Const.SUCCESS_UPLOAD_CODE && uploadResult.Data is string uploadedUrl)
                {
                    newUrl = uploadedUrl;

                    // Delete the old image if necessary
                    if (!string.IsNullOrEmpty(existing.ImageUrl))
                        await _cloudinaryService.DeleteImageAsync(existing.ImageUrl);
                    existing.ImageUrl = newUrl;
                }
                else
                {
                    return new BusinessResult(Const.FAIL_UPLOAD_CODE, "Failed to upload new image.");
                }
            }

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

            // Check if the upload was successful
            if (res_imageUrl.Data == null ||
                string.IsNullOrEmpty(res_imageUrl.ToString())) // Fix: Ensure res_imageUrl is treated as a string
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Cloudinary up image fail");

            // Create new TerrariumImage object
            var terraImage = new TerrariumImage
            {
                TerrariumId = terrariumId,
                ImageUrl = res_imageUrl.Data?.ToString() // Fix: Convert imageUrl to string explicitly
            };

            // Save the new image into the database
            var result = await _unitOfWork.TerrariumImage.CreateAsync(terraImage);
            if (result > 0)
                return new BusinessResult
                {
                    Status = 1,
                    Message = "Image created successfully.",
                    Data = terraImage
                };

            return new BusinessResult(Const.FAIL_CREATE_CODE, "Image upload failed.");
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