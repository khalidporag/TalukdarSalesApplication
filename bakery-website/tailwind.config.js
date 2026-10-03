/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        ivory: '#F8F2E9',
        cream: '#F0E7D8',
        bone: '#E4D6BF',
        sand: '#CDB798',
        caramel: '#B5803F',
        'caramel-deep': '#8F5F28',
        cocoa: '#4B3022',
        espresso: '#241710',
        ink: '#1B110C',
      },
      fontFamily: {
        display: ['"Cormorant Garamond"', 'Georgia', 'serif'],
        sans: ['"Manrope Variable"', 'Manrope', 'system-ui', 'sans-serif'],
      },
      letterSpacing: { wider2: '0.18em', widest2: '0.28em' },
      transitionTimingFunction: { silk: 'cubic-bezier(0.22, 0.61, 0.36, 1)' },
      keyframes: {
        drift: { '0%,100%': { transform: 'translateY(0) rotate(0deg)' }, '50%': { transform: 'translateY(-10px) rotate(2deg)' } },
        steam: { '0%': { opacity: '0', transform: 'translateY(8px) scaleX(0.9)' }, '40%': { opacity: '.7' }, '100%': { opacity: '0', transform: 'translateY(-26px) scaleX(1.1)' } },
        marquee: { from: { transform: 'translateX(0)' }, to: { transform: 'translateX(-50%)' } },
      },
      animation: {
        drift: 'drift 9s ease-in-out infinite',
        steam: 'steam 3.6s ease-in-out infinite',
        marquee: 'marquee 48s linear infinite',
      },
    },
  },
  plugins: [],
};
