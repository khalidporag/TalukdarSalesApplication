import { ButtonLink } from '@/components/Button';
import { useSeo } from '@/lib/seo';

export default function NotFound() {
  useSeo({ title: 'Page not found' });
  return (
    <section className="container-x flex min-h-[80svh] flex-col items-start justify-center gap-6 pt-28">
      <p className="eyebrow">404</p>
      <h1 className="display-lg">That page has<br /><span className="italic text-caramel-deep">gone out of the oven.</span></h1>
      <ButtonLink to="/">Back home</ButtonLink>
    </section>
  );
}
