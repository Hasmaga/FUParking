namespace FUParkingModel.RequestObject
{
    public class UpdateGateReqDto
    {
        public string? Description { get; set; }

        public string? Name { get; set; }

        public Guid? ParkingAreaId { get; set; }
    }
}