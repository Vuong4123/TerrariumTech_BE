using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Repositories.Enums;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.Mappers;
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
                var terra = await _unitOfWork.Terrarium
                    .GetTerrariumIdAsync(terrariumUpdateRequest.TerrariumId); // Include quan hệ

                if (terra == null)
                    return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

                // Cập nhật thuộc tính cơ bản
                terra.TerrariumName = terrariumUpdateRequest.TerrariumName;
                terra.Description = terrariumUpdateRequest.Description;
                terra.Price = terrariumUpdateRequest.Price;
                terra.Stock = terrariumUpdateRequest.Stock;
                terra.Status = terrariumUpdateRequest.Status;
                terra.bodyHTML = terrariumUpdateRequest.bodyHTML ?? string.Empty;
                terra.UpdatedAt = DateTime.UtcNow;

                var ctx = _unitOfWork.Terrarium.Context();

                // ===== XÓA DỮ LIỆU QUAN HỆ CŨ =====
                ctx.TerrariumAccessory.RemoveRange(ctx.TerrariumAccessory.Where(x => x.TerrariumId == terra.TerrariumId));
                ctx.TerrariumTankMethods.RemoveRange(ctx.TerrariumTankMethods.Where(x => x.TerrariumId == terra.TerrariumId));
                ctx.TerrariumShapes.RemoveRange(ctx.TerrariumShapes.Where(x => x.TerrariumId == terra.TerrariumId));
                ctx.TerrariumEnvironments.RemoveRange(ctx.TerrariumEnvironments.Where(x => x.TerrariumId == terra.TerrariumId));
                await _unitOfWork.Terrarium.SaveChangesAsync(); // Lưu các thay đổi đã xóa
                // ===== THÊM DỮ LIỆU QUAN HỆ MỚI =====

                // 1. Accessories
                if (terrariumUpdateRequest.AccessoryNames?.Any() == true)
                {
                    var accessories = await _unitOfWork.Accessory
                        .FindAsync(a => terrariumUpdateRequest.AccessoryNames.Contains(a.Name));

                    terra.TerrariumAccessory = accessories.Select(a => new TerrariumAccessory
                    {
                        TerrariumId = terra.TerrariumId,
                        AccessoryId = a.AccessoryId
                    }).ToList();
                }

                // 2. TankMethods
                if (!string.IsNullOrWhiteSpace(terrariumUpdateRequest.TankMethodType))
                {
                    var tankMethodList = await _unitOfWork.TankMethod
                        .FindAsync(t => t.TankMethodType == terrariumUpdateRequest.TankMethodType);
                    var tankMethod = tankMethodList.FirstOrDefault(); // Lấy phần tử đầu tiên nếu có
                    if (tankMethod != null)
                    {
                        terra.TerrariumTankMethods = new List<TerrariumTankMethod>
                {
                    new TerrariumTankMethod
                    {
                        TerrariumId = terra.TerrariumId,
                        TankMethodId = tankMethod.TankMethodId
                    }
                };
                    }
                }

                // 3. Shapes
                if (!string.IsNullOrWhiteSpace(terrariumUpdateRequest.Shape))
                {
                    var shapeList = await _unitOfWork.Shape
                        .FindAsync(s => s.ShapeName == terrariumUpdateRequest.Shape);
                    var shape = shapeList.FirstOrDefault(); // Lấy phần tử đầu tiên nếu có
                    if (shape != null)
                    {
                        terra.TerrariumShapes = new List<TerrariumShape>
                {
                    new TerrariumShape
                    {
                        TerrariumId = terra.TerrariumId,
                        ShapeId = shape.ShapeId
                    }
                };
                    }
                }

                // 4. Environments
                if (!string.IsNullOrWhiteSpace(terrariumUpdateRequest.Environment))
                {
                    var envList = await _unitOfWork.Environment
                        .FindAsync(e => e.EnvironmentName == terrariumUpdateRequest.Environment);
                    var env = envList.FirstOrDefault(); // Lấy phần tử đầu tiên nếu có
                    if (env != null)
                    {
                        terra.TerrariumEnvironments = new List<TerrariumEnvironment>
                {
                    new TerrariumEnvironment
                    {
                        TerrariumId = terra.TerrariumId,
                        EnvironmentId = env.EnvironmentId
                    }
                };
                    }
                }

                // Cập nhật & lưu
                _unitOfWork.Terrarium.Update(terra);

                var resultDto = terra.ToTerrariumResponse(); // Sử dụng mapper

                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, resultDto);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }


        public async Task<IBusinessResult> CreateTerrarium(TerrariumCreateRequest terrariumCreateRequest)
        {
            try
            {
                var ShapeName = await _unitOfWork.Shape.GetByName(terrariumCreateRequest.Shape);
                var TankMethodName = await _unitOfWork.TankMethod.GetByName(terrariumCreateRequest.TankMethodType);
                var EnvironmentName = await _unitOfWork.Environment.GetByName(terrariumCreateRequest.Environment);
                var AccessoryNames = await _unitOfWork.Accessory.GetByName(terrariumCreateRequest.AccessoryNames);
                if (ShapeName == null || TankMethodName == null || EnvironmentName == null || AccessoryNames == null || AccessoryNames.Count == 0)
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

                    foreach (var accessory in AccessoryNames)
                    {
                        var terrariumAccessory = new TerrariumAccessory
                        {
                            AccessoryId = accessory.AccessoryId,
                            TerrariumId = newTerrarium.TerrariumId  // Gán TerrariumId cho TerrariumAccessory       
                        }; 
                        _unitOfWork.TerrariumAccessory.Context().Add(terrariumAccessory);
                        await _unitOfWork.TerrariumAccessory.SaveChangesAsync();
                    }
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
                var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(id);
                if (terrarium != null)
                {
                    // Xóa các bản ghi liên quan trong bảng TerrariumImage
                    var relatedImages = await _unitOfWork.TerrariumImage.GetAllByTerrariumIdAsync(id);
                    //.GetAllAsync(x => x.TerrariumId == id);

                    foreach (var image in relatedImages)
                    {
                        await _unitOfWork.TerrariumImage.RemoveAsync(image);  // Gọi RemoveAsync cho từng đối tượng.
                    }
                    // Xóa các bản ghi liên quan trong bảng TerrariumVariant
                    var relatedVariants = await _unitOfWork.TerrariumVariant.GetAllByTerrariumIdAsync(id); // Cần phương thức GetAllByTerrariumIdAsync trong Repository của TerrariumVariant
                    foreach (var variant in relatedVariants)
                    {
                        await _unitOfWork.TerrariumVariant.RemoveAsync(variant);  // Xóa các bản ghi liên quan trong TerrariumVariant
                    }
                    var result = await _unitOfWork.Terrarium.RemoveAsync(terrarium);
                    if (result)
                    {
                        return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG, result);
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
                    }
                }
                else
                {
                    return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public Task<IBusinessResult> GetTerrariumByNameAsync(string terrariumName)
        {
            throw new NotImplementedException();
        }
    }
}
