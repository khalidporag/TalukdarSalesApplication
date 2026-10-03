import { Art, type ArtKind, type Tone } from '@/art/Art';
import { photoFor } from '@/lib/photos';

interface PictureProps {
  /** Photo key (product slug or scene key). If a file with this name exists in src/assets/photos it is used. */
  photo?: string;
  art: ArtKind;
  tone: Tone;
  crop?: 'center' | 'left' | 'right' | 'close' | 'wide';
  alt: string;
  className?: string;
  eager?: boolean;
}

/** A real photo when one has been added, otherwise the illustration. Always fills its parent. */
export function Picture({ photo, art, tone, crop, alt, className = '', eager }: PictureProps) {
  const url = photoFor(photo);
  if (url) {
    return <img src={url} alt={alt} loading={eager ? 'eager' : 'lazy'} decoding="async" className={`h-full w-full object-cover ${className}`} />;
  }
  return <Art kind={art} tone={tone} crop={crop} title={alt} className={`h-full w-full ${className}`} />;
}
