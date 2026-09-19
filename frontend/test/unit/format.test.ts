import { describe, expect, it } from "vitest";
import { formatDayDivider, formatTime, parseApiDate } from "../../src/lib/format";

describe("parseApiDate", () => {
  it("trata uma data sem fuso da API como um instante UTC (AGENTS.md §11)", () => {
    const parsed = parseApiDate("2026-10-05T14:30:00");
    expect(parsed.toISOString()).toBe("2026-10-05T14:30:00.000Z");
  });

  it("preserva o offset quando a data já vem com Z", () => {
    const parsed = parseApiDate("2026-10-05T14:30:00Z");
    expect(parsed.toISOString()).toBe("2026-10-05T14:30:00.000Z");
  });
});

describe("formatTime", () => {
  it("formata hora:minuto em pt-BR", () => {
    expect(formatTime("2026-10-05T14:05:00Z")).toMatch(/^\d{2}:\d{2}$/);
  });
});

describe("formatDayDivider", () => {
  it("mostra 'Hoje' para a data atual", () => {
    const now = new Date().toISOString();
    expect(formatDayDivider(now)).toBe("Hoje");
  });
});
