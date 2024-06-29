using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Column("DeletedDate")]        
        public DateTime? DeletedDate { get; set; }
    }
}
