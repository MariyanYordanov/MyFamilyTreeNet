import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'dateFormat',
  standalone: true
})
export class DateFormatPipe implements PipeTransform {
  transform(value: string | Date | null | undefined, format: 'short' | 'long' | 'year' | 'age' = 'short'): string {
    if (!value) return 'Неизвестна';

    const date = typeof value === 'string' ? new Date(value) : value;
    
    if (isNaN(date.getTime())) return 'Невалидна дата';

    const locale = 'bg-BG';

    switch (format) {
      case 'short':
        return date.toLocaleDateString(locale, {
          day: '2-digit',
          month: '2-digit',
          year: 'numeric'
        });
        
      case 'long':
        return date.toLocaleDateString(locale, {
          day: 'numeric',
          month: 'long',
          year: 'numeric'
        });
        
      case 'year':
        return date.getFullYear().toString();
        
      case 'age':
        const today = new Date();
        let age = today.getFullYear() - date.getFullYear();
        const monthDiff = today.getMonth() - date.getMonth();
        
        if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < date.getDate())) {
          age--;
        }
        
        return age > 0 ? `${age} г.` : 'Новородено';
        
      default:
        return date.toLocaleDateString(locale);
    }
  }
}