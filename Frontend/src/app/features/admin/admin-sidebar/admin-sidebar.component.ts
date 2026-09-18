import { Component, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/authService';

interface NavItem {
  label: string;
  route: string;
  icon: string;
}

@Component({
  selector: 'app-admin-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './admin-sidebar.component.html',
  styleUrl: './admin-sidebar.component.css'
})
export class AdminSidebarComponent {
  navigate = output<void>();

  navItems: NavItem[] = [
    {
      label: 'RentalCompanies',
      route: '/admin/rental-companies',
      icon: 'companies'
    },
    {
      label: 'Vehicles',
      route: '/admin/vehicles',
      icon: 'vehicles'
    },
    {
      label: 'Bookings',
      route: '/admin/bookings',
      icon: 'bookings'
    }
  ];

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onLogout(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: () => {
        this.authService.clearTokens();
        this.router.navigate(['/login']);
      }
    });
  }
}
