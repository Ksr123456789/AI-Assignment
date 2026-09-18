import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
  ValidationErrors,
  ValidatorFn
} from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { GenericTable, tableColumn, tableAction } from '../../../shared/generic-table/generic-table';
import {
  RentalCompanyService,
  RentalCompanyItem,
  CompanyType,
  CompanyStatus,
  RentalCompanyFilterParams
} from '../../../core/services/rentalCompanyService';

/**
 * Custom validator requiring phone to start with a number greater than 5 (i.e. 6, 7, 8, 9)
 * and be exactly 10 numeric digits.
 */
export function phoneGreaterThanFiveValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const val = control.value !== null && control.value !== undefined ? String(control.value).trim() : '';
    if (!val) {
      return null; // Empty handled by Validators.required
    }
    if (!/^\d+$/.test(val)) {
      return { onlyDigits: true };
    }
    const firstDigit = parseInt(val.charAt(0), 10);
    if (firstDigit <= 5) {
      return { startWithGreaterThanFive: true };
    }
    if (val.length !== 10) {
      return { exactTenDigits: true };
    }
    return null;
  };
}

@Component({
  selector: 'app-admin-rental-company',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, GenericTable],
  templateUrl: './admin-rental-company.component.html',
  styleUrl: './admin-rental-company.component.css'
})
export class AdminRentalCompanyComponent implements OnInit {
  // State signals
  companies = signal<RentalCompanyItem[]>([]);
  isLoading = signal<boolean>(false);

  // Search & Filter signals (matching GetPagedRentalCompanyQuery)
  searchBy = signal<string>('');
  selectedType = signal<number>(0); // 0 = all
  selectedStatus = signal<number>(0); // 0 = all
  sortBy = signal<string>('');
  sortDirection = signal<'asc' | 'desc'>('asc');

  // Pagination signals
  pageNumber = signal<number>(1);
  pageSize = signal<number>(5);
  totalCount = signal<number>(0);
  totalPages = signal<number>(0);

  // Add / Edit Modal signals
  isModalOpen = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  currentCompanyId = signal<string | null>(null);
  modalErrorMessage = signal<string | null>(null);
  companyForm!: FormGroup;

  // Delete modal signals
  isDeleteModalOpen = signal<boolean>(false);
  companyToDelete = signal<RentalCompanyItem | null>(null);

  // Computed signals
  hasCompanies = computed(() => this.companies().length > 0);
  startIndex = computed(() => (this.pageNumber() - 1) * this.pageSize() + 1);
  endIndex = computed(() => {
    const end = this.pageNumber() * this.pageSize();
    return end > this.totalCount() ? this.totalCount() : end;
  });

  companyTypeOptions = [
    { value: 0, label: 'All Types' },
    { value: CompanyType.Budget, label: 'Budget' },
    { value: CompanyType.Economy, label: 'Economy' },
    { value: CompanyType.Premium, label: 'Premium' },
    { value: CompanyType.Luxury, label: 'Luxury' }
  ];

  companyStatusOptions = [
    { value: 0, label: 'All Statuses' },
    { value: CompanyStatus.Active, label: 'Active' },
    { value: CompanyStatus.Inactive, label: 'Inactive' }
  ];

  columns: tableColumn<RentalCompanyItem>[] = [
    {
      key: 'companyName',
      label: 'Company Name',
      sortable: true,
      sortKey: 'CompanyName'
    },
    {
      key: 'companyType',
      label: 'Type',
      sortable: true,
      sortKey: 'CompanyType',
      format: (val) => {
        const map: Record<number, string> = {
          [CompanyType.Budget]: 'Budget',
          [CompanyType.Economy]: 'Economy',
          [CompanyType.Premium]: 'Premium',
          [CompanyType.Luxury]: 'Luxury'
        };
        return map[Number(val)] ?? String(val ?? '');
      }
    },
    {
      key: 'ownerName',
      label: 'Owner',
      sortable: true,
      sortKey: 'OwnerName'
    },
    {
      key: 'phoneNumber',
      label: 'Phone',
      sortable: true,
      sortKey: 'PhoneNumber',
      format: (val) => val ?? ''
    },
    {
      key: 'headquartersLocation',
      label: 'Headquarters',
      sortable: true,
      sortKey: 'HeadquartersLocation'
    },
    {
      key: 'licenseNumber',
      label: 'License',
      sortable: true,
      sortKey: 'LicenseNumber'
    },
    {
      key: 'status',
      label: 'Status',
      sortable: true,
      sortKey: 'Status',
      format: (val) => (Number(val) === CompanyStatus.Active ? 'Active' : 'Inactive')
    },
    {
      key: 'vehicleCount',
      label: 'Vehicles',
      sortable: false,
      format: (val) => `${val ?? 0} listed`
    },
    {
      type: 'actions',
      label: 'Actions'
    }
  ];

  actions: tableAction<RentalCompanyItem>[] = [
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
    private rentalCompanyService: RentalCompanyService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadData();
  }

  initForm(): void {
    this.companyForm = this.fb.group({
      companyName: ['', [Validators.required, Validators.maxLength(100)]],
      companyType: [CompanyType.Budget, [Validators.required]],
      ownerName: ['', [Validators.required, Validators.maxLength(100)]],
      phone: ['', [Validators.required, phoneGreaterThanFiveValidator()]],
      headquartersLocation: ['', [Validators.required, Validators.maxLength(200)]],
      licenseNumber: ['', [Validators.required, Validators.maxLength(50)]],
      status: [CompanyStatus.Active, [Validators.required]]
    });

    // Reset modal error message whenever user modifies the form
    this.companyForm.valueChanges.subscribe(() => {
      if (this.modalErrorMessage()) {
        this.modalErrorMessage.set(null);
      }
    });
  }

  loadData(): void {
    this.isLoading.set(true);

    const params: RentalCompanyFilterParams = {
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      sortBy: this.sortBy(),
      sortDirection: this.sortDirection(),
      searchBy: this.searchBy(),
      companyType: this.selectedType() || undefined,
      companyStatus: this.selectedStatus() || undefined
    };

    this.rentalCompanyService.getPagedRentalCompany(params).subscribe({
      next: (result) => {
        this.isLoading.set(false);
        this.companies.set(result.items || []);
        this.pageNumber.set(result.pageNumber);
        this.pageSize.set(result.pageSize);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = err.error?.message || 'Failed to load rental companies.';
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

  onTypeFilterChange(val: number): void {
    this.selectedType.set(val);
    this.pageNumber.set(1);
    this.loadData();
  }

  onStatusFilterChange(val: number): void {
    this.selectedStatus.set(val);
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
    this.selectedType.set(0);
    this.selectedStatus.set(0);
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

  onTableAction(event: { action: string; row: RentalCompanyItem }): void {
    if (event.action === 'edit') {
      this.openEditModal(event.row);
    } else if (event.action === 'delete') {
      this.openDeleteModal(event.row);
    }
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentCompanyId.set(null);
    this.modalErrorMessage.set(null);
    this.companyForm.reset({
      companyName: '',
      companyType: CompanyType.Budget,
      ownerName: '',
      phone: '',
      headquartersLocation: '',
      licenseNumber: '',
      status: CompanyStatus.Active
    });
    this.isModalOpen.set(true);
  }

  openEditModal(company: RentalCompanyItem): void {
    this.isEditMode.set(true);
    this.currentCompanyId.set(company.id);
    this.modalErrorMessage.set(null);
    this.companyForm.patchValue({
      companyName: company.companyName,
      companyType: company.companyType,
      ownerName: company.ownerName,
      phone: company.phoneNumber || company.phone || '',
      headquartersLocation: company.headquartersLocation,
      licenseNumber: company.licenseNumber,
      status: company.status
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.modalErrorMessage.set(null);
    this.isModalOpen.set(false);
  }

  saveCompany(): void {
    if (this.companyForm.invalid) {
      this.companyForm.markAllAsTouched();
      this.toastr.warning('Please fix the validation errors before submitting.', 'Validation Error');
      return;
    }

    const formValues = this.companyForm.value;
    const currentId = this.currentCompanyId();

    if (this.isEditMode() && currentId) {
      const payload = {
        id: currentId,
        companyName: formValues.companyName.trim(),
        companyType: Number(formValues.companyType),
        ownerName: formValues.ownerName.trim(),
        phone: formValues.phone.trim(),
        headquartersLocation: formValues.headquartersLocation.trim(),
        licenseNumber: formValues.licenseNumber.trim(),
        status: Number(formValues.status)
      };

      this.rentalCompanyService.updateRentalCompany(payload).subscribe({
        next: () => {
          this.closeModal();
          this.toastr.success('Rental company updated successfully.', 'Success');
          this.loadData();
        },
        error: (err) => {
          this.handleSaveError(err, 'update');
        }
      });
    } else {
      const payload = {
        companyName: formValues.companyName.trim(),
        companyType: Number(formValues.companyType),
        ownerName: formValues.ownerName.trim(),
        phone: formValues.phone.trim(),
        headquartersLocation: formValues.headquartersLocation.trim(),
        licenseNumber: formValues.licenseNumber.trim(),
        status: Number(formValues.status)
      };

      this.rentalCompanyService.addRentalCompany(payload).subscribe({
        next: () => {
          this.closeModal();
          this.toastr.success('Rental company added successfully.', 'Success');
          this.sortBy.set('');
          this.sortDirection.set('asc');
          this.pageNumber.set(1);
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
    const message = errorBody?.message || (Array.isArray(errorBody?.errors) && errorBody.errors.length ? errorBody.errors.join(', ') : `Failed to ${operation} rental company.`);

    this.toastr.error(message, 'Error');

    const lower = message.toLowerCase();
    if (lower.includes('license')) {
      const control = this.companyForm.get('licenseNumber');
      if (control) {
        control.setErrors({ serverError: message });
        control.markAsTouched();
      }
    } else if (lower.includes('phone')) {
      const control = this.companyForm.get('phone');
      if (control) {
        control.setErrors({ serverError: message });
        control.markAsTouched();
      }
    } else if (lower.includes('name') || lower.includes('company')) {
      const control = this.companyForm.get('companyName');
      if (control) {
        control.setErrors({ serverError: message });
        control.markAsTouched();
      }
    } else {
      this.modalErrorMessage.set(message);
    }
  }

  openDeleteModal(company: RentalCompanyItem): void {
    this.companyToDelete.set(company);
    this.isDeleteModalOpen.set(true);
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen.set(false);
    this.companyToDelete.set(null);
  }

  confirmDelete(): void {
    const target = this.companyToDelete();
    if (!target) return;

    if ((target.vehicleCount ?? 0) > 0) {
      this.toastr.warning(
        `Cannot delete "${target.companyName}" because it currently has ${target.vehicleCount} vehicle listing(s). Please remove all vehicles first.`,
        'Deletion Blocked'
      );
      return;
    }

    this.rentalCompanyService.deleteRentalCompany(target.id).subscribe({
      next: () => {
        this.closeDeleteModal();
        this.toastr.success('Rental company deleted successfully.', 'Success');
        this.loadData();
      },
      error: (err) => {
        this.closeDeleteModal();
        const msg = err.error?.message || 'Failed to delete rental company.';
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
    const control = this.companyForm.get(fieldName);
    return !!(control && control.invalid && (control.touched || control.dirty));
  }

  getFieldError(fieldName: string): string {
    const control = this.companyForm.get(fieldName);
    if (!control || !control.errors) return '';

    if (control.errors['serverError']) {
      return control.errors['serverError'];
    }

    if (control.errors['required']) {
      switch (fieldName) {
        case 'companyName': return 'Company name is required.';
        case 'companyType': return 'Company type is required.';
        case 'ownerName': return 'Owner name is required.';
        case 'phone': return 'Phone number is required.';
        case 'licenseNumber': return 'License number is required.';
        case 'headquartersLocation': return 'Headquarters location is required.';
        case 'status': return 'Status is required.';
        default: return 'This field is required.';
      }
    }

    if (fieldName === 'phone') {
      if (control.errors['onlyDigits']) {
        return 'Phone number must contain only numbers.';
      }
      if (control.errors['startWithGreaterThanFive']) {
        return 'Phone number must start with a number greater than 5 (6, 7, 8, or 9).';
      }
      if (control.errors['exactTenDigits']) {
        return 'Phone number must be exactly 10 digits.';
      }
    }

    if (control.errors['maxlength']) {
      return `Cannot exceed ${control.errors['maxlength'].requiredLength} characters.`;
    }

    return 'Invalid input.';
  }
}

