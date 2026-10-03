import { motion, useScroll, useSpring, AnimatePresence } from 'framer-motion';
import { ArrowUp } from 'lucide-react';
import { useEffect, useState } from 'react';

export function ScrollProgress() {
  const { scrollYProgress } = useScroll();
  const scaleX = useSpring(scrollYProgress, { stiffness: 90, damping: 24, mass: 0.4 });
  return <motion.div className="fixed left-0 top-0 z-[70] h-[2px] w-full origin-left bg-caramel" style={{ scaleX }} aria-hidden="true" />;
}

export function BackToTop() {
  const [show, setShow] = useState(false);
  useEffect(() => {
    const on = () => setShow(window.scrollY > 900);
    on();
    window.addEventListener('scroll', on, { passive: true });
    return () => window.removeEventListener('scroll', on);
  }, []);
  return (
    <AnimatePresence>
      {show ? (
        <motion.button
          initial={{ opacity: 0, y: 14 }}
          animate={{ opacity: 1, y: 0 }}
          exit={{ opacity: 0, y: 14 }}
          transition={{ duration: 0.6 }}
          onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
          className="fixed bottom-6 left-5 z-40 hidden h-12 w-12 items-center justify-center rounded-full border border-espresso/20 bg-ivory/80 text-espresso backdrop-blur transition-colors duration-500 hover:bg-espresso hover:text-ivory sm:flex"
          aria-label="Back to top"
          data-cursor
        >
          <ArrowUp size={18} strokeWidth={1.6} />
        </motion.button>
      ) : null}
    </AnimatePresence>
  );
}
