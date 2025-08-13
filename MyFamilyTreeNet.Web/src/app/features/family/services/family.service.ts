import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { map, catchError, tap, debounceTime, switchMap } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { 
  Family, 
  FamilyCreateRequest, 
  FamilyUpdateRequest, 
  FamilySearchParams, 
  FamilyListResponse 
} from '../models/family.model';

@Injectable({
  providedIn: 'root'
})
export class FamilyService {
  private readonly apiUrl = `/api/Family`;
  private familiesSubject = new BehaviorSubject<Family[]>([]);
  private selectedFamilySubject = new BehaviorSubject<Family | null>(null);

  public families$ = this.familiesSubject.asObservable();
  public selectedFamily$ = this.selectedFamilySubject.asObservable();

  constructor(private http: HttpClient) {}

  // Get all families with optional search parameters
  getFamilies(params?: FamilySearchParams): Observable<FamilyListResponse> {
    let httpParams = new HttpParams();
    
    if (params) {
      if (params.search) httpParams = httpParams.set('search', params.search);
      if (params.isPublic !== undefined) httpParams = httpParams.set('isPublic', params.isPublic.toString());
      if (params.createdByUserId) httpParams = httpParams.set('createdByUserId', params.createdByUserId);
      if (params.page) httpParams = httpParams.set('page', params.page.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get<Family[]>(this.apiUrl, { params: httpParams })
      .pipe(
        map(families => {
          // Convert array response to expected FamilyListResponse format
          const response: FamilyListResponse = {
            families: families,
            totalCount: families.length,
            page: 1,
            pageSize: families.length,
            totalPages: 1
          };
          return response;
        }),
        tap(response => this.familiesSubject.next(response.families)),
        catchError(this.handleError<FamilyListResponse>('getFamilies'))
      );
  }

  // Get a specific family by ID
  getFamily(id: number): Observable<Family> {
    return this.http.get<Family>(`${this.apiUrl}/${id}`)
      .pipe(
        tap(family => this.selectedFamilySubject.next(family)),
        catchError(this.handleError<Family>('getFamily'))
      );
  }

  // Get family by ID (alias for compatibility)
  getFamilyById(id: number): Observable<Family> {
    return this.getFamily(id);
  }

  // Create a new family
  createFamily(familyData: FamilyCreateRequest): Observable<Family> {
    return this.http.post<Family>(this.apiUrl, familyData)
      .pipe(
        tap(family => {
          const currentFamilies = this.familiesSubject.value;
          this.familiesSubject.next([...currentFamilies, family]);
        }),
        catchError(this.handleError<Family>('createFamily'))
      );
  }

  // Update an existing family
  updateFamily(id: number, familyData: Omit<FamilyUpdateRequest, 'id'>): Observable<Family> {
    const updateRequest = { ...familyData, id };
    return this.http.put<Family>(`${this.apiUrl}/${id}`, updateRequest)
      .pipe(
        tap(updatedFamily => {
          const currentFamilies = this.familiesSubject.value;
          const index = currentFamilies.findIndex(f => f.id === updatedFamily.id);
          if (index !== -1) {
            currentFamilies[index] = updatedFamily;
            this.familiesSubject.next([...currentFamilies]);
          }
          
          if (this.selectedFamilySubject.value?.id === updatedFamily.id) {
            this.selectedFamilySubject.next(updatedFamily);
          }
        }),
        catchError(this.handleError<Family>('updateFamily'))
      );
  }

  // Delete a family
  deleteFamily(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
      .pipe(
        tap(() => {
          const currentFamilies = this.familiesSubject.value;
          this.familiesSubject.next(currentFamilies.filter(f => f.id !== id));
          
          if (this.selectedFamilySubject.value?.id === id) {
            this.selectedFamilySubject.next(null);
          }
        }),
        catchError(this.handleError<void>('deleteFamily'))
      );
  }

  // Search families with debouncing for real-time search
  searchFamilies(searchTerm: Observable<string>): Observable<Family[]> {
    return searchTerm.pipe(
      debounceTime(300),
      switchMap(term => 
        term.length === 0 
          ? this.getFamilies().pipe(map(response => response.families))
          : this.getFamilies({ search: term }).pipe(map(response => response.families))
      ),
      catchError(this.handleError<Family[]>('searchFamilies', []))
    );
  }

  // Get user's families
  getUserFamilies(userId: string): Observable<FamilyListResponse> {
    return this.getFamilies({ createdByUserId: userId });
  }

  // Get public families only
  getPublicFamilies(): Observable<FamilyListResponse> {
    return this.getFamilies({ isPublic: true });
  }

  // Set selected family
  setSelectedFamily(family: Family | null): void {
    this.selectedFamilySubject.next(family);
  }

  // Get current selected family value
  getSelectedFamily(): Family | null {
    return this.selectedFamilySubject.value;
  }

  // Get family tree data for visualization
  getFamilyTreeData(familyId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${familyId}/tree`)
      .pipe(
        catchError(this.handleError<any>('getFamilyTreeData'))
      );
  }

  // Clear all cached data
  clearCache(): void {
    this.familiesSubject.next([]);
    this.selectedFamilySubject.next(null);
  }

  // Generic error handler
  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error(`${operation} failed:`, error);

      // You could add more sophisticated error handling here
      // For example, show user-friendly error messages
      
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