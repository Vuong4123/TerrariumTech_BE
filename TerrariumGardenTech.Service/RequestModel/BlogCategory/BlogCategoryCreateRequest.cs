﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.BlogCategory
{
    public class BlogCategoryCreateRequest
    {
        public string CategoryName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
