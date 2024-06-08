using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FUParkingModel.RequestObject
{
    public class UpdateCoinPackageReqDto
    {
        [JsonIgnore]
        public Guid PackageId { get; set; }

        [Required(ErrorMessage = "Package name is required")]
        public required string Name { get; set; }

        public bool IsActive { get; set; }
    }
}
