import type { Message } from "../../lib/types";
import { formatTime } from "../../lib/format";

type MessageBubbleProps = {
  message: Message;
  mine: boolean;
  onToggleStar: () => void;
  onDelete: () => void;
};

export function MessageBubble({ message, mine, onToggleStar, onDelete }: MessageBubbleProps) {
  return (
    <div className="bubble-row" data-mine={mine} data-testid="message-bubble" data-message-id={message.id}>
      <div className="bubble" data-deleted={message.isDeleted}>
        <p>{message.isDeleted ? "Mensagem apagada" : message.body}</p>
        <div className="bubble__meta">
          {message.isStarred && <span aria-label="Favoritada">⭐</span>}
          {message.editedAt && !message.isDeleted && <span>editada</span>}
          <span>{formatTime(message.sentAt)}</span>
        </div>
      </div>
      {!message.isDeleted && (
        <div className="row" style={{ alignSelf: "center" }}>
          <button
            className="btn btn--icon"
            style={{ width: 28, height: 28 }}
            aria-label={message.isStarred ? "Remover favorito" : "Favoritar"}
            onClick={onToggleStar}
            data-testid="message-star-toggle"
          >
            {message.isStarred ? "★" : "☆"}
          </button>
          {mine && (
            <button
              className="btn btn--icon"
              style={{ width: 28, height: 28 }}
              aria-label="Apagar mensagem"
              onClick={onDelete}
              data-testid="message-delete"
            >
              🗑
            </button>
          )}
        </div>
      )}
    </div>
  );
}
