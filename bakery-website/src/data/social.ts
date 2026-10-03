import type { ArtKind, Tone } from '@/art/Art';
import { brand } from './brand';

export interface SocialPost {
  key: string;
  art: ArtKind;
  tone: Tone;
  crop?: 'center' | 'left' | 'right' | 'close' | 'wide';
  url: string;
  alt: string;
}

/**
 * Placeholder feed. To show the real one, replace this function with a call to your Instagram/Facebook feed
 * (or a small proxy that returns { key, imageUrl, url, alt }) and render <img> from imageUrl in SocialSection.
 */
export async function getSocialPosts(): Promise<SocialPost[]> {
  const url = brand.instagram;
  return [
    { key: 'social-1', art: 'croissant', tone: 'honey', crop: 'center', url, alt: 'Croissants fresh from the oven' },
    { key: 'social-2', art: 'tart', tone: 'blush', crop: 'center', url, alt: 'Seasonal fruit tart' },
    { key: 'social-3', art: 'loaf', tone: 'wheat', crop: 'center', url, alt: 'Sourdough with a blistered crust' },
    { key: 'social-4', art: 'macarons', tone: 'sage', crop: 'center', url, alt: 'Macarons in a gift box' },
    { key: 'social-5', art: 'chocolate-cake', tone: 'cocoa', crop: 'center', url, alt: 'Dark chocolate ganache cake' },
    { key: 'social-6', art: 'roll', tone: 'caramel', crop: 'center', url, alt: 'Cardamom cinnamon roll' },
  ];
}
