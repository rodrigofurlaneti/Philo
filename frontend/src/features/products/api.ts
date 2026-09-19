import { api } from "../../lib/apiClient";
import type { CreatedResponse, Product } from "../../lib/types";

export const getProducts = (organizationId: number): Promise<Product[]> =>
  api<Product[]>(`/api/organizations/${organizationId}/products`);

export type ProductPayload = { name: string; sku: string | null; pageUrl: string | null };

export const createProduct = (organizationId: number, payload: ProductPayload): Promise<CreatedResponse> =>
  api<CreatedResponse>(`/api/organizations/${organizationId}/products`, {
    method: "POST",
    body: JSON.stringify(payload),
  });

export const updateProduct = (organizationId: number, productId: number, payload: ProductPayload): Promise<void> =>
  api<void>(`/api/organizations/${organizationId}/products/${productId}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });

export const deactivateProduct = (organizationId: number, productId: number): Promise<void> =>
  api<void>(`/api/organizations/${organizationId}/products/${productId}`, { method: "DELETE" });
