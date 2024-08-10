namespace FUParkingModel.ResponseObject.ParkingArea
{
    public class GetParkingAreaReqDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int MaxCapacity { get; set; }
        public int Block { get; set; }
        public string StatusParkingArea { get; set; } = null!;
        public string CreateBy { get; set; } = null!;
        public string CreateDate { get; set; } = null!;
        public string? LastModifyBy { get; set; }
        public string? LastModifyDate { get; set; }
    }
}
