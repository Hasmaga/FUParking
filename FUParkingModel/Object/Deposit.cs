using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUParkingModel.Object
{
    [Table("Deposit", Schema = "dbo")]
    public class Deposit : Common
    {
        [Column("Name")]
        public required string Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        [Column("PackageId")]
        [JsonIgnore]
        public Guid PackageId { get; set; }
        [JsonIgnore]
        public Package? Package { get; set; }
        [JsonIgnore]
        public ICollection<Transaction>? Transactions { get; set; }
    }
}
