import { AnimatePresence, motion } from 'framer-motion';
import { useEffect, useState } from 'react';
import { brand } from '@/data/brand';
import { silk } from './motion';

const SEEN = 'aurum-intro-seen';

/** A short, calm opening: the brand mark draws itself, then the curtain lifts. Shown once per visit. */
export function Loader() {
  const [show, setShow] = useState(() => {
    try { return !sessionStorage.getItem(SEEN); } catch { return true; }
  });

  useEffect(() => {
    if (!show) return;
    document.body.style.overflow = 'hidden';
    const started = performance.now();
    const done = () => {
      const wait = Math.max(0, 1900 - (performance.now() - started));
      window.setTimeout(() => {
        setShow(false);
        document.body.style.overflow = '';
        try { sessionStorage.setItem(SEEN, '1'); } catch { /* ignore */ }
      }, wait);
    };
    if (document.readyState === 'complete') done();
    else window.addEventListener('load', done, { once: true });
    return () => { document.body.style.overflow = ''; };
  }, [show]);

  return (
    <AnimatePresence>
      {show ? (
        <motion.div
          key="loader"
          className="fixed inset-0 z-[100] flex flex-col items-center justify-center bg-espresso text-ivory"
          exit={{ y: '-100%' }}
          transition={{ duration: 1.1, ease: silk }}
          aria-label="Loading"
        >
          <div className="relative">
            <svg viewBox="0 0 40 40" className="h-24 w-24 text-caramel" fill="none" stroke="currentColor" strokeWidth="0.9" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <motion.path d="M20 4c7 5 11 11 11 18a11 11 0 0 1-22 0c0-7 4-13 11-18z" initial={{ pathLength: 0 }} animate={{ pathLength: 1 }} transition={{ duration: 1.4, ease: silk }} />
              <motion.path d="M20 12v22M14 21l6 6 6-6M14 15l6 6 6-6" initial={{ pathLength: 0 }} animate={{ pathLength: 1 }} transition={{ duration: 1.2, delay: 0.5, ease: silk }} />
            </svg>
            <div className="pointer-events-none absolute -top-7 left-1/2 flex -translate-x-1/2 gap-3" aria-hidden="true">
              {[0, 1, 2].map((i) => <span key={i} className="block h-8 w-px bg-ivory/40 animate-steam" style={{ animationDelay: `${i * 0.7}s` }} />)}
            </div>
          </div>
          <motion.p className="mt-8 font-display text-3xl italic tracking-tight" initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.6, duration: 1, ease: silk }}>
            {brand.name}
          </motion.p>
          <motion.p className="mt-2 text-[0.62rem] font-semibold uppercase tracking-widest2 text-ivory/50" initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ delay: 1, duration: 1 }}>
            Warming the oven
          </motion.p>
        </motion.div>
      ) : null}
    </AnimatePresence>
  );
}
