import { useEffect, useState } from 'react';

// Keep in step with brand.hours. 0 = Sunday ... 6 = Saturday. Times are 24h.
const week: Record<number, [number, number]> = {
  6: [9, 21], 0: [9, 21], 1: [9, 21], 2: [9, 21], 3: [9, 21], 4: [9, 21],
  5: [15, 22],
};

const fmt = (h: number) => `${h % 12 === 0 ? 12 : h % 12}${h < 12 ? ' AM' : ' PM'}`;

export function openStatus(now = new Date()) {
  const [from, to] = week[now.getDay()];
  const hour = now.getHours() + now.getMinutes() / 60;
  if (hour >= from && hour < to) return { open: true, label: `Open now · until ${fmt(to)}` };
  if (hour < from) return { open: false, label: `Closed · opens today at ${fmt(from)}` };
  const [nf] = week[(now.getDay() + 1) % 7];
  return { open: false, label: `Closed · opens tomorrow at ${fmt(nf)}` };
}

/** Live "open now" label, refreshed each minute. */
export function useOpenStatus() {
  const [s, setS] = useState(() => openStatus());
  useEffect(() => {
    const t = window.setInterval(() => setS(openStatus()), 60_000);
    return () => window.clearInterval(t);
  }, []);
  return s;
}
