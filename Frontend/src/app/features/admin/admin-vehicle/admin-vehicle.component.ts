import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { GenericTable, tableColumn, tableAction } from '../../../shared/generic-table/generic-table';
import {
  VehicleService,
  VehicleItem,
  VehicleAvailabilityStatus,
  VehicleFilterParams
} from '../../../core/services/vehicleService';
import { RentalCompanyService, RentalCompanyItem } from '../../../core/services/rentalCompanyService';

@Component({
  selector: 'app-admin-vehicle',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, GenericTable],
  templateUrl: './admin-vehicle.component.html',
  styleUrl: './admin-vehicle.component.css'
})
export class AdminVehicleComponent implements OnInit {
  // State signals
  vehicles = signal<VehicleItem[]>([]);
  companies = signal<RentalCompanyItem[]>([]);
  isLoading = signal<boolean>(false);
  modalErrorMessage = signal<string | null>(null);

  // Search & Filter signals (matching GetPagedVehicleQuery)
  searchBy = signal<string>('');
  selectedCategory = signal<number>(0); // 0 = all
  selectedAvailability = signal<number>(0); // 0 = all
  sortBy = signal<string>('');
  sortDirection = signal<'asc' | 'desc'>('asc');

  // Pagination signals
  pageNumber = signal<number>(1);
  pageSize = signal<number>(5);
  totalCount = signal<number>(0);
  totalPages = signal<number>(0);

  // Modal signals
  isModalOpen = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  currentVehicleId = signal<number | null>(null);
  vehicleForm!: FormGroup;

  // Delete modal signals
  isDeleteModalOpen = signal<boolean>(false);
  vehicleToDelete = signal<VehicleItem | null>(null);

  // Computed signals
  hasVehicles = computed(() => this.vehicles().length > 0);
  startIndex = computed(() => (this.pageNumber() - 1) * this.pageSize() + 1);
  endIndex = computed(() => {
    const end = this.pageNumber() * this.pageSize();
    return end > this.totalCount() ? this.totalCount() : end;
  });

  categoryOptions = [
    { value: 0, label: 'All Categories' },
    { value: 1, label: 'Economy' },
    { value: 2, label: 'Sedan' },
    { value: 3, label: 'SUV' },
    { value: 4, label: 'Luxury' },
    { value: 5, label: 'Van/Bus' }
  ];

  availabilityOptions = [
    { value: 0, label: 'All Statuses' },
    { value: VehicleAvailabilityStatus.Available, label: 'Available' },
    { value: VehicleAvailabilityStatus.Unavailable, label: 'Unavailable' },
    { value: VehicleAvailabilityStatus.InMaintenance, label: 'In Maintenance' }
  ];

  columns: tableColumn<VehicleItem>[] = [
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
      key: 'rentalCompanyName',
      label: 'Company',
      sortable: true,
      sortKey: 'RentalCompanyName'
    },
    {
      key: 'licensePlate',
      label: 'License Plate',
      sortable: true,
      sortKey: 'LicensePlate'
    },
    {
      key: 'yearOfManufacture',
      label: 'Year',
      sortable: true,
      sortKey: 'YearOfManufacture'
    },
    {
      key: 'seatingCapacity',
      label: 'Seats',
      sortable: true,
      sortKey: 'SeatingCapacity'
    },
    {
      key: 'dailyRentalRate',
      label: 'Daily Rate',
      sortable: true,
      sortKey: 'DailyRentalRate',
      format: (val) => `$${Number(val).toFixed(2)}`
    },
    {
      key: 'availabilityStatus',
      label: 'Status',
      sortable: true,
      sortKey: 'AvailabilityStatus',
      format: (val) => {
        const map: Record<number, string> = {
          [VehicleAvailabilityStatus.Available]: 'Available',
          [VehicleAvailabilityStatus.Unavailable]: 'Unavailable',
          [VehicleAvailabilityStatus.InMaintenance]: 'In Maintenance'
        };
        return map[Number(val)] ?? String(val ?? '');
      }
    },
    {
      key: 'ongoingBookingCount',
      label: 'Bookings',
      sortable: false,
      format: (val) => Number(val) > 0 ? `${val} ongoing` : 'None'
    },
    {
      type: 'actions',
      label: 'Actions'
    }
  ];

  actions: tableAction<VehicleItem>[] = [
    {
      label: 'Edit',
      action: 'edit',
      class: 'bg-sky-500/15 text-sky-300 hover:bg-sky-500 hover:text-white border border-sky-500/30 hover:border-sky-400 font-semibold shadow-sm hover:shadow-md hover:shadow-sky-500/25 transition-all duration-200'
    },
    {
      label: 'Delete',
      action: 'delete',
      class: 'bg-rose-500/15 text-rose-300 hover:bg-rose-600 hover:text-white border border-rose-500/30 hover:border-rose-500 font-semibold shadow-sm hover:shadow-md hover:shadow-rose-600/30 transition-all duration-200'
    }
  ];

  constructor(
    private vehicleService: VehicleService,
    private rentalCompanyService: RentalCompanyService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadCompanies();
    this.loadData();
  }

  initForm(): void {
    this.vehicleForm = this.fb.group({
      rentalCompanyId: ['', [Validators.required]],
      vehicleCategoryId: [1, [Validators.required]],
      makeAndModel: ['', [Validators.required, Validators.maxLength(100)]],
      licensePlate: ['', [Validators.required, Validators.maxLength(20)]],
      registrationNumber: ['', [Validators.required, Validators.maxLength(50)]],
      yearOfManufacture: [
        new Date().getFullYear(),
        [Validators.required, Validators.min(1900), Validators.max(2100)]
      ],
      seatingCapacity: [4, [Validators.required, Validators.min(1), Validators.max(20)]],
      dailyRentalRate: [50, [Validators.required, Validators.min(0.01)]],
      availabilityStatus: [VehicleAvailabilityStatus.Available, [Validators.required]],
      mileage: [0, [Validators.required, Validators.min(0)]]
    });

    this.vehicleForm.valueChanges.subscribe(() => {
      if (this.modalErrorMessage()) {
        this.modalErrorMessage.set(null);
      }
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

  loadData(): void {
    this.isLoading.set(true);

    const params: VehicleFilterParams = {
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      sortBy: this.sortBy(),
      sortDirection: this.sortDirection(),
      searchBy: this.searchBy(),
      vehicleCategoryId: this.selectedCategory() || undefined,
      vehicleAvailabilityStatus: this.selectedAvailability() || undefined
    };

    this.vehicleService.getPagedVehicles(params).subscribe({
      next: (result) => {
        this.isLoading.set(false);
        this.vehicles.set(result.items || []);
        this.pageNumber.set(result.pageNumber);
        this.pageSize.set(result.pageSize);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = err.error?.message || 'Failed to load vehicles.';
        this.toastr.error(msg, 'Error');
      }
    });
  }

  private searchDebounceTimer?: any;

  onSearchInputChange(value: string): void {
    this.searchBy.set(value);
    if (this.searchDebounceTimer) {
      clearTimeout(this.searchDebounceTimer);
    }
    this.searchDebounceTimer = setTimeout(() => {
      this.pageNumber.set(1);
      this.loadData();
    }, 350);
  }

  clearSearch(): void {
    this.searchBy.set('');
    if (this.searchDebounceTimer) {
      clearTimeout(this.searchDebounceTimer);
    }
    this.pageNumber.set(1);
    this.loadData();
  }

  onCategoryFilterChange(val: number): void {
    this.selectedCategory.set(val);
    this.pageNumber.set(1);
    this.loadData();
  }

  onAvailabilityFilterChange(val: number): void {
    this.selectedAvailability.set(val);
    this.pageNumber.set(1);
    this.loadData();
  }

  onSearch(): void {
    this.pageNumber.set(1);
    this.loadData();
  }

  onFilterChange(): void {
    this.pageNumber.set(1);
    this.loadData();
  }

  resetFilters(): void {
    this.searchBy.set('');
    this.selectedCategory.set(0);
    this.selectedAvailability.set(0);
    this.pageNumber.set(1);
    this.loadData();
  }

  onSortChange(sortKey: string): void {
    if (this.sortBy() === sortKey) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(sortKey);
      this.sortDirection.set('asc');
    }
    this.loadData();
  }

  onTableAction(event: { action: string; row: VehicleItem }): void {
    if (event.action === 'edit') {
      this.openEditModal(event.row);
    } else if (event.action === 'delete') {
      this.openDeleteModal(event.row);
    }
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentVehicleId.set(null);
    this.modalErrorMessage.set(null);
    const defaultCompanyId = this.companies().length > 0 ? this.companies()[0].id : '';

    this.vehicleForm.reset({
      rentalCompanyId: defaultCompanyId,
      vehicleCategoryId: 1,
      makeAndModel: '',
      licensePlate: '',
      registrationNumber: '',
      yearOfManufacture: new Date().getFullYear(),
      seatingCapacity: 4,
      dailyRentalRate: 50,
      availabilityStatus: VehicleAvailabilityStatus.Available,
      mileage: 0
    });
    this.isModalOpen.set(true);
  }

  openEditModal(vehicle: VehicleItem): void {
    this.isEditMode.set(true);
    this.currentVehicleId.set(vehicle.id);
    this.modalErrorMessage.set(null);

    this.vehicleForm.patchValue({
      rentalCompanyId: vehicle.rentalCompanyId,
      vehicleCategoryId: vehicle.vehicleCategoryId,
      makeAndModel: vehicle.makeAndModel,
      licensePlate: vehicle.licensePlate,
      registrationNumber: vehicle.registrationNumber,
      yearOfManufacture: vehicle.yearOfManufacture,
      seatingCapacity: vehicle.seatingCapacity,
      dailyRentalRate: vehicle.dailyRentalRate,
      availabilityStatus: vehicle.availabilityStatus,
      mileage: vehicle.mileage
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.modalErrorMessage.set(null);
    this.isModalOpen.set(false);
  }

  saveVehicle(): void {
    if (this.vehicleForm.invalid) {
      this.vehicleForm.markAllAsTouched();
      this.toastr.warning('Please fix the validation errors before submitting.', 'Validation Error');
      return;
    }

    const formValues = this.vehicleForm.value;
    const currentId = this.currentVehicleId();

    if (this.isEditMode() && currentId !== null) {
      const payload = {
        id: currentId,
        rentalCompanyId: formValues.rentalCompanyId,
        vehicleCategoryId: Number(formValues.vehicleCategoryId),
        makeAndModel: formValues.makeAndModel.trim(),
        licensePlate: formValues.licensePlate.trim(),
        registrationNumber: formValues.registrationNumber.trim(),
        yearOfManufacture: Number(formValues.yearOfManufacture),
        seatingCapacity: Number(formValues.seatingCapacity),
        dailyRentalRate: Number(formValues.dailyRentalRate),
        availabilityStatus: Number(formValues.availabilityStatus),
        mileage: Number(formValues.mileage)
      };

      this.vehicleService.updateVehicle(payload).subscribe({
        next: () => {
          this.closeModal();
          this.toastr.success('Vehicle updated successfully.', 'Success');
          this.loadData();
        },
        error: (err) => {
          this.handleSaveError(err, 'update');
        }
      });
    } else {
      const payload = {
        rentalCompanyId: formValues.rentalCompanyId,
        vehicleCategoryId: Number(formValues.vehicleCategoryId),
        makeAndModel: formValues.makeAndModel.trim(),
        licensePlate: formValues.licensePlate.trim(),
        registrationNumber: formValues.registrationNumber.trim(),
        yearOfManufacture: Number(formValues.yearOfManufacture),
        seatingCapacity: Number(formValues.seatingCapacity),
        dailyRentalRate: Number(formValues.dailyRentalRate),
        availabilityStatus: Number(formValues.availabilityStatus),
        mileage: Number(formValues.mileage)
      };

      this.vehicleService.addVehicle(payload).subscribe({
        next: () => {
          this.closeModal();
          this.toastr.success('Vehicle added successfully.', 'Success');
          this.loadData();
        },
        error: (err) => {
          this.handleSaveError(err, 'add');
        }
      });
    }
  }

  private handleSaveError(err: any, operation: 'add' | 'update'): void {
    const errorBody = err?.error;
    const message = errorBody?.message || (Array.isArray(errorBody?.errors) && errorBody.errors.length ? errorBody.errors.join(', ') : `Failed to ${operation} vehicle.`);

    this.toastr.error(message, 'Error');

    const lower = message.toLowerCase();
    if (lower.includes('license plate')) {
      const c = this.vehicleForm.get('licensePlate');
      if (c) { c.setErrors({ serverError: message }); c.markAsTouched(); }
    } else if (lower.includes('registration')) {
      const c = this.vehicleForm.get('registrationNumber');
      if (c) { c.setErrors({ serverError: message }); c.markAsTouched(); }
    } else {
      this.modalErrorMessage.set(message);
    }
  }

  openDeleteModal(vehicle: VehicleItem): void {
    this.vehicleToDelete.set(vehicle);
    this.isDeleteModalOpen.set(true);
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen.set(false);
    this.vehicleToDelete.set(null);
  }

  confirmDelete(): void {
    const target = this.vehicleToDelete();
    if (!target) return;

    if ((target.ongoingBookingCount ?? 0) > 0) {
      this.toastr.warning(
        `Cannot delete "${target.makeAndModel}" because it has ${target.ongoingBookingCount} ongoing booking(s) (Pending, Confirmed, or Active).`,
        'Deletion Blocked'
      );
      return;
    }

    this.vehicleService.deleteVehicle(target.id).subscribe({
      next: () => {
        this.closeDeleteModal();
        this.toastr.success('Vehicle deleted successfully.', 'Success');
        this.loadData();
      },
      error: (err) => {
        this.closeDeleteModal();
        const msg = err.error?.message || 'Failed to delete vehicle.';
        this.toastr.error(msg, 'Delete Failed');
      }
    });
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages() && page !== this.pageNumber()) {
      this.pageNumber.set(page);
      this.loadData();
    }
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.vehicleForm.get(fieldName);
    return !!(control && control.invalid && (control.touched || control.dirty));
  }

  getFieldError(fieldName: string): string {
    const control = this.vehicleForm.get(fieldName);
    if (!control || !control.errors) return '';

    if (control.errors['serverError']) {
      return control.errors['serverError'];
    }

    if (control.errors['required']) {
      return 'This field is required.';
    }

    if (control.errors['maxlength']) {
      return `Cannot exceed ${control.errors['maxlength'].requiredLength} characters.`;
    }

    if (control.errors['min']) {
      return `Minimum value is ${control.errors['min'].min}.`;
    }

    if (control.errors['max']) {
      return `Maximum value is ${control.errors['max'].max}.`;
    }

    return 'Invalid input.';
  }
}
