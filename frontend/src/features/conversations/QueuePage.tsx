import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ConversationList } from "./ConversationList";
import { Thread } from "./Thread";
import {
  useAssignConversation,
  useCloseConversation,
  useConversationDetails,
  useQueue,
  useReopenConversation,
} from "./hooks";
import { useAuthStore } from "../../stores/authStore";
import { Button, EmptyState, SelectField } from "../../ui";

const STATUS_FILTERS = ["", "Open", "WaitingCustomer", "WaitingTeam", "Closed"] as const;
const STATUS_FILTER_LABELS: Record<(typeof STATUS_FILTERS)[number], string> = {
  "": "Todos",
  Open: "Abertas",
  WaitingCustomer: "Aguardando cliente",
  WaitingTeam: "Aguardando atendente",
  Closed: "Fechadas",
};

/** Fila de atendimento (README/queries.sql #7) — visão de equipe, restrita a agent/admin. */
export function QueuePage() {
  const navigate = useNavigate();
  const params = useParams<{ conversationId?: string }>();
  const conversationId = params.conversationId ? Number(params.conversationId) : null;
  const [statusFilter, setStatusFilter] = useState<(typeof STATUS_FILTERS)[number]>("");
  const userId = useAuthStore((s) => s.userId);

  const queue = useQueue(statusFilter || undefined);
  const details = useConversationDetails(conversationId);
  const assign = useAssignConversation();
  const close = useCloseConversation();
  const reopen = useReopenConversation();

  return (
    <div className="chat-shell" data-thread-open={conversationId !== null}>
      <aside className="conversation-list">
        <div className="conversation-list__header">
          <h2 style={{ fontSize: 16 }}>Fila</h2>
          <SelectField
            label="Status"
            aria-label="Filtrar por status"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as typeof statusFilter)}
            data-testid="queue-status-filter"
            style={{ minHeight: 36, width: 140 }}
          >
            {STATUS_FILTERS.map((status) => (
              <option key={status} value={status}>
                {STATUS_FILTER_LABELS[status]}
              </option>
            ))}
          </SelectField>
        </div>
        <ConversationList
          conversations={queue.data}
          isLoading={queue.isLoading}
          selectedId={conversationId}
          onSelect={(id) => navigate(`/fila/${id}`)}
          testId="queue"
        />
      </aside>

      {conversationId && details.data ? (
        <div className="stack" style={{ minHeight: 0, display: "flex", flexDirection: "column" }}>
          <div className="row" style={{ padding: "var(--gap-sm) var(--gap)", borderBottom: "1px solid var(--line-soft)", background: "var(--bg-raise)" }}>
            {details.data.assignedTo === null && userId && (
              <Button variant="ghost" onClick={() => assign.mutate({ conversationId, agentId: userId })} data-testid="assign-to-me">
                Assumir
              </Button>
            )}
            {details.data.status !== "Closed" ? (
              <Button variant="ghost" onClick={() => close.mutate(conversationId)} data-testid="close-conversation">
                Fechar
              </Button>
            ) : (
              <Button variant="ghost" onClick={() => reopen.mutate(conversationId)} data-testid="reopen-conversation">
                Reabrir
              </Button>
            )}
          </div>
          <Thread conversationId={conversationId} details={details.data} onBack={() => navigate("/fila")} />
        </div>
      ) : (
        <div className="thread">
          <EmptyState title="Selecione uma conversa da fila" description="Atenda o cliente respondendo por aqui." />
        </div>
      )}
    </div>
  );
}
