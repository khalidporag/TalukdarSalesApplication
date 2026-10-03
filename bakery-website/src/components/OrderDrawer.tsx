import { AnimatePresence, motion } from 'framer-motion';
import { Minus, Plus, ShoppingBag, X } from 'lucide-react';
import { useEffect, useState } from 'react';
import { brand } from '@/data/brand';
import { money } from '@/lib/format';
import { orderTotal, submitOrder, buildOrderMessage, type OrderDetails } from '@/lib/orders';
import { Button } from './Button';
import { useOrder } from './OrderContext';
import { silk } from './motion';

export function OrderDrawer() {
  const { lines, open, setOpen, setQty, clear } = useOrder();
  const [details, setDetails] = useState<OrderDetails>({ name: '', phone: '', when: '', note: '' });
  const [copied, setCopied] = useState(false);
  const total = orderTotal(lines);

  useEffect(() => {
    document.body.style.overflow = open ? 'hidden' : '';
    const esc = (e: KeyboardEvent) => { if (e.key === 'Escape') setOpen(false); };
    window.addEventListener('keydown', esc);
    return () => { window.removeEventListener('keydown', esc); document.body.style.overflow = ''; };
  }, [open, setOpen]);

  const set = (k: keyof OrderDetails) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => setDetails((d) => ({ ...d, [k]: e.target.value }));

  const send = async () => {
    const res = await submitOrder({ lines, details });
    window.open(res.url, '_blank', 'noopener');
  };
  const copy = async () => {
    try { await navigator.clipboard.writeText(buildOrderMessage({ lines, details })); setCopied(true); window.setTimeout(() => setCopied(false), 1800); } catch { /* clipboard blocked */ }
  };

  return (
    <AnimatePresence>
      {open ? (
        <>
          <motion.div key="shade" className="fixed inset-0 z-[85] bg-espresso/55 backdrop-blur-sm" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} transition={{ duration: 0.6 }} onClick={() => setOpen(false)} aria-hidden="true" />
          <motion.aside key="panel" className="fixed inset-y-0 right-0 z-[86] flex w-full max-w-[28rem] flex-col bg-ivory shadow-2xl" initial={{ x: '100%' }} animate={{ x: 0 }} exit={{ x: '100%' }} transition={{ duration: 0.8, ease: silk }} role="dialog" aria-modal="true" aria-label="Your order">
            <div className="flex items-center justify-between border-b border-espresso/10 px-6 py-5">
              <div>
                <p className="eyebrow">Your order</p>
                <p className="font-display text-3xl">{lines.length ? `${lines.reduce((a, l) => a + l.qty, 0)} items` : 'Nothing yet'}</p>
              </div>
              <button onClick={() => setOpen(false)} className="flex h-11 w-11 items-center justify-center rounded-full border border-espresso/20 hover:bg-espresso hover:text-ivory" aria-label="Close order"><X size={18} strokeWidth={1.5} /></button>
            </div>

            {lines.length === 0 ? (
              <div className="flex flex-1 flex-col items-center justify-center gap-4 px-8 text-center">
                <ShoppingBag size={34} strokeWidth={1} className="text-caramel" />
                <p className="font-display text-2xl">Your basket is waiting.</p>
                <p className="text-[0.92rem] text-espresso/60">Add a few things from the menu and send them to us in one message.</p>
              </div>
            ) : (
              <div className="flex-1 overflow-y-auto px-6">
                <ul className="divide-y divide-espresso/10">
                  {lines.map((l) => (
                    <li key={l.slug + l.variant} className="flex items-center gap-4 py-5">
                      <div className="min-w-0 flex-1">
                        <p className="truncate font-display text-xl">{l.name}</p>
                        <p className="text-[0.78rem] text-espresso/55">{l.variant} · {money(l.price)}</p>
                      </div>
                      <div className="flex items-center gap-1 rounded-full border border-espresso/15">
                        <button className="flex h-9 w-9 items-center justify-center" onClick={() => setQty(l.slug, l.variant, l.qty - 1)} aria-label={`Fewer ${l.name}`}><Minus size={14} /></button>
                        <span className="w-5 text-center text-[0.85rem] font-semibold">{l.qty}</span>
                        <button className="flex h-9 w-9 items-center justify-center" onClick={() => setQty(l.slug, l.variant, l.qty + 1)} aria-label={`More ${l.name}`}><Plus size={14} /></button>
                      </div>
                      <p className="w-20 text-right text-[0.9rem] font-semibold">{money(l.price * l.qty)}</p>
                    </li>
                  ))}
                </ul>

                <div className="space-y-5 border-t border-espresso/10 py-6">
                  <div><label className="label" htmlFor="o-name">Your name</label><input id="o-name" className="field" value={details.name} onChange={set('name')} autoComplete="name" /></div>
                  <div><label className="label" htmlFor="o-phone">Phone</label><input id="o-phone" className="field" value={details.phone} onChange={set('phone')} inputMode="tel" autoComplete="tel" /></div>
                  <div><label className="label" htmlFor="o-when">Needed on</label><input id="o-when" type="date" className="field" value={details.when} onChange={set('when')} /></div>
                  <div><label className="label" htmlFor="o-note">Note</label><textarea id="o-note" rows={2} className="field resize-none" value={details.note} onChange={set('note')} placeholder="Allergies, message on the box, delivery area…" /></div>
                </div>
              </div>
            )}

            {lines.length > 0 ? (
              <div className="border-t border-espresso/10 bg-cream/60 px-6 py-5">
                <div className="flex items-baseline justify-between"><span className="text-[0.7rem] font-semibold uppercase tracking-widest2 text-espresso/55">Total</span><span className="font-display text-3xl">{money(total)}</span></div>
                <p className="mt-1 text-[0.74rem] text-espresso/50">{brand.leadTime}</p>
                <div className="mt-4 grid gap-3">
                  <Button onClick={send} className="w-full">Send order on WhatsApp</Button>
                  <div className="grid grid-cols-2 gap-3">
                    <Button variant="line" small onClick={copy}>{copied ? 'Copied' : 'Copy order'}</Button>
                    <Button variant="line" small onClick={clear}>Clear</Button>
                  </div>
                </div>
              </div>
            ) : null}
          </motion.aside>
        </>
      ) : null}
    </AnimatePresence>
  );
}
