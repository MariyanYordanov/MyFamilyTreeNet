import { Routes } from '@angular/router';

export const routes: Routes = [
    {
  path: 'families',
  loadChildren: () => import('./features/family/family.routes').then(m => m.familyRoutes)
}
];
