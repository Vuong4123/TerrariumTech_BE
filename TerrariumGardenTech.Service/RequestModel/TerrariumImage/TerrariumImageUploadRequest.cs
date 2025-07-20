using Microsoft.AspNetCore.Http;

namespace TerrariumGardenTech.Service.RequestModel.TerrariumImage;

public class TerrariumImageUploadRequest
{
    public int TerrariumId { get; set; }
    public IFormFile ImageFile { get; set; }
}