namespace FUParkingModel.ResponseObject.Session
{
    public class GetHistorySessionResDto
    {
        public Guid Id { get; set; }
        public DateTime TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public string Status { get; set; } = null!;
        public string PlateNumber { get; set; } = null!;
        public int? Amount { get; set; }
        public string GateIn { get; set; } = null!;
        public string? GateOut { get; set; }
        public string? PaymentMethod { get; set; }
        public string ParkingArea { get; set; } = null!;
        public bool IsFeedback { get; set; }
        public string? FeedbackTitle { get; set; }
        public string? FeedbackDescription { get; set; }
        public int? MoneyEstimated { get; set; }
    }
}
