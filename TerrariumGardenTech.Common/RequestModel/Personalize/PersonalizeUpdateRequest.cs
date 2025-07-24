namespace TerrariumGardenTech.Common.RequestModel.Personalize;

public class PersonalizeUpdateRequest
{
    public int PersonalizeId { get; set; }

    // public int UserId { get; set; }
    public int EnvironmentId { get; set; }
    public int TankMethodId { get; set; }
    public int ShapeId { get; set; }
}