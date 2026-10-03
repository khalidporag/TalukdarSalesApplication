import { motion, useScroll, useTransform } from 'framer-motion';
import { useRef } from 'react';
import { brand } from '@/data/brand';
import { SectionHead } from './Button';
import { Picture } from './Picture';
import { MaskLines, Reveal } from './motion';

const pillars = [
  { n: '01', t: 'Real ingredients', d: 'Cultured butter, unbleached flour, whole eggs and nothing we cannot pronounce.' },
  { n: '02', t: 'Made by hand', d: 'Every loaf shaped, every pastry folded and every cake finished by a person.' },
  { n: '03', t: 'Given time', d: 'Long ferments and cold rests, because patience is the one ingredient you cannot hurry.' },
];

export function BrandStory() {
  const ref = useRef<HTMLElement>(null);
  const { scrollYProgress } = useScroll({ target: ref, offset: ['start end', 'end start'] });
  const yA = useTransform(scrollYProgress, [0, 1], ['6%', '-6%']);
  const yB = useTransform(scrollYProgress, [0, 1], ['14%', '-12%']);

  return (
    <section ref={ref} id="story" className="relative py-24 sm:py-32 lg:py-40">
      <div className="container-x grid grid-cols-12 gap-y-16 lg:gap-x-10">
        <div className="col-span-12 lg:col-span-6 xl:col-span-5">
          <Reveal><SectionHead index="01" label="Our Story" /></Reveal>
          <h2 className="display-lg mt-8 text-espresso">
            <MaskLines lines={['Good baking', <span key="t" className="italic text-caramel-deep">takes time.</span>]} />
          </h2>

          <div className="mt-10 max-w-[30rem] space-y-6 text-[1.04rem] leading-[1.75] text-espresso/70">
            <Reveal delay={0.1}><p>{brand.name} began in a home kitchen with one oven, a jar of starter and a stubborn belief: that bread, cakes and pastries taste better when no one is rushing them.</p></Reveal>
            <Reveal delay={0.2}><p>Today the kitchen is bigger, but the habits are the same. We mix in small batches, rest our doughs overnight and bake each morning, so what reaches your table was made for you, not for a shelf.</p></Reveal>
          </div>

          <Reveal delay={0.25} className="mt-12">
            <ul className="divide-y divide-espresso/12 border-y border-espresso/12">
              {pillars.map((p) => (
                <li key={p.n} className="grid grid-cols-[2.6rem_1fr] gap-x-3 py-5 sm:grid-cols-[3.2rem_11rem_1fr]">
                  <span className="font-display text-xl italic text-caramel">{p.n}</span>
                  <span className="font-display text-xl sm:text-2xl">{p.t}</span>
                  <span className="col-start-2 text-[0.92rem] leading-relaxed text-espresso/60 sm:col-start-3">{p.d}</span>
                </li>
              ))}
            </ul>
          </Reveal>
        </div>

        <div className="relative col-span-12 lg:col-span-6 lg:col-start-7 xl:col-span-6 xl:col-start-7">
          <div className="relative mx-auto w-[88%] max-w-[560px] lg:ml-auto lg:mr-0 lg:w-[86%]">
            <motion.div style={{ y: yA }} className="aspect-[4/5] overflow-hidden bg-cream shadow-[0_40px_80px_-40px_rgba(36,23,16,0.5)]">
              <Picture photo="story" art="dough" tone="ivory" alt="Dough resting on a floured bench" />
            </motion.div>
            <motion.div style={{ y: yB }} className="absolute -bottom-12 -left-6 w-[46%] sm:-left-14 lg:-left-20">
              <div className="aspect-[3/4] overflow-hidden border-[8px] border-ivory bg-cream shadow-[0_30px_60px_-30px_rgba(36,23,16,0.55)]">
                <Picture photo="story-oven" art="oven" tone="cocoa" alt="Loaves baking in a brick oven" />
              </div>
            </motion.div>
            <p className="absolute -right-2 top-10 hidden text-[0.62rem] font-semibold uppercase tracking-widest2 text-espresso/50 xl:block vertical-text">Our kitchen, 5 AM</p>
          </div>
          <Reveal delay={0.2} className="mt-24 flex items-center justify-end gap-6 sm:mt-28 lg:mt-24">
            <span className="font-display text-5xl italic text-caramel-deep sm:text-6xl">{brand.established.replace('Est. ', '')}</span>
            <span className="max-w-[10rem] text-[0.7rem] font-semibold uppercase leading-relaxed tracking-widest2 text-espresso/55">Baking every morning since then</span>
          </Reveal>
        </div>
      </div>
    </section>
  );
}
