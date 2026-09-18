import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environment/environment';

export enum CompanyType {
  Budget = 1,
  Economy = 2,
  Premium = 3,
  Luxury = 4
}

export enum CompanyStatus {
  Active = 1,
  Inactive = 2
}

export interface RentalCompanyItem {
  id: string;
  companyName: string;
  companyType: CompanyType;
  ownerName: string;
  phoneNumber?: string;
  phone?: string;
  headquartersLocation: string;
  licenseNumber: string;
  status: CompanyStatus;
  vehicleCount?: number;
}

export interface RentalCompanyFilterParams {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  searchBy?: string;
  companyType?: number;
  companyStatus?: number;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface AddRentalCompanyRequest {
  companyName: string;
  companyType: number;
  ownerName: string;
  phone: string;
  headquartersLocation: string;
  licenseNumber: string;
  status: number;
}

export interface UpdateRentalCompanyRequest extends AddRentalCompanyRequest {
  id: string;
}

@Injectable({
  providedIn: 'root'
})
export class RentalCompanyService {
  private readonly baseUrl = `${environment.apiUrl}/RentalCompany`;

  constructor(private http: HttpClient) {}

  getPagedRentalCompany(params: RentalCompanyFilterParams): Observable<PagedResult<RentalCompanyItem>> {
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
    if (params.companyType !== undefined && params.companyType !== null && params.companyType !== 0) {
      httpParams = httpParams.set('CompanyType', params.companyType.toString());
    }
    if (params.companyStatus !== undefined && params.companyStatus !== null && params.companyStatus !== 0) {
      httpParams = httpParams.set('CompanyStatus', params.companyStatus.toString());
    }

    return this.http.get<PagedResult<RentalCompanyItem>>(
      `${this.baseUrl}/GetPagedRentalCompany`,
      { params: httpParams }
    );
  }

  getAllRentalCompanies(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}`);
  }

  addRentalCompany(command: AddRentalCompanyRequest): Observable<any> {
    return this.http.post(`${this.baseUrl}`, command);
  }

  updateRentalCompany(command: UpdateRentalCompanyRequest): Observable<any> {
    let httpParams = new HttpParams()
      .set('Id', command.id)
      .set('CompanyName', command.companyName)
      .set('CompanyType', command.companyType.toString())
      .set('OwnerName', command.ownerName)
      .set('Phone', command.phone)
      .set('HeadquartersLocation', command.headquartersLocation)
      .set('LicenseNumber', command.licenseNumber)
      .set('Status', command.status.toString());

    return this.http.put(`${this.baseUrl}`, command, { params: httpParams });
  }

  deleteRentalCompany(id: string): Observable<any> {
    const params = new HttpParams().set('Id', id);
    return this.http.delete(`${this.baseUrl}`, { params });
  }
}
