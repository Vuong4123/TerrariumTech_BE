using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.ResponseModel.BlogCategory
{
    public class BlogCategoryResponse
    {
        public int BlogCategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;    

        public string Description { get; set; } = string.Empty;
       
    }
}
