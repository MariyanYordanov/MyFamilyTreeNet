export interface Family {
    id: number;
    name: string;
    description: string;
    isPublic: boolean;
    createdAt: Date;
    createdByUserId: string;
    members: number;
}

export interface FamilyMember {
    id: number;
    firstName: string;
    middleName: string;
    lastName: string;
    dateOfBirth: Date;
    dateOfDeath: Date;
    gender: string;
}


