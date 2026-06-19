import { Routes } from '@angular/router';

export const TOOLS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./tools-list/tools-list.component').then(m => m.ToolsListComponent)
  }
];
