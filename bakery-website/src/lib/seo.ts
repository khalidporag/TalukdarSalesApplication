import { useEffect } from 'react';
import { brand } from '@/data/brand';

interface Seo {
  title?: string;
  description?: string;
  path?: string;
  jsonLd?: object;
}

function setMeta(selector: string, attr: 'name' | 'property', key: string, content: string) {
  let el = document.head.querySelector<HTMLMetaElement>(selector);
  if (!el) {
    el = document.createElement('meta');
    el.setAttribute(attr, key);
    document.head.appendChild(el);
  }
  el.setAttribute('content', content);
}

/** Per-page title, description, canonical and Open Graph tags (the static ones live in index.html). */
export function useSeo({ title, description, path, jsonLd }: Seo) {
  useEffect(() => {
    const full = title ? `${title} · ${brand.name}` : `${brand.name} · Handcrafted cakes, pastries & breads`;
    document.title = full;
    if (description) {
      setMeta('meta[name="description"]', 'name', 'description', description);
      setMeta('meta[property="og:description"]', 'property', 'og:description', description);
    }
    setMeta('meta[property="og:title"]', 'property', 'og:title', full);
    if (path) {
      const canonical = document.head.querySelector<HTMLLinkElement>('link[rel="canonical"]');
      if (canonical) canonical.href = new URL(path, canonical.href).toString();
    }
    let script: HTMLScriptElement | null = null;
    if (jsonLd) {
      script = document.createElement('script');
      script.type = 'application/ld+json';
      script.dataset.page = 'true';
      script.text = JSON.stringify(jsonLd);
      document.head.appendChild(script);
    }
    return () => { script?.remove(); };
  }, [title, description, path, jsonLd]);
}
