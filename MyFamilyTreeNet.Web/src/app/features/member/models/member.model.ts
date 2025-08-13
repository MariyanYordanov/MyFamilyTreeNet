export interface Member {
  id: number;
  firstName: string;
  middleName?: string;
  lastName: string;
  fullName?: string;
  dateOfBirth?: string;
  dateOfDeath?: string;
  gender?: string;
  biography?: string;
  placeOfBirth?: string;
  placeOfDeath?: string;
  profileImageUrl?: string;
  age?: number;
  familyId: number;
  familyName: string;
  createdAt: string;
  updatedAt: string;
  isAlive?: boolean;
}

export interface CreateMemberRequest {
  firstName: string;
  middleName?: string;
  lastName: string;
  dateOfBirth?: string;
  dateOfDeath?: string;
  gender?: string;
  biography?: string;
  placeOfBirth?: string;
  placeOfDeath?: string;
  familyId: number;
}

export interface UpdateMemberRequest {
  firstName: string;
  middleName?: string;
  lastName: string;
  dateOfBirth?: string;
  dateOfDeath?: string;
  gender?: string;
  biography?: string;
  placeOfBirth?: string;
  placeOfDeath?: string;
}

export interface Relationship {
  id: number;
  primaryMemberId: number;
  relatedMemberId: number;
  relationshipType: RelationshipType;
  notes?: string;
  createdAt: string;
  primaryMemberName?: string;
  relatedMemberName?: string;
  relationshipTypeName?: string;
}

export interface CreateRelationshipRequest {
  primaryMemberId: number;
  relatedMemberId: number;
  relationshipType: RelationshipType;
  notes?: string;
}

export interface UpdateRelationshipRequest {
  relationshipType: RelationshipType;
  notes?: string;
}

export interface MemberRelationships {
  memberId: number;
  memberName: string;
  relationships: Relationship[];
}

export enum RelationshipType {
  Parent = 0,
  Child = 1,
  Spouse = 2,
  Sibling = 3,
  Grandparent = 4,
  Grandchild = 5,
  Uncle = 6,
  Aunt = 7,
  Cousin = 8,
  Nephew = 9,
  Niece = 10,
  Other = 11
}

export interface FamilyTree {
  familyId: number;
  familyName: string;
  members: Member[];
  relationships: Relationship[];
}

export interface MemberSearchParams {
  familyId?: number;
  search?: string;
  page?: number;
  pageSize?: number;
}