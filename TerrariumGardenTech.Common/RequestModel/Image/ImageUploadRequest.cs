using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.RequestModel.Image
{
    public class ImageUploadRequest
    {
        /// <summary>Ảnh upload (multipart/form-data)</summary>
        public IFormFile File { get; set; } = null!;

        ///// <summary>Thư mục lưu trên Cloudinary, ví dụ "avatars" hoặc "products/terrarium"</summary>
        //public string? Folder { get; set; }

        ///// <summary>PublicId tùy chọn, ví dụ "avatar_123"</summary>
        //public string? PublicId { get; set; }
    }
}
