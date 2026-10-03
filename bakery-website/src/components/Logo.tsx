import { brand } from '@/data/brand';

export function Mark({ className = '' }: { className?: string }) {
  return (
    <svg viewBox="0 0 40 40" className={className} fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      <path d="M20 4c7 5 11 11 11 18a11 11 0 0 1-22 0c0-7 4-13 11-18z" />
      <path d="M20 12v22M14 21l6 6 6-6M14 15l6 6 6-6" />
    </svg>
  );
}

export function Logo({ light = false, className = '' }: { light?: boolean; className?: string }) {
  return (
    <span className={`inline-flex items-center gap-3 ${light ? 'text-ivory' : 'text-espresso'} ${className}`}>
      <Mark className="h-8 w-8 text-caramel" />
      <span className="leading-none">
        <span className="block font-display text-[1.7rem] font-medium italic tracking-tight">{brand.short}</span>
        <span className="mt-0.5 block text-[0.56rem] font-semibold uppercase tracking-widest2 opacity-70">Bakehouse</span>
      </span>
    </span>
  );
}
