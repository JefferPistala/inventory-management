import { Pipe, PipeTransform } from '@angular/core';
import { parseApiDate } from '../../core/utils/date-utils';

@Pipe({ name: 'apiDate' })
export class ApiDatePipe implements PipeTransform {
  transform(value: string | null | undefined): Date | null {
    return value ? parseApiDate(value) : null;
  }
}
