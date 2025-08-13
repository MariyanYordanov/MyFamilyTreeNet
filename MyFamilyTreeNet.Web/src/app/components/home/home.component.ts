import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

import { FamilyService } from '../../features/family/services/family.service';
import { AuthService } from '../../core/services/auth.service';
import { Family } from '../../features/family/models/family.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  families: Family[] = [];
  isLoading = true;
  error: string | null = null;
  isLoggedIn = false;
  currentUser: any = null;

  constructor(
    private familyService: FamilyService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.checkAuthStatus();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private checkAuthStatus(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.isLoggedIn = !!user;
        this.currentUser = user;
        this.loadFamiliesForHome(); // Load families after auth status is determined
      });
  }

  private loadFamiliesForHome(): void {
    this.isLoading = true;
    this.error = null;

    if (this.isLoggedIn && this.currentUser) {
      // Логнат потребител - покажи неговите семейства
      this.familyService.getUserFamilies(this.currentUser.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            this.families = response.families || [];
            this.isLoading = false;
          },
          error: (error) => {
            this.error = 'Възникна грешка при зареждането на вашите семейства.';
            this.isLoading = false;
            console.error('Error loading user families:', error);
          }
        });
    } else {
      // Неавторизиран потребител - покажи последните 3 семейства
      this.familyService.getFamilies()
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            // Вземи последните 3 семейства (сортирани по дата на създаване)
            const allFamilies = response.families || [];
            this.families = allFamilies
              .sort((a, b) => new Date(b.createdAt || '').getTime() - new Date(a.createdAt || '').getTime())
              .slice(0, 3);
            this.isLoading = false;
          },
          error: (error) => {
            this.error = 'Възникна грешка при зареждането на семействата.';
            this.isLoading = false;
            console.error('Error loading latest families:', error);
          }
        });
    }
  }

  trackByFamilyId(_index: number, family: Family): number {
    return family.id;
  }

  // Public method to reload families (called from template)
  reloadFamilies(): void {
    this.loadFamiliesForHome();
  }
}