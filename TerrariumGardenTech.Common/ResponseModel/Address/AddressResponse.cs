namespace TerrariumGardenTech.Common.ResponseModel.Address;

public class AddressResponse
{
    public int Id { get; set; }

    public string TagName { get; set; } // max length 250

    public int UserId { get; set; }

    public string ReceiverName { get; set; } // max length 250

    public string ReceiverPhone { get; set; } // max length 250

    public string ReceiverAddress { get; set; }
}