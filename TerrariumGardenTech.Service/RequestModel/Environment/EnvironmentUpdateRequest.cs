namespace TerrariumGardenTech.Service.RequestModel.Environment;
public class EnvironmentUpdateRequest
{
    public int EnvironmentId { get; set; }
    public string EnvironmentName { get; set; } = string.Empty;
    public string EnvironmentDescription { get; set; } = string.Empty;
}