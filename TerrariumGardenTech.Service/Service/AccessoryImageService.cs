using Microsoft.AspNetCore.Http;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.AccessoryImage;

namespace TerrariumGardenTech.Service.Service;

public class AccessoryImageService(UnitOfWork _unitOfWork, ICloudinaryService _cloudinaryService)
    : IAccessoryImageService
{
    public async Task<IBusinessResult> GetAll()
    {
        var accessoryImages = await _unitOfWork.AccessoryImage.GetAllAsync();
        if (accessoryImages != null)
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, accessoryImages);

        return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
    }

        public async Task<IBusinessResult> GetById(int id)
        {
            var accessoryImage = await _unitOfWork.AccessoryImage.GetByIdAsync(id);
            if (accessoryImage != null)
                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, accessoryImage);

            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
        }

    public async Task<IBusinessResult> UpdateAccessoryImage(AccessoryImageUploadUpdateRequest request)
    {
        try
        {
            var existing = await _unitOfWork.AccessoryImage.GetByIdAsync(request.AccessoryImageId);
            if (existing == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Accessory image not found.");

            // Cập nhật AccessoryId nếu có field này trong DB
            existing.AccessoryId = request.AccessoryId;

            if (request.ImageFile != null)
            {
                var uploadResult = await _cloudinaryService.UploadImageAsync(
                    request.ImageFile,
                    $"accessories/{request.AccessoryId}", // đường dẫn lưu ảnh
                    null
                );

                if (uploadResult.Status == Const.SUCCESS_CREATE_CODE && uploadResult.Data is string uploadedUrl)
                {
                    // Xoá ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(existing.ImageUrl))
                    {
                        await _cloudinaryService.DeleteImageAsync(existing.ImageUrl);
                    }

                    existing.ImageUrl = uploadedUrl;
                }
                else
                {
                    return new BusinessResult(Const.FAIL_UPLOAD_CODE, "Upload ảnh mới thất bại.");
                }
            }

            var result = await _unitOfWork.AccessoryImage.UpdateAsync(existing);
            if (result > 0)
                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, existing);

            return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> CreateAccessoryImage(IFormFile imageFile, int accessoryId)
    {
        if (imageFile == null || imageFile.Length == 0)
            return new BusinessResult(Const.FAIL_CREATE_CODE, "Image file is required.");

        try
        {
            // Upload image to Cloudinary and get the result (UploadResult)
            var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile, $"terrariums/{accessoryId}", accessoryId.ToString());

            // Check if the upload was successful and the URL is valid
            if (uploadResult == null || string.IsNullOrEmpty(uploadResult.ToString()))
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Cloudinary image upload failed.");

            string? imageUrl = uploadResult.Data.ToString();  // Extract image URL

            // Create new AccessoryImage object
            var accessImage = new AccessoryImage
            {
                AccessoryId = accessoryId,
                ImageUrl = imageUrl
            };

            // Save the new image into the database
            var result = await _unitOfWork.AccessoryImage.CreateAsync(accessImage);
            if (result > 0)
                return new BusinessResult
                {
                    Status = Const.SUCCESS_CREATE_CODE,
                    Message = "Image created successfully.",
                    Data = imageUrl // Return the image URL
                };

            return new BusinessResult(Const.FAIL_CREATE_CODE, "Image upload failed.");
        }
        catch (Exception ex)
        {
            // Return exception message if an error occurred
            return new BusinessResult(Const.FAIL_CREATE_CODE, $"An error occurred: {ex.Message}");
        }
    }

    public async Task<IBusinessResult> DeleteById(int id)
    {
        var image = await _unitOfWork.AccessoryImage.GetByIdAsync(id);
        if (image == null)
            return new BusinessResult(Const.FAIL_READ_CODE, "Terrarium image not found.");

        try
        {
            // Xóa ảnh khỏi cơ sở dữ liệu
            var deleted = await _unitOfWork.AccessoryImage.RemoveAsync(image);
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

        public async Task<IBusinessResult> GetByAccessoryId(int accessoryId)
        {
            var accessoryImages = await _unitOfWork.AccessoryImage.GetAllByAccessoryIdAsync(accessoryId);
            if (accessoryImages != null && accessoryImages.Any())
                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, accessoryImages);

            return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG,
                "No images found for the given AccessoryId.");
        }
    }
