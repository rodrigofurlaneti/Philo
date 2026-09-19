import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Toaster } from "sonner";
import App from "./App";
import { ErrorBoundary } from "./components/ErrorBoundary";
import { ApiError } from "./lib/apiClient";
import { useThemeStore } from "./stores/themeStore";
import "./styles/global.css";

// Escuro é o padrão, aplicado antes do primeiro paint (AGENTS.md §7).
document.documentElement.dataset.theme = useThemeStore.getState().theme;

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 10_000,
      refetchOnWindowFocus: true,
      retry: (failureCount, error) => {
        // 401/403/404/409 não melhoram tentando de novo (AGENTS.md §4).
        if (error instanceof ApiError && [401, 403, 404, 409].includes(error.status)) return false;
        return failureCount < 2;
      },
    },
  },
});

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <App />
          <Toaster richColors position="top-right" theme="dark" />
        </BrowserRouter>
      </QueryClientProvider>
    </ErrorBoundary>
  </StrictMode>,
);
