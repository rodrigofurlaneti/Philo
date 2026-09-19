import { api, toQuery } from "../../lib/apiClient";
import type {
  ConversationDetails,
  ConversationPurpose,
  ConversationSummary,
  CreatedResponse,
  Message,
  Priority,
} from "../../lib/types";

export const getMyConversations = (organizationId: number, limit = 50): Promise<ConversationSummary[]> =>
  api<ConversationSummary[]>(`/api/organizations/${organizationId}/conversations/mine${toQuery({ limit })}`);

export const getQueue = (organizationId: number, status?: string, limit = 50): Promise<ConversationSummary[]> =>
  api<ConversationSummary[]>(`/api/organizations/${organizationId}/conversations/queue${toQuery({ status, limit })}`);

export const getConversationDetails = (organizationId: number, conversationId: number): Promise<ConversationDetails> =>
  api<ConversationDetails>(`/api/organizations/${organizationId}/conversations/${conversationId}`);

export type OpenConversationPayload = {
  customerId: number;
  purpose: ConversationPurpose;
  subject: string | null;
  priority: Priority;
  sourcePageUrl: string | null;
};

export const openConversation = (organizationId: number, payload: OpenConversationPayload): Promise<CreatedResponse> =>
  api<CreatedResponse>(`/api/organizations/${organizationId}/conversations`, {
    method: "POST",
    body: JSON.stringify(payload),
  });

export const assignConversation = (organizationId: number, conversationId: number, agentId: number): Promise<void> =>
  api<void>(`/api/organizations/${organizationId}/conversations/${conversationId}/assign`, {
    method: "POST",
    body: JSON.stringify({ agentId }),
  });

export const changeConversationStatus = (organizationId: number, conversationId: number, status: string): Promise<void> =>
  api<void>(`/api/organizations/${organizationId}/conversations/${conversationId}/status`, {
    method: "POST",
    body: JSON.stringify({ status }),
  });

export const closeConversation = (organizationId: number, conversationId: number): Promise<void> =>
  api<void>(`/api/organizations/${organizationId}/conversations/${conversationId}/close`, { method: "POST" });

export const reopenConversation = (organizationId: number, conversationId: number): Promise<void> =>
  api<void>(`/api/organizations/${organizationId}/conversations/${conversationId}/reopen`, { method: "POST" });

export type HistoryPage = { cursorSentAt?: string; cursorId?: number; limit?: number };

export const getHistory = (organizationId: number, conversationId: number, page: HistoryPage = {}): Promise<Message[]> =>
  api<Message[]>(
    `/api/organizations/${organizationId}/conversations/${conversationId}/messages${toQuery({
      cursorSentAt: page.cursorSentAt,
      cursorId: page.cursorId,
      limit: page.limit ?? 50,
    })}`,
  );

export const getUnreadCount = (organizationId: number, conversationId: number): Promise<number> =>
  api<number>(`/api/organizations/${organizationId}/conversations/${conversationId}/messages/unread-count`);

export type SendTextPayload = { clientMessageId: string; body: string; replyToId: number | null; expiresAt: string | null };

export const sendTextMessage = (organizationId: number, conversationId: number, payload: SendTextPayload): Promise<CreatedResponse> =>
  api<CreatedResponse>(`/api/organizations/${organizationId}/conversations/${conversationId}/messages/text`, {
    method: "POST",
    body: JSON.stringify(payload),
  });

export const editMessage = (organizationId: number, conversationId: number, messageId: number, body: string): Promise<void> =>
  api<void>(`/api/organizations/${organizationId}/conversations/${conversationId}/messages/${messageId}`, {
    method: "PUT",
    body: JSON.stringify({ body }),
  });

export const deleteMessage = (organizationId: number, conversationId: number, messageId: number): Promise<void> =>
  api<void>(`/api/organizations/${organizationId}/conversations/${conversationId}/messages/${messageId}`, { method: "DELETE" });

export const markDelivered = (organizationId: number, conversationId: number, messageId: number): Promise<void> =>
  api<void>(`/api/organizations/${organizationId}/conversations/${conversationId}/messages/${messageId}/delivered`, { method: "POST" });

export const markRead = (organizationId: number, conversationId: number, messageId: number): Promise<void> =>
  api<void>(`/api/organizations/${organizationId}/conversations/${conversationId}/messages/${messageId}/read`, { method: "POST" });

export const starMessage = (organizationId: number, conversationId: number, messageId: number): Promise<void> =>
  api<void>(`/api/organizations/${organizationId}/conversations/${conversationId}/messages/${messageId}/preferences/star`, { method: "PUT" });

export const unstarMessage = (organizationId: number, conversationId: number, messageId: number): Promise<void> =>
  api<void>(`/api/organizations/${organizationId}/conversations/${conversationId}/messages/${messageId}/preferences/star`, { method: "DELETE" });

export const hideMessage = (organizationId: number, conversationId: number, messageId: number): Promise<void> =>
  api<void>(`/api/organizations/${organizationId}/conversations/${conversationId}/messages/${messageId}/preferences/hide`, { method: "PUT" });
