// Formateo la fecha como 'yyyy-MM-dd' usando la fecha LOCAL.
// No uso toISOString() porque la pasa a UTC y en la noche podría cambiar el día.
export function toDateParam(date: Date | null | undefined): string | null {
  if (!date) {
    return null;
  }
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

// La API guarda las fechas en UTC pero las envía sin zona horaria
// ("2026-10-19T19:36:56"). Les agrego la "Z" para que el navegador
// sepa que son UTC y las muestre en la hora local del usuario.
export function parseApiDate(value: string): Date {
  const hasTimeZone = /([zZ]|[+-]\d{2}:\d{2})$/.test(value);
  return new Date(hasTimeZone ? value : `${value}Z`);
}
