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

  featuredFamilies: Family[] = [];
  isLoading = true;
  error: string | null = null;
  isLoggedIn = false;
  currentUser: any = null;
  statistics = {
    totalFamilies: 0,
    totalMembers: 0,
    totalStories: 0
  };

  constructor(
    private familyService: FamilyService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.checkAuthStatus();
    this.loadData();
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
      });
  }

  private loadData(): void {
    this.isLoading = true;
    this.error = null;

    // Load families and use default statistics
    this.familyService.getFamilies()
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: (data) => {
        // Set default statistics for now
        this.statistics = {
          totalFamilies: data.families?.length || 0,
          totalMembers: 0,
          totalStories: 0
        };
        
        // Get featured families (last 6 public families)
        const allFamilies = data.families || [];
        this.featuredFamilies = allFamilies
          .filter(f => f.isPublic !== false) // Only public families
          .sort((a, b) => new Date(b.createdAt || '').getTime() - new Date(a.createdAt || '').getTime())
          .slice(0, 6);
        
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading home page data:', error);
        this.error = 'Възникна грешка при зареждането на данните.';
        this.isLoading = false;
        
        // Set default values on error
        this.statistics = {
          totalFamilies: 0,
          totalMembers: 0,
          totalStories: 0
        };
      }
    });
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return '';
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    return dateObj.toLocaleDateString('bg-BG', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
  }

  trackByFamilyId(_index: number, family: Family): number {
    return family.id;
  }
}