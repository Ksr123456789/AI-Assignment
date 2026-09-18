import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/authService';

export const passwordMatchValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('password');
  const confirmPassword = control.get('confirmPassword');

  if (!password || !confirmPassword) return null;

  if (confirmPassword.errors && !confirmPassword.errors['passwordMismatch']) {
    return null;
  }

  if (password.value !== confirmPassword.value) {
    confirmPassword.setErrors({ passwordMismatch: true });
    return { passwordMismatch: true };
  } else {
    if (confirmPassword.hasError('passwordMismatch')) {
      confirmPassword.setErrors(null);
    }
    return null;
  }
};

export const futureDateValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  if (!control.value) return null;
  const selectedDate = new Date(control.value);
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  if (selectedDate <= today) {
    return { pastDate: true };
  }
  return null;
};

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  isLoading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  showPassword = false;
  showConfirmPassword = false;
  minDate: string = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    this.minDate = tomorrow.toISOString().split('T')[0];
    this.initForm();
  }

  initForm(): void {
    this.registerForm = this.fb.group(
      {
        fullName: [
          '',
          [
            Validators.required,
            Validators.maxLength(100),
            Validators.pattern(/^[a-zA-Z\s]+$/)
          ]
        ],
        email: ['', [Validators.required, Validators.email]],
        phoneNumber: [
          '',
          [
            Validators.required,
            Validators.pattern(/^[0-9]{10}$/)
          ]
        ],
        drivingLicenseNumber: [
          '',
          [
            Validators.required,
            Validators.pattern(/^[a-zA-Z0-9]+$/)
          ]
        ],
        licenseExpiryDate: ['', [Validators.required, futureDateValidator]],
        address: [
          '',
          [
            Validators.required,
            Validators.maxLength(300)
          ]
        ],
        password: [
          '',
          [
            Validators.required,
            Validators.minLength(8),
            Validators.maxLength(24),
            Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,24}$/)
          ]
        ],
        confirmPassword: ['', [Validators.required]]
      },
      { validators: passwordMatchValidator }
    );
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;
    this.successMessage = null;

    const formValues = this.registerForm.value;
    const payload = {
      fullName: formValues.fullName.trim(),
      email: formValues.email.trim(),
      phoneNumber: formValues.phoneNumber.trim(),
      drivingLicenseNumber: formValues.drivingLicenseNumber.trim(),
      licenseExpiryDate: new Date(formValues.licenseExpiryDate).toISOString(),
      address: formValues.address.trim(),
      password: formValues.password,
      confirmPassword: formValues.confirmPassword
    };

    this.authService.register(payload).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.success) {
          this.successMessage = 'Registration successful! Redirecting to login...';
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 1500);
        } else {
          this.errorMessage = response.message || 'Registration failed.';
        }
      },
      error: (error) => {
        this.isLoading = false;
        if (error.error?.message) {
          this.errorMessage = error.error.message;
        } else if (error.error?.errors && Array.isArray(error.error.errors) && error.error.errors.length > 0) {
          this.errorMessage = error.error.errors.join(', ');
        } else {
          this.errorMessage = 'Unable to complete registration. Please check your details and try again.';
        }
      }
    });
  }

  // Helper getters
  get fullName() { return this.registerForm.get('fullName'); }
  get email() { return this.registerForm.get('email'); }
  get phoneNumber() { return this.registerForm.get('phoneNumber'); }
  get drivingLicenseNumber() { return this.registerForm.get('drivingLicenseNumber'); }
  get licenseExpiryDate() { return this.registerForm.get('licenseExpiryDate'); }
  get address() { return this.registerForm.get('address'); }
  get password() { return this.registerForm.get('password'); }
  get confirmPassword() { return this.registerForm.get('confirmPassword'); }
}
