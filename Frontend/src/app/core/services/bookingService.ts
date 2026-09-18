import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environment/environment';
import { PagedResult } from './rentalCompanyService';

export enum BookingStatus {
  Pending = 1,
  Confirmed = 2,
  Active = 3,
  Completed = 4,
  Cancelled = 5 // Represents Reject / Cancel
}

export interface BookingItem {
  id: number;
  pickupDateTime: string;
  returnDateTime: string;
  pickupLocation: string;
  returnLocation: string;
  insurance: boolean;
  bookingStatus: BookingStatus;
  totalDays: number;
  rentalAmount: number;
  additionalServiceAmount: number;
  totalAmount: number;
  bookedOn: string;
  vehicleId: number;
  vehicleMakeAndModel: string;
  vehicleLicensePlate: string;
  rentalCompanyId: string;
  rentalCompanyName: string;
  customerId: string;
  customerName: string;
  customerEmail: string;
}

export interface BookingFilterParams {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  searchBy?: string;
  bookingStatus?: number;
  rentalCompanyId?: number;
  fromDate?: string;
  toDate?: string;
}

export interface UpdateBookingStatusRequest {
  bookingId: number;
  status: BookingStatus;
}

export interface UpdateBookingStatusResponse {
  success: boolean;
  message: string;
  errors?: string[];
}

export interface MyRentalItem {
  id: number;
  makeModal: string;
  seatingCapacity: number;
  pickupDateTime: string;
  returnDateTime: string;
  bookingStatus: BookingStatus;
  additionalServiceAmount: number;
  rentalAmount: number;
  totalDays: number;
  totalAmount: number;
  licensePlate: string;
  registrationNumber: string;
  yearofManufacture: number;
  dailyRentalRate: number;
  mileage: number;
}

export interface MyRentalsFilterParams {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  searchBy?: string;
  bookingStatus?: number;
  seatingCapacity?: number;
}

export interface BookVehicleRequest {
  pickUpDateTime: string;
  returnDateTime: string;
  pickupLocation: string;
  returnLocation: string;
  vehicleId: number;
  insurance: boolean;
  extraServiceIds?: number[];
  driverFullName?: string;
  driverPhoneNumber?: string;
  driverLicenseNumber?: string;
  licenseExpiryDate?: string;
}

export interface BookVehicleResponse {
  success: boolean;
  message: string;
  data?: any;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private readonly baseUrl = `${environment.apiUrl}/Booking`;

  constructor(private http: HttpClient) {}

  getPagedBookings(filter: BookingFilterParams): Observable<PagedResult<BookingItem>> {
    let params = new HttpParams()
      .set('PageNumber', filter.pageNumber.toString())
      .set('PageSize', filter.pageSize.toString());

    if (filter.sortBy) {
      params = params.set('SortBy', filter.sortBy);
    }
    if (filter.sortDirection) {
      params = params.set('SortDirection', filter.sortDirection);
    }
    if (filter.searchBy && filter.searchBy.trim()) {
      params = params.set('SearchBy', filter.searchBy.trim());
    }
    if (filter.bookingStatus !== undefined && filter.bookingStatus !== 0) {
      params = params.set('BookingStatus', filter.bookingStatus.toString());
    }
    if (filter.rentalCompanyId) {
      params = params.set('RentalCompanyId', filter.rentalCompanyId.toString());
    }
    if (filter.fromDate) {
      params = params.set('FromDate', filter.fromDate);
    }
    if (filter.toDate) {
      params = params.set('ToDate', filter.toDate);
    }

    return this.http.get<PagedResult<BookingItem>>(`${this.baseUrl}/GetPagedBooking`, { params });
  }

  getMyBookings(filter: MyRentalsFilterParams): Observable<PagedResult<MyRentalItem>> {
    let params = new HttpParams()
      .set('PageNumber', filter.pageNumber.toString())
      .set('PageSize', filter.pageSize.toString());

    if (filter.sortBy) {
      params = params.set('SortBy', filter.sortBy);
    }
    if (filter.sortDirection) {
      params = params.set('SortDirection', filter.sortDirection);
    }
    if (filter.searchBy && filter.searchBy.trim()) {
      params = params.set('SearchBy', filter.searchBy.trim());
    }
    if (filter.bookingStatus !== undefined && filter.bookingStatus !== 0) {
      params = params.set('BookingStatus', filter.bookingStatus.toString());
    }
    if (filter.seatingCapacity) {
      params = params.set('SeatingCapacity', filter.seatingCapacity.toString());
    }

    return this.http.get<PagedResult<MyRentalItem>>(`${this.baseUrl}/GetMyBookings`, { params });
  }

  updateBookingStatus(request: UpdateBookingStatusRequest): Observable<UpdateBookingStatusResponse> {
    return this.http.put<UpdateBookingStatusResponse>(`${this.baseUrl}/UpdateBookingStatus`, request);
  }

  bookVehicle(request: BookVehicleRequest): Observable<BookVehicleResponse> {
    return this.http.post<BookVehicleResponse>(`${this.baseUrl}/BookVehicle`, request);
  }
}
