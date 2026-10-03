import type { ArtKind, Tone } from '@/art/Art';

export interface GalleryItem {
  key: string;
  caption: string;
  tag: string;
  art: ArtKind;
  tone: Tone;
  crop?: 'center' | 'left' | 'right' | 'close' | 'wide';
  /** grid placement on desktop */
  cls: string;
}

/**
 * Add real photos as src/assets/photos/gallery-1.jpg ... gallery-9.jpg and they replace the illustrations.
 * `cls` controls the asymmetric masonry placement and can be changed freely.
 */
export const gallery: GalleryItem[] = [
  { key: 'gallery-1', caption: 'Laminated dough, folded by hand', tag: 'Behind the scenes', art: 'croissant', tone: 'honey', cls: 'lg:col-[1/6] lg:row-[1/3] aspect-[4/5] lg:aspect-auto' },
  { key: 'gallery-2', caption: 'Burnt Basque, cooling overnight', tag: 'Product', art: 'cheesecake', tone: 'caramel', crop: 'close', cls: 'lg:col-[6/10] lg:row-[1/2] aspect-square lg:aspect-auto' },
  { key: 'gallery-3', caption: 'Ribbon-tied gift boxes', tag: 'Packaging', art: 'box', tone: 'blush', cls: 'lg:col-[10/13] lg:row-[1/2] aspect-[3/4] lg:aspect-auto' },
  { key: 'gallery-4', caption: 'Wedding cake, two days before', tag: 'Cake decoration', art: 'tiered', tone: 'sage', cls: 'lg:col-[6/9] lg:row-[2/4] aspect-[3/4.4] lg:aspect-auto' },
  { key: 'gallery-5', caption: 'The five o\'clock bake', tag: 'Kitchen', art: 'oven', tone: 'cocoa', cls: 'lg:col-[9/13] lg:row-[2/3] aspect-[4/3] lg:aspect-auto' },
  { key: 'gallery-6', caption: 'Pistachio and raspberry', tag: 'Product', art: 'macarons', tone: 'blush', crop: 'wide', cls: 'lg:col-[1/6] lg:row-[3/4] aspect-[5/4] lg:aspect-auto' },
  { key: 'gallery-7', caption: 'A fresh batch, still warm', tag: 'Product', art: 'cookies', tone: 'honey', crop: 'right', cls: 'lg:col-[9/13] lg:row-[3/4] aspect-[4/5] lg:aspect-auto' },
  { key: 'gallery-8', caption: 'Every cake gets a steady hand', tag: 'Cake decoration', art: 'piping', tone: 'ivory', crop: 'wide', cls: 'lg:col-[1/8] lg:row-[4/6] aspect-square lg:aspect-auto' },
  { key: 'gallery-9', caption: 'Saturday morning sourdough', tag: 'Customer moments', art: 'loaf', tone: 'wheat', crop: 'right', cls: 'lg:col-[8/13] lg:row-[4/6] aspect-[3/4] lg:aspect-auto' },
];
