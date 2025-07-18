using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService
{
    public interface ICloudinaryService
    {
        Task<IBusinessResult> UploadImageAsync(IFormFile file, string folder, string publicId = null);
        Task<IBusinessResult> DeleteImageAsync(string imageUrl);
    }
}
