using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FUParkingModel.Object
{
    public class Common
    {
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Column("CreatedDate")]
        [JsonIgnore]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Column("DeletedDate")]
        [JsonIgnore]
        public DateTime? DeletedDate { get; set; }
    }
}
