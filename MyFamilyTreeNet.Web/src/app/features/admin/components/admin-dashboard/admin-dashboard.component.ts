import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

interface AdminStats {
  totalUsers: number;
  totalFamilies: number;
  totalMembers: number;
  activeUsers: number;
  newUsersThisMonth: number;
}

@Component({
  selector: 'app-admin-dashboard',
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit {
  stats = signal<AdminStats | null>(null);
  isLoading = signal(false);
  errorMessage = signal('');

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadStats();
  }

  private loadStats(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    // Mock data for demo - replace with actual API call
    const mockStats: AdminStats = {
      totalUsers: 156,
      totalFamilies: 43,
      totalMembers: 892,
      activeUsers: 23,
      newUsersThisMonth: 12
    };

    // Simulate API call delay
    setTimeout(() => {
      this.stats.set(mockStats);
      this.isLoading.set(false);
    }, 800);

    // Real API call would be:
    // this.http.get<AdminStats>(`${environment.apiUrl}/api/admin/stats`)
    //   .pipe(
    //     catchError(error => {
    //       this.errorMessage.set('Грешка при зареждане на статистиките');
    //       console.error('Error loading admin stats:', error);
    //       return of(null);
    //     }),
    //     finalize(() => this.isLoading.set(false))
    //   )
    //   .subscribe(stats => {
    //     if (stats) {
    //       this.stats.set(stats);
    //     }
    //   });
  }
}