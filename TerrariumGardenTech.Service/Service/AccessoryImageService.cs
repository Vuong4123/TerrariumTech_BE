using Microsoft.AspNetCore.Http;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.AccessoryImage;
using static System.Net.Mime.MediaTypeNames;

namespace AccessoryGardenTech.Service.Service
{
    public class AccessoryImageService(UnitOfWork _unitOfWork, ICloudinaryService _cloudinaryService ) : IAccessoryImageService
    {
        

        public async Task<IBusinessResult> GetAll()
        {
            var accessoryImages = await _unitOfWork.AccessoryImage.GetAllAsync();
            if (accessoryImages != null )
            {
                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, accessoryImages);
            }
            else
            {
                return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
            }
        }

        public async Task<IBusinessResult> GetById(int id)
        {
            var accessoryImage = await _unitOfWork.AccessoryImage.GetByIdAsync(id);
            if (accessoryImage != null)
            {
                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, accessoryImage);
            }
            else
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
            }
        }

        public async Task<IBusinessResult> UpdateAccessory(int accessoryImageId, IFormFile? newImageFile)
        {
            try
            {
                var existing = await _unitOfWork.AccessoryImage.GetByIdAsync(accessoryImageId);
                if (existing == null)
                    return new BusinessResult(Const.FAIL_READ_CODE, "Accessory image not found.");

                var newUrl = existing.ImageUrl;

                if (newImageFile != null)
                {
                    // Fix for CS0029: Extract the URL from the IBusinessResult returned by UploadImageAsync
                    var uploadResult = await _cloudinaryService.UploadImageAsync(newImageFile, $"terrariums/{accessoryImageId}", string.Empty);
                    if (uploadResult.Status == Const.SUCCESS_UPLOAD_CODE && uploadResult.Data is string uploadedUrl)
                    {
                        newUrl = uploadedUrl;

                        // Delete the old image if necessary
                        if (!string.IsNullOrEmpty(existing.ImageUrl))
                        {
                            await _cloudinaryService.DeleteImageAsync(existing.ImageUrl);
                        }
                        existing.ImageUrl = newUrl;
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_UPLOAD_CODE, "Failed to upload new image.");
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
        public async Task<IBusinessResult> CreateAccessory(IFormFile imageFile, int accessoryId)
        {
            if (imageFile == null || imageFile.Length == 0)
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Image file is required.");

            try
            {
                // Upload image to Cloudinary and get the result (UploadResult)
                var imageUrl = await _cloudinaryService.UploadImageAsync(imageFile, $"terrariums/{accessoryId}", accessoryId.ToString());

                // Check if the upload was successful
                if (imageUrl == null || string.IsNullOrEmpty(imageUrl.ToString())) // Fix: Ensure imageUrl is treated as a string
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "Cloudinary up image fail");
                }

                // Create new AccessoryImage object
                var terraImage = new AccessoryImage
                {
                    AccessoryId = accessoryId,
                    ImageUrl = imageUrl.ToString() // Fix: Convert imageUrl to string explicitly
                };

                // Save the new image into the database
                var result = await _unitOfWork.AccessoryImage.CreateAsync(terraImage);
                if (result > 0)
                {
                    return new BusinessResult
                    {
                        Status = 1,
                        Message = "Image created successfully.",
                        Data = imageUrl.ToString() // Fix: Ensure Data contains the string representation of imageUrl
                    };
                }

                return new BusinessResult(Const.FAIL_CREATE_CODE, "Image upload failed.");
            }
            catch (Exception ex)
            {
                // Return exception message if an error occurred
                return new BusinessResult(Const.FAIL_CREATE_CODE, ex.Message);
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
            {
                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, accessoryImages);
            }
            else
            {
                return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG, "No images found for the given AccessoryId.");
            }
        }
    }
}
