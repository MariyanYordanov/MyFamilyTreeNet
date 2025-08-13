import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('./components/home/home.component').then(c => c.HomeComponent)
    },
    {
        path: 'home',
        loadComponent: () => import('./components/home/home.component').then(c => c.HomeComponent)
    },
    {
        path: 'auth',
        loadChildren: () => import('./features/auth/auth.routes').then(m => m.authRoutes)
    },
    {
        path: 'families',
        loadChildren: () => import('./features/family/family.routes').then(m => m.familyRoutes)
    },
    {
        path: 'members',
        loadChildren: () => import('./features/member/member.routes').then(m => m.memberRoutes)
    },
    {
        path: 'relationships',
        loadChildren: () => import('./features/relationships/relationships.routes').then(m => m.relationshipRoutes)
    },
    {
        path: 'profile',
        loadChildren: () => import('./features/profile/profile.routes').then(m => m.profileRoutes)
    },
    {
        path: 'admin',
        loadChildren: () => import('./features/admin/admin.routes').then(m => m.adminRoutes),
        canActivate: [authGuard]
    },
    {
        path: '**',
        redirectTo: ''
    }
];
