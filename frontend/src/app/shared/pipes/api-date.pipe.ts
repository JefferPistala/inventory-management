import { Pipe, PipeTransform } from '@angular/core';
import { parseApiDate } from '../../core/utils/date-utils';

// Convierto la fecha UTC de la API en un Date local. Lo combino con el
// pipe date de Angular: {{ fecha | apiDate | date: 'dd/MM/yyyy HH:mm' }}
@Pipe({ name: 'apiDate' })
export class ApiDatePipe implements PipeTransform {
  transform(value: string | null | undefined): Date | null {
    return value ? parseApiDate(value) : null;
  }
}
