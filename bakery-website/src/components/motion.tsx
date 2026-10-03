import { motion, type Variants } from 'framer-motion';
import type { ReactNode, ElementType } from 'react';

/** Slow, soft easing used everywhere so the site moves with one temperament. */
export const silk = [0.22, 0.61, 0.36, 1] as const;

interface RevealProps {
  children: ReactNode;
  delay?: number;
  y?: number;
  duration?: number;
  className?: string;
  as?: 'div' | 'li' | 'p' | 'span' | 'section' | 'figure';
  once?: boolean;
}

/** Fade + slight rise when scrolled into view. */
export function Reveal({ children, delay = 0, y = 26, duration = 1.1, className, as = 'div', once = true }: RevealProps) {
  const Tag = motion[as] as ElementType;
  return (
    <Tag
      className={className}
      initial={{ opacity: 0, y }}
      whileInView={{ opacity: 1, y: 0 }}
      viewport={{ once, margin: '0px 0px -12% 0px' }}
      transition={{ duration, delay, ease: silk }}
    >
      {children}
    </Tag>
  );
}

/** Headline lines that rise out of a mask, one after another. */
export function MaskLines({ lines, className, lineClassName, delay = 0, immediate = false }: { lines: ReactNode[]; className?: string; lineClassName?: string; delay?: number; immediate?: boolean }) {
  return (
    <span className={className} style={{ display: 'block' }}>
      {lines.map((line, i) => (
        <span key={i} className="block overflow-hidden pb-[0.12em] -mb-[0.12em]">
          <motion.span
            className={`block ${lineClassName ?? ''}`}
            initial={{ y: '112%' }}
            {...(immediate ? { animate: { y: 0 } } : { whileInView: { y: 0 }, viewport: { once: true, margin: '0px 0px -10% 0px' } })}
            transition={{ duration: 1.35, delay: delay + i * 0.14, ease: silk }}
          >
            {line}
          </motion.span>
        </span>
      ))}
    </span>
  );
}

export const stagger = (gap = 0.1, delay = 0): Variants => ({
  hidden: {},
  show: { transition: { staggerChildren: gap, delayChildren: delay } },
});

export const rise: Variants = {
  hidden: { opacity: 0, y: 22 },
  show: { opacity: 1, y: 0, transition: { duration: 1.1, ease: silk } },
};
