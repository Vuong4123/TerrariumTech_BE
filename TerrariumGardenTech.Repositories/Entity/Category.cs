﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace TerrariumGardenTech.Repositories.Entity;

public partial class Category
{
    public long CategoryId { get; set; }

    public string CategoryName { get; set; }

    public string Description { get; set; }

    public virtual ICollection<Accessory> Accessories { get; set; } = new List<Accessory>();
}