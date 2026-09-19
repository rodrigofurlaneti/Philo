import { useEffect, useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import type { UseFormRegisterReturn } from "react-hook-form";
import { useLocation, useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { createVisitorSession, issueDevStaffSession } from "./api";
import { useDevOrganizationMembers, useDevOrganizations } from "./hooks";
import { staffTokenSchema, visitorSchema } from "./loginSchema";
import type { StaffTokenFormData, VisitorFormData } from "./loginSchema";
import { useAuthStore } from "../../stores/authStore";
import { ApiError } from "../../lib/apiClient";
import { decodeJwt } from "../../lib/jwt";
import { roleLabels } from "../../lib/labels";
import { Button, Skeleton, TextAreaField, TextField } from "../../ui";
import type { SessionResponse } from "../../lib/types";

const ORGANIZATION_ID = Number(import.meta.env.VITE_ORGANIZATION_ID ?? 1);
const IS_DEV = import.meta.env.DEV;

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

function describeDevStaffError(error: unknown): string {
  if (!(error instanceof ApiError)) return "Não foi possível entrar. Tente de novo.";
  switch (error.code) {
    case "Organization.NotFound":
      return "Organização não encontrada.";
    case "Organization.Suspended":
      return "Esta organização está suspensa.";
    case "User.NotFound":
      return "Usuário não encontrado.";
    case "Membership.NotFound":
      return "Esse usuário não pertence a essa organização.";
    case "Membership.Suspended":
      return "O vínculo desse usuário com a organização está suspenso.";
    case "Http.401":
      return "Chave de API interna inválida (VITE_DEV_INTERNAL_API_KEY não bate com InternalApi:ApiKey da API).";
    default:
      return error.message || "Não foi possível entrar.";
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

  function enterWithSession(session: SessionResponse, displayName: string | null, target: string) {
    setSession({
      userId: session.userId,
      organizationId: session.organizationId,
      role: session.role,
      accessToken: session.accessToken,
      expiresAtUtc: session.expiresAtUtc,
      displayName,
    });
    navigate(target, { replace: true });
  }

  function enterAsStaffOrVisitor(session: SessionResponse, displayName: string | null) {
    enterWithSession(session, displayName, session.role === "Customer" ? "/conversas" : "/fila");
  }

  async function onStartConversation(data: VisitorFormData) {
    try {
      const session = await createVisitorSession(ORGANIZATION_ID, data.displayName);
      enterWithSession(session, data.displayName, from);
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
        <div className="stack" style={{ width: "min(360px, 100%)" }}>
          {IS_DEV && <DevQuickLogin onEnter={enterAsStaffOrVisitor} />}

          <details data-testid="staff-token-details">
            <summary className="muted" style={{ cursor: "pointer" }}>
              {IS_DEV ? "Entrar colando um token manualmente" : "Colar token de acesso"}
            </summary>
            <form onSubmit={staffForm.handleSubmit(onStaffSession)} className="stack" style={{ marginTop: 12 }}>
              <TextAreaStaffToken
                error={staffForm.formState.errors.accessToken?.message}
                register={staffForm.register("accessToken")}
              />
              <Button type="submit" data-testid="staff-session-submit">
                Entrar com token
              </Button>
            </form>
          </details>

          <button type="button" className="btn btn--ghost" onClick={() => setMode("visitor")} data-testid="switch-visitor-mode">
            Voltar
          </button>
        </div>
      )}
    </div>
  );
}

/**
 * Só em dev: escolhe organização e depois funcionário, e loga assim que o funcionário é
 * clicado — sem copiar token nenhum. Chama /api/internal/sessions direto do navegador com a
 * chave de VITE_DEV_INTERNAL_API_KEY (nunca definida em build de produção).
 */
function DevQuickLogin({ onEnter }: { onEnter: (session: SessionResponse, displayName: string | null) => void }) {
  const [organizationId, setOrganizationId] = useState<number | null>(null);
  const [loggingInUserId, setLoggingInUserId] = useState<number | null>(null);

  const organizationsQuery = useDevOrganizations(true);
  const membersQuery = useDevOrganizationMembers(organizationId, organizationId !== null);

  const organizations = organizationsQuery.data ?? [];

  // Organização única (caso comum do app, README) ou já com só uma opção: seleciona sozinho.
  useEffect(() => {
    if (organizationId === null && organizationsQuery.data && organizationsQuery.data.length > 0) {
      setOrganizationId(organizationsQuery.data[0].id);
    }
  }, [organizationId, organizationsQuery.data]);

  async function handlePickMember(userId: number) {
    if (organizationId === null) return;
    setLoggingInUserId(userId);
    try {
      const session = await issueDevStaffSession(organizationId, userId);
      onEnter(session, null);
    } catch (error) {
      toast.error(describeDevStaffError(error));
      setLoggingInUserId(null);
    }
  }

  if (organizationsQuery.isError) {
    return (
      <p className="field__error" role="alert">
        {describeDevStaffError(organizationsQuery.error)}
      </p>
    );
  }

  return (
    <div className="stack" data-testid="dev-quick-login">
      <p className="field__hint" style={{ marginBottom: -4 }}>
        Login rápido (dev)
      </p>

      {organizationsQuery.isLoading ? (
        <Skeleton height={44} />
      ) : organizations.length === 0 ? (
        <p className="muted">Nenhuma organização cadastrada ainda.</p>
      ) : (
        <>
          {organizations.length > 1 && (
            <select
              className="field__control"
              value={organizationId ?? ""}
              onChange={(e) => setOrganizationId(Number(e.target.value))}
              data-testid="dev-organization-select"
            >
              {organizations.map((org) => (
                <option key={org.id} value={org.id}>
                  {org.name}
                </option>
              ))}
            </select>
          )}

          {membersQuery.isLoading && <Skeleton height={44} />}
          {membersQuery.isSuccess && membersQuery.data.length === 0 && (
            <p className="muted">Essa organização ainda não tem funcionários cadastrados.</p>
          )}
          {membersQuery.isSuccess && membersQuery.data.length > 0 && (
            <ul className="stack" style={{ listStyle: "none", padding: 0, margin: 0, gap: 4 }} data-testid="dev-member-list">
              {membersQuery.data.map((member) => (
                <li key={member.userId}>
                  <button
                    type="button"
                    className="btn btn--ghost"
                    style={{ width: "100%", justifyContent: "space-between", display: "flex" }}
                    disabled={loggingInUserId !== null}
                    onClick={() => handlePickMember(member.userId)}
                    data-testid={`dev-member-${member.userId}`}
                  >
                    <span>{member.displayName}</span>
                    <span className="muted">
                      {loggingInUserId === member.userId ? "Entrando…" : roleLabels[member.role]}
                    </span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </>
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
