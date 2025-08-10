using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.RequestModel.UploadImage
{
    public class FileUploadModel
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
