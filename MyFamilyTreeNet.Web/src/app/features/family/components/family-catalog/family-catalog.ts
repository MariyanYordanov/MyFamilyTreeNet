import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, Observable, takeUntil, debounceTime, startWith, catchError, of, combineLatest, map, BehaviorSubject } from 'rxjs';

import { FamilyService } from '../../services/family.service';
import { AuthService } from '../../../../core/services/auth.service';
import { Family } from '../../models/family.model';

@Component({
  selector: 'app-family-catalog',
  standalone: true,
  imports: [
    CommonModule, 
    RouterLink, 
    FormsModule
  ],
  templateUrl: './family-catalog.html',
  styleUrl: './family-catalog.css'
})
export class FamilyCatalogComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<string>();
  private familiesSubject = new BehaviorSubject<Family[]>([]);

  families: Family[] = [];
  filteredFamilies$!: Observable<Family[]>;
  isLoading = true;
  error: string | null = null;
  searchTerm = '';
  isLoggedIn = false;
  currentUser: any = null;

  constructor(
    private familyService: FamilyService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.checkAuthStatus();
    this.setupSearch();
  }

  private checkAuthStatus(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.isLoggedIn = !!user;
        this.currentUser = user;
        this.loadFamilies(); // Load families after auth status is determined
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadFamilies(): void {
    this.isLoading = true;
    this.error = null;
    
    const familiesObservable = this.isLoggedIn && this.currentUser 
      ? this.familyService.getUserFamilies(this.currentUser.id)
      : this.familyService.getFamilies();
    
    familiesObservable
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          console.log('Loaded families:', response);
          this.families = response.families || [];
          this.familiesSubject.next(this.families);
          this.isLoading = false;
        },
        error: (error) => {
          this.error = this.isLoggedIn 
            ? 'Възникна грешка при зареждането на вашите семейства.'
            : 'Възникна грешка при зареждането на семействата.';
          this.isLoading = false;
          this.familiesSubject.next([]);
          console.error('Error loading families:', error);
        }
      });
  }

  setupSearch(): void {
    this.filteredFamilies$ = combineLatest([
      this.familiesSubject.asObservable(),
      this.searchSubject.pipe(startWith(''), debounceTime(300))
    ]).pipe(
      map(([families, searchTerm]) => {
        console.log('Filtering families:', families.length, 'searchTerm:', searchTerm);
        if (!families || families.length === 0) {
          return [];
        }
        if (!searchTerm) {
          return families;
        }
        return families.filter(family =>
          family.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
          (family.description && family.description.toLowerCase().includes(searchTerm.toLowerCase()))
        );
      }),
      catchError(() => of([]))
    );
  }

  onSearchChange(event: any): void {
    this.searchTerm = event.target.value;
    this.searchSubject.next(this.searchTerm);
  }

  trackByFamilyId(_index: number, family: Family): number {
    return family.id;
  }
}