import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

// Validators.required acepta "   " como válido; este no.
export const notBlankValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value;
  return typeof value === 'string' && value.length > 0 && value.trim().length === 0
    ? { blank: true }
    : null;
};

// Máximo dos decimales (coincide con DECIMAL(18,2) de la base).
export const TWO_DECIMALS_PATTERN = /^\d+(\.\d{1,2})?$/;

// Solo enteros positivos para cantidades.
export const INTEGER_PATTERN = /^\d+$/;

// URL simple http o https para la imagen.
export const URL_PATTERN = /^https?:\/\/\S+$/i;

// Validador de grupo: el precio mínimo no puede superar al máximo.
export function rangeValidator(minKey: string, maxKey: string): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const min = group.get(minKey)?.value;
    const max = group.get(maxKey)?.value;
    return min !== null && min !== undefined && max !== null && max !== undefined && min > max
      ? { range: true }
      : null;
  };
}
