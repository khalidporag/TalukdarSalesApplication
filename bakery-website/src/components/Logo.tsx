import { brand } from '@/data/brand';

/** The Talukder Foods badge, used as supplied. */
export function Logo({ className = 'h-11', small = true }: { light?: boolean; className?: string; small?: boolean }) {
  return (
    <picture>
      <source srcSet={`${import.meta.env.BASE_URL}brand/${small ? 'logo-sm' : 'logo'}.webp`} type="image/webp" />
      <img src={`${import.meta.env.BASE_URL}brand/${small ? 'logo-sm' : 'logo'}.png`} alt={brand.name} className={`w-auto select-none ${className}`} width={360} height={169} draggable={false} />
    </picture>
  );
}
