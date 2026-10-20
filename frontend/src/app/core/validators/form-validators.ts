import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

// Validators.required acepta "   " como válido; este no.
export const notBlankValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value;
  return typeof value === 'string' && value.length > 0 && value.trim().length === 0
    ? { blank: true }
    : null;
};

// Igual que DECIMAL(18,2) en la base.
export const TWO_DECIMALS_PATTERN = /^\d+(\.\d{1,2})?$/;

export const INTEGER_PATTERN = /^\d+$/;

export const URL_PATTERN = /^https?:\/\/\S+$/i;

export function rangeValidator(minKey: string, maxKey: string): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const min = group.get(minKey)?.value;
    const max = group.get(maxKey)?.value;
    return min !== null && min !== undefined && max !== null && max !== undefined && min > max
      ? { range: true }
      : null;
  };
}
