import {
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
  HttpErrorResponse,
  HttpInterceptor,
  HttpHandler,
  HttpEvent
} from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, catchError, filter, Observable, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../services/authService';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();

  let authReq = req;
  if (token && !req.headers.has('Authorization')) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((error: unknown) => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        !req.url.includes('/Auth/login') &&
        !req.url.includes('/Auth/register') &&
        !req.url.includes('/Auth/refresh')
      ) {
        return handle401Error(authReq, next, authService);
      }
      return throwError(() => error);
    })
  );
};

function handle401Error(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService
) {
  const accessToken = authService.getAccessToken() || '';
  const refreshToken = authService.getRefreshToken();

  if (!refreshToken) {
    authService.clearTokens();
    return throwError(() => new Error('No refresh token available'));
  }

  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    return authService.refresh({ accessToken, refreshToken }).pipe(
      switchMap((res) => {
        isRefreshing = false;
        const newAccessToken = res.accessToken || authService.getAccessToken() || '';
        refreshTokenSubject.next(newAccessToken);
        return next(
          req.clone({
            setHeaders: {
              Authorization: `Bearer ${newAccessToken}`
            }
          })
        );
      }),
      catchError((refreshErr) => {
        isRefreshing = false;
        refreshTokenSubject.next(null);
        authService.clearTokens();
        return throwError(() => refreshErr);
      })
    );
  } else {
    return refreshTokenSubject.pipe(
      filter((newToken): newToken is string => newToken !== null),
      take(1),
      switchMap((newToken) => {
        return next(
          req.clone({
            setHeaders: {
              Authorization: `Bearer ${newToken}`
            }
          })
        );
      })
    );
  }
}

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return authInterceptor(req, (r) => next.handle(r));
  }
}
