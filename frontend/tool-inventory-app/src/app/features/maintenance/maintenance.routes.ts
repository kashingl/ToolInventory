import { Routes } from '@angular/router';

export const MAINTENANCE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./maintenance-list/maintenance-list.component').then(m => m.MaintenanceListComponent)
  }
];
