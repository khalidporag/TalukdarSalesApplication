import { AnimatePresence, motion } from 'framer-motion';
import { ChevronLeft, ChevronRight, X } from 'lucide-react';
import { useCallback, useEffect, useState } from 'react';
import { gallery } from '@/data/gallery';
import { SectionHead } from './Button';
import { MaskLines, Reveal, silk } from './motion';
import { Picture } from './Picture';

function Lightbox({ index, onClose, onIndex }: { index: number; onClose: () => void; onIndex: (i: number) => void }) {
  const n = gallery.length;
  const step = useCallback((d: number) => onIndex((index + d + n) % n), [index, n, onIndex]);
  useEffect(() => {
    const key = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
      if (e.key === 'ArrowRight') step(1);
      if (e.key === 'ArrowLeft') step(-1);
    };
    window.addEventListener('keydown', key);
    document.body.style.overflow = 'hidden';
    return () => { window.removeEventListener('keydown', key); document.body.style.overflow = ''; };
  }, [onClose, step]);
  const g = gallery[index];

  return (
    <motion.div className="fixed inset-0 z-[95] flex flex-col bg-ink/95 text-ivory backdrop-blur-md" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} transition={{ duration: 0.5 }} role="dialog" aria-modal="true" aria-label="Gallery image">
      <div className="flex items-center justify-between px-5 py-4 sm:px-8">
        <p className="text-[0.7rem] font-semibold uppercase tracking-widest2 text-ivory/60">{String(index + 1).padStart(2, '0')} / {String(n).padStart(2, '0')}</p>
        <button onClick={onClose} className="flex h-11 w-11 items-center justify-center rounded-full border border-ivory/25 hover:bg-ivory hover:text-espresso" aria-label="Close"><X size={18} strokeWidth={1.5} /></button>
      </div>
      <div className="relative flex min-h-0 flex-1 items-center justify-center px-4 sm:px-20" onClick={onClose}>
        <AnimatePresence mode="wait">
          <motion.figure key={g.key} className="flex max-h-full flex-col items-center" initial={{ opacity: 0, scale: 0.97 }} animate={{ opacity: 1, scale: 1 }} exit={{ opacity: 0 }} transition={{ duration: 0.6, ease: silk }} onClick={(e) => e.stopPropagation()}>
            <div className="aspect-[4/5] h-[min(70svh,720px)] max-w-[88vw] overflow-hidden bg-cocoa shadow-2xl"><Picture photo={g.key} art={g.art} tone={g.tone} crop={g.crop} alt={g.caption} eager /></div>
            <figcaption className="mt-5 text-center"><span className="eyebrow !text-caramel">{g.tag}</span><span className="mt-2 block font-display text-2xl italic">{g.caption}</span></figcaption>
          </motion.figure>
        </AnimatePresence>
        <button onClick={(e) => { e.stopPropagation(); step(-1); }} className="absolute left-3 top-1/2 flex h-12 w-12 -translate-y-1/2 items-center justify-center rounded-full border border-ivory/25 hover:bg-ivory hover:text-espresso sm:left-6" aria-label="Previous"><ChevronLeft size={20} strokeWidth={1.5} /></button>
        <button onClick={(e) => { e.stopPropagation(); step(1); }} className="absolute right-3 top-1/2 flex h-12 w-12 -translate-y-1/2 items-center justify-center rounded-full border border-ivory/25 hover:bg-ivory hover:text-espresso sm:right-6" aria-label="Next"><ChevronRight size={20} strokeWidth={1.5} /></button>
      </div>
    </motion.div>
  );
}

export function Gallery() {
  const [open, setOpen] = useState<number | null>(null);
  return (
    <section className="relative py-24 sm:py-32 lg:py-40">
      <div className="container-x">
        <div className="flex flex-col gap-8 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <Reveal><SectionHead index="07" label="Gallery" /></Reveal>
            <h2 className="display-lg mt-8"><MaskLines lines={['A look', <span key="i" className="italic text-caramel-deep">inside.</span>]} /></h2>
          </div>
          <Reveal delay={0.1}><p className="max-w-sm text-[1rem] leading-relaxed text-espresso/65">Cakes, kitchen mornings, packaging and the people who bring them home.</p></Reveal>
        </div>

        <div className="mt-14 grid grid-cols-2 gap-3 sm:gap-4 lg:grid-cols-12 lg:auto-rows-[15rem] lg:gap-5 xl:auto-rows-[17rem]">
          {gallery.map((g, i) => (
            <Reveal key={g.key} delay={(i % 3) * 0.08} className={`${g.cls} ${i % 5 === 0 ? 'max-lg:col-span-2' : ''} min-w-0 lg:h-full`}>
              <button onClick={() => setOpen(i)} className="group relative block h-full w-full overflow-hidden bg-cream text-left" aria-label={`Open ${g.caption}`} data-cursor>
                <div className="absolute inset-0 transition-transform duration-[1800ms] ease-silk group-hover:scale-[1.07]"><Picture photo={g.key} art={g.art} tone={g.tone} crop={g.crop} alt={g.caption} /></div>
                <div className="absolute inset-x-0 bottom-0 translate-y-3 bg-gradient-to-t from-espresso/80 to-transparent p-4 pt-14 opacity-0 transition-all duration-[900ms] ease-silk group-hover:translate-y-0 group-hover:opacity-100">
                  <span className="text-[0.58rem] font-semibold uppercase tracking-widest2 text-caramel">{g.tag}</span>
                  <span className="mt-1 block font-display text-lg italic leading-tight text-ivory">{g.caption}</span>
                </div>
              </button>
            </Reveal>
          ))}
        </div>
      </div>
      <AnimatePresence>{open !== null ? <Lightbox index={open} onClose={() => setOpen(null)} onIndex={setOpen} /> : null}</AnimatePresence>
    </section>
  );
}
