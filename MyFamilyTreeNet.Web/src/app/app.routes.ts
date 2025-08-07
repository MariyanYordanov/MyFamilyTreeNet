import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '',
        redirectTo: '/families',
        pathMatch: 'full'
    },
    {
        path: 'families',
        loadChildren: () => import('./features/family/family.routes').then(m => m.familyRoutes)
    },
    {
        path: 'auth',
        loadChildren: () => import('./features/auth/auth.routes').then(m => m.authRoutes)
    }
];
