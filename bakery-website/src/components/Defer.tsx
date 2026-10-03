import { Suspense, useEffect, useRef, useState, type ReactNode } from 'react';

/** Mounts its children (usually a lazy-loaded section) only when they are about to scroll into view. */
export function Defer({ children, minHeight = '70vh', id, margin = '600px' }: { children: ReactNode; minHeight?: string; id?: string; margin?: string }) {
  const ref = useRef<HTMLDivElement>(null);
  const [show, setShow] = useState(false);

  useEffect(() => {
    const el = ref.current;
    if (!el || show) return;
    if (!('IntersectionObserver' in window)) { setShow(true); return; }
    const io = new IntersectionObserver((entries) => {
      if (entries.some((e) => e.isIntersecting)) { setShow(true); io.disconnect(); }
    }, { rootMargin: `${margin} 0px` });
    io.observe(el);
    return () => io.disconnect();
  }, [show, margin]);

  return (
    <div ref={ref} id={id} style={show ? undefined : { minHeight }}>
      {show ? <Suspense fallback={<div style={{ minHeight }} />}>{children}</Suspense> : null}
    </div>
  );
}
