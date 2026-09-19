import type { ReactNode } from "react";
import { Navigate, Route, Routes } from "react-router-dom";
import { RequireAuth } from "./components/RequireAuth";
import { AppShell } from "./components/AppShell";
import { LoginPage } from "./features/auth/LoginPage";
import { ConversationsPage } from "./features/conversations/ConversationsPage";
import { QueuePage } from "./features/conversations/QueuePage";
import { ProductsPage } from "./features/products/ProductsPage";

function Protected({ children }: { children: ReactNode }) {
  return (
    <RequireAuth>
      <AppShell>{children}</AppShell>
    </RequireAuth>
  );
}

/** Todas as rotas, em um arquivo só (AGENTS.md §2/§6). */
export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route
        path="/conversas"
        element={
          <Protected>
            <ConversationsPage />
          </Protected>
        }
      />
      <Route
        path="/conversas/:conversationId"
        element={
          <Protected>
            <ConversationsPage />
          </Protected>
        }
      />

      <Route
        path="/fila"
        element={
          <Protected>
            <QueuePage />
          </Protected>
        }
      />
      <Route
        path="/fila/:conversationId"
        element={
          <Protected>
            <QueuePage />
          </Protected>
        }
      />

      <Route
        path="/produtos"
        element={
          <Protected>
            <ProductsPage />
          </Protected>
        }
      />

      <Route path="/" element={<Navigate to="/conversas" replace />} />
      <Route path="*" element={<Navigate to="/conversas" replace />} />
    </Routes>
  );
}
