using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static FUParkingModel.RequestObject.Vehicle.FileSizeAttribute;

namespace FUParkingModel.RequestObject.Vehicle
{
    public class CreateCustomerVehicleReqDto
    {
        private const int MaxFileSize = 50 * 1024 * 1024; // 50MB       

        [FromForm]
        [Required(ErrorMessage = "Must have Plate Number")]
        public string PlateNumber { get; set; } = null!;

        [FromForm]
        [Required(ErrorMessage = "Must have Vehicle Type")]
        public Guid VehicleTypeId { get; set; }

        [FromForm(Name = "PlateImage")]
        [FileSize(MaxFileSize)]
        [ValidateImageContentType]
        [Required(ErrorMessage = "Must have Plate Image")]
        public IFormFile PlateImage { get; set; } = null!;        
    }

    public class FileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;

        public FileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult($"File size should not exceed {_maxFileSize / (1024 * 1024)}MB.");
                }
            }

            return ValidationResult.Success;
        }

        public class ValidateImageContentTypeAttribute : ValidationAttribute
        {
            private readonly string[] _supportedImageMimeTypes =
            [
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/bmp",
                "image/tiff",
                "image/vnd.adobe.photoshop", // PSD
                "image/svg+xml", // SVG
                "image/webp",
                "image/heic",
                "image/heif" // HEIC variations
            ];

            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                if (value is IFormFile file)
                {
                    if (file.ContentType == null)
                    {
                        return new ValidationResult("File does not have a content type.");
                    }

                    if (!_supportedImageMimeTypes.Contains(file.ContentType.ToLower()))
                    {
                        return new ValidationResult(
                            "Only image files are allowed. Supported formats include JPEG, PNG, GIF, BMP, TIFF, PSD, SVG, WebP, HEIC, and HEIF."
                        );
                    }
                }

                return ValidationResult.Success;
            }
        }
    }
}
