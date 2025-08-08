import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { CanActivateFn, CanMatchFn } from '@angular/router';
import { map, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.currentUser$.pipe(
    take(1),
    map(user => {
      const isAuthenticated = authService.isAuthenticated();
      if (!isAuthenticated || !user) {
        // Store the attempted URL for redirecting after login
        router.navigate(['/auth/login'], { 
          queryParams: { returnUrl: state.url } 
        });
        return false;
      }
      return true;
    })
  );
};

export const authMatchGuard: CanMatchFn = (route, segments) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.currentUser$.pipe(
    take(1),
    map(user => {
      const isAuthenticated = authService.isAuthenticated();
      if (!isAuthenticated || !user) {
        // For route matching, we don't redirect, just prevent the route from matching
        return false;
      }
      return true;
    })
  );
};

export const guestGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.currentUser$.pipe(
    take(1),
    map(user => {
      const isAuthenticated = authService.isAuthenticated();
      if (isAuthenticated && user) {
        // If user is already logged in, redirect to dashboard or home
        router.navigate(['/']);
        return false;
      }
      return true;
    })
  );
};