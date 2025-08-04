import { Injectable } from "@angular/core";
import { environment } from '../../../environments/environment';
import { BehaviorSubject, Observable } from 'rxjs';
import { Family } from '../models/family.interface';
import { HttpClient } from '@angular/common/http';
import { map, debounceTime, switchMap } from 'rxjs/operators';

@Injectable({
    providedIn: "root",
})
export class FamilyService {
    private readonly apiUrl = `${environment.apiUrl}/api/family`;
    private familiesSubject = new BehaviorSubject<Family[]>([]);
    private selectedFamilySubject = new BehaviorSubject<Family | null>(null);

    public families$ = this.familiesSubject.asObservable();
    public selectedFamily$ = this.selectedFamilySubject.asObservable();

    constructor(private http: HttpClient) {}

    getFamilies(): Observable<Family[]> {
        return this.http.get<{families: Family[], total: number}>(this.apiUrl)
            .pipe(map(response => response.families || [])); 
    }

    getFamilyById(id: number): Observable<Family> {
        return this.http.get<Family>(`${this.apiUrl}/${id}`);
    }

      searchFamilies(searchTerm: Observable<string>): Observable<Family[]> {
        return searchTerm.pipe(
            debounceTime(300),
                switchMap(term => 
                    term.length === 0 
                    ? this.getFamilies()
                    : this.getFamilies().pipe(
                        map(families => families.filter(family => 
                        family.name.toLowerCase().includes(term.toLowerCase()) ||
                        (family.description && family.description.toLowerCase().includes(term.toLowerCase()))
              ))
            )
      )
    );
  }

    private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error(`${operation} failed: ${error.message}`);

      return new Observable<T>(observer => {
        if (result !== undefined) {
          observer.next(result);
        }
        observer.complete();
      });
    };
  }
}