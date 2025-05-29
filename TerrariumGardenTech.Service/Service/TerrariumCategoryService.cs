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
using TerrariumGardenTech.Service.RequestModel.Category;
using TerrariumGardenTech.Service.RequestModel.TerrariumCategory;

namespace TerrariumGardenTech.Service.Service
{
    public class TerrariumCategoryService : ITerrariumCategoryService
    {
        private readonly UnitOfWork _unitOfWork;
        public TerrariumCategoryService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

       

        public Task<IBusinessResult> DeleteById(int id)
        {
           var terrariumCategory = _unitOfWork.TerrariumCategory.GetByIdAsync(id);
            if (terrariumCategory != null)
            {
                var result = _unitOfWork.TerrariumCategory.RemoveAsync(terrariumCategory.Result);
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

        public async Task<IBusinessResult> GetAll()
        {
            var terrariumCategories = await _unitOfWork.TerrariumCategory.GetAllAsync();
            if (terrariumCategories != null && terrariumCategories.Any())
            {
                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, terrariumCategories);
            }
            else
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
            }
        }

        public async Task<IBusinessResult> GetById(int id)
        {
            var terrariumCategory = await _unitOfWork.TerrariumCategory.GetByIdAsync(id);
            if (terrariumCategory != null)
            {
                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, terrariumCategory);
            }
            else
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
            }
        }

        public async Task<IBusinessResult> Save(TerrariumCategory terrariumCategory)
        {
            try
            {
                int result = -1;
                var teraCateEntity = _unitOfWork.TerrariumCategory.GetByIdAsync(terrariumCategory.CategoryId);
                if (teraCateEntity != null)
                {
                    result = await _unitOfWork.TerrariumCategory.UpdateAsync(terrariumCategory);
                    if (result > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, terrariumCategory);
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
                    }
                }
                else
                {
                    // Create new terrarium if it does not exist
                    result = await _unitOfWork.TerrariumCategory.CreateAsync(terrariumCategory);
                    if (result > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, terrariumCategory);
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

        public async Task<IBusinessResult> UpdateTerrariumCategory(TerrariumCategoryRequest terrariumCategoryRequest)
        {
            try
            {
                //var categoryExists = await _unitOfWork.Category.AnyAsync(c => c.CategoryId == accessoryUpdateRequest.CategoryId);

                //if (!categoryExists)
                //{
                //    return new BusinessResult(Const.FAIL_CREATE_CODE, "CategoryId không tồn tại.");
                //}
                int result = -1;
                var teraCate = await _unitOfWork.TerrariumCategory.GetByIdAsync(terrariumCategoryRequest.CategoryId);
                if (teraCate != null)
                {
                    _unitOfWork.TerrariumCategory.Context().Entry(teraCate).CurrentValues.SetValues(terrariumCategoryRequest);
                    result = await _unitOfWork.TerrariumCategory.UpdateAsync(teraCate);
                    if (result > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, teraCate);
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
        public async Task<IBusinessResult> CreateTerrariumCategory(TerrariumCategoryRequest terrariumCategoryRequest)
        {

            var terrariumCategory = new TerrariumCategory()
            {
                CategoryId = terrariumCategoryRequest.CategoryId,
                CategoryName = terrariumCategoryRequest.CategoryName,
                Description = terrariumCategoryRequest.Description
            };
            var result = await _unitOfWork.TerrariumCategory.CreateAsync(terrariumCategory);
            if (result > 0)
            {
                return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, terrariumCategory);
            }
            else
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
            }
        }
    }
}
