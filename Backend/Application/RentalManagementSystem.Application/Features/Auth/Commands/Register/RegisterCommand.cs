using System.ComponentModel.DataAnnotations;
using MediatR;

namespace RentalManagementSystem.Application.Features.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<RegisterCommandResponse>
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [MaxLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full Name must contain alphabets and spaces only.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required.")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone Number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Driving License Number is required.")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Driving License Number must be alphanumeric.")]
        public string DrivingLicenseNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "License Expiry Date is required.")]
        public DateTime LicenseExpiryDate { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(24, MinimumLength = 8, ErrorMessage = "Password must be 8-24 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,24}$", ErrorMessage = "Password must be 8-24 characters with uppercase, lowercase, digit and special character.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
