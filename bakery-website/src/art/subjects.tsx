import type { ComponentType } from 'react';
import { Crumbs, Group, Linear, Plate, Radial, Shadow, rng, type SubjectProps } from './parts';

export type ArtKind =
  | 'croissant' | 'cheesecake' | 'chocolate-cake' | 'tiered' | 'tart' | 'eclair' | 'cookies'
  | 'loaf' | 'roll' | 'jar' | 'brownie' | 'macarons'
  | 'dough' | 'oven' | 'piping' | 'box' | 'hero' | 'storefront';

/* ------------------------------------------------------------------ croissant */
function Croissant({ uid, x = 0, y = 0, s = 1 }: SubjectProps & { x?: number; y?: number; s?: number }) {
  // An arch of puffy bands, thick in the middle and tapering to two horns.
  const cx = 400, cy = 712, rxo = 285, ryo = 262;
  const N = 9;
  const thick = (th: number) => 150 * Math.pow(Math.sin(th), 0.85) + 6;
  const pt = (th: number, d: number) => [cx + (rxo - d * 0.62) * Math.cos(th), cy - (ryo - d) * Math.sin(th)] as const;
  const bands = Array.from({ length: N }, (_, k) => {
    const th0 = Math.PI * (0.04 + (k / N) * 0.92), th1 = Math.PI * (0.04 + ((k + 1) / N) * 0.92), thm = (th0 + th1) / 2;
    const o0 = pt(th0, 0), o1 = pt(th1, 0), om = pt(thm, -16 * Math.sin(thm));
    const i0 = pt(th0, thick(th0)), i1 = pt(th1, thick(th1)), im = pt(thm, thick(thm) + 14 * Math.sin(thm));
    return { o0, o1, om, i0, i1, im, k, thm };
  });
  const f = (n: readonly number[]) => `${n[0].toFixed(1)} ${n[1].toFixed(1)}`;
  return (
    <Group x={x} y={y} s={s}>
      <defs>
        <Radial id={`${uid}cr`} cx={0.3} cy={0.2} r={1} stops={[[0, '#F8D68C'], [0.5, '#E0A04B'], [1, '#A9632A']]} />
        <Radial id={`${uid}cr2`} cx={0.3} cy={0.2} r={1} stops={[[0, '#F2C479'], [0.5, '#D18C3C'], [1, '#93501C']]} />
      </defs>
      <Shadow uid={uid} cx={410} cy={742} rx={300} ry={30} o={0.45} />
      {/* order: tips first, middle last so the centre sits on top */}
      {[0, 8, 1, 7, 2, 6, 3, 5, 4].map((idx) => {
        const b = bands[idx];
        return (
          <g key={idx}>
            <path d={`M${f(b.o0)} Q${f(b.om)} ${f(b.o1)} L${f(b.i1)} Q${f(b.im)} ${f(b.i0)}Z`} fill={`url(#${uid}${idx % 2 ? 'cr' : 'cr2'})`} stroke="#7d4012" strokeOpacity="0.42" strokeWidth="3" strokeLinejoin="round" />
            <path d={`M${f(b.o0)} Q${f(b.om)} ${f(b.o1)}`} fill="none" stroke="#FFE7B0" strokeOpacity="0.6" strokeWidth="5" strokeLinecap="round" transform="translate(0 7)" />
            <path d={`M${f(b.i0)} Q${f(b.im)} ${f(b.i1)}`} fill="none" stroke="#7d4012" strokeOpacity="0.28" strokeWidth="4" strokeLinecap="round" transform="translate(0 -6)" />
          </g>
        );
      })}
      <path d="M205 640 Q270 520 390 492" fill="none" stroke="#fff" strokeOpacity="0.3" strokeWidth="9" strokeLinecap="round" />
      <Crumbs seed={11} n={34} cy={738} spread={300} color="#8b4a17" />
      <g opacity="0.7">
        <circle cx="640" cy="724" r="5" fill="#E9B766" />
        <circle cx="160" cy="730" r="4" fill="#E9B766" />
      </g>
    </Group>
  );
}

/* ------------------------------------------------------------------ slices */
function Slice({ uid, t, layers, top, glaze }: SubjectProps & { layers: { c1: string; c2: string; h: number }[]; top: [string, string]; glaze?: boolean }) {
  const total = layers.reduce((a, l) => a + l.h, 0);
  const x0 = 220, x1 = 520, dx = 92, dy = 46, baseY = 725;
  let y = baseY - total;
  const topY = y;
  return (
    <g>
      <defs>
        <Linear id={`${uid}top`} x1={0} y1={0} x2={1} y2={1} stops={[[0, top[0]], [1, top[1]]]} />
        {layers.map((l, i) => <Linear key={i} id={`${uid}l${i}`} stops={[[0, l.c1], [1, l.c2]]} />)}
      </defs>
      <Plate uid={uid} t={t} />
      <Shadow uid={uid} cx={430} cy={742} rx={250} ry={26} o={0.5} />
      {/* right side */}
      <g>
        {layers.map((l, i) => {
          const yy = (y += l.h) - l.h;
          return <path key={i} d={`M${x1} ${yy} L${x1 + dx} ${yy - dy} L${x1 + dx} ${yy - dy + l.h} L${x1} ${yy + l.h}Z`} fill={`url(#${uid}l${i})`} />;
        })}
        <path d={`M${x1} ${topY} L${x1 + dx} ${topY - dy} L${x1 + dx} ${baseY - dy} L${x1} ${baseY}Z`} fill="#1a0d06" opacity="0.28" />
      </g>
      {/* front */}
      {(() => {
        let yy = topY;
        return layers.map((l, i) => {
          const el = <rect key={i} x={x0} y={yy} width={x1 - x0} height={l.h} fill={`url(#${uid}l${i})`} />;
          yy += l.h;
          return el;
        });
      })()}
      <path d={`M${x0} ${topY} L${x0 + dx} ${topY - dy} L${x1 + dx} ${topY - dy} L${x1} ${topY}Z`} fill={`url(#${uid}top)`} />
      <path d={`M${x0} ${topY} L${x1} ${topY}`} stroke="#fff" strokeOpacity="0.28" strokeWidth="3" />
      {glaze ? (
        <>
          <path d={`M${x0} ${topY} H${x1} V${topY + 26} q-14 28 -26 4 t-30 6 t-34 -8 t-36 10 t-38 -10 t-40 8 t-32 -6 t-36 8 z`} fill="#2a130a" opacity="0.92" />
          <path d={`M${x0 + 20} ${topY + 8} H${x1 - 110}`} stroke="#fff" strokeOpacity="0.35" strokeWidth="4" strokeLinecap="round" />
          <path d={`M${x0 + 60} ${topY - 6} L${x0 + 150} ${topY - dy + 4}`} stroke="#fff" strokeOpacity="0.22" strokeWidth="6" strokeLinecap="round" />
        </>
      ) : (
        <path d={`M${x0 + 80} ${topY - 10} L${x1 - 60} ${topY - 22}`} stroke="#fff" strokeOpacity="0.14" strokeWidth="14" strokeLinecap="round" />
      )}
      <Crumbs seed={5} n={22} cx={420} spread={290} color="#4a2512" />
    </g>
  );
}

const Cheesecake = (p: SubjectProps) => (
  <Slice {...p} top={['#5a2c14', '#9d5528']} layers={[
    { c1: '#6b3418', c2: '#8a4a22', h: 20 },
    { c1: '#F6E5B8', c2: '#EAC98A', h: 110 },
    { c1: '#E4C080', c2: '#D3A862', h: 28 },
  ]} />
);

const ChocolateCake = (p: SubjectProps) => (
  <Slice {...p} glaze top={['#2f170c', '#4d2815']} layers={[
    { c1: '#4a2616', c2: '#33190d', h: 50 },
    { c1: '#8a5a3c', c2: '#6d4228', h: 14 },
    { c1: '#4a2616', c2: '#33190d', h: 50 },
    { c1: '#8a5a3c', c2: '#6d4228', h: 14 },
    { c1: '#4a2616', c2: '#33190d', h: 48 },
    { c1: '#8a5a3c', c2: '#6d4228', h: 12 },
    { c1: '#4a2616', c2: '#33190d', h: 44 },
  ]} />
);

/* ------------------------------------------------------------------ tiered cake */
function Flower({ x, y, r, c, c2 }: { x: number; y: number; r: number; c: string; c2: string }) {
  return (
    <g>
      {Array.from({ length: 6 }).map((_, i) => (
        <ellipse key={i} cx={x + Math.cos((i / 6) * 6.283) * r * 0.55} cy={y + Math.sin((i / 6) * 6.283) * r * 0.55} rx={r * 0.55} ry={r * 0.42} transform={`rotate(${(i / 6) * 360} ${x + Math.cos((i / 6) * 6.283) * r * 0.55} ${y + Math.sin((i / 6) * 6.283) * r * 0.55})`} fill={c} />
      ))}
      <circle cx={x} cy={y} r={r * 0.34} fill={c2} />
    </g>
  );
}

function Tiered({ uid, t }: SubjectProps) {
  const tiers = [
    { w: 400, h: 150, y: 580 },
    { w: 300, h: 140, y: 440 },
    { w: 200, h: 120, y: 320 },
  ];
  const r = rng(9);
  return (
    <g>
      <defs>
        <Linear id={`${uid}ic`} x1={0} y1={0} x2={1} y2={0} stops={[[0, '#E9DDC6'], [0.35, '#FFFBF1'], [0.7, '#F4EAD6'], [1, '#D9C9AC']]} />
      </defs>
      <Shadow uid={uid} cx={410} cy={758} rx={300} ry={34} o={0.48} />
      <ellipse cx="400" cy="738" rx="270" ry="38" fill={t.plateEdge} />
      <ellipse cx="400" cy="728" rx="270" ry="38" fill={t.plate} />
      <rect x="388" y="700" width="24" height="40" fill={t.plateEdge} />
      {tiers.map((tr, i) => {
        const cx = 400, rx = tr.w / 2;
        return (
          <g key={i}>
            <path d={`M${cx - rx} ${tr.y} v${tr.h} a${rx} ${rx * 0.2} 0 0 0 ${tr.w} 0 v${-tr.h}Z`} fill={`url(#${uid}ic)`} />
            {Array.from({ length: 9 }).map((_, k) => (
              <path key={k} d={`M${cx - rx + 12 + k * (tr.w - 24) / 8} ${tr.y + 14} v${tr.h - 8}`} stroke="#B9A685" strokeOpacity="0.26" strokeWidth="2" />
            ))}
            <ellipse cx={cx} cy={tr.y} rx={rx} ry={rx * 0.2} fill="#FFFDF6" />
            <ellipse cx={cx} cy={tr.y} rx={rx} ry={rx * 0.2} fill="none" stroke="#CDBB9A" strokeOpacity="0.6" strokeWidth="2" />
            <path d={`M${cx - rx} ${tr.y + tr.h} a${rx} ${rx * 0.2} 0 0 0 ${tr.w} 0`} fill="none" stroke="#B9A685" strokeOpacity="0.5" strokeWidth="2" />
            {Array.from({ length: Math.round(tr.w / 22) }).map((_, k) => (
              <circle key={k} cx={cx - rx + 10 + k * 22} cy={tr.y + tr.h + Math.sin(((k * 22) / tr.w) * Math.PI) * rx * 0.2 + 2} r="7" fill="#FFFBF1" stroke="#D9C9AC" strokeOpacity="0.8" />
            ))}
          </g>
        );
      })}
      {/* flowers and leaves */}
      {[[240, 585], [270, 610], [300, 596], [248, 640], [530, 442], [500, 456], [560, 464], [455, 322], [480, 330]].map(([fx, fy], i) => (
        <g key={i}>
          <ellipse cx={fx - 30} cy={fy + 10} rx="26" ry="9" fill="#7C8A5A" transform={`rotate(${-20 + i * 11} ${fx - 30} ${fy + 10})`} />
          <ellipse cx={fx + 24} cy={fy + 14} rx="24" ry="8" fill="#8E9C68" transform={`rotate(${25 - i * 8} ${fx + 24} ${fy + 14})`} />
        </g>
      ))}
      {[[240, 585, 34, '#E7B7B0', '#C47E78'], [276, 612, 28, '#F3D9CC', '#D9A58F'], [305, 594, 24, '#FFF8EA', '#E9C98F'], [248, 642, 22, '#D99A92', '#B26A66'],
        [530, 442, 30, '#E7B7B0', '#C47E78'], [498, 458, 24, '#FFF8EA', '#E9C98F'], [562, 466, 22, '#F3D9CC', '#D9A58F'],
        [456, 322, 28, '#E7B7B0', '#C47E78'], [484, 332, 22, '#FFF8EA', '#E9C98F']].map(([fx, fy, fr, c, c2], i) => (
        <Flower key={i} x={fx as number} y={fy as number} r={fr as number} c={c as string} c2={c2 as string} />
      ))}
      {Array.from({ length: 12 }).map((_, i) => (
        <circle key={i} cx={300 + r() * 220} cy={290 + r() * 30} r={1.5 + r() * 2.5} fill="#E9C98F" />
      ))}
      <Crumbs seed={4} n={10} cy={742} spread={290} color="#c9b48b" />
    </g>
  );
}

/* ------------------------------------------------------------------ tart */
function Tart({ uid, t }: SubjectProps) {
  const r = rng(21);
  const fruits = Array.from({ length: 16 }).map((_, i) => {
    const a = (i / 16) * 6.283 + r() * 0.3;
    const d = 40 + r() * 150;
    return { x: 400 + Math.cos(a) * d * 1.35, y: 565 + Math.sin(a) * d * 0.33, r: 20 + r() * 12, k: i % 4 };
  }).sort((a, b) => a.y - b.y);
  const colors = [['#C9352F', '#8F1E1B'], ['#3E4A8C', '#232B5E'], ['#E86C3A', '#B24517'], ['#7A9A3C', '#4F6B22']];
  return (
    <g>
      <defs>
        <Linear id={`${uid}sh`} x1={0} y1={0} x2={1} y2={0} stops={[[0, '#C98E48'], [0.4, '#EBBD74'], [1, '#B97C38']]} />
        <Radial id={`${uid}cu`} cx={0.4} cy={0.35} r={0.8} stops={[[0, '#FFF3C9'], [1, '#EBCF8E']]} />
      </defs>
      <Plate uid={uid} t={t} />
      <Shadow uid={uid} cx={410} cy={745} rx={270} ry={30} o={0.5} />
      <path d="M118 560 L148 715 Q400 790 652 715 L682 560Z" fill={`url(#${uid}sh)`} />
      {Array.from({ length: 22 }).map((_, i) => (
        <path key={i} d={`M${140 + i * 24.5} ${600 + Math.sin(i) * 3} L${150 + i * 23.5} 722`} stroke="#8d5a22" strokeOpacity="0.28" strokeWidth="3" />
      ))}
      <ellipse cx="400" cy="560" rx="282" ry="82" fill="#C98E48" />
      <ellipse cx="400" cy="560" rx="262" ry="70" fill={`url(#${uid}cu)`} />
      {fruits.map((f, i) => (
        <g key={i}>
          <ellipse cx={f.x + 3} cy={f.y + 7} rx={f.r} ry={f.r * 0.5} fill="#1a0d06" opacity="0.25" />
          <circle cx={f.x} cy={f.y} r={f.r} fill={colors[f.k][0]} />
          <circle cx={f.x + f.r * 0.12} cy={f.y + f.r * 0.22} r={f.r * 0.78} fill={colors[f.k][1]} opacity="0.55" />
          <ellipse cx={f.x - f.r * 0.32} cy={f.y - f.r * 0.38} rx={f.r * 0.3} ry={f.r * 0.18} fill="#fff" opacity="0.65" />
        </g>
      ))}
      {[[330, 548], [470, 580], [405, 520]].map(([x, y], i) => (
        <path key={i} d={`M${x} ${y} q16 -22 34 -6 q-12 20 -34 6Z`} fill="#6C8A3A" />
      ))}
      <Crumbs seed={2} n={18} cy={740} spread={300} color="#8d5a22" />
    </g>
  );
}

/* ------------------------------------------------------------------ eclair */
function Eclair({ uid, t }: SubjectProps) {
  const one = (x: number, y: number, rot: number, s: number, k: string) => (
    <Group x={x} y={y} s={s} r={rot}>
      <Shadow uid={uid} cx={10} cy={96} rx={230} ry={18} o={0.4} />
      <rect x="-230" y="-62" width="460" height="132" rx="64" fill={`url(#${uid}ch)`} />
      <rect x="-230" y="12" width="460" height="58" rx="40" fill="#F3E3B9" />
      <rect x="-226" y="-14" width="452" height="36" rx="18" fill="#FFF4D2" opacity="0.9" />
      <path d="M-216 -22 H216 Q226 -62 190 -62 H-190 Q-226 -62 -216 -22Z" fill="#E9B567" />
      <path d="M-222 -26 Q-224 -64 -176 -66 H176 Q224 -64 222 -26 Q210 -4 170 -10 q-30 22 -70 -4 q-30 20 -66 -2 q-34 20 -70 -2 q-36 18 -70 0 q-38 8 -50 2Z" fill={`url(#${uid}gl${k})`} />
      <path d="M-180 -48 H60" stroke="#fff" strokeOpacity="0.5" strokeWidth="7" strokeLinecap="round" />
      <path d="M-210 20 q40 -14 80 0 t80 0 t80 0 t80 0 t80 0" fill="none" stroke="#fff" strokeOpacity="0.5" strokeWidth="5" />
    </Group>
  );
  return (
    <g>
      <defs>
        <Linear id={`${uid}ch`} stops={[[0, '#EDBE70'], [1, '#C98840']]} />
        <Linear id={`${uid}gl1`} stops={[[0, '#4a2616'], [1, '#2a130a']]} />
        <Linear id={`${uid}gl2`} stops={[[0, '#6a3a22'], [1, '#3d1f10']]} />
      </defs>
      <Plate uid={uid} t={t} rx={320} />
      {one(430, 600, 0, 0.62, '2')}
      {one(380, 660, -2, 0.78, '1')}
      <Crumbs seed={8} n={20} cy={745} spread={300} color="#3d1f10" />
    </g>
  );
}

/* ------------------------------------------------------------------ cookies */
function Cookie({ uid, x, y, rx, ry, seed, salt }: { uid: string; x: number; y: number; rx: number; ry: number; seed: number; salt?: boolean }) {
  const r = rng(seed);
  return (
    <g>
      <ellipse cx={x} cy={y + ry * 0.45} rx={rx} ry={ry} fill="#9b6330" />
      <rect x={x - rx} y={y} width={rx * 2} height={ry * 0.45} fill="#9b6330" />
      <ellipse cx={x} cy={y} rx={rx} ry={ry} fill={`url(#${uid}ck)`} />
      {Array.from({ length: 11 }).map((_, i) => {
        const a = r() * 6.283, d = Math.sqrt(r()) * 0.82;
        const cx = x + Math.cos(a) * rx * d, cy = y + Math.sin(a) * ry * d;
        return <path key={i} d={`M${cx - 15} ${cy} l10 -9 l13 3 l5 11 l-12 7z`} fill={i % 3 ? '#3a1c0e' : '#552a14'} transform={`rotate(${r() * 90} ${cx} ${cy})`} />;
      })}
      {salt ? Array.from({ length: 9 }).map((_, i) => <rect key={i} x={x - rx * 0.6 + r() * rx * 1.2} y={y - ry * 0.6 + r() * ry * 1.2} width="5" height="3" fill="#fff" opacity="0.9" />) : null}
      <path d={`M${x - rx * 0.8} ${y - ry * 0.35} Q${x} ${y - ry * 0.9} ${x + rx * 0.5} ${y - ry * 0.55}`} fill="none" stroke="#fff" strokeOpacity="0.28" strokeWidth="5" strokeLinecap="round" />
    </g>
  );
}

function Cookies({ uid, t }: SubjectProps) {
  return (
    <g>
      <defs>
        <Radial id={`${uid}ck`} cx={0.35} cy={0.3} r={0.85} stops={[[0, '#E5B66F'], [0.6, '#C48A44'], [1, '#9b6330']]} />
      </defs>
      <Plate uid={uid} t={t} />
      <Cookie uid={uid} x={420} y={690} rx={210} ry={44} seed={4} />
      <Cookie uid={uid} x={400} y={640} rx={196} ry={42} seed={9} />
      <Cookie uid={uid} x={425} y={590} rx={186} ry={40} seed={12} />
      <g transform="rotate(-24 250 620)"><Cookie uid={uid} x={250} y={598} rx={118} ry={118} seed={31} salt /></g>
      <Crumbs seed={6} n={40} cy={742} spread={300} color="#8b5a28" />
    </g>
  );
}

/* ------------------------------------------------------------------ loaf */
function Loaf({ uid, t }: SubjectProps) {
  const r = rng(14);
  return (
    <g>
      <defs>
        <Radial id={`${uid}lf`} cx={0.34} cy={0.22} r={0.95} stops={[[0, '#E9B56A'], [0.45, '#C58235'], [1, '#7b4414']]} />
        <Linear id={`${uid}bd`} stops={[[0, '#C7A06A'], [1, '#9a7442']]} />
      </defs>
      <Shadow uid={uid} cx={410} cy={760} rx={320} ry={30} o={0.5} />
      <rect x="90" y="722" width="640" height="46" rx="12" fill={`url(#${uid}bd)`} />
      <rect x="90" y="722" width="640" height="14" rx="7" fill="#fff" opacity="0.18" />
      <path d="M118 700 C130 480 270 380 410 380 C560 380 690 480 700 700 Q410 740 118 700Z" fill={`url(#${uid}lf)`} />
      <path d="M230 560 C290 470 380 450 460 462" fill="none" stroke="#FFD9A0" strokeOpacity="0.45" strokeWidth="8" strokeLinecap="round" />
      <path d="M270 440 C330 470 420 560 440 660" fill="none" stroke="#f4dcae" strokeWidth="16" strokeLinecap="round" opacity="0.9" />
      <path d="M262 446 C330 466 410 548 424 640" fill="none" stroke="#7b4414" strokeOpacity="0.5" strokeWidth="4" strokeLinecap="round" />
      <path d="M350 424 C410 450 500 530 528 640" fill="none" stroke="#f1d49c" strokeWidth="14" strokeLinecap="round" opacity="0.85" />
      <path d="M342 430 C410 446 494 520 516 630" fill="none" stroke="#7b4414" strokeOpacity="0.45" strokeWidth="4" strokeLinecap="round" />
      {Array.from({ length: 150 }).map((_, i) => {
        const a = r() * 3.1416, d = Math.sqrt(r());
        return <circle key={i} cx={410 + Math.cos(a) * 270 * d} cy={690 - Math.sin(a) * 270 * d * 0.98} r={0.8 + r() * 2.2} fill="#FFF8E8" opacity={0.25 + r() * 0.5} />;
      })}
      <g opacity="0.9">
        <path d="M580 700 l40 -10 l24 14 l-24 18 l-50 -4z" fill={t.plate} />
        <path d="M600 706 l20 -4 l14 6" stroke="#d9c7a0" strokeWidth="2" fill="none" />
      </g>
      <Crumbs seed={7} n={26} cy={770} spread={320} color="#d9c7a0" />
    </g>
  );
}

/* ------------------------------------------------------------------ cinnamon roll */
function Roll({ uid, t }: SubjectProps) {
  return (
    <g>
      <defs>
        <Radial id={`${uid}rl`} cx={0.4} cy={0.35} r={0.8} stops={[[0, '#F0C47E'], [0.7, '#CF8E45'], [1, '#9a5a22']]} />
      </defs>
      <Plate uid={uid} t={t} />
      <Shadow uid={uid} cx={420} cy={700} rx={250} ry={50} o={0.45} />
      <ellipse cx="400" cy="630" rx="255" ry="150" fill="#9a5a22" />
      <ellipse cx="400" cy="610" rx="255" ry="150" fill={`url(#${uid}rl)`} />
      <path d="M400 610 m0 0 c-8 -26 36 -34 52 -12 c22 28 -22 62 -62 52 c-48 -12 -64 -70 -22 -96 c52 -32 124 4 128 64 c4 66 -78 100 -138 76 c-70 -28 -92 -122 -32 -176 c70 -62 180 -22 196 62 c14 76 -62 142 -140 134" fill="none" stroke="#7b4414" strokeOpacity="0.55" strokeWidth="9" strokeLinecap="round" />
      <path d="M400 610 m0 0 c-8 -26 36 -34 52 -12 c22 28 -22 62 -62 52 c-48 -12 -64 -70 -22 -96 c52 -32 124 4 128 64 c4 66 -78 100 -138 76 c-70 -28 -92 -122 -32 -176 c70 -62 180 -22 196 62 c14 76 -62 142 -140 134" fill="none" stroke="#F6DDA6" strokeOpacity="0.5" strokeWidth="3" strokeLinecap="round" transform="translate(-3 -3)" />
      <path d="M200 560 q40 -70 140 -86 q60 -8 100 4 q60 4 100 40 q40 26 50 70 q-60 -30 -110 -26 q-40 -40 -100 -34 q-60 -6 -86 22 q-60 -4 -94 10Z" fill="#FFF8EA" opacity="0.88" />
      <path d="M240 540 q60 -50 160 -58" fill="none" stroke="#fff" strokeOpacity="0.8" strokeWidth="6" strokeLinecap="round" />
      <Crumbs seed={15} n={24} cy={740} spread={300} color="#9a5a22" />
    </g>
  );
}

/* ------------------------------------------------------------------ jar (tiramisu) */
function Jar({ uid, t }: SubjectProps) {
  return (
    <g>
      <defs>
        <Linear id={`${uid}gl`} x1={0} y1={0} x2={1} y2={0} stops={[[0, '#fff', 0.5], [0.12, '#fff', 0.12], [0.5, '#fff', 0.04], [0.9, '#fff', 0.16], [1, '#fff', 0.45]]} />
        <Linear id={`${uid}cc`} stops={[[0, '#FBF1D8'], [1, '#EDD9A9']]} />
        <Linear id={`${uid}bk`} stops={[[0, '#9a6a3c'], [1, '#6b4121']]} />
      </defs>
      <Plate uid={uid} t={t} rx={250} ry={46} />
      <Shadow uid={uid} cx={410} cy={742} rx={190} ry={22} o={0.5} />
      <path d="M270 330 H530 V690 Q530 724 496 724 H304 Q270 724 270 690Z" fill="#fff" opacity="0.1" />
      <path d="M278 360 H522 V684 Q522 716 492 716 H308 Q278 716 278 684Z" fill={`url(#${uid}cc)`} />
      <rect x="278" y="360" width="244" height="44" fill="#4a2616" />
      <path d="M278 404 q30 14 61 0 t61 0 t61 0 t61 0 V430 H278Z" fill="#FBF1D8" />
      <rect x="278" y="430" width="244" height="60" fill={`url(#${uid}bk)`} />
      {Array.from({ length: 14 }).map((_, i) => <rect key={i} x={286 + i * 17} y={438 + (i % 3) * 14} width="8" height="3" rx="1.5" fill="#d9b38a" opacity="0.7" />)}
      <rect x="278" y="490" width="244" height="64" fill={`url(#${uid}cc)`} />
      <rect x="278" y="554" width="244" height="58" fill={`url(#${uid}bk)`} />
      <rect x="278" y="612" width="244" height="104" fill={`url(#${uid}cc)`} />
      {Array.from({ length: 30 }).map((_, i) => <circle key={i} cx={290 + (i * 37) % 222} cy={366 + (i * 53) % 34} r={1.5 + (i % 3)} fill="#6b4121" opacity="0.7" />)}
      <path d="M270 330 H530 V690 Q530 724 496 724 H304 Q270 724 270 690Z" fill={`url(#${uid}gl)`} />
      <path d="M270 330 H530 V690 Q530 724 496 724 H304 Q270 724 270 690Z" fill="none" stroke="#fff" strokeOpacity="0.6" strokeWidth="3" />
      <rect x="262" y="314" width="276" height="26" rx="8" fill="#fff" opacity="0.5" />
      <rect x="262" y="314" width="276" height="26" rx="8" fill="none" stroke="#fff" strokeOpacity="0.8" strokeWidth="2" />
      <path d="M560 300 L640 700" stroke="#8a8a86" strokeWidth="9" strokeLinecap="round" />
      <ellipse cx="552" cy="286" rx="28" ry="16" fill="#9a9a96" transform="rotate(-10 552 286)" />
      <Crumbs seed={19} n={14} cy={742} spread={260} color="#4a2616" />
    </g>
  );
}

/* ------------------------------------------------------------------ brownie */
function Brownie({ uid, t }: SubjectProps) {
  const r = rng(33);
  const sq = (x: number, y: number, w: number, h: number, k: number) => (
    <g key={k}>
      <rect x={x} y={y} width={w} height={h} rx="8" fill={`url(#${uid}bs)`} />
      <rect x={x} y={y} width={w} height="22" rx="8" fill={`url(#${uid}bt)`} />
      {Array.from({ length: 5 }).map((_, i) => <path key={i} d={`M${x + 18 + i * (w - 36) / 4} ${y + 22} q4 18 -4 34`} stroke="#2a130a" strokeOpacity="0.6" strokeWidth="3" fill="none" />)}
      {Array.from({ length: 10 }).map((_, i) => <rect key={i} x={x + 10 + r() * (w - 28)} y={y + 4 + r() * 12} width="6" height="3" fill="#fff" opacity="0.85" />)}
      <path d={`M${x + 14} ${y + 6} H${x + w * 0.5}`} stroke="#fff" strokeOpacity="0.25" strokeWidth="4" strokeLinecap="round" />
    </g>
  );
  return (
    <g>
      <defs>
        <Linear id={`${uid}bs`} stops={[[0, '#3a1d0f'], [1, '#26120a']]} />
        <Linear id={`${uid}bt`} stops={[[0, '#6d3a1f'], [1, '#4a2616']]} />
      </defs>
      <Plate uid={uid} t={t} />
      <Shadow uid={uid} cx={410} cy={745} rx={270} ry={26} o={0.5} />
      {sq(150, 650, 330, 90, 0)}
      {sq(330, 650, 330, 90, 1)}
      {sq(240, 560, 330, 92, 2)}
      {sq(260, 470, 310, 92, 3)}
      <Crumbs seed={17} n={28} cy={742} spread={300} color="#3a1d0f" />
    </g>
  );
}

/* ------------------------------------------------------------------ macarons */
function Macaron({ uid, x, y, s, c1, c2, fill }: { uid: string; x: number; y: number; s: number; c1: string; c2: string; fill: string }) {
  return (
    <Group x={x} y={y} s={s}>
      <ellipse cx="0" cy="62" rx="110" ry="14" fill="#1a0d06" opacity="0.25" filter={`url(#${uid}soft)`} />
      <path d="M-100 30 Q-104 -34 0 -38 Q104 -34 100 30 Q60 18 0 18 Q-60 18 -100 30Z" fill={c1} />
      <path d="M-100 30 Q-60 18 0 18 Q60 18 100 30 Q110 12 96 4 Q60 -2 0 -2 Q-60 -2 -96 4 Q-110 12 -100 30Z" fill={c2} opacity="0.55" />
      <path d="M-98 34 Q-104 24 0 24 Q104 24 98 34 Q94 44 80 44 H-80 Q-94 44 -98 34Z" fill={fill} />
      <path d="M-100 44 Q-104 100 0 104 Q104 100 100 44 Q60 56 0 56 Q-60 56 -100 44Z" fill={c1} />
      <path d="M-70 -18 Q-20 -34 30 -26" fill="none" stroke="#fff" strokeOpacity="0.55" strokeWidth="7" strokeLinecap="round" />
      <path d="M-100 40 q-8 0 -8 -6 M100 40 q8 0 8 -6" stroke={c2} strokeWidth="3" fill="none" />
    </Group>
  );
}

function Macarons({ uid, t }: SubjectProps) {
  return (
    <g>
      <Plate uid={uid} t={t} rx={320} />
      <Macaron uid={uid} x={230} y={640} s={0.95} c1="#E9A7B3" c2="#D07D8E" fill="#FBEFE4" />
      <Macaron uid={uid} x={590} y={642} s={0.95} c1="#B7CE9A" c2="#8FAE6C" fill="#FBEFE4" />
      <Macaron uid={uid} x={410} y={665} s={1.05} c1="#F2D486" c2="#D9AE4C" fill="#FFF6DD" />
      <Macaron uid={uid} x={320} y={548} s={0.9} c1="#B9A2D8" c2="#8F73BA" fill="#FBEFE4" />
      <Macaron uid={uid} x={500} y={540} s={0.9} c1="#F0B089" c2="#D57F52" fill="#FBEFE4" />
      <Crumbs seed={23} n={18} cy={745} spread={300} color="#d9b38a" />
    </g>
  );
}

/* ------------------------------------------------------------------ scenes */
function Dough({ uid, t }: SubjectProps) {
  const r = rng(41);
  return (
    <g>
      <defs>
        <Radial id={`${uid}dg`} cx={0.4} cy={0.3} r={0.8} stops={[[0, '#FBF3DF'], [1, '#EAD7AC']]} />
        <Linear id={`${uid}wd`} stops={[[0, '#D8B07B'], [1, '#A87D46']]} />
      </defs>
      <rect x="0" y="640" width="800" height="360" fill="#fff" opacity="0.12" />
      <Shadow uid={uid} cx={400} cy={780} rx={360} ry={40} o={0.4} />
      <path d="M120 700 C100 600 200 520 400 512 C610 506 700 590 690 690 C680 770 540 790 400 790 C250 790 130 770 120 700Z" fill="#B99560" opacity="0.4" filter={`url(#${uid}soft)`} />
      <path d="M150 690 C140 610 230 560 400 556 C580 552 660 612 650 690 C640 750 520 768 400 768 C270 768 160 752 150 690Z" fill={`url(#${uid}dg)`} />
      <path d="M210 640 C260 596 380 584 470 590" fill="none" stroke="#fff" strokeOpacity="0.8" strokeWidth="8" strokeLinecap="round" />
      <g>
        <rect x="150" y="440" width="500" height="64" rx="32" fill={`url(#${uid}wd)`} transform="rotate(-8 400 472)" />
        <rect x="156" y="448" width="488" height="14" rx="7" fill="#fff" opacity="0.28" transform="rotate(-8 400 472)" />
        <rect x="60" y="468" width="120" height="40" rx="20" fill="#B58A52" transform="rotate(-8 120 488)" />
        <rect x="620" y="402" width="120" height="40" rx="20" fill="#B58A52" transform="rotate(-8 680 422)" />
      </g>
      {Array.from({ length: 120 }).map((_, i) => <circle key={i} cx={130 + r() * 540} cy={590 + r() * 200} r={0.8 + r() * 2.4} fill="#fff" opacity={0.3 + r() * 0.5} />)}
      <g transform="translate(560 710)">
        <ellipse cx="0" cy="50" rx="110" ry="18" fill="#1a0d06" opacity="0.3" filter={`url(#${uid}soft)`} />
        <path d="M-100 0 Q-100 70 0 70 Q100 70 100 0Z" fill={t.plate} />
        <ellipse cx="0" cy="0" rx="100" ry="22" fill="#fff" opacity="0.8" />
        <ellipse cx="0" cy="4" rx="84" ry="16" fill="#F1E3C0" />
      </g>
    </g>
  );
}

function Oven({ uid }: SubjectProps) {
  const r = rng(51);
  return (
    <g>
      <defs>
        <Radial id={`${uid}gw`} cx={0.5} cy={0.55} r={0.65} stops={[[0, '#FFD389'], [0.55, '#F29A3C'], [1, '#B8501A']]} />
        <Linear id={`${uid}br`} stops={[[0, '#6a3a26'], [1, '#42241a']]} />
      </defs>
      <rect x="0" y="0" width="800" height="1000" fill="#2a1710" opacity="0.4" />
      {Array.from({ length: 11 }).map((_, row) => Array.from({ length: 6 }).map((_, col) => (
        <rect key={`${row}${col}`} x={col * 140 - (row % 2) * 70 - 10} y={row * 62} width="132" height="54" rx="3" fill={`url(#${uid}br)`} opacity={0.55 + r() * 0.35} />
      )))}
      <path d="M130 760 V470 Q130 260 400 260 Q670 260 670 470 V760Z" fill="#1c0e08" />
      <path d="M160 740 V474 Q160 292 400 292 Q640 292 640 474 V740Z" fill={`url(#${uid}gw)`} />
      <path d="M160 740 V474 Q160 292 400 292 Q640 292 640 474 V740Z" fill="none" stroke="#FFE2A8" strokeOpacity="0.4" strokeWidth="3" />
      {[0, 1, 2].map((i) => (
        <g key={i} transform={`translate(${250 + i * 150} 700)`}>
          <ellipse cx="0" cy="12" rx="64" ry="12" fill="#5a2b10" opacity="0.5" />
          <path d="M-62 8 C-60 -50 -30 -72 0 -72 C30 -72 60 -50 62 8Z" fill="#C98233" />
          <path d="M-40 -30 C-20 -56 18 -56 34 -36" fill="none" stroke="#FFE0A6" strokeOpacity="0.6" strokeWidth="5" strokeLinecap="round" />
          <path d="M-12 -60 C0 -40 6 -20 8 4" fill="none" stroke="#F4DCAE" strokeWidth="8" strokeLinecap="round" />
        </g>
      ))}
      <rect x="100" y="760" width="600" height="46" fill="#18090a" />
      <rect x="100" y="760" width="600" height="8" fill="#fff" opacity="0.12" />
      {[0, 1, 2, 3].map((i) => <path key={i} d={`M${300 + i * 70} 250 q-20 -40 0 -80 q20 -40 0 -80`} fill="none" stroke="#fff" strokeOpacity="0.28" strokeWidth="6" strokeLinecap="round" className="animate-steam" style={{ animationDelay: `${i * 0.9}s` }} />)}
    </g>
  );
}

function Piping({ uid, t }: SubjectProps) {
  const rosettes = [[290, 470], [350, 456], [410, 450], [470, 456], [530, 470], [320, 506], [380, 504], [440, 504], [500, 506]];
  return (
    <g>
      <defs>
        <Linear id={`${uid}ic`} x1={0} y1={0} x2={1} y2={0} stops={[[0, '#E9DDC6'], [0.35, '#FFFBF1'], [0.7, '#F4EAD6'], [1, '#D9C9AC']]} />
        <Linear id={`${uid}bag`} x1={0} y1={0} x2={1} y2={1} stops={[[0, '#FFFDF6'], [1, '#E5D7BC']]} />
      </defs>
      <Shadow uid={uid} cx={400} cy={768} rx={310} ry={34} o={0.45} />
      <ellipse cx="400" cy="744" rx="280" ry="40" fill={t.plateEdge} />
      <ellipse cx="400" cy="734" rx="280" ry="40" fill={t.plate} />
      <path d="M200 500 v190 a200 40 0 0 0 400 0 v-190Z" fill={`url(#${uid}ic)`} />
      <ellipse cx="400" cy="500" rx="200" ry="40" fill="#FFFDF6" />
      {rosettes.map(([x, y], i) => (
        <g key={i}>
          <circle cx={x} cy={y} r="30" fill="#FFFBF1" stroke="#D9C9AC" strokeOpacity="0.9" strokeWidth="2" />
          <circle cx={x} cy={y} r="20" fill="none" stroke="#D9C9AC" strokeOpacity="0.8" strokeWidth="2.5" />
          <circle cx={x} cy={y} r="10" fill="none" stroke="#D9C9AC" strokeOpacity="0.8" strokeWidth="2.5" />
          <circle cx={x - 8} cy={y - 9} r="5" fill="#fff" opacity="0.8" />
        </g>
      ))}
      {/* piping bag: a soft cone ending in a gold nozzle just above the rosettes */}
      <path d="M392 96 Q480 70 590 128 L486 418 L458 418Z" fill={`url(#${uid}bag)`} />
      <path d="M392 96 Q480 70 590 128" fill="none" stroke="#D9C9AC" strokeWidth="3" />
      <path d="M470 110 L470 410 M520 112 L476 408" stroke="#D9C9AC" strokeOpacity="0.7" strokeWidth="2" fill="none" />
      <path d="M458 418 L486 418 L478 446 L466 446Z" fill="#C79A55" />
      <path d="M466 446 q6 12 12 0" fill="#E9C98F" />
      <path d="M440 436 q32 -26 64 0 q-16 26 -32 12 q-16 14 -32 -12Z" fill="#FFFBF1" stroke="#D9C9AC" strokeWidth="2" />
    </g>
  );
}

function Box({ uid, t }: SubjectProps) {
  return (
    <g>
      <defs>
        <Linear id={`${uid}bx`} stops={[[0, '#FFFBF1'], [1, '#EADBBD']]} />
        <Linear id={`${uid}rb`} stops={[[0, '#C8962E'], [1, '#B3202B']]} />
      </defs>
      <Shadow uid={uid} cx={410} cy={766} rx={290} ry={30} o={0.5} />
      <path d="M180 460 L520 460 L620 410 L280 410Z" fill="#F3E7CE" />
      <rect x="180" y="460" width="340" height="270" fill={`url(#${uid}bx)`} />
      <path d="M520 460 L620 410 V680 L520 730Z" fill="#CDB68C" />
      <rect x="180" y="460" width="340" height="24" fill="#fff" opacity="0.4" />
      <rect x="326" y="460" width="48" height="270" fill={`url(#${uid}rb)`} />
      <path d="M520 460 L620 410 V680" stroke="#fff" strokeOpacity="0.4" fill="none" />
      <path d="M350 410 L350 460 M350 410 L350 460" stroke="#B3202B" strokeWidth="2" />
      <path d="M350 456 C270 380 220 380 232 420 C244 460 320 458 350 456Z" fill={`url(#${uid}rb)`} />
      <path d="M350 456 C430 380 480 380 468 420 C456 460 380 458 350 456Z" fill={`url(#${uid}rb)`} />
      <circle cx="350" cy="456" r="18" fill="#A87635" />
      <path d="M338 466 L300 560 M362 466 L402 556" stroke="#A87635" strokeWidth="14" strokeLinecap="round" />
      <text x="262" y="660" fontFamily="Cormorant Garamond, Georgia, serif" fontStyle="italic" fontSize="38" fill="#B3202B" textAnchor="middle">Talukder</text>
      <Crumbs seed={29} n={12} cy={760} spread={300} color={t.ink} />
    </g>
  );
}

function Storefront({ uid }: SubjectProps) {
  return (
    <g>
      <defs>
        <Linear id={`${uid}aw`} x1={0} y1={0} x2={1} y2={0} stops={[[0, '#2a1710'], [1, '#3b2418']]} />
        <Linear id={`${uid}gl`} stops={[[0, '#FFE9BC'], [1, '#F0C27A']]} />
      </defs>
      <rect width="800" height="1000" fill="#3a241a" opacity="0.5" />
      <rect x="90" y="250" width="620" height="520" fill="#1d100a" />
      <rect x="120" y="330" width="560" height="400" fill={`url(#${uid}gl)`} />
      {[0, 1, 2].map((i) => <rect key={i} x={140} y={440 + i * 100} width="520" height="10" fill="#B3202B" opacity="0.8" />)}
      {Array.from({ length: 12 }).map((_, i) => (
        <g key={i} transform={`translate(${190 + (i % 4) * 135} ${430 + Math.floor(i / 4) * 100})`}>
          <ellipse cx="0" cy="0" rx="46" ry="20" fill="#C98233" />
          <ellipse cx="0" cy="-6" rx="42" ry="16" fill="#E2A857" />
        </g>
      ))}
      <path d="M70 230 H730 L700 330 H100Z" fill={`url(#${uid}aw)`} />
      {Array.from({ length: 8 }).map((_, i) => <path key={i} d={`M${100 + i * 80} 230 L${104 + i * 76} 330 H${150 + i * 76} L${140 + i * 80} 230Z`} fill={i % 2 ? '#E9DDC6' : '#C8962E'} opacity="0.92" />)}
      <text x="400" y="210" fontFamily="Cormorant Garamond, Georgia, serif" fontStyle="italic" fontSize="64" fill="#F4E2B6" textAnchor="middle">Talukder</text>
      <rect x="60" y="770" width="680" height="30" fill="#120a06" />
    </g>
  );
}

function Hero({ uid, t }: SubjectProps) {
  return (
    <g>
      <g transform="translate(70 -92) scale(0.86)"><Loaf uid={`${uid}hl`} t={t} /></g>
      <g transform="translate(-34 230) scale(0.7)"><Croissant uid={`${uid}hc`} t={t} /></g>
      <g transform="translate(330 214) scale(0.58)"><ChocolateCake uid={`${uid}hk`} t={t} /></g>
    </g>
  );
}

export const subjects: Record<ArtKind, ComponentType<SubjectProps>> = {
  croissant: Croissant as ComponentType<SubjectProps>,
  cheesecake: Cheesecake,
  'chocolate-cake': ChocolateCake,
  tiered: Tiered,
  tart: Tart,
  eclair: Eclair,
  cookies: Cookies,
  loaf: Loaf,
  roll: Roll,
  jar: Jar,
  brownie: Brownie,
  macarons: Macarons,
  dough: Dough,
  oven: Oven,
  piping: Piping,
  box: Box,
  hero: Hero,
  storefront: Storefront,
};
