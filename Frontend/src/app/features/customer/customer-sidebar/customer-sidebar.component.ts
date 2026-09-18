import { Component, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/authService';

interface CustomerNavItem {
  label: string;
  route: string;
  icon: string;
}

@Component({
  selector: 'app-customer-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './customer-sidebar.component.html',
  styleUrl: './customer-sidebar.component.css'
})
export class CustomerSidebarComponent {
  navigate = output<void>();

  navItems: CustomerNavItem[] = [
    {
      label: 'Explore Vehicles',
      route: '/customer/vehicles',
      icon: 'vehicles'
    },
    {
      label: 'My Rentals',
      route: '/customer/rentals',
      icon: 'rentals'
    }
  ];

  constructor(
    public authService: AuthService,
    private router: Router
  ) {}

  getUserEmail(): string {
    return this.authService.getUserEmail() || 'customer@portal.com';
  }

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
