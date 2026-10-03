import { BakeryExperience } from '@/components/BakeryExperience';
import { BrandStory } from '@/components/BrandStory';
import { ButtonLink } from '@/components/Button';
import { MaskLines, Reveal } from '@/components/motion';
import { brand } from '@/data/brand';
import { useSeo } from '@/lib/seo';

const timeline = [
  { y: '2021', t: 'One oven', d: 'We begin in a home kitchen, baking for friends and neighbours.' },
  { y: '2022', t: 'A first shop window', d: 'Word travels. Orders move from messages to a small storefront.' },
  { y: '2023', t: 'Celebration cakes', d: 'Birthdays, weddings and anniversaries become the heart of the bakery.' },
  { y: 'Today', t: 'Still small, still careful', d: 'More hands, the same slow habits and the same real butter.' },
];

export default function About() {
  useSeo({ title: 'Our Story', description: `The story of ${brand.name}: a small kitchen, a slow clock and real butter.`, path: '/about' });
  return (
    <>
      <header className="container-x pt-36 sm:pt-44">
        <p className="eyebrow flex items-center gap-4"><span className="h-px w-10 bg-caramel" />About</p>
        <h1 className="display-xl mt-6"><MaskLines immediate lines={['Our', <span key="s" className="italic text-caramel-deep">story.</span>]} /></h1>
      </header>
      <BrandStory />
      <section className="border-y border-espresso/10 bg-cream/50 py-24 sm:py-28">
        <div className="container-x">
          <ol className="grid gap-10 sm:grid-cols-2 lg:grid-cols-4">
            {timeline.map((x, i) => (
              <Reveal key={x.y} as="li" delay={i * 0.1}>
                <p className="font-display text-5xl italic text-caramel-deep">{x.y}</p>
                <p className="mt-4 font-display text-2xl">{x.t}</p>
                <p className="mt-2 text-[0.92rem] leading-relaxed text-espresso/60">{x.d}</p>
              </Reveal>
            ))}
          </ol>
        </div>
      </section>
      <BakeryExperience />
      <section className="container-x py-24 text-center sm:py-32">
        <h2 className="display-md">Come taste the difference.</h2>
        <div className="mt-8 flex justify-center gap-3"><ButtonLink to="/menu">Explore Our Menu</ButtonLink><ButtonLink to="/contact" variant="line">Visit Us</ButtonLink></div>
      </section>
    </>
  );
}
