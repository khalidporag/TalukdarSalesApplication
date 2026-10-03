import type { ReactNode } from 'react';
import type { ToneDef } from './tones';

export interface SubjectProps {
  uid: string;
  t: ToneDef;
}

/** Small deterministic random so the art never changes between renders. */
export function rng(seed: number) {
  let s = seed >>> 0;
  return () => {
    s = (s * 1664525 + 1013904223) >>> 0;
    return s / 4294967296;
  };
}

export const Shadow = ({ uid, cx, cy, rx, ry, o = 0.4 }: { uid: string; cx: number; cy: number; rx: number; ry: number; o?: number }) => (
  <ellipse cx={cx} cy={cy} rx={rx} ry={ry} fill="#1a0d06" opacity={o} filter={`url(#${uid}blur)`} />
);

export const Plate = ({ uid, t, cx = 400, cy = 735, rx = 300, ry = 56 }: SubjectProps & { cx?: number; cy?: number; rx?: number; ry?: number }) => (
  <g>
    <Shadow uid={uid} cx={cx + 10} cy={cy + 34} rx={rx * 0.96} ry={ry * 0.9} o={0.42} />
    <ellipse cx={cx} cy={cy + 10} rx={rx} ry={ry} fill={t.plateEdge} />
    <ellipse cx={cx} cy={cy} rx={rx} ry={ry} fill={t.plate} />
    <ellipse cx={cx} cy={cy - 2} rx={rx * 0.66} ry={ry * 0.56} fill="none" stroke={t.plateEdge} strokeWidth="2" opacity="0.7" />
    <ellipse cx={cx - rx * 0.2} cy={cy - ry * 0.35} rx={rx * 0.5} ry={ry * 0.22} fill="#fff" opacity="0.45" />
  </g>
);

export const Crumbs = ({ seed = 3, n = 26, cx = 400, cy = 735, spread = 250, color = '#7a4a22' }: { seed?: number; n?: number; cx?: number; cy?: number; spread?: number; color?: string }) => {
  const r = rng(seed);
  return (
    <g fill={color} opacity="0.55">
      {Array.from({ length: n }).map((_, i) => {
        const a = r() * Math.PI * 2;
        const d = Math.sqrt(r()) * spread;
        return <ellipse key={i} cx={cx + Math.cos(a) * d} cy={cy + Math.sin(a) * d * 0.2 + 6} rx={1.5 + r() * 3.5} ry={1 + r() * 2} transform={`rotate(${r() * 180} ${cx + Math.cos(a) * d} ${cy})`} />;
      })}
    </g>
  );
};

export const Linear = ({ id, stops, x1 = 0, y1 = 0, x2 = 0, y2 = 1 }: { id: string; stops: [number, string, number?][]; x1?: number; y1?: number; x2?: number; y2?: number }) => (
  <linearGradient id={id} x1={x1} y1={y1} x2={x2} y2={y2}>
    {stops.map(([o, c, op], i) => (
      <stop key={i} offset={o} stopColor={c} stopOpacity={op ?? 1} />
    ))}
  </linearGradient>
);

export const Radial = ({ id, stops, cx = 0.35, cy = 0.3, r = 0.8 }: { id: string; stops: [number, string][]; cx?: number; cy?: number; r?: number }) => (
  <radialGradient id={id} cx={cx} cy={cy} r={r}>
    {stops.map(([o, c], i) => (
      <stop key={i} offset={o} stopColor={c} />
    ))}
  </radialGradient>
);

export const Group = ({ children, x = 0, y = 0, s = 1, r = 0 }: { children: ReactNode; x?: number; y?: number; s?: number; r?: number }) => (
  <g transform={`translate(${x} ${y}) rotate(${r}) scale(${s})`}>{children}</g>
);
