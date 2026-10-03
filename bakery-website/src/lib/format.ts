import { brand } from '@/data/brand';

export const money = (n: number) => `${brand.currency} ${n.toLocaleString('en-IN')}`;
