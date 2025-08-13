import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { User } from '../../core/models/user.interface';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

interface UpdateProfileRequest {
  firstName: string;
  middleName: string;
  lastName: string;
  dateOfBirth?: Date;
  bio?: string;
}

interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

@Component({
  selector: 'app-profile',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.component.html'
})
export class ProfileComponent implements OnInit {
  profileForm!: FormGroup;
  passwordForm!: FormGroup;
  
  currentUser = signal<User | null>(null);
  isLoading = signal(false);
  isChangingPassword = signal(false);
  successMessage = signal('');
  errorMessage = signal('');
  
  showCurrentPassword = signal(false);
  showNewPassword = signal(false);
  showConfirmPassword = signal(false);

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private http: HttpClient,
    private router: Router
  ) {
    this.initializeForms();
  }

  ngOnInit(): void {
    this.loadUserProfile();
  }

  private initializeForms(): void {
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      middleName: ['', [Validators.required, Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      dateOfBirth: [''],
      bio: ['', Validators.maxLength(1000)]
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });
  }

  private passwordMatchValidator(form: FormGroup) {
    const newPassword = form.get('newPassword')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;
    
    if (newPassword !== confirmPassword) {
      form.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    return null;
  }

  private loadUserProfile(): void {
    this.isLoading.set(true);
    
    this.http.get<User>(`${environment.apiUrl}/api/Profile`)
      .pipe(
        catchError(error => {
          this.errorMessage.set('Грешка при зареждането на профила');
          console.error('Error loading profile:', error);
          return of(null);
        }),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe(user => {
        if (user) {
          this.currentUser.set(user);
          this.populateForm(user);
        }
      });
  }

  private populateForm(user: User): void {
    this.profileForm.patchValue({
      firstName: user.firstName,
      middleName: user.middleName,
      lastName: user.lastName,
      dateOfBirth: user.dateOfBirth ? new Date(user.dateOfBirth).toISOString().split('T')[0] : '',
      bio: user.bio || ''
    });
  }

  updateProfile(): void {
    if (this.profileForm.invalid) return;

    this.isLoading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    const formValue = this.profileForm.value;
    const updateRequest: UpdateProfileRequest = {
      firstName: formValue.firstName,
      middleName: formValue.middleName,
      lastName: formValue.lastName,
      dateOfBirth: formValue.dateOfBirth ? new Date(formValue.dateOfBirth) : undefined,
      bio: formValue.bio || undefined
    };

    this.http.put(`${environment.apiUrl}/api/Profile`, updateRequest)
      .pipe(
        catchError(error => {
          this.errorMessage.set('Грешка при актуализирането на профила');
          console.error('Error updating profile:', error);
          return of(null);
        }),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe(response => {
        if (response) {
          this.successMessage.set('Профилът е актуализиран успешно!');
          this.loadUserProfile(); // Reload to get updated data
        }
      });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) return;

    this.isChangingPassword.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    const formValue = this.passwordForm.value;
    const changePasswordRequest: ChangePasswordRequest = {
      currentPassword: formValue.currentPassword,
      newPassword: formValue.newPassword,
      confirmPassword: formValue.confirmPassword
    };

    this.http.post(`${environment.apiUrl}/api/Profile/change-password`, changePasswordRequest)
      .pipe(
        catchError(error => {
          let errorMsg = 'Грешка при промяната на паролата';
          if (error.error?.message) {
            errorMsg = error.error.message;
          } else if (error.status === 400) {
            errorMsg = 'Невалидна текуща парола';
          }
          this.errorMessage.set(errorMsg);
          console.error('Error changing password:', error);
          return of(null);
        }),
        finalize(() => this.isChangingPassword.set(false))
      )
      .subscribe(response => {
        if (response) {
          this.successMessage.set('Паролата е променена успешно!');
          this.passwordForm.reset();
        }
      });
  }

  toggleCurrentPassword(): void {
    this.showCurrentPassword.set(!this.showCurrentPassword());
  }

  toggleNewPassword(): void {
    this.showNewPassword.set(!this.showNewPassword());
  }

  toggleConfirmPassword(): void {
    this.showConfirmPassword.set(!this.showConfirmPassword());
  }

  formatDate(dateString: any): string {
    if (!dateString) return '';
    
    const date = new Date(dateString);
    return date.toLocaleDateString('bg-BG', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}