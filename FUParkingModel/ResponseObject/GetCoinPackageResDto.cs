namespace FUParkingModel.ResponseObject
{
    public class CustomerGetCoinPackageResDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string CoinAmount { get; set; }
        public required decimal Price { get; set; }
        public int? ExtraCoin { get; set; }
        public int? EXPPackage { get; set; }
    }

    public class SupervisorGetCoinPackageResDto : CustomerGetCoinPackageResDto
    {
        public required string PackageStatus { get; set; }
        public string? CreateDate { get; set; }
        public string? DeletedDate { get; set; }
    }
}
