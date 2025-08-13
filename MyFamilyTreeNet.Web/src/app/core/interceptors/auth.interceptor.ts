import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  console.log('Auth interceptor - request URL:', req.url);
  console.log('Auth interceptor - token exists:', !!token);
  console.log('Auth interceptor - token value:', token?.substring(0, 20) + '...');

  if (token) {
    const cloned = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
    console.log('Auth interceptor - Added Bearer token to request');
    return next(cloned);
  }

  console.log('Auth interceptor - No token, sending request without auth');
  return next(req);
};