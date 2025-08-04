export interface Story {
  id: number;
  title: string;
  content: string;
  familyId: number;
  authorId: string;
  authorName: string;
  createdAt: Date;
  isPublic: boolean;
  eventDate?: Date;
  likesCount: number;
}