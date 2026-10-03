export interface Testimonial {
  quote: string;
  name: string;
  context: string;
  rating: number;
}

export const testimonials: Testimonial[] = [
  {
    quote: 'I ordered the pistachio and rose cake for my mother\'s seventieth. The room went quiet when it arrived, and then everyone asked who made it.',
    name: 'Nusrat Jahan',
    context: 'Birthday cake',
    rating: 5,
  },
  {
    quote: 'The croissants are the real thing. I have stopped buying them anywhere else, and my whole office now knows the Friday box.',
    name: 'Tanvir Ahmed',
    context: 'Weekly pastry box',
    rating: 5,
  },
  {
    quote: 'They listened to everything we wanted for our wedding cake and then made it better. Beautiful to look at and even better to eat.',
    name: 'Samira & Rafi',
    context: 'Wedding cake',
    rating: 5,
  },
  {
    quote: 'Careful, consistent and always on time. Our corporate gift boxes have never gone out looking this good.',
    name: 'Mehnaz Rahman',
    context: 'Corporate gifting',
    rating: 5,
  },
];
