/** Conversores para setValueAs do react-hook-form (AGENTS.md §5). */

export function toNullableId(value: string): number | null {
  if (value === "" || value === null || value === undefined) return null;
  const parsed = Number(value);
  return Number.isFinite(parsed) && parsed > 0 ? parsed : null;
}

export function toNullableText(value: string): string | null {
  const trimmed = value?.trim();
  return trimmed ? trimmed : null;
}
