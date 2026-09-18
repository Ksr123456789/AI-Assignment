import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { CustomerSidebarComponent } from '../customer-sidebar/customer-sidebar.component';

@Component({
  selector: 'app-customer-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, CustomerSidebarComponent],
  templateUrl: './customer-layout.component.html',
  styleUrl: './customer-layout.component.css'
})
export class CustomerLayoutComponent {
  isMobileSidebarOpen = signal<boolean>(false);
}

export const customerLayout = CustomerLayoutComponent;
