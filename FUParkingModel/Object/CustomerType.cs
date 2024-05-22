using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUParkingModel.Object
{
    [Table("CustomerType", Schema = "dbo")]
    public class CustomerType : Common
    {
        [Column("Name")]
        public string? Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }
        [JsonIgnore]
        public ICollection<Customer>? Customers { get; set; }
    }
}
