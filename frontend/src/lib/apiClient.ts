import { useAuthStore } from "../stores/authStore";

/**
 * Ponto único de acesso HTTP (AGENTS.md §3). Nunca chame `fetch` fora deste arquivo —
 * o ESLint reprova (`no-restricted-globals`).
 */

export class ApiError extends Error {
  readonly status: number;
  readonly code: string;
  readonly fieldErrors?: Record<string, string[]>;

  constructor(status: number, code: string, message?: string, fieldErrors?: Record<string, string[]>) {
    super(message ?? code);
    this.name = "ApiError";
    this.status = status;
    this.code = code;
    this.fieldErrors = fieldErrors;
  }
}

type ProblemDetailsShape = {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
};

function isAuthenticatedRequest(headers: HeadersInit | undefined): boolean {
  if (!headers) return false;
  const record = headers as Record<string, string>;
  return typeof record.Authorization === "string" && record.Authorization.length > 0;
}

async function buildAuthHeaders(): Promise<HeadersInit> {
  const { accessToken } = useAuthStore.getState();
  return accessToken ? { Authorization: `Bearer ${accessToken}` } : {};
}

async function parseProblem(response: Response): Promise<ApiError> {
  let body: ProblemDetailsShape | undefined;
  try {
    body = (await response.json()) as ProblemDetailsShape;
  } catch {
    body = undefined;
  }

  if (body?.errors) {
    return new ApiError(response.status, "Validation.Failed", body.detail ?? "Dados inválidos.", body.errors);
  }

  const code = body?.title ?? `Http.${response.status}`;
  const message = body?.detail ?? response.statusText;
  return new ApiError(response.status, code, message);
}

export async function api<T>(path: string, init: RequestInit = {}): Promise<T> {
  const authHeaders = await buildAuthHeaders();
  const headers: HeadersInit = {
    "Content-Type": "application/json",
    Accept: "application/json",
    ...authHeaders,
    ...init.headers,
  };

  let response: Response;
  try {
    response = await fetch(path, { ...init, headers });
  } catch {
    throw new ApiError(0, "Network.Unreachable", "Não foi possível conectar ao servidor.");
  }

  if (response.status === 204) return undefined as T;

  if (!response.ok) {
    const authenticated = isAuthenticatedRequest(headers);
    if (response.status === 401 && authenticated) {
      useAuthStore.getState().clear();
      throw new ApiError(401, "Auth.SessionExpired", "Sua sessão expirou. Entre novamente.");
    }
    throw await parseProblem(response);
  }

  const text = await response.text();
  return (text ? (JSON.parse(text) as T) : (undefined as T));
}

export async function apiUpload<T>(path: string, file: Blob, contentType: string): Promise<T> {
  const authHeaders = await buildAuthHeaders();
  let response: Response;
  try {
    response = await fetch(path, {
      method: "PUT",
      headers: { "Content-Type": contentType, ...authHeaders },
      body: file,
    });
  } catch {
    throw new ApiError(0, "Network.Unreachable", "Não foi possível conectar ao servidor.");
  }

  if (response.status === 204) return undefined as T;
  if (!response.ok) throw await parseProblem(response);

  const text = await response.text();
  return (text ? (JSON.parse(text) as T) : (undefined as T));
}

/** Monta querystring ignorando undefined, null e string vazia. */
export function toQuery(params: Record<string, string | number | boolean | undefined | null>): string {
  const entries = Object.entries(params).filter(
    ([, value]) => value !== undefined && value !== null && value !== "",
  );
  if (entries.length === 0) return "";
  const search = new URLSearchParams();
  for (const [key, value] of entries) search.set(key, String(value));
  return `?${search.toString()}`;
}
