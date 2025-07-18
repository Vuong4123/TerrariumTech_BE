﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TerrariumGardenTech.Repositories.Enums;

namespace TerrariumGardenTech.Repositories.Entity;

public partial class Membership
{
    public int MembershipId { get; set; }
    public int UserId { get; set; }
    public decimal Price { get; set; } // Giá tại thời điểm người dùng mua

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string Status { get; set; }

    [NotMapped]
    public MembershipStatus StatusEnum
    {
        get => Enum.TryParse<MembershipStatus>(Status, out var result) ? result : MembershipStatus.Expired;
        set => Status = value.ToString();
    }

    public int PackageId { get; set; }  // Thêm PackageId liên kết với bảng MembershipPackage
    public virtual MembershipPackage Package { get; set; }  // Kết nối với MembershipPackage
    public virtual User User { get; set; }
}
