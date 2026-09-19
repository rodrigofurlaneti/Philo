import type { ConversationPurpose, ConversationStatus, Priority, ProductStatus, Role } from "./types";

export const conversationStatusLabels: Record<ConversationStatus, string> = {
  Open: "Aberta",
  WaitingCustomer: "Aguardando cliente",
  WaitingTeam: "Aguardando equipe",
  Closed: "Fechada",
};

export const conversationPurposeLabels: Record<ConversationPurpose, string> = {
  Sales: "Vendas",
  Support: "Suporte",
};

export const priorityLabels: Record<Priority, string> = {
  Low: "Baixa",
  Normal: "Normal",
  High: "Alta",
  Urgent: "Urgente",
};

export const roleLabels: Record<Role, string> = {
  Customer: "Cliente",
  Agent: "Atendente",
  Admin: "Administrador",
};

export const productStatusLabels: Record<ProductStatus, string> = {
  Active: "Ativo",
  Inactive: "Inativo",
};
