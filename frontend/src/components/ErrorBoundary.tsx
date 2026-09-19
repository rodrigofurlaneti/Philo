import { Component } from "react";
import type { ErrorInfo, ReactNode } from "react";

type ErrorBoundaryState = { error: Error | null };

export class ErrorBoundary extends Component<{ children: ReactNode }, ErrorBoundaryState> {
  state: ErrorBoundaryState = { error: null };

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error("Erro não tratado na árvore de componentes:", error, info.componentStack);
  }

  render() {
    if (this.state.error) {
      return (
        <div className="empty-state" style={{ height: "100dvh" }}>
          <h2>Algo deu errado.</h2>
          <p className="muted">Recarregue a página. Se o problema continuar, avise o suporte.</p>
          <button className="btn btn--primary" onClick={() => window.location.reload()}>
            Recarregar
          </button>
        </div>
      );
    }
    return this.props.children;
  }
}
