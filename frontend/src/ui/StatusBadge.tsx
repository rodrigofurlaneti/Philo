import type { ConversationStatus } from "../lib/types";
import { conversationStatusLabels } from "../lib/labels";

const toneByStatus: Record<ConversationStatus, "ok" | "warn" | "danger" | "pending" | "neutral"> = {
  Open: "ok",
  Pending: "pending",
  Resolved: "neutral",
  Closed: "danger",
};

type StatusBadgeProps = { status: ConversationStatus };

/** AGENTS.md §8: vira data-testid="status-<STATUS>". */
export function StatusBadge({ status }: StatusBadgeProps) {
  return (
    <span className="status-badge" data-tone={toneByStatus[status]} data-testid={`status-${status}`}>
      {conversationStatusLabels[status]}
    </span>
  );
}
