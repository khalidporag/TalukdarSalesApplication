import { createRoot } from 'react-dom/client';
import { Art } from '@/art/Art';
import { Mark } from '@/components/Logo';
import '@/index.css';

createRoot(document.getElementById('root')!).render(
  <div style={{ width: 1200, height: 630, position: 'relative', background: '#F8F2E9', overflow: 'hidden', display: 'flex' }}>
    <div style={{ width: 640, padding: '70px 64px', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 14, color: '#241710' }}>
        <Mark className="h-10 w-10 text-caramel" />
        <div><div className="font-display" style={{ fontSize: 40, fontStyle: 'italic', lineHeight: 1 }}>Aurum</div><div style={{ fontSize: 10, letterSpacing: '0.28em', fontWeight: 600, marginTop: 4 }}>BAKEHOUSE</div></div>
      </div>
      <div>
        <div className="font-display" style={{ fontSize: 92, lineHeight: 0.95, color: '#241710' }}>Made slowly.<br /><em style={{ color: '#8F5F28' }}>Loved deeply.</em></div>
        <div style={{ marginTop: 26, fontSize: 15, letterSpacing: '0.22em', fontWeight: 600, color: '#8F5F28' }}>CAKES · PASTRIES · BREADS · DHAKA</div>
      </div>
    </div>
    <div style={{ flex: 1, position: 'relative' }}><Art kind="hero" tone="honey" className="absolute inset-0 h-full w-full" /></div>
  </div>
);
