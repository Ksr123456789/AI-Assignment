import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface tableColumn<T> {
  key?: keyof T | string;
  label: string;
  sortable?: boolean;
  sortKey?: string;
  type?: 'text' | 'actions' | 'image';
  format?: (value: any) => string;
}

export type TableColumn<T> = tableColumn<T>;

export interface tableAction<T> {
  label: string;
  action: string;
  disabled?: (row: T) => boolean;
  showIf?: (row: T) => boolean;
  class?: string;
}

export type TableAction<T> = tableAction<T>;

@Component({
  selector: 'app-generic-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './generic-table.html',
  styleUrl: './generic-table.css',
})
export class GenericTable<T> {
  data = input.required<T[]>();
  columns = input.required<tableColumn<T>[]>();
  actions = input<tableAction<T>[]>();

  sortBy = input<string>();
  sortDirection = input<'desc' | 'asc'>('asc');
  sort = output<string>();
  action = output<{ action: string; row: T }>();

  onSort(column: tableColumn<T>) {
    if (column.sortable) {
      const key = (column.sortKey ?? column.key) as string;
      this.sort.emit(key);
    }
  }

  onAction(action: string, row: T) {
    this.action.emit({ action, row });
  }

  getCellValue(column: tableColumn<T>, row: T) {
    if (!column.key) {
      return '';
    }
    const value = (row as any)[column.key];

    if (column.format) {
      return column.format(value);
    }
    return value ?? '';
  }

  isStatusColumn(column: tableColumn<T>): boolean {
    const key = String(column.key ?? '').toLowerCase();
    const label = String(column.label ?? '').toLowerCase();
    return key.includes('status') || label.includes('status');
  }

  getStatusBadgeClass(val: any): string {
    const v = String(val ?? '').toLowerCase().trim();
    if (v === 'active' || v === 'available' || v === 'completed') {
      return 'bg-emerald-500/15 text-emerald-300 border-emerald-500/30';
    }
    if (v === 'inactive' || v === 'unavailable' || v === 'cancelled' || v === 'rejected') {
      return 'bg-rose-500/15 text-rose-300 border-rose-500/30';
    }
    if (v === 'pending' || v === 'in maintenance') {
      return 'bg-amber-500/15 text-amber-300 border-amber-500/30';
    }
    if (v === 'confirmed') {
      return 'bg-indigo-500/15 text-indigo-300 border-indigo-500/30';
    }
    return 'bg-slate-700/40 text-slate-300 border-slate-600/40';
  }
}

export const GenericTableComponent = GenericTable;
