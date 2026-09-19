/**
 * A API devolve datas sem fuso (ex.: "2026-10-05T14:30:00" — DATETIME do MySQL, sempre
 * gravado em UTC pelo backend). `new Date()` interpretaria isso como hora local e jogaria
 * o horário para trás em UTC-3. Tratamos toda data da API como um instante UTC explícito.
 */
export function parseApiDate(value: string): Date {
  const hasOffset = /Z$|[+-]\d{2}:\d{2}$/.test(value);
  return new Date(hasOffset ? value : `${value}Z`);
}

const timeFormatter = new Intl.DateTimeFormat("pt-BR", { hour: "2-digit", minute: "2-digit" });
const dateFormatter = new Intl.DateTimeFormat("pt-BR", { day: "2-digit", month: "2-digit", year: "numeric" });
const dayMonthFormatter = new Intl.DateTimeFormat("pt-BR", { day: "2-digit", month: "2-digit" });

export function formatTime(value: string): string {
  return timeFormatter.format(parseApiDate(value));
}

export function formatDate(value: string): string {
  return dateFormatter.format(parseApiDate(value));
}

function isSameLocalDay(a: Date, b: Date): boolean {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
}

/** Rótulo de divisor de dia na lista de mensagens: "Hoje", "Ontem" ou "dd/mm". */
export function formatDayDivider(value: string): string {
  const date = parseApiDate(value);
  const now = new Date();
  if (isSameLocalDay(date, now)) return "Hoje";

  const yesterday = new Date(now);
  yesterday.setDate(now.getDate() - 1);
  if (isSameLocalDay(date, yesterday)) return "Ontem";

  return dayMonthFormatter.format(date);
}

/** Rótulo curto para a lista de conversas: hora se for hoje, senão data. */
export function formatConversationTimestamp(value: string): string {
  const date = parseApiDate(value);
  return isSameLocalDay(date, new Date()) ? formatTime(value) : formatDate(value);
}
