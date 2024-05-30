﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUParkingModel.Object
{
    [Table("Vehicle", Schema = "dbo")]
    public class Vehicle : Common
    {
        [Column("PlateNumber")]
        public string? PlateNumber { get; set; }

        [Column("CustomerId")]
        [JsonIgnore]
        public Guid CustomerId { get; set; }
        [JsonIgnore]
        public Customer? Customer { get; set; }

        [Column("VehicleTypeId")]
        [JsonIgnore]
        public Guid VehicleTypeId { get; set; }
        public VehicleType? VehicleType { get; set; }

        [Column("PlateImage")]
        public string? PlateImage { get; set; }

        [Column("HeadImage")]
        public string? HeadImage { get; set; }

        [Column("BodyImage")]
        public string? BodyImage { get; set; }

        [Column("StatusVehicle")]
        public string? StatusVehicle { get; set; }
    }
}
