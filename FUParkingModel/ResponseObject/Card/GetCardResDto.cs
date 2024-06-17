namespace FUParkingModel.ResponseObject.Card
{
    public class GetCardResDto
    {
        public Guid Id { get; set; }
        public string CardNumber { get; set; } = null!;
        public string? PlateNumber { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
