import { Component, Input, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { Member, RelationshipType, CreateRelationshipRequest } from '../../models/member.model';
import { MemberService } from '../../services/member.service';
import { catchError, finalize, switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
// import { NameFormatPipe, AgePipe } from '../../../../shared/pipes';

interface Relationship {
  id: number;
  primaryMemberId: number;
  relatedMemberId: number;
  relationshipType: RelationshipType;
  notes?: string;
  primaryMember: Member;
  relatedMember: Member;
}


@Component({
  selector: 'app-member-relationships',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './member-relationships.component.html',
  styleUrl: './member-relationships.component.css'
})
export class MemberRelationshipsComponent implements OnInit {
  @Input() currentMember = signal<Member | null>(null);
  @Input() familyId!: number;

  relationshipForm!: FormGroup;
  showAddForm = signal(false);
  relationships = signal<Relationship[]>([]);
  availableMembers = signal<Member[]>([]);
  
  isLoading = signal(false);
  isLoadingRelationships = signal(false);
  successMessage = signal('');
  errorMessage = signal('');
  editingRelationshipId: number | null = null;

  relationshipTypes = [
    { value: RelationshipType.Parent, label: 'Родител' },
    { value: RelationshipType.Child, label: 'Дете' },
    { value: RelationshipType.Spouse, label: 'Съпруг/а' },
    { value: RelationshipType.Sibling, label: 'Брат/Сестра' },
    { value: RelationshipType.Grandparent, label: 'Дядо/Баба' },
    { value: RelationshipType.Grandchild, label: 'Внук/Внучка' },
    { value: RelationshipType.Uncle, label: 'Чичо' },
    { value: RelationshipType.Aunt, label: 'Леля' },
    { value: RelationshipType.Cousin, label: 'Братовчед/Сестричина' },
    { value: RelationshipType.Nephew, label: 'Племенник' },
    { value: RelationshipType.Niece, label: 'Племенница' },
    { value: RelationshipType.Other, label: 'Друго' }
  ];

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private memberService: MemberService
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    if (this.currentMember()) {
      this.loadRelationships();
      this.loadAvailableMembers();
    }
  }

  private initializeForm(): void {
    this.relationshipForm = this.fb.group({
      relatedMemberId: ['', Validators.required],
      relationshipType: ['', Validators.required],
      notes: ['', Validators.maxLength(500)]
    });
  }

  private loadRelationships(): void {
    const member = this.currentMember();
    if (!member) return;

    this.isLoadingRelationships.set(true);
    
    this.http.get<Relationship[]>(`/api/Relationship/member/${member.id}`)
      .pipe(
        catchError(error => {
          console.error('Error loading relationships:', error);
          return of([]);
        }),
        finalize(() => this.isLoadingRelationships.set(false))
      )
      .subscribe(relationships => {
        this.relationships.set(relationships);
      });
  }

  private loadAvailableMembers(): void {
    this.memberService.filterMembersByFamily(this.familyId)
      .pipe(
        catchError(error => {
          console.error('Error loading available members:', error);
          return of([]);
        })
      )
      .subscribe(members => {
        const currentMemberId = this.currentMember()?.id;
        const available = members.filter(m => m.id !== currentMemberId);
        this.availableMembers.set(available);
      });
  }

  createRelationship(): void {
    if (this.relationshipForm.invalid) return;

    const member = this.currentMember();
    if (!member) return;

    this.isLoading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    const formValue = this.relationshipForm.value;
    
    if (this.editingRelationshipId) {
      // Update existing relationship
      const updateRequest = {
        RelationshipType: parseInt(formValue.relationshipType),
        Notes: formValue.notes || undefined
      };
      
      this.http.put<Relationship>(`/api/Relationship/${this.editingRelationshipId}`, updateRequest)
        .pipe(
          catchError(error => {
            let errorMsg = 'Грешка при редактирането на връзката';
            if (error.error?.message) {
              errorMsg = error.error.message;
            }
            this.errorMessage.set(errorMsg);
            console.error('Error updating relationship:', error);
            return of(null);
          }),
          finalize(() => this.isLoading.set(false))
        )
        .subscribe(relationship => {
          if (relationship) {
            this.successMessage.set('Семейната връзка е редактирана успешно!');
            this.relationshipForm.reset();
            this.showAddForm.set(false);
            this.editingRelationshipId = null;
            this.loadRelationships();
          }
        });
    } else {
      // Create new relationship
      const request: CreateRelationshipRequest = {
        PrimaryMemberId: member.id,
        RelatedMemberId: parseInt(formValue.relatedMemberId),
        RelationshipType: parseInt(formValue.relationshipType),
        Notes: formValue.notes || undefined
      };

      this.http.post<Relationship>(`/api/Relationship`, request)
      .pipe(
        catchError(error => {
          let errorMsg = 'Грешка при създаването на връзката';
          if (error.error?.message) {
            errorMsg = error.error.message;
          }
          this.errorMessage.set(errorMsg);
          console.error('Error creating relationship:', error);
          return of(null);
        }),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe(relationship => {
          if (relationship) {
            this.successMessage.set('Семейната връзка е създадена успешно!');
            this.relationshipForm.reset();
            this.showAddForm.set(false);
            this.loadRelationships();
          }
        });
    }
  }

  editRelationship(relationship: Relationship): void {
    // Pre-fill form with existing relationship data
    this.relationshipForm.patchValue({
      relatedMemberId: relationship.relatedMemberId.toString(),
      relationshipType: relationship.relationshipType.toString(),
      notes: relationship.notes || ''
    });
    
    // Store the relationship being edited
    this.editingRelationshipId = relationship.id;
    this.showAddForm.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  confirmDeleteRelationship(relationship: Relationship): void {
    if (confirm(`Сигурни ли сте, че искате да изтриете връзката с ${this.getRelatedMember(relationship).firstName} ${this.getRelatedMember(relationship).lastName}?`)) {
      this.deleteRelationship(relationship);
    }
  }

  private deleteRelationship(relationship: Relationship): void {
    this.http.delete(`/api/Relationship/${relationship.id}`)
      .pipe(
        catchError(error => {
          this.errorMessage.set('Грешка при изтриването на връзката');
          console.error('Error deleting relationship:', error);
          return of(null);
        })
      )
      .subscribe(result => {
        if (result !== null) {
          this.successMessage.set('Семейната връзка е изтрита успешно!');
          this.loadRelationships(); // Reload relationships
        }
      });
  }

  cancelAdd(): void {
    this.relationshipForm.reset();
    this.showAddForm.set(false);
    this.errorMessage.set('');
    this.successMessage.set('');
    this.editingRelationshipId = null;
  }

  getRelatedMember(relationship: Relationship): Member {
    const currentMemberId = this.currentMember()?.id;
    return relationship.primaryMemberId === currentMemberId 
      ? relationship.relatedMember 
      : relationship.primaryMember;
  }

  getRelationshipTypeLabel(type: RelationshipType): string {
    const typeObj = this.relationshipTypes.find(t => t.value === type);
    return typeObj?.label || 'Неизвестно';
  }

  getFullName(member: Member): string {
    const parts = [member.firstName, member.middleName, member.lastName].filter(Boolean);
    return parts.join(' ');
  }

  getAge(member: Member): string {
    if (!member.dateOfBirth) return '';
    
    const birthDate = new Date(member.dateOfBirth);
    const deathDate = member.dateOfDeath ? new Date(member.dateOfDeath) : new Date();
    
    const age = deathDate.getFullYear() - birthDate.getFullYear();
    const monthDiff = deathDate.getMonth() - birthDate.getMonth();
    
    if (monthDiff < 0 || (monthDiff === 0 && deathDate.getDate() < birthDate.getDate())) {
      return (age - 1).toString();
    }
    
    return age.toString() + (member.dateOfDeath ? ' (починал)' : '');
  }
}