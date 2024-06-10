﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUParkingModel.Object
{
    [Table("Session", Schema = "dbo")]
    public class Session : Common
    {
        [Column("CardId")]
        [JsonIgnore]
        public Guid CardId { get; set; }
        public Card? Card { get; set; }

        [Column("GateInId")]
        public Guid GateInId { get; set; }
        public Gate? GateIn { get; set; }

        [Column("GateOutId")]
        public Guid? GateOutId { get; set; }
        public Gate? GateOut { get; set; }

        [Column("PlateNumber")]
        public string? PlateNumber { get; set; }

        [Column("ImageInUrl")]
        public string? ImageInUrl { get; set; }

        [Column("ImageOutUrl")]
        public string? ImageOutUrl { get; set; }

        [Column("TimeIn")]
        public DateTime TimeIn { get; set; }

        [Column("TimeOut")]
        public DateTime TimeOut { get; set; }

        [Column("Mode")]
        public string Mode { get; set; } = null!;

        [JsonIgnore]
        public ICollection<Payment>? Payments { get; set; }
    }
}
