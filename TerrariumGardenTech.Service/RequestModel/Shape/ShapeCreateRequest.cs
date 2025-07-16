namespace TerrariumGardenTech.Service.RequestModel.Shape;
public class ShapeCreateRequest
{
    public string ShapeName { get; set; } = string.Empty;
    public string ShapeDescription { get; set; } = string.Empty;
    public string ShapeSize { get; set; } = string.Empty;
    public string ShapeMaterial { get; set; } = string.Empty;
}