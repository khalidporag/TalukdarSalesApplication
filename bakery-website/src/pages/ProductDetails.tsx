import { ArrowLeft } from 'lucide-react';
import { useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { Picture } from '@/components/Picture';
import { AddButton, SignatureCard } from '@/components/ProductCard';
import { MaskLines, Reveal } from '@/components/motion';
import { brand } from '@/data/brand';
import { categoryLabel } from '@/data/categories';
import { bySlug, products } from '@/data/products';
import { money } from '@/lib/format';
import { useSeo } from '@/lib/seo';
import NotFound from './NotFound';

export default function ProductDetails() {
  const { slug } = useParams();
  const p = slug ? bySlug(slug) : undefined;
  const [vi, setVi] = useState(0);
  const related = useMemo(() => products.filter((x) => p && x.slug !== p.slug).sort((a, b) => Number(b.category === p?.category) - Number(a.category === p?.category)).slice(0, 3), [p]);

  const jsonLd = useMemo(() => p && ({
    '@context': 'https://schema.org',
    '@type': 'Product',
    name: p.name,
    description: p.description,
    category: categoryLabel(p.category),
    brand: { '@type': 'Brand', name: brand.name },
    offers: p.variants.filter((v) => v.price > 0).map((v) => ({ '@type': 'Offer', name: v.label, priceCurrency: 'BDT', price: v.price, availability: 'https://schema.org/InStock' })),
  }), [p]);

  useSeo({ title: p?.name, description: p ? `${p.description} ${brand.name}, ${brand.location}.` : undefined, path: p ? `/product/${p.slug}` : undefined, jsonLd });

  if (!p) return <NotFound />;
  const v = p.variants[vi] ?? p.variants[0];

  return (
    <>
      <section className="container-x pb-24 pt-28 sm:pt-36">
        <Link to="/menu" className="u-link inline-flex items-center gap-2 text-[0.7rem] font-semibold uppercase tracking-widest2 text-espresso/60 hover:text-espresso"><ArrowLeft size={14} strokeWidth={1.6} />Menu</Link>

        <div className="mt-8 grid grid-cols-12 gap-y-12 lg:gap-x-14">
          <div className="col-span-12 lg:col-span-6">
            <div className="lg:sticky lg:top-28">
              <div className="relative aspect-[4/5] overflow-hidden bg-cream shadow-[0_40px_80px_-40px_rgba(36,23,16,0.5)]">
                <Picture photo={p.slug} art={p.art} tone={p.tone} alt={p.name} eager />
                {p.note ? <span className="absolute left-4 top-4 bg-ivory/90 px-3 py-1.5 text-[0.6rem] font-semibold uppercase tracking-widest2 backdrop-blur">{p.note}</span> : null}
              </div>
            </div>
          </div>

          <div className="col-span-12 lg:col-span-5 lg:col-start-8 lg:pt-8">
            <p className="eyebrow">{categoryLabel(p.category)}</p>
            <h1 className="display-lg mt-5"><MaskLines immediate lines={[p.name]} /></h1>
            <p className="mt-6 text-[1.05rem] leading-[1.75] text-espresso/70">{p.story}</p>

            <div className="mt-10">
              <p className="label">Size</p>
              <div className="mt-3 flex flex-wrap gap-2.5" role="radiogroup" aria-label="Size">
                {p.variants.map((x, i) => (
                  <button key={x.label} role="radio" aria-checked={i === vi} onClick={() => setVi(i)} className={`rounded-full border px-5 py-2.5 text-[0.78rem] font-semibold transition-all duration-500 ${i === vi ? 'border-espresso bg-espresso text-ivory' : 'border-espresso/20 text-espresso/70 hover:border-espresso/50'}`} data-cursor>
                    {x.label}{x.price > 0 ? <span className="ml-2 opacity-70">{money(x.price)}</span> : null}
                  </button>
                ))}
              </div>
            </div>

            <div className="mt-9 flex flex-wrap items-center gap-x-8 gap-y-5">
              <p className="font-display text-5xl">{v.price > 0 ? money(v.price) : 'On request'}</p>
              <AddButton product={p} variantIndex={vi} />
            </div>

            <dl className="mt-12 divide-y divide-espresso/12 border-y border-espresso/12 text-[0.92rem]">
              <div className="grid grid-cols-[8rem_1fr] gap-4 py-5"><dt className="label !mb-0 pt-0.5">Made with</dt><dd className="leading-relaxed text-espresso/75">{p.ingredients.join(', ')}</dd></div>
              <div className="grid grid-cols-[8rem_1fr] gap-4 py-5"><dt className="label !mb-0 pt-0.5">Freshness</dt><dd className="leading-relaxed text-espresso/75">{p.category === 'cakes' || p.slug === 'celebration-cake' ? 'Made to order. Best within 2 days, kept cool.' : 'Baked fresh each morning. Best the day it is made.'}</dd></div>
              <div className="grid grid-cols-[8rem_1fr] gap-4 py-5"><dt className="label !mb-0 pt-0.5">Ordering</dt><dd className="leading-relaxed text-espresso/75">{brand.leadTime}</dd></div>
            </dl>
          </div>
        </div>
      </section>

      <section className="border-t border-espresso/10 bg-cream/50 py-24">
        <div className="container-x">
          <Reveal><h2 className="display-md">You may also like</h2></Reveal>
          <div className="mt-12 grid gap-x-8 gap-y-14 sm:grid-cols-2 lg:grid-cols-3">
            {related.map((r, i) => <SignatureCard key={r.slug} product={r} index={i} />)}
          </div>
        </div>
      </section>
    </>
  );
}
