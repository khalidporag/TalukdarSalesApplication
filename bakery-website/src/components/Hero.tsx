import { motion, useScroll, useTransform } from 'framer-motion';
import { ArrowDown, ArrowRight } from 'lucide-react';
import { useRef } from 'react';
import { brand } from '@/data/brand';
import { ButtonLink } from './Button';
import { Picture } from './Picture';
import { MaskLines, silk } from './motion';

/** Round text that turns very slowly. The circle is drawn once and rotated with CSS. */
function SealText() {
  return (
    <div className="relative h-32 w-32 sm:h-40 sm:w-40">
      <svg viewBox="0 0 160 160" className="absolute inset-0 h-full w-full animate-[spin_60s_linear_infinite]" aria-hidden="true">
        <defs><path id="seal" d="M80 80 m-62 0 a62 62 0 1 1 124 0 a62 62 0 1 1 -124 0" /></defs>
        <text fill="currentColor" fontSize="11" fontWeight="600" style={{ textTransform: 'uppercase' }}>
          <textPath href="#seal" textLength="384" lengthAdjust="spacing">Freshly baked • Made with care • Since 2021 •</textPath>
        </text>
      </svg>
      <svg viewBox="0 0 40 40" className="absolute left-1/2 top-1/2 h-10 w-10 -translate-x-1/2 -translate-y-1/2 text-caramel" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
        <path d="M20 36V12M20 12c-3-1-4-4-3-7 3 1 4 4 3 7zM20 12c3-1 4-4 3-7-3 1-4 4-3 7zM20 20c-4 0-6-3-6-6 4 0 6 3 6 6zM20 20c4 0 6-3 6-6-4 0-6 3-6 6zM20 28c-4 0-6-3-6-6 4 0 6 3 6 6zM20 28c4 0 6-3 6-6-4 0-6 3-6 6z" />
      </svg>
    </div>
  );
}

export function Hero() {
  const ref = useRef<HTMLElement>(null);
  const { scrollYProgress } = useScroll({ target: ref, offset: ['start start', 'end start'] });
  const imgY = useTransform(scrollYProgress, [0, 1], ['0%', '9%']);
  const imgScale = useTransform(scrollYProgress, [0, 1], [1.02, 1.12]);
  const textY = useTransform(scrollYProgress, [0, 1], ['0px', '-38px']);
  const smallY = useTransform(scrollYProgress, [0, 1], ['0px', '-70px']);

  return (
    <section ref={ref} id="top" className="relative isolate overflow-hidden pt-28 sm:pt-32 lg:pt-0" aria-label="Welcome">
      {/* wash of warm light */}
      <div className="pointer-events-none absolute inset-0 -z-10 bg-[radial-gradient(80%_70%_at_75%_30%,#F1E2C6_0%,transparent_70%)]" aria-hidden="true" />
      <motion.span initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ delay: 2.4, duration: 2 }} className="vertical-text absolute left-3 top-1/2 hidden -translate-y-1/2 text-[0.62rem] font-semibold uppercase tracking-widest2 text-espresso/45 2xl:block" aria-hidden="true">
        {brand.established} · {brand.location}
      </motion.span>

      <div className="container-x grid min-h-[100svh] grid-cols-12 items-center gap-y-10 pb-16 lg:pb-10">
        {/* image: first on mobile, right on desktop */}
        <div className="relative order-1 col-span-12 lg:order-2 lg:col-span-5 lg:col-start-8">
          <motion.div initial={{ opacity: 0, scale: 1.06 }} animate={{ opacity: 1, scale: 1 }} transition={{ duration: 2.2, delay: 0.2, ease: silk }} className="relative mx-auto w-[82%] max-w-[470px] sm:w-[62%] lg:w-full lg:max-w-none">
            <div className="relative aspect-[4/5.4] overflow-hidden rounded-t-[999px] bg-cream shadow-[0_50px_90px_-40px_rgba(36,23,16,0.55)] sm:aspect-[4/5.2]">
              <motion.div style={{ y: imgY, scale: imgScale }} className="absolute inset-0">
                <Picture photo="hero" art="hero" tone="honey" alt="Fresh sourdough, croissants and a slice of chocolate cake" eager />
              </motion.div>
              <div className="pointer-events-none absolute inset-0 rounded-t-[999px] ring-1 ring-inset ring-espresso/10" />
            </div>
            {/* arch outline offset behind */}
            <div className="pointer-events-none absolute -right-4 top-5 -z-10 h-full w-full rounded-t-[999px] border border-caramel/50 sm:-right-6 sm:top-7" aria-hidden="true" />

            <motion.div style={{ y: smallY }} className="absolute -left-3 bottom-[7%] w-[34%] sm:-left-10 lg:-left-16" initial={{ opacity: 0, y: 30 }} animate={{ opacity: 1 }} transition={{ duration: 1.6, delay: 1.5, ease: silk }}>
              <div className="aspect-square overflow-hidden rounded-full border-[6px] border-ivory shadow-[0_24px_50px_-20px_rgba(36,23,16,0.6)]">
                <Picture photo="hero-detail" art="cookies" tone="caramel" crop="close" alt="Brown butter chocolate chunk cookies" />
              </div>
            </motion.div>

            <motion.div className="absolute -right-2 -top-2 text-espresso sm:-right-8 sm:top-2" initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ delay: 2.2, duration: 1.6 }}>
              <SealText />
            </motion.div>
          </motion.div>
        </div>

        {/* type */}
        <motion.div style={{ y: textY }} className="relative order-2 col-span-12 lg:order-1 lg:col-span-7 lg:pr-6">
          <motion.p initial={{ opacity: 0, y: 14 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 1.1, delay: 0.6, ease: silk }} className="eyebrow mb-6 flex items-center gap-4 sm:mb-8">
            <span className="h-px w-10 bg-caramel" />Boutique bakery · {brand.location}
          </motion.p>

          <h1 className="display-xl text-espresso lg:-mr-[22%] lg:relative lg:z-10">
            <MaskLines immediate delay={0.7} lines={['Made slowly.', <span key="l" className="inline-block italic text-caramel-deep lg:translate-x-[10%]">Loved deeply.</span>]} />
          </h1>

          <motion.p initial={{ opacity: 0, y: 18 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 1.2, delay: 1.5, ease: silk }} className="mt-8 max-w-[34rem] text-[1.02rem] leading-relaxed text-espresso/70 sm:text-[1.08rem]">
            {brand.statement}
          </motion.p>

          <motion.div initial={{ opacity: 0, y: 22 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 1.2, delay: 1.75, ease: silk }} className="mt-9 flex flex-col gap-3 sm:flex-row sm:items-center sm:gap-4">
            <ButtonLink to="/menu" variant="dark">Explore Our Menu <ArrowRight size={16} strokeWidth={1.7} /></ButtonLink>
            <ButtonLink to="/menu" variant="line">Order Now</ButtonLink>
          </motion.div>

          <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ duration: 1.6, delay: 2.2 }} className="mt-12 flex items-center gap-5 text-[0.68rem] font-semibold uppercase tracking-widest2 text-espresso/55 sm:mt-14">
            <span className="relative flex h-2 w-2"><span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-caramel/60" /><span className="relative inline-flex h-2 w-2 rounded-full bg-caramel" /></span>
            Freshly baked <span className="text-caramel">•</span> Made with care
          </motion.div>
        </motion.div>
      </div>

      <a href="#story" className="absolute bottom-6 left-1/2 hidden -translate-x-1/2 flex-col items-center gap-2 text-[0.6rem] font-semibold uppercase tracking-widest2 text-espresso/45 transition-colors duration-500 hover:text-espresso lg:flex" aria-label="Scroll to our story" data-cursor>
        Scroll <ArrowDown size={14} strokeWidth={1.5} className="animate-drift" />
      </a>
    </section>
  );
}
