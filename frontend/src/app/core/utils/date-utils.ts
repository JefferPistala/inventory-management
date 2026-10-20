// No uso toISOString(): convierte a UTC y de noche puede cambiar el día.
export function toDateParam(date: Date | null | undefined): string | null {
  if (!date) {
    return null;
  }
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

// La API envía las fechas en UTC sin zona; agrego la Z para mostrarlas en hora local.
export function parseApiDate(value: string): Date {
  const hasTimeZone = /([zZ]|[+-]\d{2}:\d{2})$/.test(value);
  return new Date(hasTimeZone ? value : `${value}Z`);
}
