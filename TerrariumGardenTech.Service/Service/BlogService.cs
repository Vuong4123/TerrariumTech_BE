using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Blog;
using TerrariumGardenTech.Common.ResponseModel.Blog;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class BlogService(
    UnitOfWork _unitOfWork,
    IUserContextService userContextService,
    ICloudinaryService _cloudinaryService) : IBlogService
{
    public async Task<IBusinessResult> GetAll()
    {
        // Lấy tất cả blog từ cơ sở dữ liệu
        var blogs = await _unitOfWork.Blog.GetAllAsync();

        // Kiểm tra nếu có dữ liệu
        if (blogs != null && blogs.Any())
        {
            // Ánh xạ Blog thành BlogResponse
            var blogResponses = blogs.Select(b => new BlogResponse
            {
                BlogCategoryId = b.BlogCategoryId,
                BlogId = b.BlogId,
                UserId = b.UserId,
                Title = b.Title,
                Content = b.Content,
                UrlImage = b.UrlImage ?? string.Empty,  // Đảm bảo UrlImage không null
                IsFeatured = b.IsFeatured,
                Status = b.Status
            }).ToList();

            // Trả về kết quả với mã thành công
            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", blogResponses);
        }

        // Trả về lỗi nếu không có dữ liệu
        return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No data found.");
    }

    public async Task<IBusinessResult> GetById(int id)
    {
        // Lấy blog theo ID từ cơ sở dữ liệu
        var blog = await _unitOfWork.Blog.GetByIdAsync(id);

        // Kiểm tra nếu có dữ liệu
        if (blog != null)
        {
            // Ánh xạ Blog thành BlogResponse
            var blogResponse = new BlogResponse
            {
                BlogId = blog.BlogId,
                BlogCategoryId = blog.BlogCategoryId,
                UserId = blog.UserId,
                Title = blog.Title,
                Content = blog.Content,
                bodyHTML = blog.bodyHTML ?? string.Empty, // Đảm bảo bodyHTML không null
                UrlImage = blog.UrlImage ?? string.Empty, // Đảm bảo UrlImage không null
                IsFeatured = blog.IsFeatured,
                Status = blog.Status
            };

            // Trả về kết quả với mã thành công
            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", blogResponse);
        }

        // Trả về lỗi nếu không có dữ liệu
        return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No data found.");
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
                    await _cloudinaryService.DeleteImageAsync(blog.UrlImage); // gọi hàm bạn đã viết

                // Upload ảnh mới
                var uploadResult = await _cloudinaryService.UploadImageAsync(
                    blogUpdateRequest.ImageFile,
                    folder: "blog_images"
                );

                if (uploadResult.Status == Const.SUCCESS_CREATE_CODE)
                    blog.UrlImage = uploadResult.Data?.ToString();
                else
                    return new BusinessResult(Const.FAIL_CREATE_CODE,
                        "Upload ảnh mới thất bại: " + uploadResult.Message);
            }

            // Cập nhật các thông tin còn lại
            blog.Title = blogUpdateRequest.Title;
            blog.Content = blogUpdateRequest.Content;
            blog.bodyHTML = blogUpdateRequest.bodyHTML;
            blog.BlogCategoryId = blogUpdateRequest.BlogCategoryId;
            blog.IsFeatured = blogUpdateRequest.IsFeatured;
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
                    "blog_images"
                );

                if (uploadResult.Status == Const.SUCCESS_CREATE_CODE)
                    uploadedImageUrl = uploadResult.Data.ToString();
                else
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "Upload ảnh thất bại: " + uploadResult.Message);
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
                IsFeatured = blogCreateRequest.IsFeatured,
                Status = "Active"
            };

            var result = await _unitOfWork.Blog.CreateAsync(blog);

            if (result > 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, blog);

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
        if (blog == null) return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

        try
        {
            // Nếu có ảnh → xóa ảnh khỏi Cloudinary
            if (!string.IsNullOrEmpty(blog.UrlImage)) await _cloudinaryService.DeleteImageAsync(blog.UrlImage);

            // Xóa blog trong database
            var result = await _unitOfWork.Blog.RemoveAsync(blog);
            if (result) return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);

            return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }
}