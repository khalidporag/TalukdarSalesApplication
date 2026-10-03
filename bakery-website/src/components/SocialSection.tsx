import { Instagram } from './Icons';
import { useEffect, useState } from 'react';
import { brand } from '@/data/brand';
import { getSocialPosts, type SocialPost } from '@/data/social';
import { ButtonLink } from './Button';
import { MaskLines, Reveal } from './motion';
import { Picture } from './Picture';

export function SocialSection() {
  const [posts, setPosts] = useState<SocialPost[]>([]);
  useEffect(() => { getSocialPosts().then(setPosts); }, []);

  return (
    <section className="relative py-24 sm:py-28 lg:py-36">
      <div className="container-x">
        <div className="flex flex-col items-start gap-8 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <Reveal><p className="eyebrow flex items-center gap-4"><span>09</span><span className="h-px w-12 bg-espresso/25" /><span>Follow along</span></p></Reveal>
            <h2 className="display-lg mt-8"><MaskLines lines={['See what\'s', <span key="f" className="italic text-caramel-deep">fresh.</span>]} /></h2>
            <Reveal delay={0.1}><p className="mt-5 flex items-center gap-2 text-[1rem] font-semibold text-espresso/70"><Instagram size={17} strokeWidth={1.5} />{brand.instagramHandle}</p></Reveal>
          </div>
          <Reveal delay={0.15} className="flex flex-wrap gap-3">
            <ButtonLink href={brand.instagram} target="_blank" rel="noopener noreferrer" variant="dark">Follow Us</ButtonLink>
            <ButtonLink href={brand.facebook} target="_blank" rel="noopener noreferrer" variant="line">On Facebook</ButtonLink>
          </Reveal>
        </div>

        <div className="mt-14 grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6 lg:gap-4">
          {posts.map((p, i) => (
            <Reveal key={p.key} delay={(i % 6) * 0.07}>
              <a href={p.url} target="_blank" rel="noopener noreferrer" className="group relative block aspect-square overflow-hidden bg-cream" aria-label={p.alt} data-cursor>
                <div className="absolute inset-0 transition-transform duration-[1600ms] ease-silk group-hover:scale-[1.08]"><Picture photo={p.key} art={p.art} tone={p.tone} crop={p.crop} alt={p.alt} /></div>
                <div className="absolute inset-0 flex items-center justify-center bg-espresso/0 text-ivory opacity-0 transition-all duration-700 group-hover:bg-espresso/35 group-hover:opacity-100"><Instagram size={26} strokeWidth={1.3} /></div>
              </a>
            </Reveal>
          ))}
        </div>
      </div>
    </section>
  );
}
