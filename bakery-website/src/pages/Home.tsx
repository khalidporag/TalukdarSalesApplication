import { lazy } from 'react';
import { BrandStory } from '@/components/BrandStory';
import { Defer } from '@/components/Defer';
import { FeaturedProduct } from '@/components/FeaturedProduct';
import { Hero } from '@/components/Hero';
import { Marquee } from '@/components/Marquee';
import { SignatureProducts } from '@/components/SignatureProducts';
import { Menu } from '@/components/Menu';
import { brand } from '@/data/brand';
import { useSeo } from '@/lib/seo';

const lz = <T extends string>(loader: () => Promise<Record<T, React.ComponentType>>, name: T) => lazy(() => loader().then((m) => ({ default: m[name] })));
const BakeryExperience = lz(() => import('@/components/BakeryExperience'), 'BakeryExperience');
const CustomCake = lz(() => import('@/components/CustomCake'), 'CustomCake');
const Gallery = lz(() => import('@/components/Gallery'), 'Gallery');
const Testimonials = lz(() => import('@/components/Testimonials'), 'Testimonials');
const SocialSection = lz(() => import('@/components/SocialSection'), 'SocialSection');
const ContactSection = lz(() => import('@/components/ContactSection'), 'ContactSection');

export default function Home() {
  useSeo({ description: `${brand.name} is a boutique bakery in ${brand.location} for handcrafted cakes, laminated pastries, cookies and slow-fermented breads. Custom celebration cakes made to order.`, path: '/' });
  return (
    <>
      <Hero />
      <Marquee />
      <BrandStory />
      <SignatureProducts />
      <FeaturedProduct />
      <Menu limit={6} id="menu" />
      <Defer minHeight="90vh"><BakeryExperience /></Defer>
      <Defer id="custom" minHeight="90vh"><CustomCake /></Defer>
      <Defer id="gallery" minHeight="100vh"><Gallery /></Defer>
      <Defer minHeight="70vh"><Testimonials /></Defer>
      <Defer minHeight="70vh"><SocialSection /></Defer>
      <Defer id="contact" minHeight="90vh"><ContactSection /></Defer>
    </>
  );
}
