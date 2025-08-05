import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../../app/core/services/auth.service'
import { User } from '../../../../app/core/models/user.interface';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule
  ]
, 
  templateUrl: './header.html',
  styleUrls: ['./header.scss']
})

export class HeaderComponent implements OnInit {
  currentUser$: Observable<User | null>;
  isAdmin = false;

  constructor(private authService: AuthService) {
    this.currentUser$ = this.authService.currentUser$;
  }

  ngOnInit(): void {
    this.currentUser$.subscribe(user => {
      // if(this.currentUser$)
      // TODO: Check if user has admin role
      this.isAdmin = false;
    });
  }

  logout(): void {
    this.authService.logout();
  }
}
