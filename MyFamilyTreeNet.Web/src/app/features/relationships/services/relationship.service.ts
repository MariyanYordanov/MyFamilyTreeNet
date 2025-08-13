import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';

export interface Relationship {
  id: number;
  primaryMemberId: number;
  relatedMemberId: number;
  relationshipType: RelationshipType;
  notes?: string;
  createdByUserId: string;
  createdAt: Date;
  primaryMember?: any;
  relatedMember?: any;
}

export enum RelationshipType {
  Parent = 1,
  Child = 2,
  Spouse = 3,
  Sibling = 4,
  Grandparent = 5,
  Grandchild = 6,
  Uncle = 7,
  Aunt = 8,
  Nephew = 9,
  Niece = 10,
  Cousin = 11,
  GreatGrandparent = 12,
  GreatGrandchild = 13,
  StepParent = 14,
  StepChild = 15,
  StepSibling = 16,
  HalfSibling = 17,
  Other = 99
}

export interface CreateRelationshipRequest {
  primaryMemberId: number;
  relatedMemberId: number;
  relationshipType: RelationshipType;
  notes?: string;
}

@Injectable({
  providedIn: 'root'
})
export class RelationshipService {
  private readonly apiUrl = `${environment.apiUrl}/api/Relationship`;
  private http = inject(HttpClient);
  
  private relationshipsSubject = new BehaviorSubject<Relationship[]>([]);
  public relationships$ = this.relationshipsSubject.asObservable();

  // Get relationships for a family
  getRelationshipsByFamily(familyId: number): Observable<Relationship[]> {
    return this.http.get<Relationship[]>(`${this.apiUrl}/family/${familyId}`)
      .pipe(
        tap(relationships => this.relationshipsSubject.next(relationships)),
        catchError(this.handleError<Relationship[]>('getRelationshipsByFamily', []))
      );
  }

  // Get relationships for a specific member
  getRelationshipsByMember(memberId: number): Observable<Relationship[]> {
    return this.http.get<Relationship[]>(`${this.apiUrl}/member/${memberId}`)
      .pipe(
        catchError(this.handleError<Relationship[]>('getRelationshipsByMember', []))
      );
  }

  // Create a new relationship
  createRelationship(relationship: CreateRelationshipRequest): Observable<Relationship> {
    return this.http.post<Relationship>(this.apiUrl, relationship)
      .pipe(
        tap(newRelationship => {
          const currentRelationships = this.relationshipsSubject.value;
          this.relationshipsSubject.next([...currentRelationships, newRelationship]);
        }),
        catchError(this.handleError<Relationship>('createRelationship'))
      );
  }

  // Delete a relationship
  deleteRelationship(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
      .pipe(
        tap(() => {
          const currentRelationships = this.relationshipsSubject.value;
          this.relationshipsSubject.next(currentRelationships.filter(r => r.id !== id));
        }),
        catchError(this.handleError<void>('deleteRelationship'))
      );
  }

  // Get relationship type display names (gender-aware when possible)
  getRelationshipDisplayName(type: RelationshipType, gender?: 'Male' | 'Female'): string {
    switch (type) {
      case RelationshipType.Parent:
        return gender === 'Male' ? 'Баща' : gender === 'Female' ? 'Майка' : 'Родител';
      case RelationshipType.Child:
        return gender === 'Male' ? 'Син' : gender === 'Female' ? 'Дъщеря' : 'Дете';
      case RelationshipType.Spouse:
        return gender === 'Male' ? 'Съпруг' : gender === 'Female' ? 'Съпруга' : 'Съпруг/а';
      case RelationshipType.Sibling:
        return gender === 'Male' ? 'Брат' : gender === 'Female' ? 'Сестра' : 'Брат/Сестра';
      case RelationshipType.Grandparent:
        return gender === 'Male' ? 'Дядо' : gender === 'Female' ? 'Баба' : 'Дядо/Баба';
      case RelationshipType.Grandchild:
        return gender === 'Male' ? 'Внук' : gender === 'Female' ? 'Внучка' : 'Внук/Внучка';
      case RelationshipType.Uncle:
        return 'Чичо/Вуйчо';
      case RelationshipType.Aunt:
        return 'Леля/Тетка';
      case RelationshipType.Nephew:
        return 'Племенник';
      case RelationshipType.Niece:
        return 'Племенничка';
      case RelationshipType.Cousin:
        return gender === 'Male' ? 'Братовчед' : gender === 'Female' ? 'Сестричка' : 'Братовчед/Сестричка';
      case RelationshipType.GreatGrandparent:
        return gender === 'Male' ? 'Прадядо' : gender === 'Female' ? 'Прабаба' : 'Прадядо/Прабаба';
      case RelationshipType.GreatGrandchild:
        return gender === 'Male' ? 'Правнук' : gender === 'Female' ? 'Правнучка' : 'Правнук/Правнучка';
      case RelationshipType.StepParent:
        return gender === 'Male' ? 'Доведен баща' : gender === 'Female' ? 'Доведена майка' : 'Доведен родител';
      case RelationshipType.StepChild:
        return gender === 'Male' ? 'Доведен син' : gender === 'Female' ? 'Доведена дъщеря' : 'Доведено дете';
      case RelationshipType.StepSibling:
        return gender === 'Male' ? 'Доведен брат' : gender === 'Female' ? 'Доведена сестра' : 'Доведен брат/сестра';
      case RelationshipType.HalfSibling:
        return gender === 'Male' ? 'Полубрат' : gender === 'Female' ? 'Полусестра' : 'Полубрат/Полусестра';
      case RelationshipType.Other:
        return 'Друго';
      default:
        return 'Неизвестна връзка';
    }
  }

  // Get all relationship types for dropdowns
  getRelationshipTypes(): Array<{value: RelationshipType, label: string}> {
    return [
      { value: RelationshipType.Parent, label: 'Родител' },
      { value: RelationshipType.Child, label: 'Дете' },
      { value: RelationshipType.Spouse, label: 'Съпруг/а' },
      { value: RelationshipType.Sibling, label: 'Брат/Сестра' },
      { value: RelationshipType.Grandparent, label: 'Дядо/Баба' },
      { value: RelationshipType.Grandchild, label: 'Внук/Внучка' },
      { value: RelationshipType.Uncle, label: 'Чичо/Вуйчо' },
      { value: RelationshipType.Aunt, label: 'Леля/Тетка' },
      { value: RelationshipType.Nephew, label: 'Племенник' },
      { value: RelationshipType.Niece, label: 'Племенничка' },
      { value: RelationshipType.Cousin, label: 'Братовчед/Сестричка' },
      { value: RelationshipType.GreatGrandparent, label: 'Прадядо/Прабаба' },
      { value: RelationshipType.GreatGrandchild, label: 'Правнук/Правнучка' },
      { value: RelationshipType.StepParent, label: 'Доведен родител' },
      { value: RelationshipType.StepChild, label: 'Доведено дете' },
      { value: RelationshipType.StepSibling, label: 'Доведен брат/сестра' },
      { value: RelationshipType.HalfSibling, label: 'Полубрат/Полусестра' },
      { value: RelationshipType.Other, label: 'Друго' }
    ];
  }

  // Clear cached relationships
  clearCache(): void {
    this.relationshipsSubject.next([]);
  }

  // Generic error handler
  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error(`${operation} failed:`, error);

      if (result !== undefined) {
        return new Observable<T>(observer => {
          observer.next(result);
          observer.complete();
        });
      }
      
      return throwError(() => new Error(`${operation} failed: ${error.message || error}`));
    };
  }
}