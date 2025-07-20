using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Blog;

namespace TerrariumGardenTech.Service.Service;

public class BlogService(UnitOfWork _unitOfWork, IUserContextService userContextService, ICloudinaryService _cloudinaryService) : IBlogService
{
    public async Task<IBusinessResult> GetAll()
    {
        var blogs = await _unitOfWork.Blog.GetAllAsync();
        if (blogs != null && blogs.Any())
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, blogs);

        return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
    }

    public async Task<IBusinessResult> GetById(int id)
    {
        var blog = await _unitOfWork.Blog.GetByIdAsync(id);
        if (blog != null) return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, blog);

        return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
    }

    public async Task<IBusinessResult> Save(Blog blog)
    {
        try
        {
            var result = -1;
            var blogEntity = _unitOfWork.Blog.GetByIdAsync(blog.BlogId);
            if (blogEntity != null)
            {
                result = await _unitOfWork.Blog.UpdateAsync(blog);
                if (result > 0) return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, blog);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            // Create new terrarium if it does not exist
            result = await _unitOfWork.Blog.CreateAsync(blog);
            if (result > 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, blog);

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    public async Task<IBusinessResult> UpdateBlog(BlogUpdateRequest blogUpdateRequest)
    {
        try
        {
            // Kiểm tra blog category có tồn tại không
            var blogCategoryExists =
                await _unitOfWork.BlogCategory.AnyAsync(c => c.BlogCategoryId == blogUpdateRequest.BlogCategoryId);

            if (!blogCategoryExists)
                return new BusinessResult(Const.FAIL_CREATE_CODE, "BlogCategoryId không tồn tại.");

            // Lấy blog theo ID
            var blog = await _unitOfWork.Blog.GetByIdAsync(blogUpdateRequest.BlogId);
            if (blog == null)
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

            // Nếu có ảnh mới → xóa ảnh cũ và upload ảnh mới
            if (blogUpdateRequest.ImageFile != null)
            {
                // Xoá ảnh cũ nếu có
                if (!string.IsNullOrEmpty(blog.UrlImage))
                {
                    await _cloudinaryService.DeleteImageAsync(blog.UrlImage); // gọi hàm bạn đã viết
                }

                // Upload ảnh mới
                var uploadResult = await _cloudinaryService.UploadImageAsync(
                    blogUpdateRequest.ImageFile,
                    folder: "blog_images"
                );

                if (uploadResult.Status == Const.SUCCESS_CREATE_CODE)
                {
                    blog.UrlImage = uploadResult.Data?.ToString();
                }
                else
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "Upload ảnh mới thất bại: " + uploadResult.Message);
                }
            }

            // Cập nhật các thông tin còn lại
            blog.Title = blogUpdateRequest.Title;
            blog.Content = blogUpdateRequest.Content;
            blog.bodyHTML = blogUpdateRequest.bodyHTML;
            blog.BlogCategoryId = blogUpdateRequest.BlogCategoryId;
            blog.UpdatedAt = DateTime.UtcNow;

            // Lưu thay đổi
            var result = await _unitOfWork.Blog.UpdateAsync(blog);

            if (result > 0)
                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, blog);

            return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    public async Task<IBusinessResult> CreateBlog(BlogCreateRequest blogCreateRequest)
    {
        try
        {
            var currentUserId = userContextService.GetCurrentUser();

            string? uploadedImageUrl = null;

            // Nếu có ảnh thì upload
            if (blogCreateRequest.ImageFile != null)
            {
                var uploadResult = await _cloudinaryService.UploadImageAsync(
                    blogCreateRequest.ImageFile,
                    folder: "blog_images"
                );

                if (uploadResult.Status == Const.SUCCESS_CREATE_CODE)
                {
                    uploadedImageUrl = uploadResult.ToString();
                }
                else
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "Upload ảnh thất bại: " + uploadResult.Message);
                }
            }

            var blog = new Blog
            {
                UserId = currentUserId,
                Title = blogCreateRequest.Title,
                Content = blogCreateRequest.Content,
                UrlImage = uploadedImageUrl,
                BlogCategoryId = blogCreateRequest.BlogCategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                bodyHTML = blogCreateRequest.bodyHTML,
                Status = "Active"
            };

            var result = await _unitOfWork.Blog.CreateAsync(blog);

            if (result > 0)
            {
                return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, blog);
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
        // Lấy blog theo ID
        var blog = await _unitOfWork.Blog.GetByIdAsync(id);
        if (blog == null)
        {
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
        }

        try
        {
            // Nếu có ảnh → xóa ảnh khỏi Cloudinary
            if (!string.IsNullOrEmpty(blog.UrlImage))
            {
                await _cloudinaryService.DeleteImageAsync(blog.UrlImage);
            }

            // Xóa blog trong database
            var result = await _unitOfWork.Blog.RemoveAsync(blog);
            if (result)
            {
                return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);
            }

            return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }
}