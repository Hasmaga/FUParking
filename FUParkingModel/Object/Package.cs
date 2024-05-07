﻿using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Package", Schema = "dbo")]
    public class Package : Common
    {
        [Column("Name")]
        public string? Name { get; set; }

        [Column("CoinAmount")]
        public int CoinAmount { get; set; }

        [Column("Price")]
        public decimal Price { get; set; }

        [Column("PackageStatus")]
        public string? PackageStatus { get; set; }

        public ICollection<Deposit>? Deposits { get; set; }
    }
}
