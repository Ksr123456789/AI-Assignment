import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environment/environment';

export interface RegisterRequest {
  fullName: string;
  email: string;
  phoneNumber: string;
  drivingLicenseNumber: string;
  licenseExpiryDate: string | Date;
  address: string;
  password: string;
  confirmPassword: string;
}

export interface RegisterResponse {
  success: boolean;
  message: string;
  userId?: string;
  errors?: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  message: string;
  accessToken?: string;
  refreshToken?: string;
  expiresAt?: string;
  userId?: string;
  email?: string;
  roles?: string[];
  errors?: string[];
}

export interface LogoutRequest {
  userId?: string;
  email?: string;
}

export interface LogoutResponse {
  success: boolean;
  message: string;
  errors?: string[];
}

export interface RefreshRequest {
  accessToken: string;
  refreshToken: string;
}

export interface RefreshResponse {
  success: boolean;
  message: string;
  accessToken?: string;
  refreshToken?: string;
  expiresAt?: string;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly baseUrl = `${environment.apiUrl}/Auth`;

  constructor(private http: HttpClient) {}

  register(data: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.baseUrl}/register`, data);
  }

  login(data: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/login`, data).pipe(
      tap((response) => {
        if (response.success && response.accessToken) {
          this.setTokens(response.accessToken, response.refreshToken);
          if (response.roles && Array.isArray(response.roles)) {
            localStorage.setItem('userRoles', JSON.stringify(response.roles));
          }
          if (response.email) {
            localStorage.setItem('userEmail', response.email);
          }
        }
      })
    );
  }

  logout(data?: LogoutRequest): Observable<LogoutResponse> {
    const payload = data ?? {};
    return this.http.post<LogoutResponse>(`${this.baseUrl}/logout`, payload).pipe(
      tap(() => {
        this.clearTokens();
      })
    );
  }

  refresh(data: RefreshRequest): Observable<RefreshResponse> {
    return this.http.post<RefreshResponse>(`${this.baseUrl}/refresh`, data).pipe(
      tap((response) => {
        if (response.success && response.accessToken) {
          this.setTokens(response.accessToken, response.refreshToken);
        }
      })
    );
  }

  refreshToken(data?: RefreshRequest): Observable<RefreshResponse> {
    const payload = data ?? {
      accessToken: this.getAccessToken() ?? '',
      refreshToken: this.getRefreshToken() ?? ''
    };
    return this.refresh(payload);
  }

  setTokens(accessToken?: string, refreshToken?: string): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      if (accessToken) {
        localStorage.setItem('accessToken', accessToken);
      }
      if (refreshToken) {
        localStorage.setItem('refreshToken', refreshToken);
      }
    }
  }

  getAccessToken(): string | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      return localStorage.getItem('accessToken');
    }
    return null;
  }

  getRefreshToken(): string | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      return localStorage.getItem('refreshToken');
    }
    return null;
  }

  clearTokens(): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('userRoles');
      localStorage.removeItem('userEmail');
    }
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }

  getUserEmail(): string | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      const email = localStorage.getItem('userEmail');
      if (email) return email;
    }
    const token = this.getAccessToken();
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.email || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || null;
    } catch {
      return null;
    }
  }

  getUserRoles(): string[] {
    if (typeof window !== 'undefined' && window.localStorage) {
      const stored = localStorage.getItem('userRoles');
      if (stored) {
        try {
          const parsed = JSON.parse(stored);
          if (Array.isArray(parsed) && parsed.length > 0) {
            return parsed;
          }
        } catch {}
      }
    }

    const token = this.getAccessToken();
    if (!token) return [];

    try {
      const payloadBase64 = token.split('.')[1];
      const decoded = JSON.parse(atob(payloadBase64));
      const roleClaim =
        decoded['role'] ||
        decoded['roles'] ||
        decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

      if (Array.isArray(roleClaim)) {
        return roleClaim;
      }
      if (typeof roleClaim === 'string') {
        return [roleClaim];
      }
    } catch {}

    return [];
  }

  hasRole(role: string): boolean {
    const roles = this.getUserRoles();
    return roles.some((r) => r.toLowerCase() === role.toLowerCase());
  }

  isAdmin(): boolean {
    return this.hasRole('Admin');
  }

  isCustomer(): boolean {
    return this.hasRole('Customer');
  }
}
