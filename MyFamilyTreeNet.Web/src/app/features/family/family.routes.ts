import { Routes } from '@angular/router';

export const familyRoutes: Routes = [
  {
    path: '',
    redirectTo: 'catalog',
    pathMatch: 'full'
  },
  {
    path: 'catalog',
    loadComponent: () => import('./components/family-catalog/family-catalog').then(c => c.FamilyCatalogComponent)
  }
];