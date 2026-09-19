# AGENTS.md — Frontend Philo

Guia de padrões do frontend. Leia antes de criar ou alterar qualquer código em `frontend/`.

Descreve o que o projeto **faz hoje**, não um ideal. Quando houver divergência entre este
documento e o código, o código vence — e o documento deve ser corrigido.

---

## 1. Stack

| Item | Escolha |
|---|---|
| Build | Vite 5 + `@vitejs/plugin-react` |
| Linguagem | TypeScript 5.6, `strict: true`, `noUnusedLocals`, `noUnusedParameters` |
| UI | React 18 |
| Rotas | react-router-dom 6 |
| Estado de servidor | **TanStack React Query 5** |
| Estado de cliente | **Zustand 4** com `persist` |
| Formulários | react-hook-form 7 + **Zod 3** via `@hookform/resolvers` |
| Gráficos | **Recharts 2** |
| Notificação | `sonner` (toast) e `sweetalert2` (diálogo modal) |
| Estilo | **CSS puro com custom properties** — ver §7 |
| Lint | ESLint 9 (flat config) + typescript-eslint |
| Unitários/componentes | Vitest 3 + React Testing Library; cobertura Istanbul |
| E2E | Playwright desktop/mobile + Istanbul |
| Docs de componente | Storybook 8 |

Scripts:

```bash
npm run dev        # vite, porta 5173
npm run build      # tsc -b && vite build  ← gate do CI
npm run lint       # eslint . — também é gate
npm run test:coverage # unitários + componentes
npm run test:typecheck # tipos dos testes
npm run test:e2e:coverage # navegador + API isolada
npm run coverage:merge # HTML/LCOV para SonarCloud
npm run test:e2e   # modo opcional com API local preparada
npm run storybook
```

Não há máscara de documento no Philo (não existe CPF/CNPJ/CEP no domínio), então
`react-input-mask` **não** está na stack. Dinheiro usa `ui/MoneyField` — ver §5.

---

## 2. Estrutura

```
src/
  main.tsx        providers: ErrorBoundary > QueryClient > BrowserRouter > App + Toaster
  App.tsx         TODAS as rotas, em um arquivo só
  features/<área>/   uma pasta por domínio de negócio
  components/     componentes de aplicação (conhecem domínio/estado): AppShell,
                  RequireAuth, DataTable, MetricCard, PageHeader, QueryError,
                  ThemeToggle, ErrorBoundary
  ui/             primitivos burros e reutilizáveis: Button, Field, MoneyField, Modal,
                  StatusBadge, EmptyState, Skeleton — exportados por ui/index.ts
  lib/
    apiClient.ts  api(), apiUpload(), toQuery(), ApiError — ver §3
    types.ts      tipos de resposta da API compartilhados entre features
    describeError.ts  tradução de ApiError.code → texto
    forms.ts      conversores para setValueAs (toNullableId, toNullableText)
    format.ts     dinheiro, datas e recortes de mês em pt-BR
    labels.ts     rótulos dos enums do domínio
    swal.ts       wrappers do sweetalert2
  stores/         authStore, themeStore (Zustand + persist)
  styles/global.css   design system inteiro (arquivo único)
test/features/<área>/   specs Playwright espelhando features/
test/support/session.ts fixtures de usuário e login
```

**`ui/` vs `components/`:** se o componente importa store, React Query ou tipo de domínio,
ele é `components/`. Se recebe tudo por prop e serve a qualquer tela, é `ui/`.

### Anatomia de uma feature

```
features/<área>/
  <Área>Page.tsx        componente de página, ligado a uma rota em App.tsx
  api.ts                funções de chamada HTTP tipadas
  <coisa>Schema.ts      schema Zod, quando há formulário
  hooks.ts              hooks de React Query da área
  <Painel>.tsx          subcomponentes da tela (modais, filtros, gráficos)
```

Nomes de pasta são minúsculos, sem separador e **no plural** (`transactions`, `categories`,
`contacts`, `users` → aqui `auth`/`profile` por serem áreas, não entidades). O plural não é
estética: no backend o singular colidia com o nome das entidades de domínio, e manter os dois
lados no plural evita ter que lembrar de qual lado está a exceção.

---

## 3. Acesso à API — sempre pelo `lib/apiClient`

**Nunca chame `fetch` direto.** Use `api<T>()` ou `apiUpload<T>()`. O ESLint reprova `fetch`
fora de `lib/apiClient.ts` (`no-restricted-globals`).

```ts
import { api, toQuery } from "../../lib/apiClient";
import type { CategoryListResponse, CreatedResponse } from "../../lib/types";

export const getCategories = (type?: TransactionType): Promise<CategoryListResponse[]> =>
  api<CategoryListResponse[]>(`/api/categories${toQuery({ type })}`);

export const createCategory = (payload: CategoryPayload): Promise<CreatedResponse> =>
  api<CreatedResponse>("/api/categories", {
    method: "POST",
    body: JSON.stringify(payload),
  });
```

O que o client faz por você:

- injeta `Authorization: Bearer <accessToken>` quando há sessão;
- em **401 de requisição autenticada**, limpa a sessão e lança
  `ApiError(401, "Auth.SessionExpired")`;
- em **401 de requisição anônima** (login), devolve o erro do domínio intacto —
  senha errada precisa chegar na tela como `AppUser.InvalidCredentials`, não como
  "sua sessão expirou";
- traduz `ProblemDetails`: `ApiError.code` recebe o `title` (que é o `Error.Code` do
  backend, ex.: `"Category.NotFound"`) e `ApiError.message` recebe o `detail`;
- agrupa `ValidationProblemDetails` em `ApiError.fieldErrors` com
  `code = "Validation.Failed"`;
- devolve `undefined` em **204**;
- converte falha de rede em `ApiError(0, "Network.Unreachable")`;
- `toQuery()` monta a querystring ignorando `undefined`, `null` e string vazia.

O Philo é de **um usuário só**: não existe header de tenant (`X-Company-Id` e afins) nem
`/api/auth/refresh`. Quando o backend expuser refresh, o retry entra no `apiClient` — nunca
numa feature.

### Tratar erro por código, não por texto

```ts
export function describeAuthError(error: unknown): string {
  if (!(error instanceof ApiError)) return "Não foi possível concluir. Tente de novo.";
  switch (error.code) {
    case "AppUser.InvalidCredentials": return "Usuário ou senha inválidos.";
    case "AppUser.LockedOut":          return "Conta bloqueada por tentativas seguidas. Aguarde 15 minutos.";
    case "AppUser.Inactive":           return "Esta conta está desativada.";
    default:                           return error.message || "Não foi possível concluir.";
  }
}
```

Códigos transversais (`Network.Unreachable`, `Auth.SessionExpired`, `Validation.Failed`) já
estão em `lib/describeError.ts`; códigos de uma área ficam no `hooks.ts` dela.

---

## 4. React Query

Configuração global em `main.tsx`: `staleTime: 10_000`, `refetchOnWindowFocus: true` e
`retry` que **não insiste em erro 4xx** — 401/404/409 não melhoram tentando de novo.

- **`queryKey` inclui o escopo**: `["categories", userId, type]`,
  `["transactions", userId, "list", filters]`. Chave sem o `userId` serve o cache do
  usuário anterior quando alguém troca de conta na mesma aba.
- Cada feature exporta um objeto `*Keys` com `all(userId)` e as chaves específicas; a
  mutação invalida por `all`, não por chave exata.
- `enabled:` para query que depende de pré-condição (`enabled: userId !== null`, detalhe só
  quando o modal abre).
- `staleTime` maior para dado que quase não muda (categorias, contatos, perfil: `60_000`).
- Hooks de query ficam em `features/<área>/hooks.ts`.
- Erro de carregamento renderiza `<QueryError error={q.error} what="as categorias" onRetry={…} />`.
- Mutação usa `useMutation` + `toast.success` / `toast.error` do `sonner`, e invalida a query
  afetada no `onSuccess`.

---

## 5. Formulários

Padrão: react-hook-form + `zodResolver`, com `mode: "onBlur"`.

```tsx
const { register, handleSubmit, control, reset,
        formState: { errors, isSubmitting } } = useForm<CategoryFormData>({
  resolver: zodResolver(categorySchema),
  mode: "onBlur",
  defaultValues: emptyForm,
});
```

- O schema Zod fica em arquivo próprio (`categorySchema.ts`) e exporta o tipo:
  `export type CategoryFormData = z.infer<typeof categorySchema>`.
- Campo controlado (dinheiro, seletor de cor) usa `<Controller>`.
- **Todo campo que recebe `{...register(...)}` precisa de `forwardRef`.** O `register()`
  devolve um `ref` junto com `name`/`onChange`/`onBlur`; se o componente não repassar esse
  ref ao elemento nativo, o React o consome no componente de função, o campo **nunca é
  registrado** e o Zod valida `undefined` — o sintoma é "Required" num campo visivelmente
  preenchido. `ui/Field.tsx` já faz isso; copie o padrão.
- **Select opcional de id usa `setValueAs: toNullableId`.** Sem isso, `<select>` vazio vira
  `Number("") === 0` e o Zod acusa "Number must be greater than 0" num campo que o usuário
  nem tocou. Texto opcional usa `toNullableText`.
- **Dinheiro usa `ui/MoneyField`**, não máscara. O usuário digita só dígitos, o componente
  formata os centavos e o valor que chega ao formulário é sempre `number`.
- O payload é limpo antes de enviar: string vazia → `null` em campo opcional.

---

## 6. Rotas e autenticação

Tudo em `App.tsx`. **Uma** camada de proteção:

```tsx
<Route path="/transacoes" element={
  <Protected>              {/* RequireAuth + AppShell */}
    <TransactionsPage />
  </Protected>
} />
```

- **`RequireAuth`** checa `accessToken` e a validade pelo `expiresAtUtc`; sem sessão,
  redireciona para `/login` guardando a rota de origem em `location.state.from`.
- Rotas públicas: `/login` e `/cadastro`. Todo o resto é protegido.
- Não existe `CompanyContextGate` nem `FeatureGate`. O padrão de origem (DingFood) tem os
  dois porque lá o app é multi-empresa com features por plano; aqui cada pessoa vê só os
  próprios dados e o backend já filtra pelo `AppUserId` do token. Se um dia surgir conta
  compartilhada ou plano com features, o gate novo entra **envolvendo** o children de
  `Protected` — não espalhado pelas telas.

### authStore

`userId`, `userName`, `email`, `accessToken`, `expiresAtUtc` — persistido em `localStorage`
sob a chave `Philo-auth`. Trocar essa chave desloga todo mundo; só com migração planejada.
Leia com seletor para evitar re-render desnecessário:

```ts
const accessToken = useAuthStore((s) => s.accessToken);
```

Fora de componente, use `useAuthStore.getState()` (é o que o `apiClient` faz).

---

## 7. Design system — CSS puro com tokens

**Não há Tailwind.** Todo o design system está em `src/styles/global.css`, com custom
properties em `:root`, organizado em 14 seções comentadas.

### Tokens reais (use estes)

| Categoria | Tokens |
|---|---|
| Fundo | `--bg`, `--bg-raise`, `--bg-press` |
| Linha | `--line`, `--line-soft` |
| Texto | `--ink`, `--ink-dim`, `--ink-faint` |
| Marca | `--mint`, `--mint-deep`, `--mint-ink` |
| Status | `--ok`, `--warn`, `--danger`, `--pending`, `--paid`, `--canceled`, `--income`, `--expense` |
| Gráficos | `--viz-surface`, `--viz-grid`, `--viz-1`…`--viz-6`, `--viz-income`, `--viz-expense` |
| Tipografia | `--font-display`, `--font-cond`, `--font-body` |
| Forma | `--radius`, `--radius-sm`, `--radius-lg`, `--touch` (48px, alvo de toque) |
| Espaço | `--gap-xs`, `--gap-sm`, `--gap`, `--gap-lg`, `--gap-xl` |
| Movimento | `--ease-standard`, `--ease-in-out`, `--duration-instant\|fast\|medium\|base\|entrance\|slow\|slower` |

### Tema: `data-theme`, não `prefers-color-scheme`

O tema é **escolha do usuário**, não do sistema operacional. `themeStore.ts` escreve
`document.documentElement.dataset.theme` e o CSS reage a `:root[data-theme="light"]`.
O padrão é **escuro**, aplicado em `main.tsx` antes do primeiro paint.

```css
/* ✅ certo */
.minha-coisa { background: var(--bg-raise); color: var(--ink); }
:root[data-theme="light"] .minha-coisa { border-color: var(--line-soft); }

/* ❌ errado — ignora a escolha do usuário */
@media (prefers-color-scheme: dark) { .minha-coisa { … } }
```

### Cor de gráfico

Os slots `--viz-*` são uma paleta **validada**, não escolhida no olho: banda de luminosidade,
piso de croma, separação para daltonismo (ΔE ≥ 8 entre pares adjacentes) e contraste contra a
superfície, em **cada tema separadamente** — o tema claro não é o escuro invertido.

Regras que valem para qualquer gráfico novo:

- atribua os hues em **ordem fixa** (`--viz-1`, depois `--viz-2`…), nunca ciclando; a partir
  do 7º item, agregue em "Outras" ou facete — não gere um tom novo;
- **um eixo só**; duas medidas de escalas diferentes viram dois gráficos;
- legenda sempre presente com 2+ séries e rótulo escrito — identidade nunca é só cor;
- o par receita/despesa (`--viz-income` / `--viz-expense`) fica na faixa de alerta do teste
  de daltonismo, então ele **sempre** anda com encoding secundário: ordem fixa das barras,
  legenda escrita e a visão em tabela;
- todo gráfico oferece "Ver tabela" (`ChartTable`) — é acessibilidade e é o alívio exigido
  pelos tons que ficam abaixo de 3:1 no tema claro;
- barras finas (`maxBarSize`), grade recessiva, tooltip própria (`ChartTooltip`) usando
  tinta de texto para o número e a cor da série só no marcador ao lado.

Se for trocar um slot, revalide a paleta antes — não confie no olho.

### Acessibilidade

`ui/Field.tsx` é a referência: `useId()`, `htmlFor`, `aria-invalid`, `aria-describedby`
ligando dica e erro. Use `Field` / `TextField` / `SelectField` / `TextAreaField` em vez de
montar `<label>` + `<input>` na mão. Erro usa `role="alert"`; carregamento usa `role="status"`.
`Modal` põe `role="dialog"`, `aria-modal`, fecha no `Esc` e move o foco para dentro.

---

## 8. Testes

Playwright, em `test/features/<área>/`, espelhando `src/features/`.

```bash
npm run test:e2e
npm run test:e2e:ui
```

Os testes rodam contra a **API real** (`http://localhost:5080`, perfil HTTP) e um banco de
verdade. `test/support/session.ts` cria um usuário novo por teste via `POST /api/auth/register`
e injeta a sessão no `localStorage` com `addInitScript` — nada de estado compartilhado entre
specs.

⚠️ `addInitScript` re-injeta a sessão a **cada navegação**. Teste que precisa observar a
ausência de sessão (logout, expiração) tem que entrar pela UI, não pelo helper.

**Seletores por `data-testid`.** Todo elemento interativo ou de verificação recebe um.
`DataTable` nomeia as linhas a partir do seu próprio testid: passar
`data-testid="categories-table"` faz as linhas virarem `categories-table-row`.

```tsx
<input data-testid="transaction-amount" … />
<StatusBadge status={row.status} />   {/* vira data-testid="status-PENDING" */}
```

Diálogo de confirmação é `sweetalert2`, ou seja, HTML — confirme com
`page.locator(".swal2-confirm")`, não com `page.on("dialog")`.

Unitários e componentes usam Vitest/React Testing Library em `test/unit/`.
Cubra comportamento, falhas e acessibilidade; telas também têm Playwright.
Primitivos podem ter stories, que não substituem asserções.

---

## 9. Build e CI

```bash
npm ci
npm run lint      # eslint .
npm run build     # tsc -b && vite build
```

Lint, tipos, testes unitários, build e Playwright desktop/mobile são gates do CI.
O modo `test:e2e:coverage` inicia Vite em 5175 e a WebApi isolada em 5081, com
SQLite em memória. Não depende de MySQL nem acessa produção. Consulte
`test/README.md` para a coleta e combinação da cobertura e envio ao SonarCloud.
Não use `any` para calar o compilador; `no-explicit-any` continua sendo erro.
Em desenvolvimento o proxy é do Vite:

```ts
proxy: { "/api": { target: "http://localhost:5080" }, "/health": { … } }
```

⚠️ O proxy aponta para **HTTP :5080**. Se você subir a API no perfil HTTPS, o front leva 502.
Rode a API no perfil HTTP quando estiver testando pelo frontend.

O bundle é dividido em `react`, `charts` (recharts) e `dialogs` (sweetalert2) via
`manualChunks`, para que um deploy da aplicação não invalide o cache dessas bibliotecas.

---

## 10. Receita: tela nova

1. `src/features/<área>/api.ts` — funções tipadas usando `api<T>()`.
2. Tipos de resposta em `lib/types.ts` se forem compartilhados; locais ao `api.ts` se não.
3. `src/features/<área>/hooks.ts` — `useQuery`/`useMutation` com `queryKey` contendo o `userId`.
4. `src/features/<área>/<Área>Page.tsx` — a tela, usando `ui/` e `components/`.
5. Rota em `App.tsx`, dentro de `<Protected>`.
6. Estilo com os tokens do `global.css`; CSS local só se for realmente específico da tela.
7. `data-testid` nos elementos que o E2E vai tocar.
8. Spec Playwright em `test/features/<área>/`.
9. Primitivo novo em `ui/` ganha uma story.

---

## 11. Armadilhas conhecidas

1. **Campo sem `forwardRef`** engole o ref do react-hook-form e o Zod valida `undefined`
   ("Required" em campo preenchido). §5.
2. **`setValueAs` faltando em select opcional de id** vira `0` e reprova na validação. §5.
3. **`queryKey` sem `userId`** serve o cache da conta anterior depois de trocar de usuário. §4.
4. **401 nem sempre é sessão expirada** — no login é senha errada. Só trate como expiração
   quando a requisição levou token. §3.
5. **Proxy do Vite aponta para HTTP :5080**, não HTTPS. §9.
6. **`addInitScript` do helper de teste re-injeta a sessão a cada navegação** e mascara
   logout. §8.
7. **sweetalert2 não é `window.confirm`** — nos testes, use `.swal2-confirm`. §8.
8. **`DataTable` renomeia o testid das linhas** a partir do testid da tabela. §8.
9. **O `tsc -b` emite se `noEmit` sair do `tsconfig.app.json`** e suja `src/` com `.d.ts`/`.js`.
10. **Diferencie os modos E2E.** `test:e2e` requer API local; `test:e2e:coverage` cria ambiente isolado. §9.
11. **A data da API vem sem fuso** (`2026-10-05T00:00:00`). Formate com `lib/format.ts`, que
    monta em UTC — `new Date()` local joga o dia para trás em UTC-3.

---

## 12. Antes de abrir PR

- [ ] `npm run build` sem erro (gate do CI)
- [ ] `npm run lint` sem erro (gate do CI)
- [ ] `npm run test:typecheck`, `npm run test:coverage` e `npm run test:e2e:coverage` aprovados
- [ ] `npm run coverage:merge` gerado na mesma revisão
- [ ] Nenhum `fetch` direto — tudo via `api()` / `apiUpload()`
- [ ] `queryKey` inclui o `userId`
- [ ] Erro tratado por `ApiError.code`, não por texto da mensagem
- [ ] Campo novo com `{...register(...)}` usa `forwardRef`
- [ ] Cores e espaçamentos vindos dos tokens do `global.css`
- [ ] Tema claro conferido via `[data-theme="light"]` (não via SO)
- [ ] Gráfico novo: ordem fixa de hue, legenda, visão em tabela
- [ ] `data-testid` nos elementos interativos
- [ ] Rota nova dentro de `<Protected>`
- [ ] Sem `any` novo

## Atualização de UI — Material UI

A migração é progressiva: `MaterialTheme` integra MUI ao `themeStore`, com verde
Philo, português e os dois temas. O CSS com tokens continua atendendo os
componentes legados e os gráficos; não há `CssBaseline` global nesta etapa.
`AppShell` usa Drawer permanente no desktop e temporário abaixo de 900px.
`MetricCard` usa Card e `ui/Modal` usa Dialog com confinamento/restauração de foco.
Os filtros de transações usam TextField, Autocomplete, Collapse e Chip.
Os selects MUI nos testes são operados por combobox/option, não `selectOption`.
No celular, abra `nav-open` antes de interagir com links da navegação ou logout.
O Painel mostra A pagar separadamente e apresenta vencimentos antes dos gráficos.

## Motion

`MaterialTheme` configura `LazyMotion` e `MotionConfig` com `reducedMotion="user"`.
Use `m` de `motion/react` dentro desse provider. Entradas de página duram 180 ms;
etiquetas de filtro, 150 ms. `useReducedMotion` desativa também o fade quando
movimento reduzido está ativo. Não anime números financeiros nem atrase ações.
Drawer, Dialog e Collapse mantêm suas transições MUI, sem animação duplicada.

## Responsividade — celulares e tablets

- Reutilize os breakpoints MUI: `sm` 600, `md` 900 e `lg` 1200.
- Em grids, limite mínimos à largura disponível com `minmax(min(100%, ...), 1fr)`.
- `DataTable` apresenta cartões abaixo de 1200px, preservando tabela, rótulos e
  ações. `ChartTable` reutiliza esse componente. Não duplique controles por tamanho.
- Nos quatro formulários em modal, passe botões em `Modal.actions`; submit usa
  `form={formId}` ligado ao formulário. Apenas os campos ficam na área rolável.
- `Modal` acompanha `visualViewport`, tem fallback `dvh` e respeita áreas seguras.
- Alvos de toque usam `--touch` (48px). Campos em celular/tablet usam fonte 16px.
- Teste nomes longos, filtros, alturas pequenas e temas sem ocultar overflow da
  página como correção. Preserve zoom e a semântica acessível.
- Consulte `test/RESPONSIVENESS.md` e mantenha a matriz Playwright de 320 a 1024px.
