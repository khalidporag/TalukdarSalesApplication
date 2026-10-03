import { AnimatePresence, motion } from 'framer-motion';
import { Check, ImagePlus, X } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';
import type { ArtKind, Tone } from '@/art/Art';
import { brand } from '@/data/brand';
import { whatsappLink } from '@/lib/orders';
import { Button, SectionHead } from './Button';
import { MaskLines, Reveal, silk } from './motion';
import { Picture } from './Picture';

const events: { id: string; label: string; line: string; art: ArtKind; tone: Tone }[] = [
  { id: 'Birthday', label: 'Birthday', line: 'Candles, laughter and a cake that is entirely theirs.', art: 'piping', tone: 'blush' },
  { id: 'Wedding', label: 'Wedding', line: 'Tiered, hand-finished and designed around your day.', art: 'tiered', tone: 'ivory' },
  { id: 'Anniversary', label: 'Anniversary', line: 'Something small, rich and made for two.', art: 'cheesecake', tone: 'caramel' },
  { id: 'Corporate', label: 'Corporate', line: 'Gift boxes and platters that carry your brand well.', art: 'box', tone: 'sage' },
  { id: 'Custom Celebration', label: 'Custom Celebration', line: 'Tell us the occasion. We will take it from there.', art: 'macarons', tone: 'wheat' },
];

interface Form { name: string; phone: string; type: string; date: string; guests: string; flavour: string; message: string }
const empty: Form = { name: '', phone: '', type: 'Birthday', date: '', guests: '', flavour: '', message: '' };

function Enquiry({ initialType, onClose }: { initialType: string; onClose: () => void }) {
  const [f, setF] = useState<Form>({ ...empty, type: initialType });
  const [file, setFile] = useState<{ name: string; url: string } | null>(null);
  const [errors, setErrors] = useState<Partial<Record<keyof Form, string>>>({});
  const [sent, setSent] = useState(false);
  const first = useRef<HTMLInputElement>(null);

  useEffect(() => {
    first.current?.focus({ preventScroll: true });
    const esc = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose(); };
    window.addEventListener('keydown', esc);
    document.body.style.overflow = 'hidden';
    return () => { window.removeEventListener('keydown', esc); document.body.style.overflow = ''; };
  }, [onClose]);

  useEffect(() => () => { if (file) URL.revokeObjectURL(file.url); }, [file]);

  const set = (k: keyof Form) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => setF((x) => ({ ...x, [k]: e.target.value }));

  const submit = (e: React.FormEvent) => {
    e.preventDefault();
    const err: typeof errors = {};
    if (!f.name.trim()) err.name = 'Please tell us your name';
    if (f.phone.replace(/\D/g, '').length < 8) err.phone = 'A phone number we can call or message';
    setErrors(err);
    if (Object.keys(err).length) return;
    const msg = [
      `Hello ${brand.name}, I would like to discuss a cake.`,
      '',
      `Name: ${f.name}`, `Phone: ${f.phone}`, `Occasion: ${f.type}`,
      f.date ? `Date: ${f.date}` : '', f.guests ? `Guests: ${f.guests}` : '', f.flavour ? `Flavour / style: ${f.flavour}` : '',
      f.message ? `Message: ${f.message}` : '',
      file ? `(I will send a reference image: ${file.name})` : '',
    ].filter(Boolean).join('\n');
    window.open(whatsappLink(msg), '_blank', 'noopener');
    setSent(true);
  };

  return (
    <motion.div className="fixed inset-0 z-[88] flex items-end justify-center bg-espresso/60 backdrop-blur-sm sm:items-center sm:p-6" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} transition={{ duration: 0.5 }} onClick={onClose}>
      <motion.div role="dialog" aria-modal="true" aria-label="Cake enquiry" className="relative grid max-h-[94svh] w-full max-w-5xl overflow-hidden bg-ivory text-espresso shadow-2xl sm:grid-cols-[0.82fr_1.18fr]" initial={{ y: 60, opacity: 0 }} animate={{ y: 0, opacity: 1 }} exit={{ y: 40, opacity: 0 }} transition={{ duration: 0.8, ease: silk }} onClick={(e) => e.stopPropagation()}>
        <div className="relative hidden bg-cocoa sm:block">
          <div className="absolute inset-0"><Picture photo="custom-enquiry" art="tiered" tone="ivory" alt="A tiered celebration cake" /></div>
          <div className="absolute inset-x-0 bottom-0 bg-gradient-to-t from-espresso/85 to-transparent p-8 pt-24 text-ivory">
            <p className="font-display text-3xl italic leading-tight">Every cake begins with a conversation.</p>
            <p className="mt-3 text-[0.82rem] text-ivory/70">{brand.leadTime}</p>
          </div>
        </div>

        <button onClick={onClose} className="absolute right-4 top-4 z-20 flex h-11 w-11 items-center justify-center rounded-full border border-espresso/20 bg-ivory text-espresso transition-colors duration-500 hover:bg-espresso hover:text-ivory" aria-label="Close"><X size={18} strokeWidth={1.5} /></button>
        <div className="overflow-y-auto p-6 sm:p-10">
          {sent ? (
            <div className="flex min-h-[22rem] flex-col items-start justify-center gap-5">
              <span className="flex h-14 w-14 items-center justify-center rounded-full bg-caramel/15 text-caramel-deep"><Check size={26} strokeWidth={1.6} /></span>
              <h3 className="display-md">Thank you, {f.name.split(' ')[0]}.</h3>
              <p className="max-w-md text-[1rem] leading-relaxed text-espresso/70">Your message is ready in WhatsApp. Send it, attach your reference photo{file ? ` (${file.name})` : ''} if you have one, and we will reply within a day with ideas and a quote.</p>
              <Button variant="line" onClick={onClose}>Close</Button>
            </div>
          ) : (
            <form onSubmit={submit} noValidate>
              <p className="eyebrow">Discuss your cake</p>
              <h3 className="display-md mt-3">Tell us about your day.</h3>

              <div className="mt-8 grid gap-x-8 gap-y-6 sm:grid-cols-2">
                <div>
                  <label className="label" htmlFor="c-name">Name</label>
                  <input ref={first} id="c-name" className="field" value={f.name} onChange={set('name')} autoComplete="name" aria-invalid={!!errors.name} />
                  {errors.name ? <p className="mt-1 text-[0.75rem] text-caramel-deep">{errors.name}</p> : null}
                </div>
                <div>
                  <label className="label" htmlFor="c-phone">Phone</label>
                  <input id="c-phone" className="field" value={f.phone} onChange={set('phone')} inputMode="tel" autoComplete="tel" aria-invalid={!!errors.phone} />
                  {errors.phone ? <p className="mt-1 text-[0.75rem] text-caramel-deep">{errors.phone}</p> : null}
                </div>
                <div>
                  <label className="label" htmlFor="c-type">Event</label>
                  <select id="c-type" className="field" value={f.type} onChange={set('type')}>{events.map((e) => <option key={e.id}>{e.id}</option>)}</select>
                </div>
                <div>
                  <label className="label" htmlFor="c-date">Event date</label>
                  <input id="c-date" type="date" className="field" value={f.date} onChange={set('date')} />
                </div>
                <div>
                  <label className="label" htmlFor="c-guests">Number of guests</label>
                  <input id="c-guests" inputMode="numeric" className="field" value={f.guests} onChange={set('guests')} placeholder="e.g. 25" />
                </div>
                <div>
                  <label className="label" htmlFor="c-flavour">Cake preference</label>
                  <input id="c-flavour" className="field" value={f.flavour} onChange={set('flavour')} placeholder="Flavour, style, colours" />
                </div>
                <div className="sm:col-span-2">
                  <label className="label" htmlFor="c-msg">Message</label>
                  <textarea id="c-msg" rows={3} className="field resize-none" value={f.message} onChange={set('message')} placeholder="Anything we should know: allergies, a name on the cake, a theme…" />
                </div>
                <div className="sm:col-span-2">
                  <span className="label">Reference image</span>
                  <label className="flex cursor-pointer items-center gap-4 border border-dashed border-espresso/30 p-4 transition-colors duration-500 hover:border-caramel hover:bg-cream/50" data-cursor>
                    {file ? <img src={file.url} alt="" className="h-14 w-14 object-cover" /> : <span className="flex h-14 w-14 items-center justify-center bg-cream text-caramel-deep"><ImagePlus size={22} strokeWidth={1.4} /></span>}
                    <span className="text-[0.86rem] text-espresso/70">{file ? file.name : 'Add a photo or inspiration (optional)'}<span className="block text-[0.72rem] text-espresso/45">You will attach it in WhatsApp when you send.</span></span>
                    <input type="file" accept="image/*" className="sr-only" onChange={(e) => { const x = e.target.files?.[0]; if (x) setFile({ name: x.name, url: URL.createObjectURL(x) }); }} />
                  </label>
                </div>
              </div>
              <div className="mt-8 flex flex-wrap items-center justify-between gap-4">
                <p className="max-w-[16rem] text-[0.74rem] leading-relaxed text-espresso/50">Sent through WhatsApp. We never share your details.</p>
                <Button type="submit">Send enquiry</Button>
              </div>
            </form>
          )}
        </div>
      </motion.div>
    </motion.div>
  );
}

export function CustomCake() {
  const [active, setActive] = useState(0);
  const [open, setOpen] = useState(false);
  const e = events[active];

  return (
    <section className="relative overflow-hidden bg-cocoa py-24 text-ivory sm:py-32 lg:py-40">
      <div className="pointer-events-none absolute -right-40 top-10 h-[30rem] w-[30rem] rounded-full bg-caramel/15 blur-[120px]" aria-hidden="true" />
      <div className="container-x relative grid grid-cols-12 items-center gap-y-14 lg:gap-x-12">
        <div className="col-span-12 lg:col-span-6">
          <Reveal><SectionHead index="06" label="Celebrations" light /></Reveal>
          <h2 className="display-lg mt-8"><MaskLines lines={['Made for', <span key="y" className="italic text-caramel">your moments.</span>]} /></h2>
          <Reveal delay={0.1}><p className="mt-7 max-w-md text-[1.02rem] leading-[1.75] text-ivory/70">Custom cakes designed with you, baked from scratch and delivered with care. Choose an occasion to begin.</p></Reveal>

          <Reveal delay={0.15}>
            <ul className="mt-10 border-t border-ivory/15" role="tablist" aria-label="Occasions">
              {events.map((x, i) => (
                <li key={x.id} className="border-b border-ivory/15">
                  <button role="tab" aria-selected={i === active} onClick={() => setActive(i)} onMouseEnter={() => setActive(i)} onFocus={() => setActive(i)} className="group flex w-full items-baseline justify-between gap-6 py-5 text-left" data-cursor>
                    <span className="flex items-baseline gap-5">
                      <span className="text-[0.66rem] font-semibold tracking-widest2 text-caramel">0{i + 1}</span>
                      <span className={`font-display text-[1.9rem] leading-none transition-all duration-700 ease-silk sm:text-[2.4rem] ${i === active ? 'translate-x-2 text-ivory' : 'text-ivory/50 group-hover:text-ivory/80'}`}>{x.label}</span>
                    </span>
                    <span className={`hidden max-w-[15rem] text-right text-[0.8rem] leading-snug text-ivory/60 transition-opacity duration-700 sm:block ${i === active ? 'opacity-100' : 'opacity-0'}`}>{x.line}</span>
                  </button>
                </li>
              ))}
            </ul>
          </Reveal>

          <Reveal delay={0.2} className="mt-10 flex flex-wrap items-center gap-5">
            <Button variant="light" onClick={() => setOpen(true)}>Discuss Your Cake</Button>
            <p className="text-[0.78rem] text-ivory/55">{brand.leadTime}</p>
          </Reveal>
        </div>

        <div className="col-span-12 lg:col-span-5 lg:col-start-8">
          <div className="relative mx-auto aspect-[4/5] w-[88%] max-w-[480px] lg:w-full">
            <div className="absolute -right-4 -top-4 h-full w-full border border-caramel/40 sm:-right-6 sm:-top-6" aria-hidden="true" />
            <div className="relative h-full w-full overflow-hidden bg-espresso shadow-[0_50px_90px_-40px_rgba(0,0,0,0.8)]">
              <AnimatePresence mode="sync">
                <motion.div key={e.id} className="absolute inset-0" initial={{ opacity: 0, scale: 1.05 }} animate={{ opacity: 1, scale: 1 }} exit={{ opacity: 0 }} transition={{ duration: 1.1, ease: silk }}>
                  <Picture photo={`custom-${e.id.toLowerCase().split(' ')[0]}`} art={e.art} tone={e.tone} alt={`${e.label} cake`} />
                </motion.div>
              </AnimatePresence>
            </div>
          </div>
        </div>
      </div>

      <AnimatePresence>{open ? <Enquiry initialType={e.id} onClose={() => setOpen(false)} /> : null}</AnimatePresence>
    </section>
  );
}
