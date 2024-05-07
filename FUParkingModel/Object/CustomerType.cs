using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("CustomerType", Schema = "dbo")]
    public class CustomerType : Common
    {
        [Column("Name")]
        public string? Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }

        public ICollection<Customer>? Customers { get; set; }
    }
}
