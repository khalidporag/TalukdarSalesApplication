import { createRoot } from 'react-dom/client';
import { Art, type ArtKind, type Tone } from '@/art/Art';

const items: [ArtKind, Tone][] = [
  ['croissant', 'honey'], ['cheesecake', 'caramel'], ['chocolate-cake', 'cocoa'], ['tiered', 'sage'], ['tart', 'blush'], ['eclair', 'cocoa'],
  ['cookies', 'honey'], ['loaf', 'wheat'], ['roll', 'caramel'], ['jar', 'cocoa'], ['brownie', 'cocoa'], ['macarons', 'blush'],
  ['dough', 'ivory'], ['oven', 'cocoa'], ['piping', 'ivory'], ['box', 'blush'], ['hero', 'honey'], ['storefront', 'caramel'],
];
createRoot(document.getElementById('root')!).render(
  <div style={{ display: 'grid', gridTemplateColumns: 'repeat(6, 300px)', gap: 8, padding: 8 }}>
    {items.map(([k, t]) => (
      <div key={k + t} style={{ width: 300, height: 375 }}><Art kind={k} tone={t} title={k} className="w-full h-full" /></div>
    ))}
  </div>
);
