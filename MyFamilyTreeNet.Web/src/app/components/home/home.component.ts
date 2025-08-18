import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject, takeUntil, forkJoin } from 'rxjs';

import { FamilyService } from '../../features/family/services/family.service';
import { AuthService } from '../../core/services/auth.service';
import { StatisticsService, PlatformStatistics } from '../../core/services/statistics.service';
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
  statistics: PlatformStatistics = {
    totalFamilies: 0,
    totalMembers: 0,
    totalStories: 0
  };

  constructor(
    private familyService: FamilyService,
    private authService: AuthService,
    private statisticsService: StatisticsService
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

    // Load statistics and featured families in parallel
    forkJoin({
      statistics: this.statisticsService.getPlatformStatistics(),
      families: this.familyService.getFamilies()
    })
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: (data) => {
        this.statistics = data.statistics;
        
        // Get featured families (last 6 public families)
        const allFamilies = data.families.families || [];
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