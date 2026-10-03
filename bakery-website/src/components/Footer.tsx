import { Link } from 'react-router-dom';
import { brand } from '@/data/brand';
import { Facebook, Instagram, WhatsApp } from './Icons';
import { Logo } from './Logo';

export function Footer() {
  const social = [
    { label: 'Facebook', href: brand.facebook, Icon: Facebook },
    { label: 'Instagram', href: brand.instagram, Icon: Instagram },
    { label: 'WhatsApp', href: `https://wa.me/${brand.whatsapp}`, Icon: WhatsApp },
  ];
  return (
    <footer className="relative border-t border-ivory/10 bg-ink text-ivory">
      <div className="container-x grid grid-cols-2 gap-x-6 gap-y-12 py-20 sm:py-24 lg:grid-cols-12 lg:gap-x-10">
        <div className="col-span-2 lg:col-span-4">
          <Logo light />
          <p className="mt-7 max-w-xs font-display text-2xl italic leading-snug text-ivory/85">{brand.tagline}</p>
          <p className="mt-4 max-w-xs text-[0.88rem] leading-relaxed text-ivory/55">{brand.statement}</p>
          <div className="mt-7 flex gap-3">
            {social.map(({ label, href, Icon }) => (
              <a key={label} href={href} target="_blank" rel="noopener noreferrer" className="flex h-11 w-11 items-center justify-center rounded-full border border-ivory/20 transition-colors duration-500 hover:bg-ivory hover:text-espresso" aria-label={label} data-cursor><Icon size={18} /></a>
            ))}
          </div>
        </div>

        <nav className="lg:col-span-2 lg:col-start-6" aria-label="Footer">
          <p className="eyebrow !text-caramel">Explore</p>
          <ul className="mt-5 space-y-3 text-[0.92rem] text-ivory/75">
            <li><Link to="/" className="u-link">Home</Link></li>
            <li><Link to="/about" className="u-link">Our Story</Link></li>
            <li><Link to="/menu" className="u-link">Menu</Link></li>
            <li><Link to="/#signature" className="u-link">Signature</Link></li>
            <li><Link to="/#gallery" className="u-link">Gallery</Link></li>
            <li><Link to="/contact" className="u-link">Contact</Link></li>
          </ul>
        </nav>

        <div className="col-span-1 lg:col-span-2">
          <p className="eyebrow !text-caramel">Visit</p>
          <address className="mt-5 space-y-1 text-[0.92rem] not-italic leading-relaxed text-ivory/75">
            {brand.address.map((l) => <span key={l} className="block">{l}</span>)}
            <a href={`tel:${brand.phone.replace(/\s/g, '')}`} className="u-link mt-3 block">{brand.phone}</a>
            <a href={`mailto:${brand.email}`} className="u-link block">{brand.email}</a>
          </address>
        </div>

        <div className="col-span-1 lg:col-span-3">
          <p className="eyebrow !text-caramel">Hours</p>
          <ul className="mt-5 space-y-3 text-[0.92rem] text-ivory/75">
            {brand.hours.map((h) => <li key={h.days}><span className="block text-ivory/50">{h.days}</span>{h.time}</li>)}
          </ul>
        </div>
      </div>

      <div className="container-x flex flex-col gap-3 border-t border-ivory/10 py-7 text-[0.74rem] text-ivory/45 sm:flex-row sm:items-center sm:justify-between">
        <p>© {new Date().getFullYear()} {brand.name}. All rights reserved.</p>
        <p>Baked with care in {brand.location}.</p>
      </div>
    </footer>
  );
}
