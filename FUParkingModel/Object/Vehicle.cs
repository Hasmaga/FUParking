using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Vehicle", Schema = "dbo")]
    public class Vehicle : Common
    {
        [Column("PlateNumber")]
        public required string PlateNumber { get; set; }

        [Column("CustomerId")]
        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Column("VehicleTypeId")]
        public Guid VehicleTypeId { get; set; }
        public VehicleType? VehicleType { get; set; }

        [Column("PlateImage")]
        public required string PlateImage { get; set; }

        [Column("StatusVehicle")]
        public required string StatusVehicle { get; set; }

        [Column("StaffId")]
        public Guid? StaffId { get; set; }
        public User? Staff { get; set; }

        [Column("LastModifyById")]
        public Guid? LastModifyById { get; set; }
        public User? LastModifyBy { get; set; }

        [Column("LastModifyDate")]
        public DateTime? LastModifyDate { get; set; }
    }
}
