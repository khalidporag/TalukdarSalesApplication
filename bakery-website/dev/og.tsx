import { createRoot } from 'react-dom/client';
import { Art } from '@/art/Art';
import { Logo } from '@/components/Logo';
import '@/index.css';

createRoot(document.getElementById('root')!).render(
  <div style={{ width: 1200, height: 630, position: 'relative', background: '#F8F2E9', overflow: 'hidden', display: 'flex' }}>
    <div style={{ width: 640, padding: '70px 64px', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
      <Logo className="h-24" small={false} />
      <div>
        <div className="font-display" style={{ fontSize: 92, lineHeight: 0.95, color: '#241710' }}>Made slowly.<br /><em style={{ color: '#B3202B' }}>Loved deeply.</em></div>
        <div style={{ marginTop: 26, fontSize: 15, letterSpacing: '0.22em', fontWeight: 600, color: '#B3202B' }}>CAKES · PASTRIES · BREADS · DHAKA</div>
      </div>
    </div>
    <div style={{ flex: 1, position: 'relative' }}><Art kind="hero" tone="honey" className="absolute inset-0 h-full w-full" /></div>
  </div>
);
