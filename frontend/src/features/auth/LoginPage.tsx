import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { UseFormRegisterReturn } from "react-hook-form";
import { useLocation, useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { createVisitorSession } from "./api";
import { staffTokenSchema, visitorSchema } from "./loginSchema";
import type { StaffTokenFormData, VisitorFormData } from "./loginSchema";
import { useAuthStore } from "../../stores/authStore";
import { ApiError } from "../../lib/apiClient";
import { decodeJwt } from "../../lib/jwt";
import { Button, TextAreaField, TextField } from "../../ui";

const ORGANIZATION_ID = Number(import.meta.env.VITE_ORGANIZATION_ID ?? 1);

function describeVisitorError(error: unknown): string {
  if (!(error instanceof ApiError)) return "Não foi possível iniciar a conversa. Tente de novo.";
  switch (error.code) {
    case "Organization.NotFound":
      return "Organização não encontrada. Confira a configuração do app.";
    case "Organization.Suspended":
      return "Este atendimento está temporariamente indisponível.";
    default:
      return error.message || "Não foi possível iniciar a conversa.";
  }
}

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const setSession = useAuthStore((s) => s.setSession);
  const [mode, setMode] = useState<"visitor" | "staff">("visitor");
  const from = (location.state as { from?: { pathname: string } } | null)?.from?.pathname ?? "/conversas";

  const visitorForm = useForm<VisitorFormData>({
    resolver: zodResolver(visitorSchema),
    mode: "onBlur",
    defaultValues: { displayName: "" },
  });

  const staffForm = useForm<StaffTokenFormData>({
    resolver: zodResolver(staffTokenSchema),
    mode: "onBlur",
    defaultValues: { accessToken: "" },
  });

  async function onStartConversation(data: VisitorFormData) {
    try {
      const session = await createVisitorSession(ORGANIZATION_ID, data.displayName);
      setSession({
        userId: session.userId,
        organizationId: session.organizationId,
        role: session.role,
        accessToken: session.accessToken,
        expiresAtUtc: session.expiresAtUtc,
        displayName: data.displayName,
      });
      navigate(from, { replace: true });
    } catch (error) {
      toast.error(describeVisitorError(error));
    }
  }

  function onStaffSession(data: StaffTokenFormData) {
    const claims = decodeJwt(data.accessToken);
    if (!claims) {
      staffForm.setError("accessToken", { message: "Token inválido." });
      return;
    }
    if (claims.exp * 1000 <= Date.now()) {
      staffForm.setError("accessToken", { message: "Token expirado." });
      return;
    }
    setSession({
      userId: Number(claims.sub),
      organizationId: Number(claims.org),
      role: claims.role,
      accessToken: data.accessToken,
      expiresAtUtc: new Date(claims.exp * 1000).toISOString(),
      displayName: null,
    });
    navigate(claims.role === "Customer" ? "/conversas" : "/fila", { replace: true });
  }

  return (
    <div className="empty-state" style={{ height: "100dvh" }}>
      <div className="app-rail__brand" style={{ width: 56, height: 56, fontSize: 24 }} aria-hidden="true">
        Φ
      </div>
      <h1 style={{ fontSize: 24 }}>Philo</h1>
      <p className="muted">Converse com nossa equipe de vendas e suporte.</p>

      {mode === "visitor" ? (
        <form onSubmit={visitorForm.handleSubmit(onStartConversation)} className="stack" style={{ width: "min(360px, 100%)" }}>
          <TextField
            label="Como podemos te chamar?"
            placeholder="Seu nome"
            data-testid="visitor-name"
            error={visitorForm.formState.errors.displayName?.message}
            {...visitorForm.register("displayName")}
          />
          <Button type="submit" disabled={visitorForm.formState.isSubmitting} data-testid="start-conversation">
            {visitorForm.formState.isSubmitting ? "Entrando…" : "Começar a conversar"}
          </Button>
          <button type="button" className="btn btn--ghost" onClick={() => setMode("staff")} data-testid="switch-staff-mode">
            Sou da equipe
          </button>
        </form>
      ) : (
        <form onSubmit={staffForm.handleSubmit(onStaffSession)} className="stack" style={{ width: "min(360px, 100%)" }}>
          <TextAreaStaffToken
            error={staffForm.formState.errors.accessToken?.message}
            register={staffForm.register("accessToken")}
          />
          <Button type="submit" data-testid="staff-session-submit">
            Entrar
          </Button>
          <button type="button" className="btn btn--ghost" onClick={() => setMode("visitor")} data-testid="switch-visitor-mode">
            Voltar
          </button>
        </form>
      )}
    </div>
  );
}

// Separado para manter o forwardRef do register() intacto (AGENTS.md §5).
function TextAreaStaffToken({ error, register }: { error?: string; register: UseFormRegisterReturn }) {
  return (
    <TextAreaField
      label="Token de acesso da equipe"
      hint="Emitido pelo backend do site via /api/internal/sessions."
      error={error}
      rows={4}
      data-testid="staff-token-input"
      {...register}
    />
  );
}
