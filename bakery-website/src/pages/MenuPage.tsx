import { Menu } from '@/components/Menu';
import { MaskLines } from '@/components/motion';
import { brand } from '@/data/brand';
import { products } from '@/data/products';
import { useSeo } from '@/lib/seo';

export default function MenuPage() {
  useSeo({
    title: 'Menu',
    description: `Cakes, pastries, cookies, breads and desserts from ${brand.name}. Choose a size and order on WhatsApp.`,
    path: '/menu',
    jsonLd: {
      '@context': 'https://schema.org',
      '@type': 'Menu',
      name: `${brand.name} menu`,
      hasMenuItem: products.map((p) => ({ '@type': 'MenuItem', name: p.name, description: p.description, offers: { '@type': 'Offer', priceCurrency: 'BDT', price: Math.min(...p.variants.filter((v) => v.price).map((v) => v.price)) || undefined } })),
    },
  });
  return (
    <>
      <header className="container-x pt-36 sm:pt-44">
        <p className="eyebrow flex items-center gap-4"><span className="h-px w-10 bg-caramel" />Menu</p>
        <h1 className="display-xl mt-6"><MaskLines immediate lines={['The', <span key="m" className="italic text-caramel-deep">menu.</span>]} /></h1>
      </header>
      <Menu heading={false} id="menu" />
    </>
  );
}
