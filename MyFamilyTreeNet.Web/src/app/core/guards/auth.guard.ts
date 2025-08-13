import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { CanActivateFn, CanMatchFn } from '@angular/router';
import { map, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Check authentication status directly - this is more reliable
  const isAuthenticated = authService.isAuthenticated();
  console.log('Auth guard - isAuthenticated:', isAuthenticated);
  console.log('Auth guard - token:', authService.getToken());
  console.log('Auth guard - current user:', authService.getCurrentUser());
  
  if (!isAuthenticated) {
    console.log('Auth guard - redirecting to login, returnUrl:', state.url);
    router.navigate(['/auth/login'], { 
      queryParams: { returnUrl: state.url } 
    });
    return false;
  }
  
  console.log('Auth guard - access granted');
  return true;
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