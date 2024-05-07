using System.ComponentModel.DataAnnotations.Schema;

namespace FUParkingModel.Object
{
    [Table("User", Schema = "dbo")]
    public class User : Common
    {
        [Column("RoleId")]
        public Guid RoleId { get; set; }
        public Role? Role { get; set; }

        [Column("FullName")]
        public string? FullName { get; set; }

        [Column("Email")]
        public string? Email { get; set; }

        [Column("PasswordHash")]
        public string? PasswordHash { get; set; }

        [Column("StatusUser")]
        public string? StatusUser { get; set; }
    }
}
