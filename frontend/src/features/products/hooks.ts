import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import * as api from "./api";
import { useAuthStore } from "../../stores/authStore";
import { describeError } from "../../lib/describeError";

export const productsKeys = {
  all: (userId: number | null) => ["products", userId] as const,
};

export function useProducts() {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  return useQuery({
    queryKey: productsKeys.all(userId),
    queryFn: () => api.getProducts(organizationId!),
    enabled: organizationId !== null,
    staleTime: 60_000,
  });
}

export function useCreateProduct() {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: api.ProductPayload) => api.createProduct(organizationId!, payload),
    onSuccess: () => {
      toast.success("Produto criado.");
      queryClient.invalidateQueries({ queryKey: productsKeys.all(userId) });
    },
    onError: (error) => toast.error(describeError(error)),
  });
}

export function useUpdateProduct() {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, payload }: { productId: number; payload: api.ProductPayload }) =>
      api.updateProduct(organizationId!, productId, payload),
    onSuccess: () => {
      toast.success("Produto atualizado.");
      queryClient.invalidateQueries({ queryKey: productsKeys.all(userId) });
    },
    onError: (error) => toast.error(describeError(error)),
  });
}

export function useDeactivateProduct() {
  const organizationId = useAuthStore((s) => s.organizationId);
  const userId = useAuthStore((s) => s.userId);
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (productId: number) => api.deactivateProduct(organizationId!, productId),
    onSuccess: () => {
      toast.success("Produto desativado.");
      queryClient.invalidateQueries({ queryKey: productsKeys.all(userId) });
    },
    onError: (error) => toast.error(describeError(error)),
  });
}
