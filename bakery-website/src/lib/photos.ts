/**
 * Real photography. Drop files into src/assets/photos/ named after the product slug
 * (e.g. butter-croissant.jpg) or one of the scene keys (hero, story, kitchen-mix, kitchen-bake, kitchen-decorate,
 * kitchen-serve, gallery-1 ... gallery-9, social-1 ... social-6, custom-wedding, custom-birthday ...).
 * They are picked up automatically, optimised and lazy-loaded. Anything without a photo falls back to the illustration.
 */
const files = import.meta.glob('../assets/photos/*.{jpg,jpeg,png,webp,avif}', { eager: true, query: '?url', import: 'default' }) as Record<string, string>;

const byKey: Record<string, string> = {};
for (const [path, url] of Object.entries(files)) {
  const key = path.split('/').pop()!.replace(/\.[^.]+$/, '');
  byKey[key] = url;
}

export const photoFor = (key?: string) => (key ? byKey[key] : undefined);
