import { useState } from "react";
import { PageHeader } from "../../components/PageHeader";
import { QueryError } from "../../components/QueryError";
import { DataTable } from "../../components/DataTable";
import { Button } from "../../ui";
import { confirmDialog } from "../../lib/swal";
import { productStatusLabels } from "../../lib/labels";
import type { Product } from "../../lib/types";
import { useCreateProduct, useDeactivateProduct, useProducts, useUpdateProduct } from "./hooks";
import { ProductFormModal } from "./ProductFormModal";

export function ProductsPage() {
  const products = useProducts();
  const createProduct = useCreateProduct();
  const updateProduct = useUpdateProduct();
  const deactivateProduct = useDeactivateProduct();
  const [editing, setEditing] = useState<Product | "new" | null>(null);

  async function handleDeactivate(product: Product) {
    const confirmed = await confirmDialog({
      title: `Desativar ${product.name}?`,
      text: "O produto deixa de aparecer para vínculo em novas conversas.",
      danger: true,
      confirmLabel: "Desativar",
    });
    if (confirmed) deactivateProduct.mutate(product.id);
  }

  return (
    <div className="page">
      <PageHeader
        title="Produtos"
        subtitle="Catálogo vinculado às conversas de vendas e suporte."
        actions={
          <Button onClick={() => setEditing("new")} data-testid="new-product">
            + Produto
          </Button>
        }
      />

      {products.isLoading && <p className="muted">Carregando…</p>}
      {products.isError && <QueryError error={products.error} what="os produtos" onRetry={() => products.refetch()} />}

      {products.data && (
        <DataTable
          testId="products-table"
          rows={products.data}
          rowKey={(p) => p.id}
          emptyMessage="Nenhum produto cadastrado ainda."
          columns={[
            { header: "Nome", render: (p) => p.name },
            { header: "SKU", render: (p) => p.sku ?? "—" },
            { header: "Status", render: (p) => productStatusLabels[p.status] },
            {
              header: "Ações",
              align: "right",
              render: (p) => (
                <div className="row" style={{ justifyContent: "flex-end" }}>
                  <Button variant="ghost" onClick={() => setEditing(p)} data-testid="edit-product">
                    Editar
                  </Button>
                  {p.status === "Active" && (
                    <Button variant="ghost" onClick={() => handleDeactivate(p)} data-testid="deactivate-product">
                      Desativar
                    </Button>
                  )}
                </div>
              ),
            },
          ]}
        />
      )}

      {editing && (
        <ProductFormModal
          product={editing === "new" ? undefined : editing}
          onClose={() => setEditing(null)}
          onSubmit={async (payload) => {
            if (editing === "new") await createProduct.mutateAsync(payload);
            else await updateProduct.mutateAsync({ productId: editing.id, payload });
          }}
        />
      )}
    </div>
  );
}
