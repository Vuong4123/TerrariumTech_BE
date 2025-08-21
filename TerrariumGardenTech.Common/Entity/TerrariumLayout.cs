using System.ComponentModel.DataAnnotations.Schema;
using TerrariumGardenTech.Repositories.Entity;
using static TerrariumGardenTech.Common.Enums.CommonData;

namespace TerrariumGardenTech.Common.Entity;

public class TerrariumLayout
{
    public int LayoutId { get; set; }
    public int UserId { get; set; } // User tạo layout
    public string LayoutName { get; set; }
    public int TerrariumId { get; set; }

    public string Status { get; set; } = LayoutStatus.Pending;
    public decimal? FinalPrice { get; set; }
    public int? ReviewedBy { get; set; } // Manager review (nullable)
    public DateTime? ReviewDate { get; set; }
    public string? ReviewNotes { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    // Navigation Properties với tên rõ ràng
    [ForeignKey("UserId")]
    public virtual User User { get; set; } // User tạo

    [ForeignKey("ReviewedBy")]
    public virtual User? Reviewer { get; set; } // Manager review

    [ForeignKey("TerrariumId")]
    public virtual Terrarium Terrarium { get; set; }
}
