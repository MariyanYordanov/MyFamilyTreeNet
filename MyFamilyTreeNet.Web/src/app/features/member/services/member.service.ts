import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, map, tap, switchMap, debounceTime, distinctUntilChanged } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { 
  Member, 
  CreateMemberRequest, 
  UpdateMemberRequest, 
  MemberRelationships, 
  FamilyTree,
  MemberSearchParams,
  Relationship,
  CreateRelationshipRequest,
  UpdateRelationshipRequest
} from '../models/member.model';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  private readonly apiUrl = `${environment.apiUrl}/member`;
  private membersSubject = new BehaviorSubject<Member[]>([]);
  private searchSubject = new BehaviorSubject<string>('');

  public members$ = this.membersSubject.asObservable();
  public search$ = this.searchSubject.asObservable();

  public searchResults$ = this.search$.pipe(
    debounceTime(300),
    distinctUntilChanged(),
    switchMap(searchTerm => 
      this.getMembers({ search: searchTerm })
        .pipe(map(response => response.members))
    )
  );

  constructor(private http: HttpClient) {}

  getMembers(params?: MemberSearchParams): Observable<{ members: Member[], totalCount: number, page: number, pageSize: number }> {
    let httpParams = new HttpParams();
    
    if (params?.familyId) {
      httpParams = httpParams.set('familyId', params.familyId.toString());
    }
    if (params?.search) {
      httpParams = httpParams.set('search', params.search);
    }
    if (params?.page) {
      httpParams = httpParams.set('page', params.page.toString());
    }
    if (params?.pageSize) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get<Member[]>(this.apiUrl, { params: httpParams, observe: 'response' }).pipe(
      map(response => ({
        members: response.body || [],
        totalCount: parseInt(response.headers.get('X-Total-Count') || '0'),
        page: parseInt(response.headers.get('X-Page') || '1'),
        pageSize: parseInt(response.headers.get('X-Page-Size') || '50')
      })),
      tap(result => this.membersSubject.next(result.members))
    );
  }

  getMember(id: number): Observable<Member> {
    return this.http.get<Member>(`${this.apiUrl}/${id}`);
  }

  createMember(member: CreateMemberRequest): Observable<Member> {
    return this.http.post<Member>(this.apiUrl, member).pipe(
      tap(newMember => {
        const currentMembers = this.membersSubject.value;
        this.membersSubject.next([...currentMembers, newMember]);
      })
    );
  }

  updateMember(id: number, member: UpdateMemberRequest): Observable<Member> {
    return this.http.put<Member>(`${this.apiUrl}/${id}`, member).pipe(
      tap(updatedMember => {
        const currentMembers = this.membersSubject.value;
        const index = currentMembers.findIndex(m => m.id === id);
        if (index !== -1) {
          currentMembers[index] = updatedMember;
          this.membersSubject.next([...currentMembers]);
        }
      })
    );
  }

  deleteMember(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        const currentMembers = this.membersSubject.value;
        const filteredMembers = currentMembers.filter(m => m.id !== id);
        this.membersSubject.next(filteredMembers);
      })
    );
  }

  getMemberRelationships(id: number): Observable<MemberRelationships> {
    return this.http.get<MemberRelationships>(`${this.apiUrl}/${id}/relationships`);
  }

  getFamilyTree(familyId: number): Observable<FamilyTree> {
    return this.http.get<FamilyTree>(`${this.apiUrl}/family/${familyId}/tree`);
  }

  searchMembers(searchTerm: string): void {
    this.searchSubject.next(searchTerm);
  }

  clearSearch(): void {
    this.searchSubject.next('');
  }

  filterMembersByFamily(familyId: number): Observable<Member[]> {
    return this.members$.pipe(
      map(members => members.filter(member => member.familyId === familyId))
    );
  }

  filterMembersByAge(minAge: number, maxAge: number): Observable<Member[]> {
    return this.members$.pipe(
      map(members => members.filter(member => 
        member.age !== undefined && 
        member.age >= minAge && 
        member.age <= maxAge
      ))
    );
  }

  filterAliveMembers(): Observable<Member[]> {
    return this.members$.pipe(
      map(members => members.filter(member => !member.dateOfDeath))
    );
  }

  filterDeceasedMembers(): Observable<Member[]> {
    return this.members$.pipe(
      map(members => members.filter(member => !!member.dateOfDeath))
    );
  }

  sortMembersByName(): Observable<Member[]> {
    return this.members$.pipe(
      map(members => [...members].sort((a, b) => 
        `${a.firstName} ${a.lastName}`.localeCompare(`${b.firstName} ${b.lastName}`, 'bg')
      ))
    );
  }

  sortMembersByAge(): Observable<Member[]> {
    return this.members$.pipe(
      map(members => [...members].sort((a, b) => (b.age || 0) - (a.age || 0)))
    );
  }

  sortMembersByBirthDate(): Observable<Member[]> {
    return this.members$.pipe(
      map(members => [...members].sort((a, b) => {
        if (!a.dateOfBirth) return 1;
        if (!b.dateOfBirth) return -1;
        return new Date(b.dateOfBirth).getTime() - new Date(a.dateOfBirth).getTime();
      }))
    );
  }

  getMembersCount(): Observable<number> {
    return this.members$.pipe(
      map(members => members.length)
    );
  }

  getAliveCount(): Observable<number> {
    return this.members$.pipe(
      map(members => members.filter(m => !m.dateOfDeath).length)
    );
  }

  getDeceasedCount(): Observable<number> {
    return this.members$.pipe(
      map(members => members.filter(m => !!m.dateOfDeath).length)
    );
  }

  getAverageAge(): Observable<number> {
    return this.members$.pipe(
      map(members => {
        const membersWithAge = members.filter(m => m.age !== undefined);
        if (membersWithAge.length === 0) return 0;
        const sum = membersWithAge.reduce((acc, m) => acc + (m.age || 0), 0);
        return Math.round(sum / membersWithAge.length);
      })
    );
  }
}