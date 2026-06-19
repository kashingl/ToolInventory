import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'tools', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'tools',
    canActivate: [authGuard],
    loadChildren: () => import('./features/tools/tools.routes').then(m => m.TOOLS_ROUTES)
  },
  {
    path: 'checkouts',
    canActivate: [authGuard],
    loadChildren: () => import('./features/checkouts/checkouts.routes').then(m => m.CHECKOUTS_ROUTES)
  },
  {
    path: 'maintenance',
    canActivate: [authGuard],
    loadChildren: () => import('./features/maintenance/maintenance.routes').then(m => m.MAINTENANCE_ROUTES)
  },
  { path: '**', redirectTo: 'tools' }
];
