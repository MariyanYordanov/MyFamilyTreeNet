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
  PrimaryMemberId: number;
  RelatedMemberId: number;
  RelationshipType: RelationshipType;
  Notes?: string;
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