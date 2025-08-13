import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';

export const memberRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/member-list/member-list.component').then(m => m.MemberListComponent),
    title: 'Членове - MyFamilyTreeNet',
    canActivate: [authGuard]
  },
  {
    path: 'create',
    loadComponent: () => import('./components/member-form/member-form.component').then(m => m.MemberFormComponent),
    title: 'Добавяне на член - MyFamilyTreeNet',
    canActivate: [authGuard]
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./components/member-form/member-form.component').then(m => m.MemberFormComponent),
    title: 'Редактиране на член - MyFamilyTreeNet',
    canActivate: [authGuard]
  },
  {
    path: ':id',
    loadComponent: () => import('./components/member-detail/member-detail.component').then(m => m.MemberDetailComponent),
    title: 'Детайли за член - MyFamilyTreeNet',
    canActivate: [authGuard]
  }
];