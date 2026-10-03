import { AnimatePresence, motion } from 'framer-motion';
import { useEffect, useState } from 'react';
import { Logo } from './Logo';
import { silk } from './motion';

const SEEN = 'talukder-intro-seen';

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
          <motion.div initial={{ opacity: 0, scale: 0.94 }} animate={{ opacity: 1, scale: 1 }} transition={{ duration: 1.3, ease: silk }}>
            <Logo className="h-28 sm:h-32" small={false} />
          </motion.div>
          <motion.p className="mt-8 text-[0.62rem] font-semibold uppercase tracking-widest2 text-ivory/50" initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ delay: 1, duration: 1 }}>
            Warming the oven
          </motion.p>
        </motion.div>
      ) : null}
    </AnimatePresence>
  );
}
