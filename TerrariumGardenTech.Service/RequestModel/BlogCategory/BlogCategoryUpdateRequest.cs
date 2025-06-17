using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.BlogCategory
{
    public class BlogCategoryUpdateRequest
    {
        public int BlogCategoryId { get; set; }

        public string CategoryName { get; set; }

        public string Description { get; set; } 
    }
}
