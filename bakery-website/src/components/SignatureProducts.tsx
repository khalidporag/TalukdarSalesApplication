import { AnimatePresence, LayoutGroup, motion } from 'framer-motion';
import { useMemo, useState } from 'react';
import { categories, type CategoryId } from '@/data/categories';
import { products } from '@/data/products';
import { SectionHead } from './Button';
import { MaskLines, Reveal } from './motion';
import { SignatureCard } from './ProductCard';

export function SignatureProducts() {
  const sig = useMemo(() => products.filter((p) => p.signature), []);
  const present = useMemo(() => new Set(sig.map((p) => p.category)), [sig]);
  const [cat, setCat] = useState<'all' | CategoryId>('all');
  const list = sig.filter((p) => cat === 'all' || p.category === cat);

  return (
    <section id="signatures" className="relative bg-cream/55 py-24 sm:py-32 lg:py-40">
      <div className="container-x">
        <div className="flex flex-col gap-10 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <Reveal><SectionHead index="02" label="Favourites" /></Reveal>
            <h2 className="display-lg mt-8 text-espresso"><MaskLines lines={['Our', <span key="s" className="italic text-caramel-deep">Signatures</span>]} /></h2>
          </div>
          <div className="max-w-sm lg:pb-3">
            <Reveal delay={0.15}><p className="text-[1rem] leading-relaxed text-espresso/65">The few things we are known for, made the same careful way every time.</p></Reveal>
          </div>
        </div>

        <Reveal delay={0.2} className="no-scrollbar -mx-5 mt-12 flex gap-3 overflow-x-auto px-5 pb-2 sm:mx-0 sm:flex-wrap sm:overflow-visible sm:px-0">
          {categories.filter((c) => c.id === 'all' || present.has(c.id as CategoryId)).map((c) => (
            <button key={c.id} className="pill shrink-0" aria-pressed={cat === c.id} onClick={() => setCat(c.id)} data-cursor>{c.label}</button>
          ))}
        </Reveal>

        <LayoutGroup>
          <motion.div layout className="mt-14 grid grid-cols-1 gap-x-8 gap-y-16 sm:grid-cols-2 lg:grid-cols-3 lg:gap-x-10">
            <AnimatePresence mode="popLayout">
              {list.map((p, i) => (
                <div key={p.slug} className={i % 3 === 1 ? 'lg:mt-16' : i % 3 === 2 ? 'lg:mt-6' : ''}>
                  <SignatureCard product={p} index={i} />
                </div>
              ))}
            </AnimatePresence>
          </motion.div>
        </LayoutGroup>
      </div>
    </section>
  );
}
