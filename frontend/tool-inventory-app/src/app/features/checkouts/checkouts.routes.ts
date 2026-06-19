import { Routes } from '@angular/router';

export const CHECKOUTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./checkouts-list/checkouts-list.component').then(m => m.CheckoutsListComponent)
  }
];
