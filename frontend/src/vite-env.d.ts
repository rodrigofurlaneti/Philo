/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_ORGANIZATION_ID: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
