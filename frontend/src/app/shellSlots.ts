import { createContext } from 'react';
// DOM destinations only, never auth/session or business data.
export const ShellSlots = createContext<{ account: HTMLElement | null; closeAccount: () => void } | null>(null);
