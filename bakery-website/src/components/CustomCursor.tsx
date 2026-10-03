import { motion, useMotionValue, useSpring } from 'framer-motion';
import { useEffect, useState } from 'react';

/** A quiet ring that trails the pointer and swells over links and buttons. Desktop with a mouse only. */
export function CustomCursor() {
  const [enabled, setEnabled] = useState(false);
  const [hover, setHover] = useState(false);
  const x = useMotionValue(-100);
  const y = useMotionValue(-100);
  const sx = useSpring(x, { stiffness: 220, damping: 26, mass: 0.5 });
  const sy = useSpring(y, { stiffness: 220, damping: 26, mass: 0.5 });

  useEffect(() => {
    const fine = window.matchMedia('(hover: hover) and (pointer: fine)').matches;
    const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (!fine || reduce) return;
    setEnabled(true);
    document.body.classList.add('has-cursor');
    const move = (e: PointerEvent) => {
      x.set(e.clientX);
      y.set(e.clientY);
      const el = e.target as HTMLElement | null;
      setHover(!!el?.closest('a, button, [data-cursor], input, textarea, select, label'));
    };
    window.addEventListener('pointermove', move, { passive: true });
    return () => {
      window.removeEventListener('pointermove', move);
      document.body.classList.remove('has-cursor');
    };
  }, [x, y]);

  if (!enabled) return null;
  return (
    <>
      <motion.div className="pointer-events-none fixed left-0 top-0 z-[90] -ml-1 -mt-1 h-2 w-2 rounded-full bg-caramel" style={{ x, y }} aria-hidden="true" />
      <motion.div
        className="pointer-events-none fixed left-0 top-0 z-[89] rounded-full border border-caramel/70"
        style={{ x: sx, y: sy, translateX: '-50%', translateY: '-50%' }}
        animate={{ width: hover ? 64 : 34, height: hover ? 64 : 34, opacity: hover ? 0.9 : 0.55, backgroundColor: hover ? 'rgba(181,128,63,0.10)' : 'rgba(181,128,63,0)' }}
        transition={{ duration: 0.5, ease: [0.22, 0.61, 0.36, 1] }}
        aria-hidden="true"
      />
    </>
  );
}
