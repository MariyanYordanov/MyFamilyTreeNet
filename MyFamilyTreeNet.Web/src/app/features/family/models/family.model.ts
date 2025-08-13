export interface Family {
  id: number;
  name: string;
  description: string;
  isPublic?: boolean;
  createdAt: Date;
  updatedAt?: Date;
  createdByUserId: string;
  memberCount: number;
  photoCount?: number;
  storyCount?: number;
  photoUrl?: string;
  members?: FamilyMember[];
  photos?: any[];
  stories?: any[];
}

export interface FamilyMember {
  id: number;
  firstName: string;
  middleName?: string;
  lastName: string;
  dateOfBirth?: Date;
  dateOfDeath?: Date;
  gender: string;
  familyId: number;
  familyName?: string;
  age?: number;
  isAlive?: boolean;
  biography?: string;
  placeOfBirth?: string;
  placeOfDeath?: string;
  createdAt: Date;
  updatedAt?: Date;
}

export interface FamilyCreateRequest {
  name: string;
  description: string;
  isPublic?: boolean;
}

export interface FamilyUpdateRequest {
  id: number;
  name?: string;
  description?: string;
  isPublic?: boolean;
}

export interface FamilySearchParams {
  search?: string;
  isPublic?: boolean;
  createdByUserId?: string;
  page?: number;
  pageSize?: number;
}

export interface FamilyListResponse {
  families: Family[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}