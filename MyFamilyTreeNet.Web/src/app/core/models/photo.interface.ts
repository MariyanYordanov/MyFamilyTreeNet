export interface Photo {
  id: number;
  title: string;
  description?: string;
  url: string;
  familyId: number;
  uploadedByUserId: string;
  uploadedAt: Date;
  dateTaken?: Date;
  location?: string;
  fileSize: number;
  contentType: string;
  likesCount: number;
}