﻿using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Blog;

namespace TerrariumGardenTech.Service.Service;

public class BlogService(UnitOfWork _unitOfWork, IUserContextService userContextService) : IBlogService
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
            var blogCategoryExists =
                await _unitOfWork.BlogCategory.AnyAsync(c => c.BlogCategoryId == blogUpdateRequest.BlogCategoryId);

            if (!blogCategoryExists) return new BusinessResult(Const.FAIL_CREATE_CODE, "BlogCategoryId không tồn tại.");

            var result = -1;
            var blog = await _unitOfWork.Blog.GetByIdAsync(blogUpdateRequest.BlogId);
            if (blog != null)
            {
                _unitOfWork.Blog.Context().Entry(blog).CurrentValues.SetValues(blogUpdateRequest);
                result = await _unitOfWork.Blog.UpdateAsync(blog);
                if (result > 0) return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, blog);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
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
            var GetCurrentUser = userContextService.GetCurrentUser();
            var blog = new Blog
            {
                UserId = GetCurrentUser,
                Title = blogCreateRequest.Title,
                Content = blogCreateRequest.Content,
                UrlImage = blogCreateRequest.UrlImage,
                BlogCategoryId = blogCreateRequest.BlogCategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                bodyHTML = blogCreateRequest.bodyHTML,
                Status = "Active" // Mặc định trạng thái là Active
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

    public Task<IBusinessResult> DeleteById(int id)
    {
        var blog = _unitOfWork.Blog.GetByIdAsync(id);
        if (blog != null)
        {
            var result = _unitOfWork.Blog.RemoveAsync(blog.Result);
            if (result.Result)
                return Task.FromResult<IBusinessResult>(new BusinessResult(Const.SUCCESS_DELETE_CODE,
                    Const.SUCCESS_DELETE_MSG));

            return Task.FromResult<IBusinessResult>(new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG));
        }

        return Task.FromResult<IBusinessResult>(new BusinessResult(Const.WARNING_NO_DATA_CODE,
            Const.WARNING_NO_DATA_MSG));
    }
}