import Swal from "sweetalert2";

/** Wrappers finos do sweetalert2, temáticos ao design system (AGENTS.md §2/§8). */

export async function confirmDialog(options: { title: string; text?: string; confirmLabel?: string; danger?: boolean }) {
  const result = await Swal.fire({
    title: options.title,
    text: options.text,
    icon: options.danger ? "warning" : "question",
    showCancelButton: true,
    confirmButtonText: options.confirmLabel ?? "Confirmar",
    cancelButtonText: "Cancelar",
    background: "var(--bg-raise)",
    color: "var(--ink)",
    confirmButtonColor: options.danger ? "var(--danger)" : "var(--mint-deep)",
    cancelButtonColor: "var(--line)",
  });
  return result.isConfirmed;
}
