import type { ConversationSummary } from "../../lib/types";
import { formatConversationTimestamp } from "../../lib/format";
import { conversationPurposeLabels } from "../../lib/labels";
import { EmptyState, Skeleton } from "../../ui";

type ConversationListProps = {
  conversations: ConversationSummary[] | undefined;
  isLoading: boolean;
  selectedId: number | null;
  onSelect: (id: number) => void;
  testId: string;
};

function initials(id: number): string {
  return `C${id}`.slice(0, 2).toUpperCase();
}

export function ConversationList({ conversations, isLoading, selectedId, onSelect, testId }: ConversationListProps) {
  if (isLoading) {
    return (
      <div className="stack" style={{ padding: "var(--gap)" }}>
        {[0, 1, 2, 3].map((i) => (
          <div key={i} className="row">
            <Skeleton width={44} height={44} circle />
            <div className="stack" style={{ flex: 1, gap: 6 }}>
              <Skeleton height={12} width="60%" />
              <Skeleton height={10} width="90%" />
            </div>
          </div>
        ))}
      </div>
    );
  }

  if (!conversations || conversations.length === 0) {
    return <EmptyState title="Nenhuma conversa ainda" description="Quando alguém escrever, ela aparece aqui." />;
  }

  return (
    <div className="conversation-list__items" data-testid={testId}>
      {conversations.map((conversation) => (
        <button
          key={conversation.id}
          className="conversation-item"
          aria-current={conversation.id === selectedId}
          onClick={() => onSelect(conversation.id)}
          data-testid={`${testId}-row`}
        >
          <span className="avatar" aria-hidden="true">
            {initials(conversation.customerId)}
          </span>
          <span className="conversation-item__body">
            <span className="conversation-item__top">
              <span className="conversation-item__name">
                {conversation.subject || conversationPurposeLabels[conversation.purpose]}
              </span>
              <span className="conversation-item__time">{formatConversationTimestamp(conversation.lastActivityAt)}</span>
            </span>
            <span className="conversation-item__preview">
              {conversationPurposeLabels[conversation.purpose]} · {conversation.status}
            </span>
          </span>
        </button>
      ))}
    </div>
  );
}
