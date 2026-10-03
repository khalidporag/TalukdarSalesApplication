import { useId } from 'react';
import { tones, type Tone } from './tones';
import { subjects, type ArtKind } from './subjects';

export type { Tone, ArtKind };

interface ArtProps {
  kind: ArtKind;
  tone: Tone;
  /** Shifts the composition so the same subject can appear in different crops (gallery, social). */
  crop?: 'center' | 'left' | 'right' | 'close' | 'wide';
  className?: string;
  title?: string;
}

/**
 * Original illustrated product art. It stands in for photography until real photos are added
 * (src/assets/photos/<slug>.jpg is picked up automatically by <Picture>). 800 x 1000 portrait.
 */
export function Art({ kind, tone, crop = 'center', className, title }: ArtProps) {
  const uid = useId().replace(/:/g, '');
  const t = tones[tone];
  const Subject = subjects[kind];
  const wide: ArtKind[] = ['croissant', 'eclair', 'macarons', 'cookies', 'tart', 'loaf', 'brownie'];
  const flat: ArtKind[] = ['hero', 'oven', 'storefront', 'dough'];
  const scale = flat.includes(kind) ? 1.04 : wide.includes(kind) ? 1.2 : 1.28;
  const lift = flat.includes(kind) ? 500 : 640;
  const view = {
    center: '0 0 800 1000',
    left: '60 20 700 875',
    right: '40 40 700 875',
    close: '130 250 540 675',
    wide: '0 330 800 470',
  }[crop];

  return (
    <svg
      viewBox={view}
      preserveAspectRatio="xMidYMid slice"
      className={className}
      role={title ? 'img' : 'presentation'}
      aria-label={title}
      aria-hidden={title ? undefined : true}
    >
      <defs>
        <linearGradient id={`${uid}bg`} x1="0" y1="0" x2="0.4" y2="1">
          <stop offset="0" stopColor={t.bg1} />
          <stop offset="1" stopColor={t.bg2} />
        </linearGradient>
        <radialGradient id={`${uid}light`} cx="0.2" cy="0.12" r="0.9">
          <stop offset="0" stopColor="#fff" stopOpacity="0.55" />
          <stop offset="0.55" stopColor="#fff" stopOpacity="0" />
        </radialGradient>
        <radialGradient id={`${uid}vig`} cx="0.5" cy="0.55" r="0.78">
          <stop offset="0.55" stopColor="#000" stopOpacity="0" />
          <stop offset="1" stopColor="#1a0d06" stopOpacity="0.38" />
        </radialGradient>
        <linearGradient id={`${uid}tbl`} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor={t.table} stopOpacity="0.0" />
          <stop offset="0.12" stopColor={t.table} stopOpacity="0.55" />
          <stop offset="1" stopColor={t.table} stopOpacity="0.9" />
        </linearGradient>
        <filter id={`${uid}blur`} x="-30%" y="-30%" width="160%" height="160%">
          <feGaussianBlur stdDeviation="16" />
        </filter>
        <filter id={`${uid}soft`} x="-10%" y="-10%" width="120%" height="120%">
          <feGaussianBlur stdDeviation="1.4" />
        </filter>
        <filter id={`${uid}grain`} x="0" y="0" width="100%" height="100%">
          <feTurbulence type="fractalNoise" baseFrequency="0.9" numOctaves="2" seed="7" result="n" />
          <feColorMatrix in="n" type="matrix" values="0 0 0 0 0  0 0 0 0 0  0 0 0 0 0  0 0 0 0.55 0" />
        </filter>
      </defs>

      <rect width="800" height="1000" fill={`url(#${uid}bg)`} />
      <rect y="690" width="800" height="310" fill={`url(#${uid}tbl)`} />
      <path d="M0 690H800" stroke="#fff" strokeOpacity="0.22" strokeWidth="1.5" />
      <rect width="800" height="1000" fill={`url(#${uid}light)`} />

      <g transform={`translate(400 ${lift}) scale(${scale}) translate(-400 -${lift})`}>
        <Subject uid={uid} t={t} />
      </g>

      <rect width="800" height="1000" fill={`url(#${uid}vig)`} />
      <rect width="800" height="1000" filter={`url(#${uid}grain)`} opacity="0.16" />
      {title ? <title>{title}</title> : null}
    </svg>
  );
}
