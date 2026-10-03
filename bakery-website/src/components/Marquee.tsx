const words = ['Butter croissants', 'Slow sourdough', 'Celebration cakes', 'Brown butter cookies', 'Seasonal tarts', 'Baked every morning'];

/** A single quiet line between sections. */
export function Marquee({ dark = false }: { dark?: boolean }) {
  const row = [...words, ...words];
  return (
    <div className={`overflow-hidden border-y py-5 ${dark ? 'border-ivory/10 bg-espresso text-ivory/70' : 'border-espresso/10 bg-cream/60 text-espresso/70'}`} aria-hidden="true">
      <div className="flex w-max animate-marquee whitespace-nowrap">
        {[0, 1].map((k) => (
          <div key={k} className="flex shrink-0 items-center">
            {row.map((w, i) => (
              <span key={`${k}${i}`} className="flex items-center font-display text-2xl italic sm:text-3xl">
                <span className="px-8">{w}</span><span className="text-caramel not-italic">✦</span>
              </span>
            ))}
          </div>
        ))}
      </div>
    </div>
  );
}
