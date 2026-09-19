import { z } from "zod";

export const visitorSchema = z.object({
  displayName: z.string().trim().min(2, "Digite pelo menos 2 letras.").max(100),
});
export type VisitorFormData = z.infer<typeof visitorSchema>;

export const staffTokenSchema = z.object({
  accessToken: z.string().trim().min(10, "Cole o token de acesso da equipe."),
});
export type StaffTokenFormData = z.infer<typeof staffTokenSchema>;
