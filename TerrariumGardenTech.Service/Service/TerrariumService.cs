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
            var terrariumList = _unitOfWork.Terrarium.GetAllTerrariumAsync();
            if (terrariumList != null)
            {
                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, await terrariumList);
            }
            else
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
            }

        }

        // public async Task<BusinessResult> GetAllOfParam(string type, string shape, string tankMethod, string theme, string size)
        // {
        //     // Đảm bảo bạn đang sử dụng từ khóa `await` để lấy dữ liệu từ Task
        //     var result = await _unitOfWork.Terrarium.GetAllAsync();

        //     // Chuyển đổi kết quả thành IQueryable để có thể truy vấn tiếp
        //     var query = result.AsQueryable();

        //     // Áp dụng các bộ lọc nếu các tham số có giá trị
        //     if (!string.IsNullOrEmpty(type))
        //     {
        //         query = query.Where(t => t.Type.Contains(type));
        //     }

        //     if (!string.IsNullOrEmpty(shape))
        //     {
        //         query = query.Where(t => t.Shape.Contains(shape));
        //     }

        //     if (!string.IsNullOrEmpty(tankMethod))
        //     {
        //         query = query.Where(t => t.TankMethod.Contains(tankMethod));
        //     }

        //     if (!string.IsNullOrEmpty(theme))
        //     {
        //         query = query.Where(t => t.Theme.Contains(theme));
        //     }

        //     if (!string.IsNullOrEmpty(size))
        //     {
        //         query = query.Where(t => t.Size.Contains(size));
        //     }

        //     // Lấy kết quả từ cơ sở dữ liệu và trả về kết quả dưới dạng danh sách
        //     var result2 = await query.ToListAsync();

        //     // Trả về kết quả
        //     return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", result2);
        // }

        public async Task<IBusinessResult> GetById(int id)
        {
            var terrarium = await _unitOfWork.Terrarium.GetTerrariumIdAsync(id);
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
                }
                else
                {
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


            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
            }


        }

        public async Task<IBusinessResult> UpdateTerrarium(TerrariumUpdateRequest terrariumUpdateRequest)
        {
            try
            {

                var terra = await _unitOfWork.Terrarium.GetByIdAsync(terrariumUpdateRequest.TerrariumId);
                if (terra != null)
                {
                    _unitOfWork.Terrarium.Context().Entry(terra).CurrentValues.SetValues(terrariumUpdateRequest);
                    var result = await _unitOfWork.Terrarium.UpdateAsync(terra);
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
                var ShapeName = await _unitOfWork.Shape.GetByName(terrariumCreateRequest.Shape);
                var TankMethodName = await _unitOfWork.TankMethod.GetByName(terrariumCreateRequest.TankMethodType);
                var EnvironmentName = await _unitOfWork.Environment.GetByName(terrariumCreateRequest.Environment);
                if (ShapeName == null || TankMethodName == null || EnvironmentName == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }

                var newTerrarium = new Terrarium
                {
                    TerrariumName = terrariumCreateRequest.TerrariumName,
                    bodyHTML = terrariumCreateRequest.bodyHTML,
                    Description = terrariumCreateRequest.Description,
                    Price = terrariumCreateRequest.Price,
                    Stock = terrariumCreateRequest.Stock,
                    Status = terrariumCreateRequest.Status,
                    CreatedAt = DateTime.Now,
                };

                var result = await _unitOfWork.Terrarium.CreateAsync(newTerrarium);

                if (result > 0)
                {
                    var TerrariumShape = new TerrariumShape
                    {
                        ShapeId = ShapeName.ShapeId,
                        TerrariumId = newTerrarium.TerrariumId
                    };
                    _unitOfWork.TerrariumShape.Context().Add(TerrariumShape);
                    await _unitOfWork.TerrariumShape.SaveChangesAsync();
                    var TerrariumTankMethod = new TerrariumTankMethod
                    {
                        TankMethodId = TankMethodName.TankMethodId,
                        TerrariumId = newTerrarium.TerrariumId
                    };
                    _unitOfWork.TerrariumTankMethod.Context().Add(TerrariumTankMethod);
                    await _unitOfWork.TerrariumTankMethod.SaveChangesAsync();

                    var TerrariumEnvironment = new TerrariumEnvironment
                    {
                        EnvironmentId = EnvironmentName.EnvironmentId,
                        TerrariumId = newTerrarium.TerrariumId
                    };
                    _unitOfWork.TerrariumEnvironment.Context().Add(TerrariumEnvironment);
                    await _unitOfWork.TerrariumEnvironment.SaveChangesAsync();

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

        public Task<IBusinessResult> GetTerrariumByNameAsync(string terrariumName)
        {
            throw new NotImplementedException();
        }
    }
}
