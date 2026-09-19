import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import * as api from "./api";
import { useAuthStore } from "../../stores/authStore";
import { ApiError } from "../../lib/apiClient";

/** Cada feature exporta um objeto *Keys com all(userId) e as chaves específicas (AGENTS.md §4). */
export const conversationsKeys = {
  all: (userId: number | null) => ["conversations", userId] as const,
  mine: (userId: number | null) => [...conversationsKeys.all(userId), "mine"] as const,
  queue: (userId: number | null, status?: string) => [...conversationsKeys.all(userId), "queue", status ?? "all"] as const,
  details: (userId: number | null, conversationId: number) => [...conversationsKeys.all(userId), "details", conversationId] as const,
};

export const messagesKeys = {
  all: (userId: number | null, conversationId: number) => ["messages", userId, conversationId] as const,
  history: (userId: number | null, conversationId: number) => [...messagesKeys.all(userId, conversationId), "history"] as const,
  unreadCount: (userId: number | null, conversationId: number) => [...messagesKeys.all(userId, conversationId), "unread-count"] as const,
};

function describeConversationError(error: unknown): string {
  if (!(error instanceof ApiError)) return "Não foi possível concluir. Tente de novo.";
  switch (error.code) {
    case "Conversation.CustomerMustBeCustomerRole":
      return "Só clientes podem abrir uma conversa nova.";
    case "Membership.NotFound":
      return "Usuário sem vínculo com esta organização.";
    case "Message.ConversationClosed":
      return "Esta conversa está fechada. Reabra para continuar.";
    case "Message.ReplyNotVisible":
      return "A mensagem respondida não está mais visível.";
    case "Participant.RequiresStaff":
    case "Auth.Forbidden":
      return "Você não tem permissão para essa ação.";
    default:
      return error.message || "Não foi possível concluir.";
  }
}

export function useMyConversations() {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  return useQuery({
    queryKey: conversationsKeys.mine(userId),
    queryFn: () => api.getMyConversations(organizationId!),
    enabled: organizationId !== null,
    staleTime: 10_000,
  });
}

export function useQueue(status?: string) {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  return useQuery({
    queryKey: conversationsKeys.queue(userId, status),
    queryFn: () => api.getQueue(organizationId!, status),
    enabled: organizationId !== null,
    staleTime: 10_000,
  });
}

export function useConversationDetails(conversationId: number | null) {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  return useQuery({
    queryKey: conversationsKeys.details(userId, conversationId ?? 0),
    queryFn: () => api.getConversationDetails(organizationId!, conversationId!),
    enabled: organizationId !== null && conversationId !== null,
    staleTime: 60_000,
  });
}

export function useOpenConversation() {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: api.OpenConversationPayload) => api.openConversation(organizationId!, payload),
    onSuccess: () => {
      toast.success("Conversa iniciada.");
      queryClient.invalidateQueries({ queryKey: conversationsKeys.all(userId) });
    },
    onError: (error) => toast.error(describeConversationError(error)),
  });
}

export function useAssignConversation() {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ conversationId, agentId }: { conversationId: number; agentId: number }) =>
      api.assignConversation(organizationId!, conversationId, agentId),
    onSuccess: () => {
      toast.success("Conversa atribuída.");
      queryClient.invalidateQueries({ queryKey: conversationsKeys.all(userId) });
    },
    onError: (error) => toast.error(describeConversationError(error)),
  });
}

export function useChangeConversationStatus() {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ conversationId, status }: { conversationId: number; status: string }) =>
      api.changeConversationStatus(organizationId!, conversationId, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: conversationsKeys.all(userId) });
    },
    onError: (error) => toast.error(describeConversationError(error)),
  });
}

export function useCloseConversation() {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (conversationId: number) => api.closeConversation(organizationId!, conversationId),
    onSuccess: () => {
      toast.success("Conversa fechada.");
      queryClient.invalidateQueries({ queryKey: conversationsKeys.all(userId) });
    },
    onError: (error) => toast.error(describeConversationError(error)),
  });
}

export function useReopenConversation() {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (conversationId: number) => api.reopenConversation(organizationId!, conversationId),
    onSuccess: () => {
      toast.success("Conversa reaberta.");
      queryClient.invalidateQueries({ queryKey: conversationsKeys.all(userId) });
    },
    onError: (error) => toast.error(describeConversationError(error)),
  });
}

export function useHistory(conversationId: number | null) {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  return useQuery({
    queryKey: messagesKeys.history(userId, conversationId ?? 0),
    queryFn: () => api.getHistory(organizationId!, conversationId!),
    enabled: organizationId !== null && conversationId !== null,
    staleTime: 5_000,
    refetchInterval: 8_000,
  });
}

export function useSendTextMessage(conversationId: number) {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: api.SendTextPayload) => api.sendTextMessage(organizationId!, conversationId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: messagesKeys.all(userId, conversationId) });
      queryClient.invalidateQueries({ queryKey: conversationsKeys.all(userId) });
    },
    onError: (error) => toast.error(describeConversationError(error)),
  });
}

export function useDeleteMessage(conversationId: number) {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (messageId: number) => api.deleteMessage(organizationId!, conversationId, messageId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: messagesKeys.all(userId, conversationId) }),
    onError: (error) => toast.error(describeConversationError(error)),
  });
}

export function useMarkRead(conversationId: number) {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (messageId: number) => api.markRead(organizationId!, conversationId, messageId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: messagesKeys.all(userId, conversationId) }),
  });
}

export function useToggleStar(conversationId: number) {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ messageId, starred }: { messageId: number; starred: boolean }) =>
      starred ? api.unstarMessage(organizationId!, conversationId, messageId) : api.starMessage(organizationId!, conversationId, messageId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: messagesKeys.all(userId, conversationId) }),
    onError: (error) => toast.error(describeConversationError(error)),
  });
}

export function useHideMessage(conversationId: number) {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (messageId: number) => api.hideMessage(organizationId!, conversationId, messageId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: messagesKeys.all(userId, conversationId) }),
    onError: (error) => toast.error(describeConversationError(error)),
  });
}
