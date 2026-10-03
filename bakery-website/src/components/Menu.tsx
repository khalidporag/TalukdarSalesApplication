import { AnimatePresence, LayoutGroup, motion } from 'framer-motion';
import { ArrowRight } from 'lucide-react';
import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { categories, categoryLabel, type CategoryId } from '@/data/categories';
import { products, type Product } from '@/data/products';
import { money } from '@/lib/format';
import { SectionHead } from './Button';
import { MaskLines, Reveal, silk } from './motion';
import { Picture } from './Picture';
import { AddButton } from './ProductCard';

function MenuItem({ p }: { p: Product }) {
  const [vi, setVi] = useState(0);
  const v = p.variants[vi];
  return (
    <motion.article
      layout
      initial={{ opacity: 0, y: 24 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, scale: 0.97 }}
      transition={{ duration: 0.9, ease: silk }}
      className="group flex flex-col"
    >
      <Link to={`/product/${p.slug}`} className="relative block aspect-[5/4] overflow-hidden bg-cream" aria-label={`View ${p.name}`} data-cursor>
        <div className="absolute inset-0 transition-transform duration-[1600ms] ease-silk group-hover:scale-[1.06]">
          <Picture photo={p.slug} art={p.art} tone={p.tone} crop="center" alt={p.name} />
        </div>
        {p.note ? <span className="absolute left-3 top-3 bg-ivory/90 px-2.5 py-1 text-[0.58rem] font-semibold uppercase tracking-widest2 text-espresso backdrop-blur">{p.note}</span> : null}
      </Link>
      <div className="flex flex-1 flex-col pt-5">
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="eyebrow !text-espresso/45">{categoryLabel(p.category)}</p>
            <h3 className="mt-1.5 font-display text-[1.6rem] leading-tight">{p.name}</h3>
          </div>
          <p className="shrink-0 pt-5 text-[0.95rem] font-semibold text-caramel-deep">{v.price > 0 ? money(v.price) : 'On request'}</p>
        </div>
        <p className="mt-2 text-[0.9rem] leading-relaxed text-espresso/60">{p.description}</p>

        <div className="mt-5 flex flex-wrap gap-2" role="radiogroup" aria-label={`${p.name} size`}>
          {p.variants.map((x, i) => (
            <button key={x.label} role="radio" aria-checked={i === vi} onClick={() => setVi(i)} className={`rounded-full border px-3.5 py-1.5 text-[0.7rem] font-semibold tracking-wide transition-all duration-500 ${i === vi ? 'border-espresso bg-espresso text-ivory' : 'border-espresso/20 text-espresso/65 hover:border-espresso/50'}`} data-cursor>
              {x.label}
            </button>
          ))}
        </div>

        <div className="mt-6 flex items-center justify-between gap-3 border-t border-espresso/10 pt-5">
          <Link to={`/product/${p.slug}`} className="u-link text-[0.68rem] font-semibold uppercase tracking-widest2 text-espresso/60 hover:text-espresso">Details</Link>
          <AddButton product={p} variantIndex={vi} small />
        </div>
      </div>
    </motion.article>
  );
}

interface MenuProps {
  /** Show only the first n products and a link to the full menu (home page). */
  limit?: number;
  id?: string;
  heading?: boolean;
}

export function Menu({ limit, id = 'menu', heading = true }: MenuProps) {
  const [cat, setCat] = useState<'all' | CategoryId>('all');
  const filtered = useMemo(() => products.filter((p) => cat === 'all' || p.category === cat), [cat]);
  const list = limit ? filtered.slice(0, limit) : filtered;
  const counts = useMemo(() => {
    const c: Record<string, number> = { all: products.length };
    products.forEach((p) => { c[p.category] = (c[p.category] ?? 0) + 1; });
    return c;
  }, []);

  return (
    <section id={id} className="relative py-24 sm:py-32">
      <div className="container-x">
        {heading ? (
          <div className="flex flex-col gap-8 lg:flex-row lg:items-end lg:justify-between">
            <div>
              <Reveal><SectionHead index="04" label="The Menu" /></Reveal>
              <h2 className="display-lg mt-8"><MaskLines lines={['Pick what', <span key="m" className="italic text-caramel-deep">you love.</span>]} /></h2>
            </div>
            <Reveal delay={0.1}><p className="max-w-sm text-[1rem] leading-relaxed text-espresso/65">Choose a size, add it to your order, and send it to us on WhatsApp. We confirm everything personally.</p></Reveal>
          </div>
        ) : null}

        <div className="sticky top-[4.4rem] z-30 -mx-5 mt-10 border-y border-espresso/10 bg-ivory/90 px-5 py-4 backdrop-blur-lg sm:mx-0 sm:px-0">
          <div className="no-scrollbar flex gap-2.5 overflow-x-auto" role="tablist" aria-label="Menu categories">
            {categories.map((c) => (
              <button key={c.id} role="tab" aria-selected={cat === c.id} aria-pressed={cat === c.id} className="pill shrink-0" onClick={() => setCat(c.id)} data-cursor>
                {c.label}<span className="ml-2 text-[0.62rem] opacity-60">{counts[c.id] ?? 0}</span>
              </button>
            ))}
          </div>
        </div>

        <LayoutGroup>
          <motion.div layout className="mt-12 grid grid-cols-1 gap-x-8 gap-y-14 sm:grid-cols-2 lg:grid-cols-3">
            <AnimatePresence mode="popLayout">
              {list.map((p) => <MenuItem key={p.slug} p={p} />)}
            </AnimatePresence>
          </motion.div>
        </LayoutGroup>

        {limit && filtered.length > limit ? (
          <Reveal className="mt-16 flex justify-center">
            <Link to="/menu" className="btn btn-line" data-cursor><span>See the full menu <ArrowRight size={16} strokeWidth={1.7} /></span></Link>
          </Reveal>
        ) : null}
      </div>
    </section>
  );
}
