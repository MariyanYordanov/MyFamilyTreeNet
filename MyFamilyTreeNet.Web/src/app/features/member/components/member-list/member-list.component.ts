import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject, takeUntil, debounceTime, distinctUntilChanged, switchMap, startWith, combineLatest, map } from 'rxjs';
import { MemberService } from '../../services/member.service';
import { FamilyService } from '../../../../core/services/family.service';
import { Member, MemberSearchParams } from '../../models/member.model';
import { Family } from '../../../../core/models/family.interface';

@Component({
  selector: 'app-member-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './member-list.component.html',
  styleUrl: './member-list.component.scss'
})
export class MemberListComponent implements OnInit, OnDestroy {
  Math = Math;
  parseInt = parseInt;
  private destroy$ = new Subject<void>();
  
  members = signal<Member[]>([]);
  families = signal<Family[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  
  searchTerm = signal('');
  selectedFamilyId = signal<number | null>(null);
  currentPage = signal(1);
  pageSize = signal(20);
  totalCount = signal(0);
  sortBy = signal<'name' | 'age' | 'birthDate'>('name');
  sortDirection = signal<'asc' | 'desc'>('asc');
  showAliveOnly = signal(false);
  
  totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize()));
  hasMembers = computed(() => this.members().length > 0);
  canGoNext = computed(() => this.currentPage() < this.totalPages());
  canGoPrevious = computed(() => this.currentPage() > 1);
  
  aliveCount = computed(() => this.members().filter(m => !m.dateOfDeath).length);
  deceasedCount = computed(() => this.members().filter(m => !!m.dateOfDeath).length);
  averageAge = computed(() => {
    const membersWithAge = this.members().filter(m => m.age !== undefined);
    if (membersWithAge.length === 0) return 0;
    const sum = membersWithAge.reduce((acc, m) => acc + (m.age || 0), 0);
    return Math.round(sum / membersWithAge.length);
  });

  constructor(
    private memberService: MemberService,
    private familyService: FamilyService
  ) {}

  ngOnInit(): void {
    this.loadFamilies();
    this.setupSearch();
    this.loadMembers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadFamilies(): void {
    this.familyService.getFamilies()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (families) => this.families.set(families),
        error: (error) => {
          console.error('Error loading families:', error);
          this.error.set('Грешка при зареждане на семействата');
        }
      });
  }

  private setupSearch(): void {
    const searchSubject = new Subject<string>();
    
    searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(searchTerm => {
      this.searchTerm.set(searchTerm);
      this.currentPage.set(1);
      this.loadMembers();
    });

    (this as any).onSearchChange = (value: string) => searchSubject.next(value);
  }

  private loadMembers(): void {
    this.loading.set(true);
    this.error.set(null);

    const params: MemberSearchParams = {
      search: this.searchTerm() || undefined,
      familyId: this.selectedFamilyId() || undefined,
      page: this.currentPage(),
      pageSize: this.pageSize()
    };

    this.memberService.getMembers(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          let members = response.members;
          
          if (this.showAliveOnly()) {
            members = members.filter(m => !m.dateOfDeath);
          }
          
          members = this.sortMembers(members);
          
          this.members.set(members);
          this.totalCount.set(response.totalCount);
          this.loading.set(false);
        },
        error: (error) => {
          console.error('Error loading members:', error);
          this.error.set('Грешка при зареждане на членовете');
          this.loading.set(false);
        }
      });
  }

  private sortMembers(members: Member[]): Member[] {
    const sorted = [...members];
    const direction = this.sortDirection() === 'asc' ? 1 : -1;

    switch (this.sortBy()) {
      case 'name':
        return sorted.sort((a, b) => 
          `${a.firstName} ${a.lastName}`.localeCompare(`${b.firstName} ${b.lastName}`, 'bg') * direction
        );
      case 'age':
        return sorted.sort((a, b) => ((a.age || 0) - (b.age || 0)) * direction);
      case 'birthDate':
        return sorted.sort((a, b) => {
          if (!a.dateOfBirth) return 1;
          if (!b.dateOfBirth) return -1;
          return (new Date(a.dateOfBirth).getTime() - new Date(b.dateOfBirth).getTime()) * direction;
        });
      default:
        return sorted;
    }
  }

  onSearchChange(value: string): void {
  }

  onFamilyFilterChange(familyId: string): void {
    this.selectedFamilyId.set(familyId ? parseInt(familyId) : null);
    this.currentPage.set(1);
    this.loadMembers();
  }

  onSortChange(sortBy: 'name' | 'age' | 'birthDate'): void {
    if (this.sortBy() === sortBy) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(sortBy);
      this.sortDirection.set('asc');
    }
    this.loadMembers();
  }

  onAliveFilterChange(showAliveOnly: boolean): void {
    this.showAliveOnly.set(showAliveOnly);
    this.loadMembers();
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
    this.loadMembers();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      this.loadMembers();
    }
  }

  nextPage(): void {
    if (this.canGoNext()) {
      this.goToPage(this.currentPage() + 1);
    }
  }

  previousPage(): void {
    if (this.canGoPrevious()) {
      this.goToPage(this.currentPage() - 1);
    }
  }

  onDeleteMember(member: Member): void {
    if (confirm(`Сигурни ли сте, че искате да изтриете ${member.firstName} ${member.lastName}?`)) {
      this.memberService.deleteMember(member.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.loadMembers();
          },
          error: (error) => {
            console.error('Error deleting member:', error);
            this.error.set('Грешка при изтриване на члена');
          }
        });
    }
  }

  clearFilters(): void {
    this.searchTerm.set('');
    this.selectedFamilyId.set(null);
    this.showAliveOnly.set(false);
    this.currentPage.set(1);
    this.loadMembers();
  }

  getAgeDisplay(member: Member): string {
    if (member.age === undefined) return 'Неизвестна';
    return member.dateOfDeath ? `${member.age} г. (починал)` : `${member.age} г.`;
  }

  getFullName(member: Member): string {
    return `${member.firstName} ${member.middleName || ''} ${member.lastName}`.replace(/\s+/g, ' ').trim();
  }

  formatDate(dateString?: string): string {
    if (!dateString) return 'Неизвестна';
    return new Date(dateString).toLocaleDateString('bg-BG');
  }
}