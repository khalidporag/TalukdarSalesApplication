import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import type { OrderLine } from '@/lib/orders';

interface OrderState {
  lines: OrderLine[];
  count: number;
  open: boolean;
  setOpen: (v: boolean) => void;
  add: (line: Omit<OrderLine, 'qty'>, qty?: number) => void;
  setQty: (slug: string, variant: string, qty: number) => void;
  clear: () => void;
}

const Ctx = createContext<OrderState | null>(null);
const KEY = 'aurum-order-v1';

export function OrderProvider({ children }: { children: ReactNode }) {
  const [lines, setLines] = useState<OrderLine[]>(() => {
    try { return JSON.parse(localStorage.getItem(KEY) ?? '[]') as OrderLine[]; } catch { return []; }
  });
  const [open, setOpen] = useState(false);

  useEffect(() => {
    try { localStorage.setItem(KEY, JSON.stringify(lines)); } catch { /* private mode */ }
  }, [lines]);

  const add = useCallback((line: Omit<OrderLine, 'qty'>, qty = 1) => {
    setLines((prev) => {
      const i = prev.findIndex((l) => l.slug === line.slug && l.variant === line.variant);
      if (i >= 0) return prev.map((l, k) => (k === i ? { ...l, qty: l.qty + qty } : l));
      return [...prev, { ...line, qty }];
    });
  }, []);

  const setQty = useCallback((slug: string, variant: string, qty: number) => {
    setLines((prev) => prev.flatMap((l) => (l.slug === slug && l.variant === variant ? (qty > 0 ? [{ ...l, qty }] : []) : [l])));
  }, []);

  const clear = useCallback(() => setLines([]), []);
  const count = lines.reduce((a, l) => a + l.qty, 0);
  const value = useMemo(() => ({ lines, count, open, setOpen, add, setQty, clear }), [lines, count, open, add, setQty, clear]);
  return <Ctx.Provider value={value}>{children}</Ctx.Provider>;
}

export function useOrder() {
  const v = useContext(Ctx);
  if (!v) throw new Error('useOrder must be used inside OrderProvider');
  return v;
}
