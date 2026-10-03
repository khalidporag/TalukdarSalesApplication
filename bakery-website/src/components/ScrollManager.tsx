import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';

/** Scroll to the top on a new page, or to #section on the home page (waiting for lazy sections to mount). */
export function ScrollManager() {
  const { pathname, hash } = useLocation();
  useEffect(() => {
    if (!hash) { window.scrollTo({ top: 0, behavior: 'auto' }); return; }
    let tries = 0;
    const go = () => {
      const el = document.getElementById(hash.slice(1));
      if (el) { el.scrollIntoView({ behavior: 'smooth', block: 'start' }); return; }
      if (tries++ < 20) window.setTimeout(go, 120);
    };
    go();
  }, [pathname, hash]);
  return null;
}
