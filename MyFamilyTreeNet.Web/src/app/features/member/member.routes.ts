import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';

export const memberRoutes: Routes = [
  {
    path: '',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./components/member-list/member-list.component').then(m => m.MemberListComponent),
        title: 'Членове - MyFamilyTreeNet'
      },
      {
        path: 'create',
        loadComponent: () => import('./components/member-form/member-form.component').then(m => m.MemberFormComponent),
        title: 'Добавяне на член - MyFamilyTreeNet'
      },
      {
        path: ':id/edit',
        loadComponent: () => import('./components/member-form/member-form.component').then(m => m.MemberFormComponent),
        title: 'Редактиране на член - MyFamilyTreeNet'
      }
    ]
  }
];