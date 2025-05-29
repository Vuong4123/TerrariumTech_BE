using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Terrarium;

namespace TerrariumGardenTech.Service.Service
{
    public class TerrariumService : ITerrariumService
    {

        private readonly UnitOfWork _unitOfWork;
        public TerrariumService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IBusinessResult> GetAll()
        {
            var terrariumList = _unitOfWork.Terrarium.GetAllAsync();
            if (terrariumList != null)
            {
                return new BusinessResult(Const.SUCCESS_READ_CODE,Const.SUCCESS_READ_MSG, await terrariumList);
            }
            else
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);       
            }

        }

        public async Task<IBusinessResult> GetById(int id)
        {
            var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(id);
            if (terrarium != null)
            {
                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, terrarium);
            }
            else
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
            }
        }


        public async Task<IBusinessResult> Save(Terrarium terrarium)
        {
            try
            {
                int result = -1;
                var terrariumEntity = _unitOfWork.Terrarium.GetByIdAsync(terrarium.TerrariumId);
                if (terrariumEntity != null)
                {
                    result = await _unitOfWork.Terrarium.UpdateAsync(terrarium);
                    if (result > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, terrarium);
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
                    }
                }else{
                    // Create new terrarium if it does not exist
                    result = await _unitOfWork.Terrarium.CreateAsync(terrarium);
                    if (result > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, terrarium);
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
                    }
                }
                
                
            }catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
            }
    
            
        }

        public async Task<IBusinessResult> UpdateTerrarium(TerrariumUpdateRequest terrariumUpdateRequest)
        {
            try
            {
                var categoryExists = await _unitOfWork.TerrariumCategory.AnyAsync(c => c.CategoryId == terrariumUpdateRequest.CategoryId);

                if (!categoryExists)
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "CategoryId không tồn tại.");
                }
                int result = -1;
                var terra = await _unitOfWork.Terrarium.GetByIdAsync(terrariumUpdateRequest.TerrariumId);
                if (terra != null)
                {
                    _unitOfWork.Terrarium.Context().Entry(terra).CurrentValues.SetValues(terrariumUpdateRequest);
                    result = await _unitOfWork.Terrarium.UpdateAsync(terra);
                    if (result > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, terra);
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

        
        public async Task<IBusinessResult> CreateTerrarium(TerrariumCreateRequest terrariumCreateRequest)
        {
            try
            {
                var categoryExists = await _unitOfWork.TerrariumCategory.AnyAsync(c => c.CategoryId == terrariumCreateRequest.CategoryId);

                if (!categoryExists)
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "CategoryId không tồn tại.");
                }
                var newTerrarium = new Terrarium
                {
                    Name = terrariumCreateRequest.Name,
                    Description = terrariumCreateRequest.Description,
                    Price = terrariumCreateRequest.Price,
                    Stock = terrariumCreateRequest.Stock,
                    CategoryId = terrariumCreateRequest.CategoryId,
                    CreatedAt = terrariumCreateRequest.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = terrariumCreateRequest.UpdatedAt,
                    Status = terrariumCreateRequest.Status
                };

                var result = await _unitOfWork.Terrarium.CreateAsync(newTerrarium);
                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, newTerrarium);
                }
                else
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
            }
        }

        public async Task<IBusinessResult> DeleteById(int id)
        {
            try
            {
                var trrarium = await _unitOfWork.Terrarium.GetByIdAsync(id);
                if (trrarium != null)
                {
                    var result = await _unitOfWork.Terrarium.RemoveAsync(trrarium);
                    if (result)
                    {
                        return await Task.FromResult<IBusinessResult>(new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG));
                    }
                    else
                    {
                        return await Task.FromResult<IBusinessResult>(new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG));
                    }
                }
                else
                {
                    return await Task.FromResult<IBusinessResult>(new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG));
                }
            }
            catch (Exception ex)
            {
                return await Task.FromResult<IBusinessResult>(new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString()));
            }
        }
    }
}
