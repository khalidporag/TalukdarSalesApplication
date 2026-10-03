// Static-hosting helpers: SPA fallback (404.html) and a sitemap for the real routes.
import { copyFileSync, writeFileSync, readFileSync } from 'node:fs';
const site = process.env.SITE_URL ?? 'https://example.com';
copyFileSync('dist/index.html', 'dist/404.html');
const routes = ['/', '/menu', '/about', '/contact'];
const slugs = [...readFileSync('src/data/products.ts', 'utf8').matchAll(/slug:\s*'([^']+)'/g)].map((m) => `/product/${m[1]}`);
const urls = [...routes, ...slugs].map((r) => `  <url><loc>${site}${r}</loc></url>`).join('\n');
writeFileSync('dist/sitemap.xml', `<?xml version="1.0" encoding="UTF-8"?>\n<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">\n${urls}\n</urlset>\n`);
writeFileSync('dist/robots.txt', `User-agent: *\nAllow: /\nSitemap: ${site}/sitemap.xml\n`);
console.log('postbuild: 404.html, sitemap.xml, robots.txt');
