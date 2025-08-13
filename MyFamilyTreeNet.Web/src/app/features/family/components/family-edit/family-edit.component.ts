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
    if (this.familyId) {
      this.loadFamily();
    } else {
      this.error = 'Invalid family ID';
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

      const updateData = this.familyForm.value;
      
      this.familyService.updateFamily(this.familyId, updateData).subscribe({
        next: () => {
          this.router.navigate(['/families', this.familyId]);
        },
        error: (error) => {
          console.error('Error updating family:', error);
          this.error = 'Failed to update family. Please try again.';
          this.isSubmitting = false;
        }
      });
    }
  }

  onCancel() {
    this.router.navigate(['/families', this.familyId]);
  }

  get name() { return this.familyForm.get('name'); }
  get description() { return this.familyForm.get('description'); }
}