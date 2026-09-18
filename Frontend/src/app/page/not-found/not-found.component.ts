import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/authService';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './not-found.component.html',
  styleUrl: './not-found.component.css'
})
export class NotFoundComponent {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  goBack(): void {
    window.history.back();
  }

  goToDashboard(): void {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    if (this.authService.isAdmin()) {
      this.router.navigate(['/admin/rental-companies']);
    } else {
      this.router.navigate(['/customer/vehicles']);
    }
  }
}
