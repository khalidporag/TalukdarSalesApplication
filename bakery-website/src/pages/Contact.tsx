import { ContactSection } from '@/components/ContactSection';
import { CustomCake } from '@/components/CustomCake';
import { MaskLines } from '@/components/motion';
import { brand } from '@/data/brand';
import { useSeo } from '@/lib/seo';

export default function Contact() {
  useSeo({ title: 'Contact & Location', description: `Find ${brand.name} in ${brand.location}. Opening hours, phone, WhatsApp, directions and custom cake enquiries.`, path: '/contact' });
  return (
    <>
      <header className="container-x pt-36 sm:pt-44">
        <p className="eyebrow flex items-center gap-4"><span className="h-px w-10 bg-caramel" />Contact</p>
        <h1 className="display-xl mt-6"><MaskLines immediate lines={['Find', <span key="u" className="italic text-caramel-deep">us.</span>]} /></h1>
      </header>
      <div className="mt-16"><ContactSection /></div>
      <div id="custom"><CustomCake /></div>
    </>
  );
}
