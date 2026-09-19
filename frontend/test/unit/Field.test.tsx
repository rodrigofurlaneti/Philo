import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { createRef } from "react";
import { TextField } from "../../src/ui/Field";

describe("TextField", () => {
  it("liga label, dica e erro via aria-describedby (AGENTS.md §7)", () => {
    render(<TextField label="Nome" hint="Como você quer ser chamado" error="Obrigatório" />);
    const input = screen.getByLabelText("Nome");
    expect(input).toHaveAttribute("aria-invalid", "true");
    expect(screen.getByRole("alert")).toHaveTextContent("Obrigatório");
  });

  it("repassa o ref para o elemento nativo (armadilha §11.1: campo sem forwardRef)", () => {
    const ref = createRef<HTMLInputElement>();
    render(<TextField label="Nome" ref={ref} />);
    expect(ref.current).toBeInstanceOf(HTMLInputElement);
  });
});
