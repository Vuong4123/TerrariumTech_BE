using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Accessory;
using TerrariumGardenTech.Service.RequestModel.Category;

namespace TerrariumGardenTech.Service.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly UnitOfWork _unitOfWork;
        public CategoryService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }
        // Implement methods for ICategoryService here
        // For example, you can add methods like GetAllCategories, GetCategoryById, etc.
   
       

        public async Task<IBusinessResult> GetAll()
        {
            var categories = await _unitOfWork.Category.GetAllAsync();
            if (categories != null && categories.Any())
            {
                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, categories);
            }
            else
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
            }
        }

        public async Task<IBusinessResult> GetById(int id)
        {
            var category = await _unitOfWork.Category.GetByIdAsync(id);
            if (category != null)
            {
                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, category);
            }
            else
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
            }
        }

        public async Task<IBusinessResult> Save(Category category)
        {
            try
            {
                int result = -1;
                var categoryyEntity = _unitOfWork.Category.GetByIdAsync(category.CategoryId);
                if (categoryyEntity != null)
                {
                    result = await _unitOfWork.Category.UpdateAsync(category);
                    if (result > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, category);
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
                    }
                }
                else
                {
                    // Create new terrarium if it does not exist
                    result = await _unitOfWork.Category.CreateAsync(category);
                    if (result > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, category);
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
                    }
                }


            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
            }
        }

        public async Task<IBusinessResult> UpdateCategory(CategoryRequest categoryRequest)
        {
            try
            {
                //var categoryExists = await _unitOfWork.Category.AnyAsync(c => c.CategoryId == accessoryUpdateRequest.CategoryId);

                //if (!categoryExists)
                //{
                //    return new BusinessResult(Const.FAIL_CREATE_CODE, "CategoryId không tồn tại.");
                //}
                int result = -1;
                var cate = await _unitOfWork.Category.GetByIdAsync(categoryRequest.CategoryId);
                if (cate != null)
                {
                    _unitOfWork.Category.Context().Entry(cate ).CurrentValues.SetValues(categoryRequest);
                    result = await _unitOfWork.Category.UpdateAsync(cate);
                    if (result > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, cate);
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
                    }
                }
                else
                {
                    return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
            }
        }

        public async Task<IBusinessResult> CreateCategory(CategoryRequest categoryRequest)
        {
            
            var category = new Category
            {
                CategoryId = categoryRequest.CategoryId,
                CategoryName = categoryRequest.CategoryName,
                Description = categoryRequest.Description
            };
            var result = await _unitOfWork.Category.CreateAsync(category);
            if (result > 0)
            {
                return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, category);
            }
            else
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
            }
        }

        public Task<IBusinessResult> DeleteById(int id)
        {
            var category = _unitOfWork.Category.GetByIdAsync(id);
            if (category != null)
            {
                var result = _unitOfWork.Category.RemoveAsync(category.Result);
                if (result.Result)
                {
                    return Task.FromResult<IBusinessResult>(new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG));
                }
                else
                {
                    return Task.FromResult<IBusinessResult>(new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG));
                }
            }
            else
            {
                return Task.FromResult<IBusinessResult>(new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG));
            }
        }
    }
}
