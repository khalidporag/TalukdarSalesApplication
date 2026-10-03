import { motion, useScroll, useTransform } from 'framer-motion';
import { useRef } from 'react';
import { bySlug } from '@/data/products';
import { money } from '@/lib/format';
import { ButtonLink } from './Button';
import { Picture } from './Picture';
import { MaskLines, Reveal } from './motion';
import { AddButton } from './ProductCard';

/** The single product we want people to remember, set like a fashion advertisement. */
export function FeaturedProduct({ slug = 'basque-cheesecake' }: { slug?: string }) {
  const p = bySlug(slug)!;
  const ref = useRef<HTMLElement>(null);
  const { scrollYProgress } = useScroll({ target: ref, offset: ['start end', 'end start'] });
  const imgY = useTransform(scrollYProgress, [0, 1], ['-7%', '7%']);
  const frameY = useTransform(scrollYProgress, [0, 1], ['4%', '-4%']);
  const textY = useTransform(scrollYProgress, [0, 1], ['24px', '-24px']);

  return (
    <section ref={ref} id="signature" className="relative overflow-hidden bg-espresso py-24 text-ivory sm:py-32 lg:py-44">
      <div className="pointer-events-none absolute -left-40 top-1/3 h-[34rem] w-[34rem] rounded-full bg-caramel/10 blur-[120px]" aria-hidden="true" />
      <div className="container-x relative grid grid-cols-12 items-center gap-y-16 lg:gap-x-12">
        <div className="relative col-span-12 lg:col-span-6">
          <motion.div style={{ y: frameY }} className="pointer-events-none absolute -left-4 top-6 h-full w-[92%] border border-caramel/40 sm:-left-8 sm:top-10" aria-hidden="true" />
          <div className="relative ml-auto aspect-[4/5] w-[92%] overflow-hidden bg-cocoa shadow-[0_60px_100px_-40px_rgba(0,0,0,0.8)]">
            <motion.div style={{ y: imgY }} className="absolute -inset-[8%]">
              <Picture photo={`${p.slug}-feature`} art="cheesecake" tone="caramel" alt={p.name} />
            </motion.div>
          </div>
          <span className="vertical-text absolute -right-1 top-0 hidden text-[0.62rem] font-semibold uppercase tracking-widest2 text-ivory/40 sm:block">Baked to order · Serves 6 to 8</span>
        </div>

        <motion.div style={{ y: textY }} className="col-span-12 lg:col-span-5 lg:col-start-8">
          <Reveal><p className="eyebrow !text-caramel flex items-center gap-4"><span>03</span><span className="h-px w-12 bg-ivory/30" /><span>The Signature</span></p></Reveal>
          <h2 className="display-lg mt-8 text-ivory"><MaskLines lines={['Burnt on', <span key="p" className="italic text-caramel">purpose.</span>]} /></h2>
          <Reveal delay={0.1}><p className="mt-8 max-w-[28rem] text-[1.04rem] leading-[1.8] text-ivory/70">{p.story} Ours is baked at a fierce heat, left to rest overnight, and served with nothing but a fork.</p></Reveal>

          <Reveal delay={0.2}>
            <dl className="mt-10 grid grid-cols-2 gap-x-8 gap-y-5 border-t border-ivory/15 pt-8 text-[0.9rem]">
              <div><dt className="text-[0.62rem] font-semibold uppercase tracking-widest2 text-caramel">Made with</dt><dd className="mt-1.5 leading-relaxed text-ivory/80">{p.ingredients.join(', ')}</dd></div>
              <div><dt className="text-[0.62rem] font-semibold uppercase tracking-widest2 text-caramel">Best enjoyed</dt><dd className="mt-1.5 leading-relaxed text-ivory/80">Slightly cool, a little soft, with strong black coffee.</dd></div>
            </dl>
          </Reveal>

          <Reveal delay={0.25} className="mt-10 flex flex-wrap items-center gap-x-8 gap-y-5">
            <p className="font-display text-4xl text-ivory"><span className="text-[0.8rem] font-sans font-semibold uppercase tracking-widest2 text-ivory/50">from </span>{money(Math.min(...p.variants.map((v) => v.price)))}</p>
            <AddButton product={p} variantIndex={1} label="Order This" variant="light" />
            <ButtonLink to={`/product/${p.slug}`} variant="line-light" small>Details</ButtonLink>
          </Reveal>
        </motion.div>
      </div>
    </section>
  );
}
