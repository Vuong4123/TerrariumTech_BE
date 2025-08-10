namespace TerrariumGardenTech.Common.ResponseModel.User
{
    public sealed class UserProfileResponse
    {
        //public int UserId { get; init; }
        public string FullName { get; init; }
        public string Gender { get; init; }
        public string PhoneNumber { get; init; }
        public DateTime? DateOfBirth { get; init; }
        public string? AvatarUrl { get; init; }
        public string? BackgroundUrl { get; init; }
        public string Email { get; init; }
    }
}
