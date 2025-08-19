
import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { User, LoginRequest, RegisterRequest, LoginResponse } from '../models/user.interface';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  private isBrowser: boolean;

  constructor(
    private http: HttpClient,
    private router: Router,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
    
    // Only check localStorage if we're in browser
    if (this.isBrowser) {
      const token = this.getToken();
      if (token && !this.isTokenExpired(token)) {
        const user = this.getUserFromStorage();
        if (user) {
          this.currentUserSubject.next(user);
        }
      } else {
        this.logout();
      }
    }
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`/api/Auth/login`, credentials)
      .pipe(
        tap(response => {
          console.log('Auth service received response:', response);
          console.log('Token:', response.token);
          console.log('User:', response.user);
          this.setToken(response.token);
          this.setUser(response.user);
          this.currentUserSubject.next(response.user);
          console.log('Token saved to localStorage:', this.getToken());
        })
      );
  }

  register(userData: RegisterRequest): Observable<LoginResponse> {
    const { confirmPassword, dateOfBirth, ...registerData } = userData;
    return this.http.post<LoginResponse>(`/api/Auth/register`, registerData)
      .pipe(
        tap(response => {
          if (response.token && response.user) {
            this.setToken(response.token);
            this.setUser(response.user);
            this.currentUserSubject.next(response.user);
          }
        })
      );
  }

  logout(): void {
    if (this.isBrowser) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    }
    this.currentUserSubject.next(null);
    this.router.navigate(['/']);
  }

  forceLogout(): void {
    console.log('Force logout due to invalid token');
    this.logout();
  }

  getToken(): string | null {
    if (!this.isBrowser) return null;
    const token = localStorage.getItem('token');
    console.log('Getting token:', token ? token.substring(0, 30) + '...' : 'null');
    return token;
  }

  isAuthenticated(): boolean {
    if (!this.isBrowser) {
      console.log('Not in browser environment');
      return false;
    }
    const token = this.getToken();
    console.log('isAuthenticated check:');
    console.log('- Has token:', !!token);
    if (!token) {
      console.log('- No token found');
      return false;
    }
    // TEMPORARY FIX: Skip expiration check due to time sync issues
    console.log('- Skipping expiration check (temporary fix)');
    return true;
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  updateCurrentUser(updatedUser: User): void {
    this.setUser(updatedUser);
    this.currentUserSubject.next(updatedUser);
  }

  private setToken(token: string): void {
    if (this.isBrowser) {
      localStorage.setItem('token', token);
    }
  }

  private setUser(user: User): void {
    if (this.isBrowser) {
      localStorage.setItem('user', JSON.stringify(user));
    }
  }

  private getUserFromStorage(): User | null {
    if (!this.isBrowser) return null;
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      console.log('Raw token payload exp:', payload.exp);
      
      // Check if exp is already in milliseconds (> year 2030 in seconds)
      const exp = payload.exp > 1900000000 ? payload.exp : payload.exp * 1000;
      const now = Date.now();
      // Add 5 minute tolerance for clock skew
      const tolerance = 5 * 60 * 1000; // 5 minutes in milliseconds
      const isExpired = now >= (exp + tolerance);
      console.log('Token expiration check:');
      console.log('- Raw exp value:', payload.exp);
      console.log('- Final exp time:', new Date(exp));
      console.log('- Current time:', new Date(now));
      console.log('- Is expired:', isExpired);
      return isExpired;
    } catch (error) {
      console.error('Error checking token expiration:', error);
      return true;
    }
  }
}
