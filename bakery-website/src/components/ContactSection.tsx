import { motion } from 'framer-motion';
import { Clock, MapPin, Phone } from 'lucide-react';
import { brand } from '@/data/brand';
import { ButtonLink } from './Button';
import { Facebook, Instagram, WhatsApp } from './Icons';
import { MaskLines, Reveal } from './motion';
import { useOpenStatus } from '@/lib/hours';

/** A drawn neighbourhood rather than an embedded map: quiet, fast and on-brand. Swap for a Google Maps iframe if preferred. */
function MapArt() {
  return (
    <svg viewBox="0 0 600 520" className="h-full w-full" preserveAspectRatio="xMidYMid slice" role="img" aria-label={`Map showing ${brand.name}`}>
      <rect width="600" height="520" fill="#EADCC2" />
      <g fill="#DCCBAA">
        {[[30, 40, 120, 90], [180, 30, 100, 110], [310, 50, 110, 70], [450, 30, 120, 100], [30, 170, 90, 120], [160, 180, 130, 70], [330, 170, 80, 120], [440, 190, 130, 90], [40, 330, 130, 110], [200, 300, 100, 130], [330, 330, 120, 100], [480, 330, 90, 150]].map(([x, y, w, h], i) => <rect key={i} x={x} y={y} width={w} height={h} rx="6" />)}
      </g>
      <path d="M0 140 H600 M0 300 H600 M150 0 V520 M300 0 V520 M430 0 V520" stroke="#F7EEDC" strokeWidth="16" />
      <path d="M0 140 H600 M0 300 H600 M150 0 V520 M300 0 V520 M430 0 V520" stroke="#fff" strokeOpacity="0.5" strokeWidth="2" strokeDasharray="10 12" />
      <path d="M-20 420 C150 340 300 460 620 300" stroke="#C7D3C0" strokeWidth="38" fill="none" strokeLinecap="round" opacity="0.75" />
      <g fill="#B9C6A3" opacity="0.7"><circle cx="90" cy="230" r="30" /><circle cx="520" cy="110" r="26" /></g>
      <g transform="translate(300 220)">
        <circle r="46" fill="#C8962E" opacity="0.14"><animate attributeName="r" values="26;62;26" dur="4.5s" repeatCount="indefinite" /><animate attributeName="opacity" values="0.3;0;0.3" dur="4.5s" repeatCount="indefinite" /></circle>
        <path d="M0 -64 C-24 -64 -38 -46 -38 -26 C-38 2 0 36 0 36 C0 36 38 2 38 -26 C38 -46 24 -64 0 -64Z" fill="#241710" />
        <circle cy="-28" r="13" fill="#F8F2E9" />
        <path d="M0 -37 c4 3 6 6 6 10 a6 6 0 0 1 -12 0 c0 -4 2 -7 6 -10Z" fill="#C8962E" />
      </g>
    </svg>
  );
}

export function ContactSection() {
  const status = useOpenStatus();
  return (
    <section className="relative bg-espresso py-24 text-ivory sm:py-32 lg:py-40">
      <div className="container-x grid grid-cols-12 gap-y-14 lg:gap-x-12">
        <div className="col-span-12 lg:col-span-6">
          <Reveal><p className="eyebrow !text-caramel flex items-center gap-4"><span>10</span><span className="h-px w-12 bg-ivory/30" /><span>Visit us</span></p></Reveal>
          <h2 className="display-xl mt-8 !text-[clamp(3.2rem,8vw,7.4rem)] text-ivory"><MaskLines lines={['Come say', <span key="h" className="italic text-caramel">hello.</span>]} /></h2>

          <Reveal delay={0.15}>
            <p className={`mt-8 inline-flex items-center gap-3 rounded-full border px-4 py-2 text-[0.7rem] font-semibold uppercase tracking-widest2 ${status.open ? 'border-caramel/50 text-caramel' : 'border-ivory/20 text-ivory/60'}`}>
              <span className={`h-2 w-2 rounded-full ${status.open ? 'bg-caramel' : 'bg-ivory/40'}`} />{status.label}
            </p>
          </Reveal>

          <Reveal delay={0.2}>
            <dl className="mt-10 divide-y divide-ivory/12 border-y border-ivory/12">
              <div className="grid grid-cols-[2.2rem_1fr] gap-4 py-5"><MapPin size={18} strokeWidth={1.4} className="mt-1 text-caramel" /><div><dt className="sr-only">Address</dt><dd className="font-display text-2xl leading-snug">{brand.address.map((l) => <span key={l} className="block">{l}</span>)}</dd></div></div>
              <div className="grid grid-cols-[2.2rem_1fr] gap-4 py-5"><Phone size={18} strokeWidth={1.4} className="mt-1 text-caramel" /><div><dt className="sr-only">Phone</dt><dd className="font-display text-2xl"><a href={`tel:${brand.phone.replace(/\s/g, '')}`} className="u-link">{brand.phone}</a></dd></div></div>
              <div className="grid grid-cols-[2.2rem_1fr] gap-4 py-5"><Clock size={18} strokeWidth={1.4} className="mt-1 text-caramel" /><div><dt className="sr-only">Opening hours</dt><dd className="space-y-1">{brand.hours.map((h) => <span key={h.days} className="flex justify-between gap-6 text-[0.95rem]"><span className="text-ivory/60">{h.days}</span><span>{h.time}</span></span>)}</dd></div></div>
            </dl>
          </Reveal>

          <Reveal delay={0.25} className="mt-9 flex flex-wrap items-center gap-3">
            <ButtonLink href={brand.mapsUrl} target="_blank" rel="noopener noreferrer" variant="light">Open in Google Maps</ButtonLink>
            <a href={`https://wa.me/${brand.whatsapp}`} target="_blank" rel="noopener noreferrer" className="flex h-12 w-12 items-center justify-center rounded-full border border-ivory/25 transition-colors duration-500 hover:bg-ivory hover:text-espresso" aria-label="WhatsApp" data-cursor><WhatsApp size={19} /></a>
            <a href={brand.facebook} target="_blank" rel="noopener noreferrer" className="flex h-12 w-12 items-center justify-center rounded-full border border-ivory/25 transition-colors duration-500 hover:bg-ivory hover:text-espresso" aria-label="Facebook" data-cursor><Facebook size={19} /></a>
            <a href={brand.instagram} target="_blank" rel="noopener noreferrer" className="flex h-12 w-12 items-center justify-center rounded-full border border-ivory/25 transition-colors duration-500 hover:bg-ivory hover:text-espresso" aria-label="Instagram" data-cursor><Instagram size={19} /></a>
          </Reveal>
        </div>

        <div className="col-span-12 lg:col-span-6">
          <motion.a href={brand.mapsUrl} target="_blank" rel="noopener noreferrer" initial={{ opacity: 0, y: 30 }} whileInView={{ opacity: 1, y: 0 }} viewport={{ once: true }} transition={{ duration: 1.2 }} className="group relative block aspect-[4/4.2] overflow-hidden shadow-[0_50px_90px_-40px_rgba(0,0,0,0.7)] lg:aspect-auto lg:h-full lg:min-h-[34rem]" aria-label="Open the map" data-cursor>
            <div className="absolute inset-0 transition-transform duration-[2000ms] ease-[cubic-bezier(0.22,0.61,0.36,1)] group-hover:scale-[1.04]"><MapArt /></div>
            <div className="absolute inset-x-0 bottom-0 flex items-center justify-between bg-gradient-to-t from-espresso/85 to-transparent p-6 pt-16">
              <span className="font-display text-2xl italic">{brand.name}</span>
              <span className="text-[0.64rem] font-semibold uppercase tracking-widest2 text-ivory/80">Get directions →</span>
            </div>
          </motion.a>
        </div>
      </div>
    </section>
  );
}
