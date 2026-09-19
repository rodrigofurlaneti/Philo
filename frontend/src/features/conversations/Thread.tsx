import { useEffect, useRef } from "react";
import type { ConversationDetails, Message } from "../../lib/types";
import { formatDayDivider } from "../../lib/format";
import { conversationPurposeLabels, conversationStatusLabels } from "../../lib/labels";
import { MessageBubble } from "./MessageBubble";
import { Composer } from "./Composer";
import { QueryError } from "../../components/QueryError";
import { Skeleton, StatusBadge } from "../../ui";
import { useAuthStore } from "../../stores/authStore";
import {
  useDeleteMessage,
  useHistory,
  useMarkRead,
  useSendTextMessage,
  useToggleStar,
} from "./hooks";

type ThreadProps = {
  conversationId: number;
  details: ConversationDetails | undefined;
  onBack?: () => void;
};

function groupByDay(messages: Message[]): { divider: string; items: Message[] }[] {
  const groups: { divider: string; items: Message[] }[] = [];
  for (const message of messages) {
    const divider = formatDayDivider(message.sentAt);
    const last = groups[groups.length - 1];
    if (last && last.divider === divider) last.items.push(message);
    else groups.push({ divider, items: [message] });
  }
  return groups;
}

export function Thread({ conversationId, details, onBack }: ThreadProps) {
  const userId = useAuthStore((s) => s.userId);
  const history = useHistory(conversationId);
  const sendMessage = useSendTextMessage(conversationId);
  const deleteMessage = useDeleteMessage(conversationId);
  const toggleStar = useToggleStar(conversationId);
  const markRead = useMarkRead(conversationId);
  const scrollRef = useRef<HTMLDivElement>(null);

  const messages = [...(history.data ?? [])].reverse();

  useEffect(() => {
    scrollRef.current?.scrollTo({ top: scrollRef.current.scrollHeight });
  }, [messages.length]);

  useEffect(() => {
    const unread = messages.filter((m) => m.senderId !== userId && !m.isDeleted);
    for (const message of unread.slice(-5)) {
      markRead.mutate(message.id);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [messages.length]);

  const isClosed = details?.status === "Closed";

  return (
    <section className="thread" data-testid="thread">
      <header className="thread__header">
        {onBack && (
          <button className="btn btn--icon" aria-label="Voltar para a lista" onClick={onBack} data-testid="thread-back">
            ←
          </button>
        )}
        <span className="avatar" aria-hidden="true">
          {details ? `C${details.customerId}`.slice(0, 2).toUpperCase() : "…"}
        </span>
        <div style={{ flex: 1 }}>
          <p className="thread__title">{details?.subject || (details && conversationPurposeLabels[details.purpose]) || "Conversa"}</p>
          {details && <p className="thread__subtitle">{conversationPurposeLabels[details.purpose]}</p>}
        </div>
        {details && <StatusBadge status={details.status} />}
      </header>

      <div className="thread__scroll" ref={scrollRef}>
        {history.isLoading && (
          <div className="stack">
            <Skeleton height={40} width="60%" />
            <Skeleton height={40} width="45%" />
          </div>
        )}
        {history.isError && <QueryError error={history.error} what="as mensagens" onRetry={() => history.refetch()} />}
        {!history.isLoading &&
          !history.isError &&
          groupByDay(messages).map((group) => (
            <div key={group.divider}>
              <div className="day-divider">{group.divider}</div>
              {group.items.map((message) => (
                <MessageBubble
                  key={message.id}
                  message={message}
                  mine={message.senderId === userId}
                  onToggleStar={() => toggleStar.mutate({ messageId: message.id, starred: message.isStarred })}
                  onDelete={() => deleteMessage.mutate(message.id)}
                />
              ))}
            </div>
          ))}
      </div>

      <Composer
        disabled={isClosed}
        disabledReason={`Conversa ${conversationStatusLabels[details?.status ?? "Closed"].toLowerCase()}. Reabra para continuar.`}
        sending={sendMessage.isPending}
        onSend={(body) =>
          sendMessage.mutate({
            clientMessageId: crypto.randomUUID(),
            body,
            replyToId: null,
            expiresAt: null,
          })
        }
      />
    </section>
  );
}
