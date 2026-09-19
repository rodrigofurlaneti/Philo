import type { ReactNode } from "react";
import { NavLink, useNavigate } from "react-router-dom";
import { useAuthStore } from "../stores/authStore";
import { ThemeToggle } from "./ThemeToggle";

type AppShellProps = { children: ReactNode };

/** Casca do app: trilho de navegação + conteúdo (AGENTS.md §2 — componente de aplicação). */
export function AppShell({ children }: AppShellProps) {
  const role = useAuthStore((s) => s.role);
  const clear = useAuthStore((s) => s.clear);
  const navigate = useNavigate();
  const isStaff = role === "Agent" || role === "Admin";

  function logout() {
    clear();
    navigate("/login", { replace: true });
  }

  return (
    <div className="app-shell">
      <nav className="app-rail" aria-label="Navegação principal">
        <div className="app-rail__brand" aria-hidden="true">
          Φ
        </div>
        <div className="app-rail__nav">
          <NavLink to="/conversas" className="app-rail__item" aria-label="Conversas" data-testid="nav-conversations">
            💬
          </NavLink>
          {isStaff && (
            <NavLink to="/fila" className="app-rail__item" aria-label="Fila da equipe" data-testid="nav-queue">
              📥
            </NavLink>
          )}
          {isStaff && (
            <NavLink to="/produtos" className="app-rail__item" aria-label="Produtos" data-testid="nav-products">
              🏷️
            </NavLink>
          )}
        </div>
        <ThemeToggle />
        <button className="btn btn--icon" aria-label="Sair" onClick={logout} data-testid="logout-button">
          ⏻
        </button>
      </nav>
      <div style={{ minWidth: 0 }}>{children}</div>
    </div>
  );
}
