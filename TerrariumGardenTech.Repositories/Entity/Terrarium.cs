﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace TerrariumGardenTech.Repositories.Entity;

public partial class Terrarium
{
    public int TerrariumId { get; set; }

    public string TerrariumName { get; set; }

    public string Description { get; set; }

    public decimal? Price { get; set; }

    public int Stock { get; set; }

    public TerrariumStatusEnum Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string bodyHTML { get; set; }

    


    public virtual ICollection<TerrariumAccessory> TerrariumAccessory { get; set; } = [];
    public virtual ICollection<TerrariumImage> TerrariumImages { get; set; } = [];
    public virtual ICollection<TerrariumVariant> TerrariumVariants { get; set; } = [];
    public virtual ICollection<TerrariumTankMethod> TerrariumTankMethods { get; set; } = [];
    public virtual ICollection<TerrariumEnvironment> TerrariumEnvironments { get; set; } = [];
    public virtual ICollection<TerrariumShape> TerrariumShapes { get; set; } = [];
}