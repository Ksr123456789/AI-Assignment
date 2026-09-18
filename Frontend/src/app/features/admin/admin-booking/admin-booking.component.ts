import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { GenericTable, tableColumn, tableAction } from '../../../shared/generic-table/generic-table';
import {
  BookingService,
  BookingItem,
  BookingStatus,
  BookingFilterParams
} from '../../../core/services/bookingService';

@Component({
  selector: 'app-admin-booking',
  standalone: true,
  imports: [CommonModule, FormsModule, GenericTable],
  templateUrl: './admin-booking.component.html',
  styleUrl: './admin-booking.component.css'
})
export class AdminBookingComponent implements OnInit {
  // State signals
  bookings = signal<BookingItem[]>([]);
  isLoading = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);

  // Search & Filter signals (matching GetPagedBookingQuery)
  searchBy = signal<string>('');
  selectedStatus = signal<number>(0); // 0 = all
  fromBookedDate = signal<string>('');
  toBookedDate = signal<string>('');
  sortBy = signal<string>('');
  sortDirection = signal<'asc' | 'desc'>('asc');

  // Pagination signals
  pageNumber = signal<number>(1);
  pageSize = signal<number>(5);
  totalCount = signal<number>(0);
  totalPages = signal<number>(0);

  // Status transition modal signals
  statusModalOpen = signal<boolean>(false);
  selectedBooking = signal<BookingItem | null>(null);
  targetStatus = signal<BookingStatus | null>(null);
  targetStatusLabel = signal<string>('');

  // Details modal signals
  detailsModalOpen = signal<boolean>(false);

  // Computed signals
  hasBookings = computed(() => this.bookings().length > 0);
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

  columns: tableColumn<BookingItem>[] = [
    {
      key: 'id',
      label: 'ID',
      sortable: true,
      sortKey: 'Id',
      format: (val) => `#${val}`
    },
    {
      key: 'customerName',
      label: 'Customer',
      sortable: true,
      sortKey: 'CustomerName'
    },
    {
      key: 'vehicleMakeAndModel',
      label: 'Vehicle',
      sortable: true,
      sortKey: 'VehicleMakeAndModel'
    },
    {
      key: 'rentalCompanyName',
      label: 'Rental Company',
      sortable: true,
      sortKey: 'RentalCompanyName'
    },
    {
      key: 'bookedOn',
      label: 'Booked Date',
      sortable: true,
      sortKey: 'BookedOn',
      format: (val) => (val ? new Date(val).toLocaleDateString() : '')
    },
    {
      key: 'pickupDateTime',
      label: 'Pickup Date',
      sortable: true,
      sortKey: 'PickupDateTime',
      format: (val) => (val ? new Date(val).toLocaleDateString() : '')
    },
    {
      key: 'returnDateTime',
      label: 'Return Date',
      sortable: true,
      sortKey: 'ReturnDateTime',
      format: (val) => (val ? new Date(val).toLocaleDateString() : '')
    },
    {
      key: 'totalAmount',
      label: 'Total Rate',
      sortable: true,
      sortKey: 'TotalAmount',
      format: (val) => `$${Number(val).toFixed(2)}`
    },
    {
      key: 'bookingStatus',
      label: 'Status',
      sortable: true,
      sortKey: 'BookingStatus',
      format: (val) => {
        const map: Record<number, string> = {
          [BookingStatus.Pending]: 'Pending',
          [BookingStatus.Confirmed]: 'Confirmed',
          [BookingStatus.Active]: 'Active',
          [BookingStatus.Completed]: 'Completed',
          [BookingStatus.Cancelled]: 'Cancelled'
        };
        return map[Number(val)] ?? String(val ?? '');
      }
    },
    {
      type: 'actions',
      label: 'Update Status & Actions'
    }
  ];

  /**
   * Action flow per requirements:
   * - Booking is Pending (1)   => Confirm (2) & Reject (5)
   * - Booking is Confirmed (2) => Active (3)  & Reject (5)
   * - Booking is Active (3)    => Complete (4)
   * - Details is always available to review booking info
   */
  actions: tableAction<BookingItem>[] = [
    {
      label: 'Confirm',
      action: 'confirm',
      showIf: (row) => row.bookingStatus === BookingStatus.Pending,
      class: 'bg-emerald-600/20 text-emerald-300 hover:bg-emerald-600 hover:text-white border border-emerald-500/30 font-medium'
    },
    {
      label: 'Active',
      action: 'active',
      showIf: (row) => row.bookingStatus === BookingStatus.Confirmed,
      class: 'bg-sky-600/20 text-sky-300 hover:bg-sky-600 hover:text-white border border-sky-500/30 font-medium'
    },
    {
      label: 'Complete',
      action: 'complete',
      showIf: (row) => row.bookingStatus === BookingStatus.Active,
      class: 'bg-indigo-600/20 text-indigo-300 hover:bg-indigo-600 hover:text-white border border-indigo-500/30 font-medium'
    },
    {
      label: 'Reject',
      action: 'reject',
      showIf: (row) =>
        row.bookingStatus === BookingStatus.Pending || row.bookingStatus === BookingStatus.Confirmed,
      class: 'bg-rose-600/20 text-rose-300 hover:bg-rose-600 hover:text-white border border-rose-500/30 font-medium'
    },
    {
      label: 'Details',
      action: 'details',
      class: 'bg-slate-700/50 text-slate-300 hover:bg-slate-700 hover:text-white border border-slate-600/40 text-xs'
    }
  ];

  constructor(
    private bookingService: BookingService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);

    const params: BookingFilterParams = {
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      sortBy: this.sortBy(),
      sortDirection: this.sortDirection(),
      searchBy: this.searchBy(),
      bookingStatus: this.selectedStatus() || undefined,
      fromDate: this.fromBookedDate() || undefined,
      toDate: this.toBookedDate() || undefined
    };

    this.bookingService.getPagedBookings(params).subscribe({
      next: (result) => {
        this.isLoading.set(false);
        this.bookings.set(result.items || []);
        this.pageNumber.set(result.pageNumber);
        this.pageSize.set(result.pageSize);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = err.error?.message || 'Failed to load bookings.';
        this.toastr.error(msg, 'Error');
      }
    });
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
    this.selectedStatus.set(0);
    this.fromBookedDate.set('');
    this.toBookedDate.set('');
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

  onTableAction(event: { action: string; row: BookingItem }): void {
    const row = event.row;

    switch (event.action) {
      case 'confirm':
        this.openStatusConfirmation(row, BookingStatus.Confirmed, 'Confirm Booking');
        break;
      case 'active':
        this.openStatusConfirmation(row, BookingStatus.Active, 'Mark as Active');
        break;
      case 'complete':
        this.openStatusConfirmation(row, BookingStatus.Completed, 'Mark as Completed');
        break;
      case 'reject':
        this.openStatusConfirmation(row, BookingStatus.Cancelled, 'Reject Booking');
        break;
      case 'details':
        this.openDetailsModal(row);
        break;
    }
  }

  openStatusConfirmation(booking: BookingItem, newStatus: BookingStatus, label: string): void {
    this.selectedBooking.set(booking);
    this.targetStatus.set(newStatus);
    this.targetStatusLabel.set(label);
    this.statusModalOpen.set(true);
  }

  closeStatusModal(): void {
    this.statusModalOpen.set(false);
    this.selectedBooking.set(null);
    this.targetStatus.set(null);
  }

  confirmStatusUpdate(): void {
    const booking = this.selectedBooking();
    const status = this.targetStatus();

    if (!booking || status === null) return;

    this.isSubmitting.set(true);

    this.bookingService
      .updateBookingStatus({
        bookingId: booking.id,
        status: status
      })
      .subscribe({
        next: (res) => {
          this.isSubmitting.set(false);
          this.closeStatusModal();
          this.toastr.success(res.message || 'Booking status updated successfully.', 'Success');
          this.loadData();
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.closeStatusModal();
          const msg = err.error?.message || err.error?.errors?.[0] || 'Failed to update booking status.';
          this.toastr.error(msg, 'Error');
        }
      });
  }

  openDetailsModal(booking: BookingItem): void {
    this.selectedBooking.set(booking);
    this.detailsModalOpen.set(true);
  }

  closeDetailsModal(): void {
    this.detailsModalOpen.set(false);
    this.selectedBooking.set(null);
  }

  getStatusName(status: number): string {
    const map: Record<number, string> = {
      [BookingStatus.Pending]: 'Pending',
      [BookingStatus.Confirmed]: 'Confirmed',
      [BookingStatus.Active]: 'Active',
      [BookingStatus.Completed]: 'Completed',
      [BookingStatus.Cancelled]: 'Cancelled / Rejected'
    };
    return map[status] ?? 'Unknown';
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages() && page !== this.pageNumber()) {
      this.pageNumber.set(page);
      this.loadData();
    }
  }
}
