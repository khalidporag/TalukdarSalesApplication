export type Tone = 'honey' | 'caramel' | 'cocoa' | 'sage' | 'blush' | 'wheat' | 'ivory';

export interface ToneDef {
  bg1: string;
  bg2: string;
  table: string;
  plate: string;
  plateEdge: string;
  ink: string;
}

export const tones: Record<Tone, ToneDef> = {
  honey: { bg1: '#EBD2A4', bg2: '#CFA867', table: '#C99B5C', plate: '#FAF4E8', plateEdge: '#E7D9BF', ink: '#3A2412' },
  caramel: { bg1: '#DDB78B', bg2: '#B07B4C', table: '#A06E42', plate: '#F6EBDA', plateEdge: '#E2CFB1', ink: '#2F1B0E' },
  cocoa: { bg1: '#7A5340', bg2: '#3B2318', table: '#2E1B12', plate: '#EFE4D1', plateEdge: '#D6C5A8', ink: '#1A0E08' },
  sage: { bg1: '#CDD1B5', bg2: '#9FA882', table: '#909A72', plate: '#F8F3E7', plateEdge: '#E3DCC5', ink: '#2B3320' },
  blush: { bg1: '#F0D1C7', bg2: '#D8A294', table: '#C98F80', plate: '#FBF4EA', plateEdge: '#EBD8C6', ink: '#472A25' },
  wheat: { bg1: '#E7D6B3', bg2: '#C6AB78', table: '#B79A66', plate: '#F5ECDB', plateEdge: '#DECDAC', ink: '#3A2A14' },
  ivory: { bg1: '#F4ECDD', bg2: '#DECFB3', table: '#D2C09F', plate: '#FFFBF3', plateEdge: '#ECE0C8', ink: '#3A2C1E' },
};
