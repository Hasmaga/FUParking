﻿namespace FUParkingModel.ResponseObject.SessionCheckOut
{
    public class CheckOutResDto
    {
        public string Message { get; set; } = null!;
        public int? Amount { get; set; }
        public string ImageIn { get; set; } = null!;
        public string ImageInBody { get; set; } = null!;
        public string PlateNumber { get; set; } = null!;
        public DateTime TimeIn { get; set; }
        public string TypeOfCustomer { get; set; } = null!;
    }
}
