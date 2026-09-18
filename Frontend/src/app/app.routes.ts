import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { AdminLayoutComponent } from './features/admin/admin-layout/admin-layout.component';
import { AdminRentalCompanyComponent } from './features/admin/admin-rental-company/admin-rental-company.component';
import { AdminVehicleComponent } from './features/admin/admin-vehicle/admin-vehicle.component';
import { AdminBookingComponent } from './features/admin/admin-booking/admin-booking.component';
import { CustomerLayoutComponent } from './features/customer/customer-layout/customer-layout.component';
import { CustomerVehiclesComponent } from './features/customer/customer-vehicles/customer-vehicles.component';
import { CustomerRentalsComponent } from './features/customer/customer-rentals/customer-rentals.component';
import { NotFoundComponent } from './page/not-found/not-found.component';
import { adminGuard } from './core/guard/admin.guard';
import { customerGuard } from './core/guard/customer.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: 'admin',
    component: AdminLayoutComponent,
    canActivate: [adminGuard],
    canActivateChild: [adminGuard],
    children: [
      { path: '', redirectTo: 'rental-companies', pathMatch: 'full' },
      { path: 'rental-companies', component: AdminRentalCompanyComponent },
      { path: 'rental-company', redirectTo: 'rental-companies', pathMatch: 'full' },
      { path: 'vehicles', component: AdminVehicleComponent },
      { path: 'vehicle', redirectTo: 'vehicles', pathMatch: 'full' },
      { path: 'bookings', component: AdminBookingComponent },
      { path: 'booking', redirectTo: 'bookings', pathMatch: 'full' }
    ]
  },
  {
    path: 'customer',
    component: CustomerLayoutComponent,
    canActivate: [customerGuard],
    canActivateChild: [customerGuard],
    children: [
      { path: '', redirectTo: 'vehicles', pathMatch: 'full' },
      { path: 'vehicles', component: CustomerVehiclesComponent },
      { path: 'vehicle', redirectTo: 'vehicles', pathMatch: 'full' },
      { path: 'rentals', component: CustomerRentalsComponent },
      { path: 'rental', redirectTo: 'rentals', pathMatch: 'full' },
      { path: 'my-rentals', redirectTo: 'rentals', pathMatch: 'full' }
    ]
  },
  { path: 'not-found', component: NotFoundComponent },
  { path: '**', component: NotFoundComponent }
];
