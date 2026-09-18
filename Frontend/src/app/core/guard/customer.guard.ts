import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/authService';

export const customerGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Strictly Customer role only. Admins or unauthenticated users must NOT access customer portal
  if (authService.isAuthenticated() && authService.isCustomer() && !authService.isAdmin()) {
    return true;
  }

  // Invalid route access redirects to not-found page
  return router.createUrlTree(['/not-found']);
};
