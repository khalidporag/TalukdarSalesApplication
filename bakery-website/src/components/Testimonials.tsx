import { AnimatePresence, motion } from 'framer-motion';
import { ArrowLeft, ArrowRight, Star } from 'lucide-react';
import { useEffect, useState } from 'react';
import { testimonials } from '@/data/testimonials';
import { SectionHead } from './Button';
import { Reveal, silk } from './motion';

const initials = (n: string) => n.split(/[ &]+/).filter(Boolean).slice(0, 2).map((p) => p[0]).join('');

export function Testimonials() {
  const [i, setI] = useState(0);
  const [dir, setDir] = useState(1);
  const [paused, setPaused] = useState(false);
  const n = testimonials.length;
  const go = (d: number) => { setDir(d); setI((x) => (x + d + n) % n); };

  useEffect(() => {
    if (paused) return;
    const t = window.setInterval(() => { setDir(1); setI((x) => (x + 1) % n); }, 8000);
    return () => window.clearInterval(t);
  }, [paused, n]);

  const t = testimonials[i];
  return (
    <section className="relative overflow-hidden bg-cream/60 py-24 sm:py-32 lg:py-36" onMouseEnter={() => setPaused(true)} onMouseLeave={() => setPaused(false)} aria-roledescription="carousel" aria-label="Customer words">
      <span className="pointer-events-none absolute -top-10 left-4 select-none font-display text-[20rem] leading-none text-caramel/15 sm:left-16 sm:text-[30rem]" aria-hidden="true">“</span>
      <div className="container-x relative">
        <Reveal><SectionHead index="08" label="Kind words" /></Reveal>

        <div className="mx-auto mt-14 max-w-4xl">
          <div className="relative min-h-[19rem] sm:min-h-[16rem]" aria-live="polite">
            <AnimatePresence mode="wait" custom={dir}>
              <motion.figure
                key={i}
                custom={dir}
                initial={{ opacity: 0, x: dir * 40 }}
                animate={{ opacity: 1, x: 0 }}
                exit={{ opacity: 0, x: dir * -40 }}
                transition={{ duration: 0.9, ease: silk }}
                drag="x"
                dragConstraints={{ left: 0, right: 0 }}
                dragElastic={0.2}
                onDragEnd={(_, info) => { if (info.offset.x < -60) go(1); else if (info.offset.x > 60) go(-1); }}
              >
                <div className="flex gap-1 text-caramel" aria-label={`${t.rating} out of 5`}>{Array.from({ length: t.rating }).map((_, k) => <Star key={k} size={15} fill="currentColor" strokeWidth={0} />)}</div>
                <blockquote className="mt-6 font-display text-[1.75rem] italic leading-[1.28] text-espresso sm:text-[2.5rem] lg:text-[2.9rem]">{t.quote}</blockquote>
                <figcaption className="mt-8 flex items-center gap-4">
                  <span className="flex h-12 w-12 items-center justify-center rounded-full bg-espresso font-display text-lg italic text-ivory">{initials(t.name)}</span>
                  <span><span className="block text-[0.95rem] font-semibold">{t.name}</span><span className="block text-[0.72rem] uppercase tracking-widest2 text-espresso/50">{t.context}</span></span>
                </figcaption>
              </motion.figure>
            </AnimatePresence>
          </div>

          <div className="mt-12 flex items-center justify-between border-t border-espresso/15 pt-6">
            <div className="flex gap-2" role="tablist" aria-label="Choose testimonial">
              {testimonials.map((_, k) => (
                <button key={k} role="tab" aria-selected={k === i} aria-label={`Testimonial ${k + 1}`} onClick={() => { setDir(k > i ? 1 : -1); setI(k); }} className="group py-3" data-cursor>
                  <span className={`block h-px transition-all duration-700 ease-silk ${k === i ? 'w-12 bg-espresso' : 'w-6 bg-espresso/30 group-hover:bg-espresso/60'}`} />
                </button>
              ))}
            </div>
            <div className="flex gap-2">
              <button onClick={() => go(-1)} className="flex h-11 w-11 items-center justify-center rounded-full border border-espresso/20 transition-colors duration-500 hover:bg-espresso hover:text-ivory" aria-label="Previous"><ArrowLeft size={17} strokeWidth={1.5} /></button>
              <button onClick={() => go(1)} className="flex h-11 w-11 items-center justify-center rounded-full border border-espresso/20 transition-colors duration-500 hover:bg-espresso hover:text-ivory" aria-label="Next"><ArrowRight size={17} strokeWidth={1.5} /></button>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
