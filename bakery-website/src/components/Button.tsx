import { Link } from 'react-router-dom';
import type { ReactNode, ButtonHTMLAttributes } from 'react';

type Variant = 'dark' | 'light' | 'line' | 'line-light';
const cls: Record<Variant, string> = { dark: 'btn btn-dark', light: 'btn btn-light', line: 'btn btn-line', 'line-light': 'btn btn-line-light' };

interface Common { variant?: Variant; small?: boolean; className?: string; children: ReactNode }

export function ButtonLink({ to, href, variant = 'dark', small, className = '', children, ...rest }: Common & { to?: string; href?: string; target?: string; rel?: string; onClick?: () => void }) {
  const c = `${cls[variant]} ${small ? 'btn-sm' : ''} ${className}`;
  if (to) return <Link to={to} className={c} data-cursor {...rest}><span>{children}</span></Link>;
  return <a href={href} className={c} data-cursor {...rest}><span>{children}</span></a>;
}

export function Button({ variant = 'dark', small, className = '', children, ...rest }: Common & ButtonHTMLAttributes<HTMLButtonElement>) {
  return <button className={`${cls[variant]} ${small ? 'btn-sm' : ''} ${className}`} data-cursor {...rest}><span>{children}</span></button>;
}

export function SectionHead({ index, label, title, className = '', light = false }: { index: string; label: string; title?: ReactNode; className?: string; light?: boolean }) {
  return (
    <div className={className}>
      <p className={`eyebrow flex items-center gap-4 ${light ? '!text-caramel' : ''}`}>
        <span>{index}</span><span className={`h-px w-12 ${light ? 'bg-ivory/30' : 'bg-espresso/25'}`} /><span>{label}</span>
      </p>
      {title}
    </div>
  );
}
