namespace FUParkingModel.ResponseObject.ParkingArea
{
    public class GetParkingAreaReqDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int MaxCapacity { get; set; }
        public int Block { get; set; }
        public string Mode { get; set; } = null!;
        public string StatusParkingArea { get; set; } = null!;
        public string CreateBy { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public string? LastModifyBy { get; set; }
        public DateTime? LastModifyDate { get; set; }
    }
}
