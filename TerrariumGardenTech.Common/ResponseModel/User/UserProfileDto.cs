using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.ResponseModel.User
{
    public class UserProfileDto
    {
        public int UserId { get; init; }
        public string FullName { get; init; }
        public string Gender { get; init; }
        public string PhoneNumber { get; init; }
        public DateTime? DateOfBirth { get; init; }
        public string? AvatarUrl { get; init; }
        public string? BackgroundUrl { get; init; }
        public string Email { get; init; }
    }
}
