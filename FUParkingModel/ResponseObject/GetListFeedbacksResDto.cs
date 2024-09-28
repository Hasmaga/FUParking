namespace FUParkingModel.ResponseObject
{
    public class GetListFeedbacksResDto
    {
        public Guid Id { get; set; }
        public required string CustomerName { get; set; }
        public required string ParkingAreaName { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required DateTime CreatedDate { get; set; }
    }
}
