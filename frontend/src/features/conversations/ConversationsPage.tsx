import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ConversationList } from "./ConversationList";
import { Thread } from "./Thread";
import { NewConversationModal } from "./NewConversationModal";
import { useConversationDetails, useMyConversations } from "./hooks";
import { Button, EmptyState } from "../../ui";

/** Tela de conversas do cliente — sidebar de conversas + thread ativa, estilo mensageiro. */
export function ConversationsPage() {
  const navigate = useNavigate();
  const params = useParams<{ conversationId?: string }>();
  const conversationId = params.conversationId ? Number(params.conversationId) : null;
  const [showNewConversation, setShowNewConversation] = useState(false);

  const conversations = useMyConversations();
  const details = useConversationDetails(conversationId);

  return (
    <div className="chat-shell" data-thread-open={conversationId !== null}>
      <aside className="conversation-list">
        <div className="conversation-list__header">
          <h2 style={{ fontSize: 16 }}>Conversas</h2>
          <Button variant="ghost" onClick={() => setShowNewConversation(true)} data-testid="open-new-conversation">
            + Nova
          </Button>
        </div>
        <ConversationList
          conversations={conversations.data}
          isLoading={conversations.isLoading}
          selectedId={conversationId}
          onSelect={(id) => navigate(`/conversas/${id}`)}
          testId="my-conversations"
        />
      </aside>

      {conversationId ? (
        <Thread conversationId={conversationId} details={details.data} onBack={() => navigate("/conversas")} />
      ) : (
        <div className="thread">
          <EmptyState
            title="Selecione uma conversa"
            description="Ou comece uma nova para falar com nossa equipe."
            action={
              <Button onClick={() => setShowNewConversation(true)} data-testid="empty-new-conversation">
                Nova conversa
              </Button>
            }
          />
        </div>
      )}

      {showNewConversation && (
        <NewConversationModal
          onClose={() => setShowNewConversation(false)}
          onCreated={(id) => navigate(`/conversas/${id}`)}
        />
      )}
    </div>
  );
}
