using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("Deposit", Schema = "dbo")]
    public class Deposit : Common
    {
        [Column("Name")]
        public string? Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        [Column("PackageId")]
        public Guid PackageId { get; set; }
        public Package? Package { get; set; }       

        public ICollection<Transaction>? Transactions { get; set; }
    }
}
