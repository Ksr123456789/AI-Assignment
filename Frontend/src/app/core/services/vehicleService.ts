import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environment/environment';
import { PagedResult } from './rentalCompanyService';

export enum VehicleAvailabilityStatus {
  Available = 1,
  Unavailable = 2,
  InMaintenance = 3
}

export interface VehicleItem {
  id: number;
  rentalCompanyId: string;
  rentalCompanyName: string;
  vehicleCategoryId: number;
  vehicleCategoryName: string;
  makeAndModel: string;
  licensePlate: string;
  registrationNumber: string;
  yearOfManufacture: number;
  seatingCapacity: number;
  dailyRentalRate: number;
  availabilityStatus: VehicleAvailabilityStatus;
  mileage: number;
  rentalCompanyStatus?: number;
  ongoingBookingCount?: number;
}

export interface VehicleFilterParams {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  searchBy?: string;
  vehicleCategoryId?: number;
  vehicleAvailabilityStatus?: number;
}

export interface AddVehicleRequest {
  rentalCompanyId: string;
  vehicleCategoryId: number;
  makeAndModel: string;
  licensePlate: string;
  registrationNumber: string;
  yearOfManufacture: number;
  seatingCapacity: number;
  dailyRentalRate: number;
  availabilityStatus: number;
  mileage: number;
}

export interface UpdateVehicleRequest extends AddVehicleRequest {
  id: number;
}

@Injectable({
  providedIn: 'root'
})
export class VehicleService {
  private readonly baseUrl = `${environment.apiUrl}/Vehicle`;

  constructor(private http: HttpClient) {}

  getPagedVehicles(params: VehicleFilterParams): Observable<PagedResult<VehicleItem>> {
    let httpParams = new HttpParams()
      .set('PageNumber', params.pageNumber.toString())
      .set('PageSize', params.pageSize.toString());

    if (params.sortBy) {
      httpParams = httpParams.set('SortBy', params.sortBy);
    }
    if (params.sortDirection) {
      httpParams = httpParams.set('SortDirection', params.sortDirection);
    }
    if (params.searchBy && params.searchBy.trim()) {
      httpParams = httpParams.set('SearchBy', params.searchBy.trim());
    }
    if (params.vehicleCategoryId !== undefined && params.vehicleCategoryId !== null && params.vehicleCategoryId !== 0) {
      httpParams = httpParams.set('VehicleCategoryId', params.vehicleCategoryId.toString());
    }
    if (params.vehicleAvailabilityStatus !== undefined && params.vehicleAvailabilityStatus !== null && params.vehicleAvailabilityStatus !== 0) {
      httpParams = httpParams.set('VehicleAvailabilityStatus', params.vehicleAvailabilityStatus.toString());
    }

    return this.http.get<PagedResult<VehicleItem>>(`${this.baseUrl}/GetPagedVehicle`, {
      params: httpParams
    });
  }

  addVehicle(command: AddVehicleRequest): Observable<any> {
    return this.http.post(`${this.baseUrl}`, command);
  }

  updateVehicle(command: UpdateVehicleRequest): Observable<any> {
    return this.http.put(`${this.baseUrl}`, command);
  }

  deleteVehicle(id: number): Observable<any> {
    const params = new HttpParams().set('Id', id.toString());
    return this.http.delete(`${this.baseUrl}`, { params });
  }
}
