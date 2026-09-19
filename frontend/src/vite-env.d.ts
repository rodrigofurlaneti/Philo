/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_ORGANIZATION_ID: string;
  readonly VITE_DEV_INTERNAL_API_KEY?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
