import { Routes } from '@angular/router';

export const relationshipRoutes: Routes = [
  {
    path: 'create',
    loadComponent: () => import('./components/relationship-create/relationship-create.component').then(c => c.RelationshipCreateComponent)
  }
];