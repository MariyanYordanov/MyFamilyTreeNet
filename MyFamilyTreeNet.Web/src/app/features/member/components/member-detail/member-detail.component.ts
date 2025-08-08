import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Subject, takeUntil, switchMap } from 'rxjs';
import { MemberService } from '../../services/member.service';
import { Member, MemberRelationships } from '../../models/member.model';
import { DateFormatPipe, NameFormatPipe } from '../../../../shared/pipes';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, DateFormatPipe, NameFormatPipe],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.scss'
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  member = signal<Member | null>(null);
  relationships = signal<MemberRelationships | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);
  
  memberId: number | null = null;

  constructor(
    private route: ActivatedRoute,
    private memberService: MemberService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => {
        const id = params.get('id');
        if (id) {
          this.memberId = parseInt(id);
          this.loadMemberData(this.memberId);
        } else {
          this.error.set('Invalid member ID');
        }
        return [];
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      error: (error) => {
        console.error('Error loading member:', error);
        this.error.set('Грешка при зареждане на члена');
        this.loading.set(false);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadMemberData(memberId: number) {
    this.loading.set(true);
    this.error.set(null);

    return this.memberService.getMember(memberId).pipe(
      switchMap(member => {
        this.member.set(member);
        return this.memberService.getMemberRelationships(memberId);
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (relationships) => {
        this.relationships.set(relationships);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading member data:', error);
        this.error.set('Грешка при зареждане на данните за члена');
        this.loading.set(false);
      }
    });
  }

  onDeleteMember(): void {
    if (!this.member()) return;
    
    const memberName = this.member()!.firstName + ' ' + this.member()!.lastName;
    if (confirm(`Сигурни ли сте, че искате да изтриете ${memberName}?`)) {
      this.memberService.deleteMember(this.member()!.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            window.history.back();
          },
          error: (error) => {
            console.error('Error deleting member:', error);
            this.error.set('Грешка при изтриване на члена');
          }
        });
    }
  }

  getRelationshipTypeText(type: number): string {
    const types = {
      0: 'Родител',
      1: 'Дете',
      2: 'Съпруг/а',
      3: 'Брат/Сестра',
      4: 'Дядо/Баба',
      5: 'Внук/Внучка',
      6: 'Друго'
    };
    return types[type as keyof typeof types] || 'Неизвестно';
  }

  getLifeSpan(): string {
    const member = this.member();
    if (!member) return '';
    
    if (member.dateOfBirth && member.dateOfDeath) {
      const birth = new Date(member.dateOfBirth).getFullYear();
      const death = new Date(member.dateOfDeath).getFullYear();
      return `${birth} - ${death}`;
    } else if (member.dateOfBirth) {
      const birth = new Date(member.dateOfBirth).getFullYear();
      return `р. ${birth}`;
    }
    return '';
  }

  getAgeText(): string {
    const member = this.member();
    if (!member) return '';
    
    if (member.age !== undefined) {
      return member.dateOfDeath 
        ? `${member.age} години (починал)`
        : `${member.age} години`;
    }
    return '';
  }
}