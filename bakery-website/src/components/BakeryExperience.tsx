import type { ArtKind, Tone } from '@/art/Art';
import { SectionHead } from './Button';
import { MaskLines, Reveal } from './motion';
import { Picture } from './Picture';

const moments: { key: string; n: string; title: string; text: string; art: ArtKind; tone: Tone; offset: string; ratio: string }[] = [
  { key: 'kitchen-mix', n: '01', title: 'Mixing', text: 'Small batches, mixed by hand, rested overnight.', art: 'dough', tone: 'ivory', offset: 'lg:mt-0', ratio: 'aspect-[4/5]' },
  { key: 'kitchen-bake', n: '02', title: 'Baking', text: 'In the oven before sunrise, out when the crust sings.', art: 'oven', tone: 'cocoa', offset: 'lg:mt-24', ratio: 'aspect-[4/5.4]' },
  { key: 'kitchen-decorate', n: '03', title: 'Decorating', text: 'Every cake finished slowly, one steady line at a time.', art: 'piping', tone: 'ivory', offset: 'lg:mt-8', ratio: 'aspect-[4/5]' },
  { key: 'kitchen-serve', n: '04', title: 'Serving', text: 'Boxed with care and tied with ribbon, ready to gift.', art: 'box', tone: 'blush', offset: 'lg:mt-32', ratio: 'aspect-[4/5.4]' },
];

export function BakeryExperience() {
  return (
    <section id="kitchen" className="relative overflow-hidden py-24 sm:py-32 lg:py-40">
      <div className="container-x">
        <div className="grid grid-cols-12 gap-y-6">
          <div className="col-span-12 lg:col-span-7">
            <Reveal><SectionHead index="05" label="Fresh from the oven" /></Reveal>
            <h2 className="display-lg mt-8"><MaskLines lines={['From our kitchen', <span key="t" className="italic text-caramel-deep">to your table.</span>]} /></h2>
          </div>
          <div className="col-span-12 flex items-end lg:col-span-4 lg:col-start-9">
            <Reveal delay={0.15}><p className="text-[1rem] leading-relaxed text-espresso/65">Four unhurried steps stand between flour and the moment you open the box.</p></Reveal>
          </div>
        </div>

        <div className="mt-16 grid grid-cols-2 gap-x-4 gap-y-12 sm:gap-x-8 lg:mt-20 lg:grid-cols-4 lg:gap-x-8">
          {moments.map((m, i) => (
            <Reveal key={m.key} delay={(i % 2) * 0.12} className={`${m.offset} ${i % 2 ? 'mt-10 sm:mt-14' : ''} lg:mt-0 ${m.offset}`}>
              <figure className="group">
                <div className={`relative overflow-hidden bg-cream ${m.ratio}`}>
                  <div className="absolute inset-0 transition-transform duration-[1800ms] ease-silk group-hover:scale-[1.06]">
                    <Picture photo={m.key} art={m.art} tone={m.tone} alt={m.title} />
                  </div>
                  <span className="absolute left-3 top-3 bg-ivory/90 px-2.5 py-1 font-display text-lg italic leading-none text-espresso backdrop-blur sm:text-xl">{m.n}</span>
                </div>
                <figcaption className="mt-5">
                  <p className="font-display text-2xl sm:text-[1.9rem]">{m.title}</p>
                  <p className="mt-1.5 max-w-[16rem] text-[0.88rem] leading-relaxed text-espresso/60">{m.text}</p>
                </figcaption>
              </figure>
            </Reveal>
          ))}
        </div>
      </div>
    </section>
  );
}
