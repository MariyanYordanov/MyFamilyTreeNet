import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, Observable, takeUntil, debounceTime, switchMap, startWith, catchError, of } from 'rxjs';

import { FamilyService } from '../../../../core/services/family.service';
import { Family } from '../../../../core/models/family.interface';

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

  families: Family[] = [];
  filteredFamilies$!: Observable<Family[]>;
  isLoading = true;
  error: string | null = null;
  searchTerm = '';

  constructor(private familyService: FamilyService) {}

  ngOnInit(): void {
    this.loadFamilies();
    this.setupSearch();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadFamilies(): void {
    this.isLoading = true;
    this.error = null;
    
    this.familyService.getFamilies()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (families) => {
          this.families = families;
          this.isLoading = false;
        },
        error: (error) => {
          this.error = 'Възникна грешка при зареждането на семействата.';
          this.isLoading = false;
          console.error('Error loading families:', error);
        }
      });
  }

  setupSearch(): void {
    this.filteredFamilies$ = this.searchSubject.pipe(
      startWith(''),
      debounceTime(300),
      switchMap(searchTerm => {
        const filtered = this.families.filter(family =>
          family.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
          (family.description && family.description.toLowerCase().includes(searchTerm.toLowerCase()))
        );
        return of(filtered);
      }),
      catchError(() => of(this.families))
    );
  }

  onSearchChange(event: any): void {
    this.searchTerm = event.target.value;
    this.searchSubject.next(this.searchTerm);
  }

  trackByFamilyId(index: number, family: Family): number {
    return family.id;
  }
}