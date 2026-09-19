/** Tipos de resposta da API compartilhados entre features (AGENTS.md §2). */

export type Role = "Customer" | "Agent" | "Admin";

export type SessionResponse = {
  accessToken: string;
  expiresAtUtc: string;
  userId: number;
  organizationId: number;
  role: Role;
};

export type ConversationPurpose = "Sales" | "Support";
export type ConversationStatus = "Open" | "Pending" | "Resolved" | "Closed";
export type Priority = "Low" | "Normal" | "High" | "Urgent";

export type ConversationSummary = {
  id: number;
  customerId: number;
  assignedTo: number | null;
  purpose: ConversationPurpose;
  subject: string | null;
  status: ConversationStatus;
  priority: Priority;
  lastActivityAt: string;
};

export type ConversationParticipant = {
  userId: number;
  joinedAt: string;
  leftAt: string | null;
  isActive: boolean;
};

export type ConversationDetails = {
  id: number;
  organizationId: number;
  customerId: number;
  assignedTo: number | null;
  createdBy: number;
  purpose: ConversationPurpose;
  subject: string | null;
  status: ConversationStatus;
  priority: Priority;
  sourcePageUrl: string | null;
  lastActivityAt: string;
  createdAt: string;
  closedAt: string | null;
  participants: ConversationParticipant[];
};

export type MessageType = "Text" | "Attachment" | "System";

export type Message = {
  id: number;
  senderId: number;
  messageType: MessageType;
  body: string | null;
  replyToId: number | null;
  sentAt: string;
  editedAt: string | null;
  isDeleted: boolean;
  isStarred: boolean;
};

export type ProductStatus = "Active" | "Inactive";

export type Product = {
  id: number;
  sku: string | null;
  name: string;
  pageUrl: string | null;
  status: ProductStatus;
};

export type CreatedResponse = { id: number };
