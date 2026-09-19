import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { Button, Modal, TextField } from "../../ui";
import { productSchema } from "./productSchema";
import type { ProductFormData } from "./productSchema";
import { toNullableText } from "../../lib/forms";
import type { Product } from "../../lib/types";

type ProductFormModalProps = {
  product?: Product;
  onClose: () => void;
  onSubmit: (payload: { name: string; sku: string | null; pageUrl: string | null }) => Promise<void>;
};

export function ProductFormModal({ product, onClose, onSubmit }: ProductFormModalProps) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ProductFormData>({
    resolver: zodResolver(productSchema),
    mode: "onBlur",
    defaultValues: { name: product?.name ?? "", sku: product?.sku ?? "", pageUrl: product?.pageUrl ?? "" },
  });

  async function submit(data: ProductFormData) {
    await onSubmit({ name: data.name, sku: toNullableText(data.sku ?? ""), pageUrl: toNullableText(data.pageUrl ?? "") });
    onClose();
  }

  return (
    <Modal
      title={product ? "Editar produto" : "Novo produto"}
      onClose={onClose}
      actions={
        <>
          <Button variant="ghost" type="button" onClick={onClose}>
            Cancelar
          </Button>
          <Button form="product-form" type="submit" disabled={isSubmitting} data-testid="product-form-submit">
            Salvar
          </Button>
        </>
      }
    >
      <form id="product-form" className="stack" onSubmit={handleSubmit(submit)}>
        <TextField label="Nome" error={errors.name?.message} data-testid="product-name" {...register("name")} />
        <TextField label="SKU (opcional)" error={errors.sku?.message} data-testid="product-sku" {...register("sku")} />
        <TextField label="URL da página (opcional)" error={errors.pageUrl?.message} data-testid="product-page-url" {...register("pageUrl")} />
      </form>
    </Modal>
  );
}
