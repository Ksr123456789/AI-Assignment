import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import {
  BookingService,
  MyRentalItem,
  MyRentalsFilterParams,
  BookingStatus
} from '../../../core/services/bookingService';

@Component({
  selector: 'app-customer-rentals',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-rentals.component.html',
  styleUrl: './customer-rentals.component.css'
})
export class CustomerRentalsComponent implements OnInit {
  // State signals
  rentals = signal<MyRentalItem[]>([]);
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  isCancelling = signal<boolean>(false);

  // Search & Filter signals (matching MyPagedRentalsQuery)
  searchBy = signal<string>('');
  selectedStatus = signal<number>(0);
  sortBy = signal<string>('Id');
  sortDirection = signal<'asc' | 'desc'>('desc');

  // Pagination signals
  pageNumber = signal<number>(1);
  pageSize = signal<number>(6);
  totalCount = signal<number>(0);
  totalPages = signal<number>(0);

  // Modal signals
  cancelModalOpen = signal<boolean>(false);
  rentalToCancel = signal<MyRentalItem | null>(null);
  detailsModalOpen = signal<boolean>(false);
  selectedRental = signal<MyRentalItem | null>(null);

  // Computed signals
  hasRentals = computed(() => this.rentals().length > 0);
  startIndex = computed(() => (this.pageNumber() - 1) * this.pageSize() + 1);
  endIndex = computed(() => {
    const end = this.pageNumber() * this.pageSize();
    return end > this.totalCount() ? this.totalCount() : end;
  });

  statusOptions = [
    { value: 0, label: 'All Statuses' },
    { value: BookingStatus.Pending, label: 'Pending' },
    { value: BookingStatus.Confirmed, label: 'Confirmed' },
    { value: BookingStatus.Active, label: 'Active' },
    { value: BookingStatus.Completed, label: 'Completed' },
    { value: BookingStatus.Cancelled, label: 'Cancelled / Rejected' }
  ];

  constructor(
    private bookingService: BookingService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadRentals();
  }

  loadRentals(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const params: MyRentalsFilterParams = {
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      sortBy: this.sortBy(),
      sortDirection: this.sortDirection(),
      searchBy: this.searchBy(),
      bookingStatus: this.selectedStatus() || undefined
    };

    this.bookingService.getMyBookings(params).subscribe({
      next: (result) => {
        this.isLoading.set(false);
        this.rentals.set(result.items || []);
        this.pageNumber.set(result.pageNumber);
        this.pageSize.set(result.pageSize);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message || 'Failed to load your rentals.');
      }
    });
  }

  onSearch(): void {
    this.pageNumber.set(1);
    this.loadRentals();
  }

  onStatusChange(): void {
    this.pageNumber.set(1);
    this.loadRentals();
  }

  resetFilters(): void {
    this.searchBy.set('');
    this.selectedStatus.set(0);
    this.pageNumber.set(1);
    this.loadRentals();
  }

  openCancelModal(rental: MyRentalItem): void {
    this.rentalToCancel.set(rental);
    this.cancelModalOpen.set(true);
  }

  closeCancelModal(): void {
    this.cancelModalOpen.set(false);
    this.rentalToCancel.set(null);
  }

  confirmCancel(): void {
    const rental = this.rentalToCancel();
    if (!rental) return;

    this.isCancelling.set(true);

    this.bookingService
      .updateBookingStatus({
        bookingId: rental.id,
        status: BookingStatus.Cancelled
      })
      .subscribe({
        next: () => {
          this.isCancelling.set(false);
          this.closeCancelModal();
          this.toastr.success('Booking reservation cancelled successfully.', 'Cancellation Complete');
          this.showSuccess('Booking reservation cancelled successfully.');
          this.loadRentals();
        },
        error: (err) => {
          this.isCancelling.set(false);
          this.closeCancelModal();
          const msg = err.error?.message || 'Failed to cancel booking reservation.';
          this.toastr.error(msg, 'Error');
          this.errorMessage.set(msg);
        }
      });
  }

  openDetailsModal(rental: MyRentalItem): void {
    this.selectedRental.set(rental);
    this.detailsModalOpen.set(true);
  }

  closeDetailsModal(): void {
    this.detailsModalOpen.set(false);
    this.selectedRental.set(null);
  }

  goToFleet(): void {
    this.router.navigate(['/customer/vehicles']);
  }

  isPending(status: number): boolean {
    return status === BookingStatus.Pending;
  }

  getStatusBadgeClass(status: number): string {
    switch (status) {
      case BookingStatus.Pending:
        return 'bg-amber-500/10 text-amber-300 border-amber-500/20';
      case BookingStatus.Confirmed:
        return 'bg-sky-500/10 text-sky-300 border-sky-500/20';
      case BookingStatus.Active:
        return 'bg-emerald-500/10 text-emerald-300 border-emerald-500/20';
      case BookingStatus.Completed:
        return 'bg-indigo-500/10 text-indigo-300 border-indigo-500/20';
      case BookingStatus.Cancelled:
        return 'bg-rose-500/10 text-rose-300 border-rose-500/20';
      default:
        return 'bg-slate-700/40 text-slate-300 border-slate-600/30';
    }
  }

  getStatusName(status: number): string {
    const map: Record<number, string> = {
      [BookingStatus.Pending]: 'Pending Approval',
      [BookingStatus.Confirmed]: 'Confirmed',
      [BookingStatus.Active]: 'Active on the Road',
      [BookingStatus.Completed]: 'Completed',
      [BookingStatus.Cancelled]: 'Cancelled'
    };
    return map[status] ?? 'Unknown';
  }

  showSuccess(msg: string): void {
    this.successMessage.set(msg);
    setTimeout(() => {
      this.successMessage.set(null);
    }, 4000);
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages() && page !== this.pageNumber()) {
      this.pageNumber.set(page);
      this.loadRentals();
    }
  }
}
