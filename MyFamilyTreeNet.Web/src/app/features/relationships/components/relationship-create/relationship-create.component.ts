import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { RelationshipService, RelationshipType, CreateRelationshipRequest } from '../../services/relationship.service';
import { MemberService } from '../../../member/services/member.service';
import { Member } from '../../../member/models/member.model';

@Component({
  selector: 'app-relationship-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './relationship-create.component.html',
  styleUrls: ['./relationship-create.component.scss']
})
export class RelationshipCreateComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private relationshipService = inject(RelationshipService);
  private memberService = inject(MemberService);

  relationshipForm: FormGroup;
  primaryMember: Member | null = null;
  availableMembers: Member[] = [];
  relationshipTypes = this.relationshipService.getRelationshipTypes();
  
  isLoading = true;
  isSubmitting = false;
  error: string | null = null;
  primaryMemberId: number = 0;

  constructor() {
    this.relationshipForm = this.fb.group({
      relatedMemberId: ['', Validators.required],
      relationshipType: ['', Validators.required],
      notes: ['', Validators.maxLength(500)]
    });
  }

  ngOnInit() {
    this.primaryMemberId = Number(this.route.snapshot.queryParamMap.get('primaryMemberId'));
    
    if (this.primaryMemberId) {
      this.loadPrimaryMember();
    } else {
      this.error = 'Primary member ID is required';
      this.isLoading = false;
    }
  }

  private loadPrimaryMember() {
    this.memberService.getMember(this.primaryMemberId).subscribe({
      next: (member) => {
        this.primaryMember = member;
        this.loadAvailableMembers();
      },
      error: (error) => {
        console.error('Error loading primary member:', error);
        this.error = 'Failed to load member data';
        this.isLoading = false;
      }
    });
  }

  private loadAvailableMembers() {
    if (!this.primaryMember?.familyId) {
      this.error = 'Member family information not available';
      this.isLoading = false;
      return;
    }

    this.memberService.getMembersByFamily(this.primaryMember.familyId).subscribe({
      next: (members) => {
        // Exclude the primary member from available options
        this.availableMembers = members.filter(m => m.id !== this.primaryMemberId);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading family members:', error);
        this.error = 'Failed to load family members';
        this.isLoading = false;
      }
    });
  }

  onSubmit() {
    if (this.relationshipForm.valid && !this.isSubmitting) {
      this.isSubmitting = true;
      this.error = null;

      const formValue = this.relationshipForm.value;
      const relationshipRequest: CreateRelationshipRequest = {
        primaryMemberId: this.primaryMemberId,
        relatedMemberId: Number(formValue.relatedMemberId),
        relationshipType: Number(formValue.relationshipType) as RelationshipType,
        notes: formValue.notes || undefined
      };

      this.relationshipService.createRelationship(relationshipRequest).subscribe({
        next: () => {
          // Navigate back to member detail page
          this.router.navigate(['/members', this.primaryMemberId]);
        },
        error: (error) => {
          console.error('Error creating relationship:', error);
          this.error = 'Failed to create relationship. Please try again.';
          this.isSubmitting = false;
        }
      });
    }
  }

  onCancel() {
    this.router.navigate(['/members', this.primaryMemberId]);
  }

  getSelectedMember(): Member | null {
    const selectedId = this.relationshipForm.get('relatedMemberId')?.value;
    if (selectedId) {
      return this.availableMembers.find(m => m.id === Number(selectedId)) || null;
    }
    return null;
  }

  getRelationshipPreview(): string {
    const selectedMember = this.getSelectedMember();
    const relationshipType = this.relationshipForm.get('relationshipType')?.value;
    
    if (!selectedMember || !relationshipType || !this.primaryMember) {
      return '';
    }

    const primaryName = `${this.primaryMember.firstName} ${this.primaryMember.lastName}`;
    const relatedName = `${selectedMember.firstName} ${selectedMember.lastName}`;
    const relationshipLabel = this.relationshipService.getRelationshipDisplayName(
      Number(relationshipType) as RelationshipType,
      selectedMember.gender as 'Male' | 'Female'
    );

    return `${primaryName} → ${relationshipLabel} → ${relatedName}`;
  }

  get relatedMemberId() { return this.relationshipForm.get('relatedMemberId'); }
  get relationshipType() { return this.relationshipForm.get('relationshipType'); }
  get notes() { return this.relationshipForm.get('notes'); }
}