import { z } from "zod";

export const newConversationSchema = z.object({
  purpose: z.enum(["Sales", "Support"]),
  priority: z.enum(["Low", "Normal", "High", "Urgent"]),
  subject: z.string().trim().max(200).optional(),
});
export type NewConversationFormData = z.infer<typeof newConversationSchema>;
