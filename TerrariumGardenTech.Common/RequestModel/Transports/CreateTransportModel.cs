namespace TerrariumGardenTech.Common.RequestModel.Transports
{
    public class CreateTransportModel
    {
        public int OrderId { get; set; }
        public DateTime EstimateCompletedDate { get; set; }
        public string? Note { get; set; }
        public bool IsRefund { get; set; }
        public int? UserId { get; set; }
    }
}
