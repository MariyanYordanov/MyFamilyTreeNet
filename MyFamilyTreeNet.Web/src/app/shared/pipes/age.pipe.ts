import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'age',
  standalone: true
})
export class AgePipe implements PipeTransform {
  transform(
    birthDate: string | Date | null | undefined,
    deathDate?: string | Date | null,
    format: 'number' | 'text' | 'detailed' = 'text'
  ): string | number {
    
    if (!birthDate) return format === 'number' ? 0 : 'Неизвестна възраст';

    const birth = typeof birthDate === 'string' ? new Date(birthDate) : birthDate;
    if (isNaN(birth.getTime())) return format === 'number' ? 0 : 'Невалидна дата';

    const endDate = deathDate 
      ? (typeof deathDate === 'string' ? new Date(deathDate) : deathDate)
      : new Date();

    if (deathDate && isNaN(endDate.getTime())) {
      return format === 'number' ? 0 : 'Невалидна дата на смърт';
    }

    // Calculate age
    let age = endDate.getFullYear() - birth.getFullYear();
    const monthDiff = endDate.getMonth() - birth.getMonth();
    const dayDiff = endDate.getDate() - birth.getDate();

    // Adjust age if birthday hasn't occurred yet this year
    if (monthDiff < 0 || (monthDiff === 0 && dayDiff < 0)) {
      age--;
    }

    // Handle negative ages (future birth dates)
    if (age < 0) {
      return format === 'number' ? 0 : 'Бъдеща дата';
    }

    switch (format) {
      case 'number':
        return age;
        
      case 'text':
        if (deathDate) {
          return `${age} г. (починал)`;
        }
        return age === 0 ? 'Новородено' : `${age} г.`;
        
      case 'detailed':
        const years = age;
        const totalMonths = (endDate.getFullYear() - birth.getFullYear()) * 12 + monthDiff;
        const months = totalMonths % 12;
        
        if (years === 0 && months === 0) {
          // Calculate days for babies
          const timeDiff = endDate.getTime() - birth.getTime();
          const days = Math.floor(timeDiff / (1000 * 3600 * 24));
          
          if (days === 0) return 'Роден днес';
          if (days === 1) return '1 ден';
          if (days < 7) return `${days} дни`;
          if (days < 30) {
            const weeks = Math.floor(days / 7);
            return weeks === 1 ? '1 седмица' : `${weeks} седмици`;
          }
          return `${Math.floor(days / 30)} месец${Math.floor(days / 30) === 1 ? '' : 'а'}`;
        }
        
        let result = '';
        
        if (years > 0) {
          if (years === 1) {
            result += '1 година';
          } else {
            result += `${years} години`;
          }
        }
        
        if (months > 0) {
          if (result) result += ' и ';
          if (months === 1) {
            result += '1 месец';
          } else {
            result += `${months} месеца`;
          }
        }
        
        if (!result) result = 'Новородено';
        
        if (deathDate) {
          result += ' (починал)';
        }
        
        return result;
        
      default:
        return age;
    }
  }
}