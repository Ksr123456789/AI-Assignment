import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/authService';

export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Strictly Admin role only. Customers or unauthenticated users must NOT access admin portal
  if (authService.isAuthenticated() && authService.isAdmin() && !authService.isCustomer()) {
    return true;
  }

  // Invalid route access redirects to not-found page
  return router.createUrlTree(['/not-found']);
};
