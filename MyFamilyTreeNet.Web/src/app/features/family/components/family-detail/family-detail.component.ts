import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Subject, takeUntil, switchMap } from 'rxjs';
import { FamilyService } from '../../services/family.service';
import { MemberService } from '../../../member/services/member.service';
import { Family } from '../../models/family.model';
import { Member } from '../../../member/models/member.model';
import { DateFormatPipe, NameFormatPipe, AgePipe } from '../../../../shared/pipes';
import { FamilyTreeComponent } from '../family-tree/family-tree.component';

@Component({
  selector: 'app-family-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, DateFormatPipe, NameFormatPipe, AgePipe, FamilyTreeComponent],
  templateUrl: './family-detail.component.html',
  styleUrl: './family-detail.component.scss'
})
export class FamilyDetailComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  family = signal<Family | null>(null);
  members = signal<Member[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  
  familyId: number | null = null;
  
  statistics = computed(() => {
    const membersList = this.members();
    const totalMembers = membersList.length;
    const aliveMembers = membersList.filter(m => !m.dateOfDeath).length;
    const deceasedMembers = totalMembers - aliveMembers;
    
    const ages = membersList
      .filter(m => m.age !== undefined)
      .map(m => m.age!);
    const averageAge = ages.length > 0 
      ? Math.round(ages.reduce((sum, age) => sum + age, 0) / ages.length)
      : 0;
    
    const generations = this.calculateGenerations(membersList);
    
    return {
      totalMembers,
      aliveMembers,
      deceasedMembers,
      averageAge,
      generations
    };
  });

  constructor(
    private route: ActivatedRoute,
    private familyService: FamilyService,
    private memberService: MemberService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (params) => {
        const id = params.get('id');
        if (id) {
          this.familyId = parseInt(id);
          this.loadFamilyData(this.familyId);
        } else {
          this.error.set('Невалиден ID на семейство');
        }
      },
      error: (error) => {
        console.error('Error loading family:', error);
        this.error.set('Грешка при зареждане на семейството');
        this.loading.set(false);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadFamilyData(familyId: number): void {
    this.loading.set(true);
    this.error.set(null);

    this.familyService.getFamilyById(familyId).pipe(
      switchMap(family => {
        this.family.set(family);
        return this.memberService.getMembers({ familyId });
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (response) => {
        this.members.set(response.members);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading family data:', error);
        this.error.set('Грешка при зареждане на данните за семейството');
        this.loading.set(false);
      }
    });
  }

  private calculateGenerations(members: Member[]): number {
    return Math.max(...members.map(m => {
      const birthYear = m.dateOfBirth ? new Date(m.dateOfBirth).getFullYear() : new Date().getFullYear();
      return Math.floor((new Date().getFullYear() - birthYear) / 25);
    }), 0) + 1;
  }


  refreshData(): void {
    if (this.familyId) {
      this.loadFamilyData(this.familyId);
    }
  }
}