using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TerrariumGardenTech.Repositories.Entity
{
    public class UserMemberShip : BaseEntity
    {
        [Key]
        public int UserMemberShipId { get; set; }
        public int UserId { get; set; }
        public int MembershipId { get; set; }
        public DateTime StartAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpriedAt { get; set; }
        public decimal Amount { get; set; }
        [ForeignKey(nameof(UserId))] // Fixed missing closing parenthesis  
        public virtual User User { get; set; } = null!;
        [ForeignKey(nameof(MembershipId))]
        public virtual Membership Membership { get; set; } = null!;
    }
}
