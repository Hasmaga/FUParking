namespace FUParkingModel.ResponseObject.Feedback
{
    public class GetFeedbackByCustomerResDto
    {
        public Guid Id { get; set; }
        public string ParkingAreaName { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public Guid SessionId { get; set; }
    }
}
