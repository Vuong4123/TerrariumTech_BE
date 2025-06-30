namespace TerrariumGardenTech.Service.RequestModel.Shape;
public class ShapeCreateRequest
{
    public string ShapeName { get; set; } = string.Empty;
    public string ShapeDescription { get; set; } = string.Empty;
    public string ShapeSize { get; set; } = string.Empty;
    public int ShapeHeight { get; set; }
    public int ShapeWidth { get; set; }
    public int ShapeLength { get; set; }
    public float ShapeVolume { get; set; }
    public string ShapeMaterial { get; set; } = string.Empty;
}