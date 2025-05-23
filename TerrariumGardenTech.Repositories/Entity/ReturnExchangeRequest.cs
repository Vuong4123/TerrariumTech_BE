﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace TerrariumGardenTech.Repositories.Entity;

public partial class ReturnExchangeRequest
{
    public long RequestId { get; set; }

    public long? OrderId { get; set; }

    public string RequestType { get; set; }

    public string Reason { get; set; }

    public string Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; }

    public virtual ICollection<ReturnExchangeRequestItem> ReturnExchangeRequestItems { get; set; } = new List<ReturnExchangeRequestItem>();
}