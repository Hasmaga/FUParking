﻿namespace FUParkingModel.ResponseObject.Transaction
{
    public class GetInfoWalletTransResDto
    {
        public int Amount { get; set; }
        public string TransactionDescription { get; set; } = null!;
        public string TransactionStatus { get; set; } = null!;
        public DateTime Date { get; set; }
    }
}
