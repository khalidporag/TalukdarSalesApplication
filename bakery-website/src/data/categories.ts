export type CategoryId = 'cakes' | 'pastries' | 'cookies' | 'breads' | 'desserts' | 'specials';

export const categories: { id: 'all' | CategoryId; label: string }[] = [
  { id: 'all', label: 'All' },
  { id: 'cakes', label: 'Cakes' },
  { id: 'pastries', label: 'Pastries' },
  { id: 'cookies', label: 'Cookies' },
  { id: 'breads', label: 'Breads' },
  { id: 'desserts', label: 'Desserts' },
  { id: 'specials', label: 'Specials' },
];

export const categoryLabel = (id: CategoryId) => categories.find((c) => c.id === id)?.label ?? id;
