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

        public Task<IBusinessResult> GetById(int id)
        {
            var terrarium = _unitOfWork.Terrarium.GetByIdAsync(id);
            if (terrarium != null)
            {
                return Task.FromResult<IBusinessResult>(new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, terrarium));
            }
            else
            {
                return Task.FromResult<IBusinessResult>(new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG));
            }
        }

        
        public async Task<IBusinessResult> Save(Terrarium terrarium)
        {
            try
            {
                int result = -1;
                var terrariumEntity = _unitOfWork.Terrarium.GetById(terrarium.TerrariumId);
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
                int result = -1;
                var terra = _unitOfWork.Terrarium.GetById(terrariumUpdateRequest.TerrariumId);
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
        public Task<IBusinessResult> CreateTerrarium(TerrariumCreateRequest terrariumCreateRequest)
        {
            throw new NotImplementedException();
        }

        public Task<IBusinessResult> DeleteById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
