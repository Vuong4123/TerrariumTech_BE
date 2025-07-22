namespace TerrariumGardenTech.Common.RequestModel.Shape;

public class ShapeUpdateRequest
{
    public int ShapeId { get; set; }
    public string ShapeName { get; set; } = string.Empty;
    public string ShapeDescription { get; set; } = string.Empty;
    public string ShapeMaterial { get; set; } = string.Empty;
}