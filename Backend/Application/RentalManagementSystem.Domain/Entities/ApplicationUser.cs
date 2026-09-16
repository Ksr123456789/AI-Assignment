using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace RentalManagementSystem.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [MaxLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full Name must contain alphabets and spaces only.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required.")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone Number must be exactly 10 digits.")]
        public override string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Driving License Number is required.")]
        [MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Driving License Number must be alphanumeric.")]
        public string DrivingLicenseNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "License Expiry Date is required.")]
        [DataType(DataType.Date)]
        public DateTime LicenseExpiryDate { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
        public string Address { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
