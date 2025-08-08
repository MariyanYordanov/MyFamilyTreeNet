import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'nameFormat',
  standalone: true
})
export class NameFormatPipe implements PipeTransform {
  transform(
    firstName: string | null | undefined,
    middleName?: string | null,
    lastName?: string | null,
    format: 'full' | 'short' | 'initials' | 'formal' | 'reverse' = 'full'
  ): string {
    
    if (!firstName) return 'Неизвестно име';

    const first = firstName?.trim() || '';
    const middle = middleName?.trim() || '';
    const last = lastName?.trim() || '';

    switch (format) {
      case 'full':
        return [first, middle, last]
          .filter(part => part.length > 0)
          .join(' ')
          .replace(/\s+/g, ' ')
          .trim();
        
      case 'short':
        return [first, last]
          .filter(part => part.length > 0)
          .join(' ')
          .trim();
        
      case 'initials':
        const firstInitial = first.charAt(0).toUpperCase();
        const middleInitial = middle ? middle.charAt(0).toUpperCase() + '.' : '';
        const lastInitial = last ? last.charAt(0).toUpperCase() + '.' : '';
        return [firstInitial + '.', middleInitial, lastInitial]
          .filter(part => part.length > 1 || part === firstInitial + '.')
          .join(' ');
          
      case 'formal':
        // "г-н/г-жа Име Фамилия" format
        const title = 'г-н/г-жа'; // Could be determined by gender in future
        return [title, first, last]
          .filter(part => part.length > 0)
          .join(' ')
          .trim();
          
      case 'reverse':
        // "Фамилия, Име Презиме" format
        const firstMiddle = [first, middle].filter(part => part.length > 0).join(' ');
        return last ? `${last}, ${firstMiddle}` : firstMiddle;
        
      default:
        return [first, middle, last]
          .filter(part => part.length > 0)
          .join(' ')
          .replace(/\s+/g, ' ')
          .trim();
    }
  }
}