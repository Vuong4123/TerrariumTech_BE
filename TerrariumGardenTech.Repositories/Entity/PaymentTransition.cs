﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace TerrariumGardenTech.Repositories.Entity;

public partial class PaymentTransition
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public string PaymentMethod { get; set; }

    public decimal? PaymentAmount { get; set; }


    public DateTime? PaymentDate { get; set; }

    public string Status { get; set; }

    public virtual Order Order { get; set; }
}