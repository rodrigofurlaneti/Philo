import { z } from "zod";

export const productSchema = z.object({
  name: z.string().trim().min(2, "Digite o nome do produto.").max(150),
  sku: z.string().trim().max(60).optional(),
  pageUrl: z.string().trim().max(2048).optional(),
});
export type ProductFormData = z.infer<typeof productSchema>;
