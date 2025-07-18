using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.Base
{
    public class CreateUploadResult
    {
        public bool Success { get; set; }  // Indicates if the upload was successful
        public string Message { get; set; }  // Contains any error message or success message
        public string ImageUrl { get; set; }  // Contains the URL of the uploaded image (if successful)
    }
}
