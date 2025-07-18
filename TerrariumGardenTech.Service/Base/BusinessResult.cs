namespace TerrariumGardenTech.Service.Base;

public interface IBusinessResult
{
    int Status { get; set; }
    string? Message { get; set; }
    object? Data { get; set; }
}

public class BusinessResult : IBusinessResult
{
    public BusinessResult()
    {
        Status = -1;
        Message = "Action fail";
    }

    public BusinessResult(int status, string message)
    {
        Status = status;
        Message = message;
    }

    public BusinessResult(int status, string message, object data)
    {
        Status = status;
        Message = message;
        Data = data;
    }

    public int Status { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
}