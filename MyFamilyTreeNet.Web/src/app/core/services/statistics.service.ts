import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PlatformStatistics {
  totalFamilies: number;
  totalMembers: number;
  totalStories: number;
}

@Injectable({
  providedIn: 'root'
})
export class StatisticsService {
  private apiUrl = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) {}

  getPlatformStatistics(): Observable<PlatformStatistics> {
    return this.http.get<PlatformStatistics>(`${this.apiUrl}/statistics/platform`);
  }
}