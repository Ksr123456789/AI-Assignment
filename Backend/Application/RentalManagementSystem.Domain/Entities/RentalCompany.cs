using System.ComponentModel.DataAnnotations;
using RentalManagementSystem.Domain.Enums;

namespace RentalManagementSystem.Domain.Entities
{
    public class RentalCompany
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string CompanyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        public CompanyType CompanyType { get; set; }

        [Required]
        [MaxLength(100)]
        public string OwnerName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone must be exactly 10 digits.")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string HeadquartersLocation { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LicenseNumber { get; set; } = string.Empty;

        [Required]
        public CompanyStatus Status { get; set; } = CompanyStatus.Active;

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
