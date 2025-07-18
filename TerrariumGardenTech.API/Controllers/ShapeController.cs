using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Shape;
using TerrariumGardenTech.Service.ResponseModel.Shape;

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShapeController : ControllerBase
    {
        public IShapeService _shapeService;
        public ShapeController(IShapeService shapeService)
        {
            _shapeService = shapeService ?? throw new ArgumentNullException(nameof(shapeService));
        }
        // GET: api/<RoleController>
        [HttpGet("get-all")]
        public async Task<IBusinessResult> Get()
        {
            var result = await _shapeService.GetAllShapesAsync();
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }
            var shape = (result.Data as IEnumerable<Shape>)?.Select(r => new ShapeResponse
            {
                ShapeId = r.ShapeId,
                ShapeName = r.ShapeName,
                ShapeDescription = r.ShapeDescription,
                ShapeMaterial = r.ShapeMaterial
            }).ToList();
            
            if (shape == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
            }
            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", shape);
        }

        // GET api/<RoleController>/5
        [HttpGet("get-{id}")]
        public async Task<IBusinessResult> Get(int id)
        {
            var result = await _shapeService.GetShapeByIdAsync(id);
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }
            // Kiểm tra kiểu dữ liệu của result.Data (đảm bảo nó là Category, không phải IEnumerable)
            if (result.Data is Shape shape)
            {
                // Ánh xạ dữ liệu từ Category sang CategoryRequest
                var shapeResopnse = new ShapeResponse
                {
                    ShapeId = shape.ShapeId,
                    ShapeName = shape.ShapeName,
                    ShapeDescription = shape.ShapeDescription,
                    ShapeMaterial = shape.ShapeMaterial
                };

                // Trả về BusinessResult với dữ liệu đã ánh xạ
                return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", shape);
            }

            // Trả về lỗi nếu không thể ánh xạ
            return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
        }


        // POST api/<RoleController>
        [HttpPost("add-shape")]
        public async Task<IBusinessResult> Post([FromBody] ShapeCreateRequest shapeCreateRequest)
        {
            if (shapeCreateRequest == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Invalid request data.");
            }
            return await _shapeService.CreateShapeAsync(shapeCreateRequest);
        }

        // PUT api/<RoleController>/5
        [HttpPut("update-shape-{id}")]
        public async Task<IBusinessResult> Put([FromBody] ShapeUpdateRequest shapeUpdateRequest)
        {
            if (shapeUpdateRequest == null || !ModelState.IsValid)
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Invalid request data.");
            }
            return await _shapeService.UpdateShapeAsync(shapeUpdateRequest);
        }

        // DELETE api/<RoleController>/5
        [HttpDelete("delete-shape-{id}")]
        public async Task<IBusinessResult> Delete(int id)
        {
            var result = await _shapeService.DeleteShapeAsync(id);
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }
            if (result.Data is bool isDeleted)
            {
                return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Shape deleted successfully.");
            }
            return new BusinessResult(Const.FAIL_DELETE_CODE, "Failed to delete role.");
        }
    }
}
