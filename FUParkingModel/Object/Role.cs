using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FUParkingModel.Object
{
    [Table("Role", Schema = "dbo")]
    public class Role : Common
    {
        [Column("Name")]
        public required string Name { get; set; }

        [Column("Description")]
        public string? Description { get; set; }
        [JsonIgnore]

        public ICollection<User>? Users { get; set; }
    }
}
