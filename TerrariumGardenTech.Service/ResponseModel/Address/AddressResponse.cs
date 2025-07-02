namespace TerrariumGardenTech.Service.ResponseModel.Address
{
    public class AddressResponse
    {
        public int AddressId { get; set; }

        public int UserId { get; set; }

        public string AddressLine1 { get; set; } = string.Empty;

        public string AddressLine2 { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string PostalCode { get; set; } = string.Empty;
    }
}
