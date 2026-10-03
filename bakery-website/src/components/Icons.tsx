interface P { size?: number; className?: string; strokeWidth?: number }

export const Instagram = ({ size = 20, className, strokeWidth = 1.5 }: P) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" className={className} aria-hidden="true">
    <rect x="3" y="3" width="18" height="18" rx="5" /><circle cx="12" cy="12" r="4" /><circle cx="17.2" cy="6.8" r="0.9" fill="currentColor" stroke="none" />
  </svg>
);

export const Facebook = ({ size = 20, className, strokeWidth = 1.5 }: P) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" className={className} aria-hidden="true">
    <path d="M14 8.5V7c0-.8.5-1.2 1.2-1.2H17V3h-2.4C12.3 3 11 4.4 11 6.6v1.9H8.5V11.5H11V21h3v-9.5h2.4l.5-3H14Z" />
  </svg>
);

export const WhatsApp = ({ size = 20, className, strokeWidth = 1.5 }: P) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" className={className} aria-hidden="true">
    <path d="M3.5 20.5l1.2-4.1A8.5 8.5 0 1 1 8 19.4l-4.5 1.1Z" /><path d="M9 8.6c.2 2.9 2.6 5.3 5.6 5.8.5.1 1.2-.5 1.3-1.1l-1.7-1-1 .6c-1-.4-2-1.4-2.4-2.4l.7-.9-.9-1.8c-.5.1-1.1.4-1.6.8Z" />
  </svg>
);
