import { Injectable } from "@angular/core";
import { environment } from '../../../environments/environment';
import { BehaviorSubject, Observable } from 'rxjs';
import { Family } from '../models/family.interface';
import { HttpClient } from '@angular/common/http';

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
        return this.http.get<Family[]>(this.apiUrl); 
    }

    getFamilyById(id: number): Observable<Family> {
        return this.http.get<Family>(`${this.apiUrl}/${id}`);
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