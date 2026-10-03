import { AnimatePresence, motion } from 'framer-motion';
import { Check, Plus } from 'lucide-react';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { categoryLabel } from '@/data/categories';
import { startingPrice, type Product } from '@/data/products';
import { money } from '@/lib/format';
import { Picture } from './Picture';
import { useOrder } from './OrderContext';
import { silk } from './motion';

/** "Added" tick that returns to the plus after a moment. */
export function useAdded() {
  const [added, setAdded] = useState(false);
  const flash = () => {
    setAdded(true);
    window.setTimeout(() => setAdded(false), 1800);
  };
  return [added, flash] as const;
}

export function AddButton({ product, variantIndex = 0, small = false, label = 'Add to Order', variant = 'dark' }: { product: Product; variantIndex?: number; small?: boolean; label?: string; variant?: 'dark' | 'light' }) {
  const { add, setOpen } = useOrder();
  const [added, flash] = useAdded();
  const v = product.variants[variantIndex] ?? product.variants[0];
  const quote = v.price === 0;
  return (
    <button
      onClick={() => {
        if (quote) {
          setOpen(false);
          const el = document.getElementById('custom');
          if (el) el.scrollIntoView({ behavior: 'smooth' });
          else window.location.assign(`${import.meta.env.BASE_URL}#custom`);
          return;
        }
        add({ slug: product.slug, name: product.name, variant: v.label, price: v.price });
        flash();
      }}
      className={`btn ${variant === 'light' ? 'btn-light' : 'btn-dark'} ${small ? 'btn-sm' : ''}`}
      data-cursor
    >
      <span>
        <AnimatePresence mode="wait" initial={false}>
          {added ? (
            <motion.span key="a" initial={{ opacity: 0, y: 6 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0, y: -6 }} transition={{ duration: 0.35 }} className="inline-flex items-center gap-2"><Check size={15} strokeWidth={2} />Added</motion.span>
          ) : (
            <motion.span key="b" initial={{ opacity: 0, y: 6 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0, y: -6 }} transition={{ duration: 0.35 }} className="inline-flex items-center gap-2"><Plus size={15} strokeWidth={2} />{quote ? 'Request a quote' : label}</motion.span>
          )}
        </AnimatePresence>
      </span>
    </button>
  );
}

/** Editorial signature card: image first, details settle in on hover. */
export function SignatureCard({ product, index }: { product: Product; index: number }) {
  return (
    <motion.article
      layout
      initial={{ opacity: 0, y: 40 }}
      whileInView={{ opacity: 1, y: 0 }}
      viewport={{ once: true, margin: '0px 0px -10% 0px' }}
      exit={{ opacity: 0, scale: 0.97 }}
      transition={{ duration: 1.1, delay: (index % 3) * 0.1, ease: silk }}
      className="group relative transition-transform duration-[900ms] ease-silk hover:-translate-y-2"
    >
      <Link to={`/product/${product.slug}`} className="block" aria-label={`View ${product.name}`} data-cursor>
        <div className={`relative overflow-hidden bg-cream shadow-[0_24px_50px_-34px_rgba(36,23,16,0.45)] transition-shadow duration-[900ms] ease-silk group-hover:shadow-[0_40px_70px_-30px_rgba(36,23,16,0.55)] ${index % 3 === 0 ? 'aspect-[4/5.6]' : 'aspect-[4/5]'}`}>
          <div className="absolute inset-0 transition-transform duration-[1600ms] ease-silk group-hover:scale-[1.07]">
            <Picture photo={product.slug} art={product.art} tone={product.tone} alt={product.name} />
          </div>
          <span className="absolute left-4 top-4 bg-ivory/90 px-3 py-1.5 text-[0.6rem] font-semibold uppercase tracking-widest2 text-espresso backdrop-blur">{categoryLabel(product.category)}</span>
          {product.note ? <span className="absolute right-4 top-4 text-[0.6rem] font-semibold uppercase tracking-widest2 text-ivory drop-shadow">{product.note}</span> : null}
          {/* hover details */}
          <div className="absolute inset-x-0 bottom-0 translate-y-6 bg-gradient-to-t from-espresso/85 via-espresso/55 to-transparent p-5 pt-16 opacity-0 transition-all duration-[900ms] ease-silk group-hover:translate-y-0 group-hover:opacity-100 max-lg:translate-y-0 max-lg:opacity-100 max-lg:from-espresso/70 max-lg:pt-12">
            <p className="max-w-[18rem] text-[0.86rem] leading-relaxed text-ivory/90 max-lg:hidden">{product.description}</p>
            <span className="mt-3 inline-flex items-center gap-2 text-[0.66rem] font-semibold uppercase tracking-widest2 text-ivory">View details <span className="h-px w-8 bg-caramel transition-all duration-700 group-hover:w-14" /></span>
          </div>
        </div>
        <div className="mt-5 flex items-baseline justify-between gap-4">
          <h3 className="font-display text-[1.7rem] leading-tight text-espresso sm:text-[1.9rem]">{product.name}</h3>
          <p className="shrink-0 text-[0.86rem] font-semibold text-caramel-deep">{product.variants.length > 1 ? 'from ' : ''}{money(startingPrice(product))}</p>
        </div>
      </Link>
    </motion.article>
  );
}
