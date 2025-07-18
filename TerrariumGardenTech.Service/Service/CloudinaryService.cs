﻿using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var cloudinarySettings = configuration.GetSection("CloudinarySettings");
        var account = new Account(
            cloudinarySettings["CloudName"],
            cloudinarySettings["ApiKey"],
            cloudinarySettings["ApiSecret"]
        );
        _cloudinary = new Cloudinary(account);
    }

    // Upload ảnh lên Cloudinary
    public async Task<IBusinessResult> UploadImageAsync(IFormFile file, string folder, string publicId = null)
    {
        if (file == null || file.Length == 0)
            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);

        try
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = folder,
                PublicId = publicId
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            // Check if the upload was successful
            if (uploadResult.StatusCode == HttpStatusCode.OK)
                // Return success result with image URL
                return new BusinessResult
                {
                    Status = Const.SUCCESS_CREATE_CODE,
                    Message = "Image uploaded successfully",
                    Data = uploadResult.SecureUrl.ToString()
                };

            // Return failure result if the upload failed
            return new BusinessResult(Const.FAIL_CREATE_CODE, "Image upload failed.");
        }
        catch (Exception ex)
        {
            // Return exception message if an error occurred
            return new BusinessResult(Const.FAIL_CREATE_CODE, ex.Message);
        }
    }

    // Xoá ảnh trên Cloudinary
    public async Task<IBusinessResult> DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return new BusinessResult(Const.FAIL_DELETE_CODE, "Image URL is invalid.");

        try
        {
            var publicId = imageUrl.Split('/').Last().Split('.').First(); // Lấy publicId từ URL
            var deleteParams = new DeletionParams(publicId);
            var deleteResult = await _cloudinary.DestroyAsync(deleteParams);

            if (deleteResult.StatusCode == HttpStatusCode.OK)
                return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);

            return new BusinessResult(Const.FAIL_DELETE_CODE, "Failed to delete image from Cloudinary.");
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }
}