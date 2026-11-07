declare module '@/utils/auth.js' {
  export function setToken(token: string, freshToken: string): void;
  export const clearTokens: () => void;
  export const getToken: () => string | undefined;
}

