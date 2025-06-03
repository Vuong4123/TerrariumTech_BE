using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.Blog
{
    public class BlogCreateRequest
    {
        public int BlogId { get; set; }

        public int UserId { get; set; }

        public int BlogCategoryId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string Status { get; set; }
    }
}
