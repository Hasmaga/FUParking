﻿namespace FUParkingModel.ResponseObject.Gate
{
    public class GetGateResDto
    {
        public Guid Id { get; set; }
        public string ParkingAreaName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string GateTypeName { get; set; } = null!;
        public string StatusGate { get; set; } = null!;
        public string CreatedBy { get; set; } = null!;
        public string LastModifyBy { get; set; } = null!;
    }
}