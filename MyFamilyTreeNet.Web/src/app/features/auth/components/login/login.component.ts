import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { LoginRequest } from '../../../../core/models/user.interface';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})

export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  isLoading = false;
  returnUrl = '/families';
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
      rememberMe: [false]
    });
  }

  ngOnInit(): void {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/families';
    
    // Redirect if already logged in
    if (this.authService.isAuthenticated()) {
      this.router.navigate([this.returnUrl]);
    }
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      const loginData: LoginRequest = this.loginForm.value;

      this.authService.login(loginData)
        .pipe(
          catchError(error => {
            console.error('Login error:', error);
            let errorMessage = 'Грешка при влизане. Моля опитайте отново.';
            
            if (error.status === 401) {
              errorMessage = 'Невалиден имейл или парола.';
            } else if (error.error?.message) {
              errorMessage = error.error.message;
            } else if (error.message) {
              errorMessage = error.message;
            }
            
            alert(errorMessage);
            return of(null);
          }),
          finalize(() => this.isLoading = false)
        )
        .subscribe(response => {
          if (response) {
            console.log('Login successful', response);
            console.log('ReturnUrl:', this.returnUrl);
            
            // Parse the decoded URL properly for navigation
            const decodedUrl = decodeURIComponent(this.returnUrl);
            console.log('Decoded URL:', decodedUrl);
            
            // Split URL into path and query string
            const urlParts = decodedUrl.split('?');
            const path = urlParts[0];
            const queryString = urlParts[1];
            
            if (queryString) {
              // Parse query parameters
              const queryParams: any = {};
              const pairs = queryString.split('&');
              pairs.forEach(pair => {
                const [key, value] = pair.split('=');
                queryParams[key] = decodeURIComponent(value || '');
              });
              
              console.log('Navigating to path:', path, 'with query params:', queryParams);
              this.router.navigate([path], { queryParams }).then(success => {
                console.log('Navigation success:', success);
              }).catch(error => {
                console.error('Navigation error:', error);
              });
            } else {
              console.log('Navigating to path:', path);
              this.router.navigate([path]).then(success => {
                console.log('Navigation success:', success);
              }).catch(error => {
                console.error('Navigation error:', error);
              });
            }
          }
        });
    }
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }
}
