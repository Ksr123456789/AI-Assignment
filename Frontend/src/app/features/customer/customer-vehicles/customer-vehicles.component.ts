import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { GenericTable, tableColumn, tableAction } from '../../../shared/generic-table/generic-table';
import {
  VehicleService,
  VehicleItem,
  VehicleAvailabilityStatus,
  VehicleFilterParams
} from '../../../core/services/vehicleService';
import { RentalCompanyService, RentalCompanyItem, CompanyStatus } from '../../../core/services/rentalCompanyService';
import {
  BookingService,
  BookVehicleRequest,
  BookingStatus,
  MyRentalItem
} from '../../../core/services/bookingService';
import { AuthService } from '../../../core/services/authService';

export interface ExtraServiceOption {
  id: number;
  name: string;
  pricePerDay: number;
  description: string;
}

@Component({
  selector: 'app-customer-vehicles',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, GenericTable],
  templateUrl: './customer-vehicles.component.html',
  styleUrl: './customer-vehicles.component.css'
})
export class CustomerVehiclesComponent implements OnInit {
  // State signals
  vehicles = signal<VehicleItem[]>([]);
  companies = signal<RentalCompanyItem[]>([]);
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  isBookingSubmitting = signal<boolean>(false);

  // Active bookings validation signals
  activeRentalsCount = signal<number>(0);
  myExistingBookings = signal<MyRentalItem[]>([]);
  modalValidationError = signal<string | null>(null);

  // Search & Filter signals (server-side & client-side range refinements)
  searchBy = signal<string>('');
  selectedCategory = signal<number>(0); // 0 = all
  selectedCompany = signal<string>(''); // '' = all
  selectedSeating = signal<number>(0); // 0 = all
  minDailyRate = signal<number | null>(null);
  maxDailyRate = signal<number | null>(null);
  sortBy = signal<string>('DailyRentalRate');
  sortDirection = signal<'asc' | 'desc'>('asc');

  // Pagination signals
  pageNumber = signal<number>(1);
  pageSize = signal<number>(5);
  totalCount = signal<number>(0);
  totalPages = signal<number>(0);

  // Booking Modal signals
  isBookModalOpen = signal<boolean>(false);
  selectedVehicle = signal<VehicleItem | null>(null);
  bookingForm!: FormGroup;

  // Additional Services list
  availableExtraServices: ExtraServiceOption[] = [
    { id: 1, name: 'GPS Navigation System', pricePerDay: 8, description: 'Pre-loaded maps & voice assistance' },
    { id: 2, name: 'Child Safety Seat', pricePerDay: 10, description: 'Suitable for toddlers & infants' },
    { id: 3, name: 'Additional Driver Registration', pricePerDay: 12, description: 'Full legal coverage for secondary driver' },
    { id: 4, name: 'Portable Wi-Fi Hotspot', pricePerDay: 9, description: 'Unlimited 5G connectivity on the go' }
  ];

  selectedServiceIds = signal<number[]>([]);

  // Computed signals
  hasVehicles = computed(() => this.vehicles().length > 0);
  startIndex = computed(() => (this.pageNumber() - 1) * this.pageSize() + 1);
  endIndex = computed(() => {
    const end = this.pageNumber() * this.pageSize();
    return end > this.totalCount() ? this.totalCount() : end;
  });

  // Estimated booking calculation signals
  // Rule 6: Auto-calculate: Total Days = Return Date - Pickup Date
  calcDays = signal<number>(1);
  // Rule 7: Auto-calculate: Total Amount = Daily Rate × Total Days (plus optional services)
  calcInsuranceTotal = computed(() => {
    const hasInsurance = this.bookingForm?.get('insurance')?.value;
    return hasInsurance ? 15 * this.calcDays() : 0;
  });
  calcServicesTotal = computed(() => {
    const days = this.calcDays();
    const selected = this.selectedServiceIds();
    return this.availableExtraServices
      .filter((s) => selected.includes(s.id))
      .reduce((sum, s) => sum + s.pricePerDay * days, 0);
  });
  calcRentalTotal = computed(() => {
    const v = this.selectedVehicle();
    if (!v) return 0;
    return (Number(v.dailyRentalRate) || 0) * this.calcDays();
  });
  calcGrandTotal = computed(() => {
    return this.calcRentalTotal() + this.calcInsuranceTotal() + this.calcServicesTotal();
  });

  categoryOptions = [
    { value: 0, label: 'All Categories' },
    { value: 1, label: 'Economy' },
    { value: 2, label: 'Sedan' },
    { value: 3, label: 'SUV' },
    { value: 4, label: 'Luxury' },
    { value: 5, label: 'Van / Minibus' }
  ];

  seatingOptions = [
    { value: 0, label: 'All Capacities' },
    { value: 2, label: '2 Seats' },
    { value: 4, label: '4 Seats' },
    { value: 5, label: '5 Seats' },
    { value: 7, label: '7 Seats' },
    { value: 8, label: '8+ Seats' }
  ];

  columns: tableColumn<VehicleItem>[] = [
    {
      type: 'image',
      label: 'Vehicle Image',
      format: () => ''
    },
    {
      key: 'makeAndModel',
      label: 'Make & Model',
      sortable: true,
      sortKey: 'MakeAndModel'
    },
    {
      key: 'vehicleCategoryName',
      label: 'Category',
      sortable: true,
      sortKey: 'VehicleCategoryName'
    },
    {
      key: 'dailyRentalRate',
      label: 'Daily Rate',
      sortable: true,
      sortKey: 'DailyRentalRate',
      format: (val) => `$${Number(val).toFixed(2)} / day`
    },
    {
      key: 'seatingCapacity',
      label: 'Seating',
      sortable: true,
      sortKey: 'SeatingCapacity',
      format: (val) => `${val} Seats`
    },
    {
      key: 'rentalCompanyName',
      label: 'Company Name',
      sortable: true,
      sortKey: 'RentalCompanyName'
    },
    {
      key: 'rentalCompanyStatus',
      label: 'Company Status',
      sortable: false,
      format: (val) => Number(val) === CompanyStatus.Inactive ? 'Inactive' : 'Active'
    },
    {
      type: 'actions',
      label: 'Action'
    }
  ];

  actions: tableAction<VehicleItem>[] = [
    {
      label: 'Reserve',
      action: 'reserve',
      disabled: (row) => row.rentalCompanyStatus === CompanyStatus.Inactive || Number(row.rentalCompanyStatus) === 2,
      class: 'bg-emerald-600 hover:bg-emerald-500 text-white font-semibold text-xs px-4 py-2 rounded-xl shadow-md shadow-emerald-600/20 disabled:opacity-40 disabled:cursor-not-allowed disabled:hover:bg-emerald-600'
    }
  ];

  constructor(
    private vehicleService: VehicleService,
    private rentalCompanyService: RentalCompanyService,
    private bookingService: BookingService,
    private authService: AuthService,
    private fb: FormBuilder,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initBookingForm();
    this.loadCompanies();
    this.loadVehicles();
    this.loadExistingCustomerBookings();
  }

  loadExistingCustomerBookings(): void {
    this.bookingService.getMyBookings({ pageNumber: 1, pageSize: 50 }).subscribe({
      next: (res) => {
        const items = res.items || [];
        this.myExistingBookings.set(items);
        const active = items.filter(
          (b) =>
            b.bookingStatus === BookingStatus.Pending ||
            b.bookingStatus === BookingStatus.Confirmed ||
            b.bookingStatus === BookingStatus.Active
        );
        this.activeRentalsCount.set(active.length);
      },
      error: () => {}
    });
  }

  loadCompanies(): void {
    this.rentalCompanyService.getAllRentalCompanies().subscribe({
      next: (res) => {
        this.companies.set(res.data || []);
      },
      error: () => {}
    });
  }

  initBookingForm(vehicle?: VehicleItem): void {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    tomorrow.setHours(10, 0, 0, 0);

    const threeDaysLater = new Date(tomorrow);
    threeDaysLater.setDate(threeDaysLater.getDate() + 3);

    // Auto-filled driver profile info from auth session
    const customerEmail = this.authService.getUserEmail() || 'customer@portal.com';
    let storedProfile: any = {};
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        const p = localStorage.getItem('customerProfile');
        if (p) storedProfile = JSON.parse(p);
      } catch {}
    }

    // Default license expiry date to next year if not in profile
    const nextYear = new Date();
    nextYear.setFullYear(nextYear.getFullYear() + 2);
    const defaultExpiryDate = storedProfile.licenseExpiryDate
      ? new Date(storedProfile.licenseExpiryDate).toISOString().split('T')[0]
      : nextYear.toISOString().split('T')[0];

    this.bookingForm = this.fb.group({
      vehicleInfo: [{ value: vehicle ? `${vehicle.makeAndModel} (${vehicle.licensePlate})` : '', disabled: true }],
      rentalCompany: [{ value: vehicle?.rentalCompanyName || '', disabled: true }],
      pickupDateTime: [this.formatDateForInput(tomorrow), [Validators.required]],
      returnDateTime: [this.formatDateForInput(threeDaysLater), [Validators.required]],
      pickupLocation: ['Main Airport Terminal Hub', [Validators.required, Validators.maxLength(150)]],
      returnLocation: ['Main Airport Terminal Hub', [Validators.required, Validators.maxLength(150)]],
      driverFullName: [storedProfile.fullName || 'Registered Customer', [Validators.required, Validators.maxLength(100)]],
      driverEmail: [storedProfile.email || customerEmail, [Validators.required, Validators.email]],
      driverPhone: [storedProfile.phoneNumber || '9876543210', [Validators.required]],
      driverLicense: [storedProfile.drivingLicenseNumber || 'DL987654321', [Validators.required]],
      licenseExpiryDate: [defaultExpiryDate, [Validators.required]],
      insurance: [true]
    });

    this.selectedServiceIds.set([]);
    this.modalValidationError.set(null);
    this.recalculateDays();

    this.bookingForm.valueChanges.subscribe(() => {
      this.recalculateDays();
      this.modalValidationError.set(null);
    });
  }

  // Auto-calculate: Total Days = Return Date - Pickup Date
  recalculateDays(): void {
    const pickupVal = this.bookingForm?.get('pickupDateTime')?.value;
    const returnVal = this.bookingForm?.get('returnDateTime')?.value;

    if (pickupVal && returnVal) {
      const p = new Date(pickupVal).getTime();
      const r = new Date(returnVal).getTime();
      const diffMs = r - p;
      if (diffMs > 0) {
        const days = Math.ceil(diffMs / (1000 * 60 * 60 * 24));
        this.calcDays.set(Math.max(1, days));
      } else {
        this.calcDays.set(1);
      }
    }
  }

  formatDateForInput(date: Date): string {
    const pad = (n: number) => (n < 10 ? '0' + n : n);
    const yyyy = date.getFullYear();
    const MM = pad(date.getMonth() + 1);
    const dd = pad(date.getDate());
    const hh = pad(date.getHours());
    const mm = pad(date.getMinutes());
    return `${yyyy}-${MM}-${dd}T${hh}:${mm}`;
  }

  get minPickupDateString(): string {
    return this.formatDateForInput(new Date());
  }

  loadVehicles(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    let effectiveSearch = this.searchBy().trim();
    if (this.selectedCompany()) {
      const comp = this.companies().find((c) => c.id === this.selectedCompany());
      if (comp && !effectiveSearch) {
        effectiveSearch = comp.companyName;
      }
    }

    const params: VehicleFilterParams = {
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      sortBy: this.sortBy(),
      sortDirection: this.sortDirection(),
      searchBy: effectiveSearch || undefined,
      vehicleCategoryId: this.selectedCategory() || undefined,
      vehicleAvailabilityStatus: VehicleAvailabilityStatus.Available
    };

    this.vehicleService.getPagedVehicles(params).subscribe({
      next: (result) => {
        this.isLoading.set(false);
        let items = result.items || [];

        if (this.selectedCompany()) {
          items = items.filter((v) => v.rentalCompanyId === this.selectedCompany());
        }
        if (this.selectedSeating() > 0) {
          if (this.selectedSeating() >= 8) {
            items = items.filter((v) => v.seatingCapacity >= 8);
          } else {
            items = items.filter((v) => v.seatingCapacity === this.selectedSeating());
          }
        }
        if (this.minDailyRate() !== null && this.minDailyRate()! > 0) {
          items = items.filter((v) => v.dailyRentalRate >= this.minDailyRate()!);
        }
        if (this.maxDailyRate() !== null && this.maxDailyRate()! > 0) {
          items = items.filter((v) => v.dailyRentalRate <= this.maxDailyRate()!);
        }

        this.vehicles.set(items);
        this.pageNumber.set(result.pageNumber);
        this.pageSize.set(result.pageSize);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message || 'Failed to retrieve available vehicles.');
      }
    });
  }

  onSearch(): void {
    this.pageNumber.set(1);
    this.loadVehicles();
  }

  onFilterChange(): void {
    this.pageNumber.set(1);
    this.loadVehicles();
  }

  resetFilters(): void {
    this.searchBy.set('');
    this.selectedCategory.set(0);
    this.selectedCompany.set('');
    this.selectedSeating.set(0);
    this.minDailyRate.set(null);
    this.maxDailyRate.set(null);
    this.pageNumber.set(1);
    this.loadVehicles();
  }

  onSortChange(sortKey: string): void {
    if (this.sortBy() === sortKey) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(sortKey);
      this.sortDirection.set('asc');
    }
    this.loadVehicles();
  }

  onTableAction(event: { action: string; row: VehicleItem }): void {
    if (event.action === 'reserve') {
      this.openBookModal(event.row);
    }
  }

  openBookModal(vehicle: VehicleItem): void {
    if (vehicle.rentalCompanyStatus === CompanyStatus.Inactive || Number(vehicle.rentalCompanyStatus) === 2) {
      this.toastr.error(
        `Cannot book "${vehicle.makeAndModel}" because its rental company (${vehicle.rentalCompanyName}) is currently inactive. Inactive companies cannot receive new bookings.`,
        'Booking Unavailable'
      );
      return;
    }
    this.selectedVehicle.set(vehicle);
    this.initBookingForm(vehicle);
    this.loadExistingCustomerBookings();
    this.isBookModalOpen.set(true);
  }

  closeBookModal(): void {
    this.isBookModalOpen.set(false);
    this.selectedVehicle.set(null);
    this.modalValidationError.set(null);
  }

  toggleService(serviceId: number): void {
    const current = this.selectedServiceIds();
    if (current.includes(serviceId)) {
      this.selectedServiceIds.set(current.filter((id) => id !== serviceId));
    } else {
      this.selectedServiceIds.set([...current, serviceId]);
    }
  }

  isServiceSelected(serviceId: number): boolean {
    return this.selectedServiceIds().includes(serviceId);
  }

  submitBooking(): void {
    this.modalValidationError.set(null);

    if (this.bookingForm.invalid) {
      this.bookingForm.markAllAsTouched();
      this.modalValidationError.set('Please fill out all required booking and driver fields.');
      return;
    }

    const vehicle = this.selectedVehicle();
    if (!vehicle) return;

    if (vehicle.rentalCompanyStatus === CompanyStatus.Inactive || Number(vehicle.rentalCompanyStatus) === 2) {
      this.modalValidationError.set(
        'Cannot book this vehicle because its rental company is currently inactive.'
      );
      return;
    }

    const values = this.bookingForm.value;
    const now = new Date();
    const pickupDate = new Date(values.pickupDateTime);
    const returnDate = new Date(values.returnDateTime);

    // Business Rule 1: Customer cannot have more than 5 active rentals (Pending + Confirmed + Active)
    if (this.activeRentalsCount() >= 5) {
      this.modalValidationError.set(
        'Limit Reached: Customer cannot have more than 5 active rentals (Pending, Confirmed, or Active). Please complete or cancel an existing rental.'
      );
      return;
    }

    // Business Rule 3: Pickup Date must be today or future
    const fiveMinutesAgo = new Date(now.getTime() - 5 * 60 * 1000);
    if (pickupDate < fiveMinutesAgo) {
      this.modalValidationError.set('Pickup Date & Time must be today or a future date and time.');
      return;
    }

    // Business Rule 4: Return Date must be after Pickup Date
    if (returnDate <= pickupDate) {
      this.modalValidationError.set('Return Date & Time must be after the Pickup Date & Time.');
      return;
    }

    // Business Rule 2: Customer cannot book same vehicle twice with overlapping dates while status is Pending
    const hasOverlappingPendingBooking = this.myExistingBookings().some((b) => {
      if (b.bookingStatus !== BookingStatus.Pending) return false;
      // Match vehicle model / plate
      const isSameVehicle =
        b.makeModal.toLowerCase() === vehicle.makeAndModel.toLowerCase() ||
        b.licensePlate.toLowerCase() === vehicle.licensePlate.toLowerCase();
      if (!isSameVehicle) return false;

      const existingPickup = new Date(b.pickupDateTime).getTime();
      const existingReturn = new Date(b.returnDateTime).getTime();
      const newPickup = pickupDate.getTime();
      const newReturn = returnDate.getTime();

      // Overlap condition: newPickup < existingReturn && newReturn > existingPickup
      return newPickup < existingReturn && newReturn > existingPickup;
    });

    if (hasOverlappingPendingBooking) {
      this.modalValidationError.set(
        'Overlapping Booking: You already have a Pending reservation for this vehicle with overlapping dates.'
      );
      return;
    }

    // Business Rule 5: Customer must have valid (non-expired) driving license
    if (values.licenseExpiryDate) {
      const expiryDate = new Date(values.licenseExpiryDate);
      if (expiryDate <= now) {
        this.modalValidationError.set(
          'Driving License Expired: Customer must possess a valid (non-expired) driving license.'
        );
        return;
      }
      if (expiryDate < returnDate) {
        this.modalValidationError.set(
          'License Expiry Conflict: Your driving license will expire before the scheduled return date.'
        );
        return;
      }
    }

    this.isBookingSubmitting.set(true);

    const payload: BookVehicleRequest = {
      vehicleId: vehicle.id,
      pickUpDateTime: pickupDate.toISOString(),
      returnDateTime: returnDate.toISOString(),
      pickupLocation: values.pickupLocation.trim(),
      returnLocation: values.returnLocation.trim(),
      insurance: !!values.insurance,
      extraServiceIds: this.selectedServiceIds(),
      driverFullName: values.driverFullName?.trim(),
      driverPhoneNumber: values.driverPhone?.trim(),
      driverLicenseNumber: values.driverLicense?.trim(),
      licenseExpiryDate: values.licenseExpiryDate ? new Date(values.licenseExpiryDate).toISOString() : undefined
    };

    this.bookingService.bookVehicle(payload).subscribe({
      next: (res) => {
        this.isBookingSubmitting.set(false);
        this.closeBookModal();
        this.toastr.success('Vehicle reserved successfully! Status is set to Pending.', 'Reservation Success');
        this.showSuccess('Vehicle reserved successfully! Status is set to Pending.');
        this.loadVehicles();
        this.loadExistingCustomerBookings();
      },
      error: (err) => {
        this.isBookingSubmitting.set(false);
        const msg =
          err.error?.message ||
          err.error?.detail ||
          (err.error?.errors && err.error?.errors[0]) ||
          'Failed to complete reservation.';
        this.toastr.error(msg, 'Reservation Failed');
        this.modalValidationError.set(msg);
      }
    });
  }

  goToMyRentals(): void {
    this.router.navigate(['/customer/rentals']);
  }

  showSuccess(msg: string): void {
    this.successMessage.set(msg);
    setTimeout(() => {
      this.successMessage.set(null);
    }, 4500);
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages() && page !== this.pageNumber()) {
      this.pageNumber.set(page);
      this.loadVehicles();
    }
  }
}
