import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { FamilyService } from '../../services/family.service';
import { Family } from '../../models/family.model';

@Component({
  selector: 'app-family-edit',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './family-edit.component.html',
  styleUrls: ['./family-edit.component.scss']
})
export class FamilyEditComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private familyService = inject(FamilyService);

  familyForm: FormGroup;
  family: Family | null = null;
  isLoading = true;
  isSubmitting = false;
  error: string | null = null;
  familyId: number = 0;

  constructor() {
    this.familyForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)],
      isPublic: [true]
    });
  }

  ngOnInit() {
    this.familyId = Number(this.route.snapshot.paramMap.get('id'));
    console.log('FamilyEditComponent - familyId:', this.familyId);
    
    if (this.familyId && !isNaN(this.familyId)) {
      // Edit mode - load existing family
      console.log('Edit mode - loading family');
      this.loadFamily();
    } else {
      // Create mode - no family ID
      console.log('Create mode - no family ID');
      this.familyId = 0;
      this.isLoading = false;
    }
  }

  private loadFamily() {
    this.familyService.getFamilyById(this.familyId).subscribe({
      next: (family) => {
        this.family = family;
        this.familyForm.patchValue({
          name: family.name,
          description: family.description || '',
          isPublic: family.isPublic
        });
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading family:', error);
        this.error = 'Failed to load family data';
        this.isLoading = false;
      }
    });
  }

  onSubmit() {
    if (this.familyForm.valid && !this.isSubmitting) {
      this.isSubmitting = true;
      this.error = null;

      const formData = this.familyForm.value;
      
      if (this.familyId > 0) {
        // Edit existing family
        this.familyService.updateFamily(this.familyId, formData).subscribe({
          next: () => {
            this.router.navigate(['/families', this.familyId]);
          },
          error: (error) => {
            console.error('Error updating family:', error);
            this.error = 'Failed to update family';
            this.isSubmitting = false;
          }
        });
      } else {
        // Create new family
        console.log('Creating family with data:', formData);
        this.familyService.createFamily(formData).subscribe({
          next: (newFamily) => {
            console.log('Family created successfully:', newFamily);
            this.router.navigate(['/families', newFamily.id]);
          },
          error: (error) => {
            console.error('Error creating family:', error);
            console.error('Error details:', error.error);
            this.error = error.error?.message || 'Failed to create family. Please try again.';
            this.isSubmitting = false;
          }
        });
      }
    }
  }

  onCancel() {
    if (this.familyId > 0) {
      this.router.navigate(['/families', this.familyId]);
    } else {
      this.router.navigate(['/families']);
    }
  }

  get name() { return this.familyForm.get('name'); }
  get description() { return this.familyForm.get('description'); }
}