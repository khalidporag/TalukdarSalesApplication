import { AnimatePresence, motion } from 'framer-motion';
import { Menu, ShoppingBag, X } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link, NavLink, useLocation, useNavigate } from 'react-router-dom';
import { brand } from '@/data/brand';
import { Logo } from './Logo';
import { useOrder } from './OrderContext';
import { silk } from './motion';

const links = [
  { label: 'Home', to: '/' },
  { label: 'Our Story', to: '/#story' },
  { label: 'Menu', to: '/menu' },
  { label: 'Signature', to: '/#signature' },
  { label: 'Gallery', to: '/#gallery' },
  { label: 'Contact', to: '/contact' },
];

export function Navbar() {
  const [scrolled, setScrolled] = useState(false);
  const [open, setOpen] = useState(false);
  const { count, setOpen: openOrder } = useOrder();
  const { pathname, hash } = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    const on = () => setScrolled(window.scrollY > 40);
    on();
    window.addEventListener('scroll', on, { passive: true });
    return () => window.removeEventListener('scroll', on);
  }, []);

  useEffect(() => { setOpen(false); }, [pathname, hash]);
  useEffect(() => {
    document.body.style.overflow = open ? 'hidden' : '';
    return () => { document.body.style.overflow = ''; };
  }, [open]);

  const go = (to: string) => {
    setOpen(false);
    navigate(to);
  };

  return (
    <>
      <header className={`fixed inset-x-0 top-0 z-50 transition-all duration-700 ease-silk ${scrolled ? 'border-b border-espresso/10 bg-ivory/80 py-3 backdrop-blur-xl' : 'border-b border-transparent py-6'}`}>
        <div className="container-x flex items-center justify-between gap-6">
          <Link to="/" aria-label={`${brand.name} home`} data-cursor><Logo className="h-12 sm:h-14" /></Link>

          <nav className="hidden items-center gap-9 lg:flex" aria-label="Main">
            {links.map((l) => (
              l.to.includes('#') ? (
                <Link key={l.label} to={l.to} className={`u-link text-[0.74rem] font-semibold uppercase tracking-wider2 text-espresso/80 hover:text-espresso ${pathname === '/' && hash === l.to.slice(1) ? 'is-active' : ''}`}>{l.label}</Link>
              ) : (
                <NavLink key={l.label} to={l.to} end className="u-link text-[0.74rem] font-semibold uppercase tracking-wider2 text-espresso/80 hover:text-espresso">{l.label}</NavLink>
              )
            ))}
          </nav>

          <div className="flex items-center gap-3">
            <button onClick={() => openOrder(true)} className="relative flex h-11 w-11 items-center justify-center rounded-full border border-espresso/20 text-espresso transition-colors duration-500 hover:bg-espresso hover:text-ivory" aria-label={`Your order, ${count} items`} data-cursor>
              <ShoppingBag size={18} strokeWidth={1.5} />
              <AnimatePresence>
                {count > 0 ? (
                  <motion.span key={count} initial={{ scale: 0.4, opacity: 0 }} animate={{ scale: 1, opacity: 1 }} exit={{ scale: 0.4, opacity: 0 }} transition={{ duration: 0.5, ease: silk }} className="absolute -right-1 -top-1 flex h-5 min-w-5 items-center justify-center rounded-full bg-caramel px-1 text-[0.62rem] font-bold text-ivory">
                    {count}
                  </motion.span>
                ) : null}
              </AnimatePresence>
            </button>
            <button onClick={() => (pathname === '/menu' ? openOrder(true) : go('/menu'))} className="btn btn-dark btn-sm hidden sm:inline-flex" data-cursor><span>Order Now</span></button>
            <button onClick={() => setOpen(true)} className="flex h-11 w-11 items-center justify-center rounded-full border border-espresso/20 lg:hidden" aria-label="Open menu" aria-expanded={open}>
              <Menu size={19} strokeWidth={1.5} />
            </button>
          </div>
        </div>
      </header>

      <AnimatePresence>
        {open ? (
          <motion.div key="drawer" className="fixed inset-0 z-[80] flex flex-col bg-espresso text-ivory" initial={{ clipPath: 'inset(0 0 100% 0)' }} animate={{ clipPath: 'inset(0 0 0% 0)' }} exit={{ clipPath: 'inset(0 0 100% 0)' }} transition={{ duration: 0.9, ease: silk }} role="dialog" aria-modal="true" aria-label="Menu">
            <div className="container-x flex items-center justify-between py-6">
              <Logo className="h-16" />
              <button onClick={() => setOpen(false)} className="flex h-11 w-11 items-center justify-center rounded-full border border-ivory/25" aria-label="Close menu"><X size={19} strokeWidth={1.5} /></button>
            </div>
            <nav className="container-x flex flex-1 flex-col justify-center gap-1" aria-label="Mobile">
              {links.map((l, i) => (
                <motion.button key={l.label} onClick={() => go(l.to)} initial={{ opacity: 0, y: 26 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.25 + i * 0.07, duration: 0.9, ease: silk }} className="flex items-baseline gap-5 border-b border-ivory/10 py-4 text-left font-display text-[2.6rem] leading-none">
                  <span className="text-[0.7rem] font-sans font-semibold tracking-widest2 text-caramel">0{i + 1}</span>{l.label}
                </motion.button>
              ))}
            </nav>
            <div className="container-x flex items-center justify-between gap-4 pb-8 pt-4">
              <p className="text-[0.7rem] font-semibold uppercase tracking-widest2 text-ivory/50">{brand.location}</p>
              <button onClick={() => go('/menu')} className="btn btn-light btn-sm"><span>Order Now</span></button>
            </div>
          </motion.div>
        ) : null}
      </AnimatePresence>
    </>
  );
}
