import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { MemberService } from '../../services/member.service';
import { FamilyService } from '../../../family/services/family.service';
import { Member, CreateMemberRequest, UpdateMemberRequest } from '../../models/member.model';
import { Family } from '../../../family/models/family.model';

@Component({
  selector: 'app-member-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './member-form.component.html',
  styleUrl: './member-form.component.scss'
})
export class MemberFormComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  memberForm: FormGroup;
  isEditMode = signal(false);
  isLoading = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  
  member = signal<Member | null>(null);
  families = signal<Family[]>([]);
  
  memberId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private memberService: MemberService,
    private familyService: FamilyService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.memberForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadFamilies();
    this.checkEditMode();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createForm(): FormGroup {
    return this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      middleName: ['', [Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      dateOfBirth: [''],
      dateOfDeath: [''],
      gender: [''],
      placeOfBirth: ['', [Validators.maxLength(100)]],
      placeOfDeath: ['', [Validators.maxLength(100)]],
      biography: ['', [Validators.maxLength(1000)]],
      familyId: ['', [Validators.required]]
    });
  }

  private loadFamilies(): void {
    this.familyService.getFamilies()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.families.set(response.families);
          
          if (response.families.length === 1) {
            this.memberForm.patchValue({ familyId: response.families[0].id });
          }
        },
        error: (error) => {
          console.error('Error loading families:', error);
          this.error.set('Грешка при зареждане на семействата');
        }
      });
  }

  private checkEditMode(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.memberId = parseInt(id);
      this.isEditMode.set(true);
      this.loadMember(this.memberId);
    } else {
      const familyId = this.route.snapshot.queryParamMap.get('familyId');
      if (familyId) {
        this.memberForm.patchValue({ familyId: parseInt(familyId) });
      }
    }
  }

  private loadMember(id: number): void {
    this.isLoading.set(true);
    
    this.memberService.getMember(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (member) => {
          this.member.set(member);
          this.populateForm(member);
          this.isLoading.set(false);
        },
        error: (error) => {
          console.error('Error loading member:', error);
          this.error.set('Грешка при зареждане на члена');
          this.isLoading.set(false);
        }
      });
  }

  private populateForm(member: Member): void {
    this.memberForm.patchValue({
      firstName: member.firstName,
      middleName: member.middleName || '',
      lastName: member.lastName,
      dateOfBirth: member.dateOfBirth ? this.formatDateForInput(member.dateOfBirth) : '',
      dateOfDeath: member.dateOfDeath ? this.formatDateForInput(member.dateOfDeath) : '',
      gender: member.gender || '',
      placeOfBirth: member.placeOfBirth || '',
      placeOfDeath: member.placeOfDeath || '',
      biography: member.biography || '',
      familyId: member.familyId
    });
  }

  private formatDateForInput(dateString: string): string {
    const date = new Date(dateString);
    return date.toISOString().split('T')[0];
  }

  onSubmit(): void {
    if (this.memberForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);
    this.success.set(null);

    const formValue = this.memberForm.value;
    
    const memberData = {
      firstName: formValue.firstName,
      middleName: formValue.middleName || undefined,
      lastName: formValue.lastName,
      dateOfBirth: formValue.dateOfBirth || undefined,
      dateOfDeath: formValue.dateOfDeath || undefined,
      gender: formValue.gender || undefined,
      placeOfBirth: formValue.placeOfBirth || undefined,
      placeOfDeath: formValue.placeOfDeath || undefined,
      biography: formValue.biography || undefined,
      familyId: parseInt(formValue.familyId)
    };

    if (this.isEditMode() && this.memberId) {
      const updateData: UpdateMemberRequest = { ...memberData };
      delete (updateData as any).familyId;

      this.memberService.updateMember(this.memberId, updateData)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (updatedMember) => {
            this.success.set('Членът беше успешно обновен');
            this.isLoading.set(false);
            setTimeout(() => {
              this.router.navigate(['/members', updatedMember.id]);
            }, 1500);
          },
          error: (error) => {
            console.error('Error updating member:', error);
            this.error.set(error.error?.message || 'Грешка при обновяване на члена');
            this.isLoading.set(false);
          }
        });
    } else {
      const createData: CreateMemberRequest = memberData as CreateMemberRequest;

      this.memberService.createMember(createData)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (newMember) => {
            this.success.set('Членът беше успешно създаден');
            this.isLoading.set(false);
            setTimeout(() => {
              this.router.navigate(['/members', newMember.id]);
            }, 1500);
          },
          error: (error) => {
            console.error('Error creating member:', error);
            this.error.set(error.error?.message || 'Грешка при създаване на члена');
            this.isLoading.set(false);
          }
        });
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.memberForm.controls).forEach(key => {
      const control = this.memberForm.get(key);
      control?.markAsTouched();
    });
  }

  onCancel(): void {
    if (this.isEditMode() && this.memberId) {
      this.router.navigate(['/members', this.memberId]);
    } else {
      this.router.navigate(['/members']);
    }
  }

  onReset(): void {
    if (this.isEditMode() && this.member()) {
      this.populateForm(this.member()!);
    } else {
      this.memberForm.reset();
      const familyId = this.route.snapshot.queryParamMap.get('familyId');
      if (familyId) {
        this.memberForm.patchValue({ familyId: parseInt(familyId) });
      }
    }
    this.error.set(null);
    this.success.set(null);
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.memberForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.memberForm.get(fieldName);
    if (field && field.errors) {
      if (field.errors['required']) {
        return 'Това поле е задължително';
      }
      if (field.errors['maxlength']) {
        return `Максимална дължина: ${field.errors['maxlength'].requiredLength} символа`;
      }
    }
    return '';
  }

  onBirthDateChange(): void {
    const birthDate = this.memberForm.get('dateOfBirth')?.value;
    const deathDate = this.memberForm.get('dateOfDeath')?.value;
    
    if (birthDate && deathDate) {
      const birth = new Date(birthDate);
      const death = new Date(deathDate);
      
      if (birth > death) {
        this.memberForm.get('dateOfDeath')?.setErrors({ invalidDate: true });
      } else {
        const deathControl = this.memberForm.get('dateOfDeath');
        if (deathControl?.errors?.['invalidDate']) {
          delete deathControl.errors['invalidDate'];
          if (Object.keys(deathControl.errors).length === 0) {
            deathControl.setErrors(null);
          }
        }
      }
    }
  }

  onDeathDateChange(): void {
    this.onBirthDateChange();
  }

  getSelectedFamilyName(): string {
    const selectedFamilyId = this.memberForm.get('familyId')?.value;
    if (!selectedFamilyId) return '';
    
    const family = this.families().find(f => f.id === parseInt(selectedFamilyId));
    return family?.name || '';
  }
}