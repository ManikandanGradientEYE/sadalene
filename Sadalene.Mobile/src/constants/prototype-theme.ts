import { Platform } from "react-native";

// Palette lifted directly from prototype/sadalene-prototype.html's :root custom properties.
export const Colors = {
  ink: "#2c1810",
  maroon: "#5c1a24",
  maroonDk: "#3d1018",
  gold: "#b8860b",
  goldLt: "#d4a520",
  cream: "#f7f0e3",
  creamDk: "#ece2cc",
  thread: "#8a2332",
  paper: "#fffdf8",
  grey: "#8a7f70",
  line: "#e4d8be",
  ok: "#3f6b3f",
  danger: "#b3261e",
} as const;

// The prototype's --font-display is 'Cormorant Garamond', falling back to Georgia/serif — no Google
// Font package is installed for this UI-only pass, so we go straight to the serif fallback.
export const Fonts = {
  display: Platform.select({ ios: "Georgia", android: "serif", default: "Georgia" }),
  body: undefined, // system default, close enough to Inter without pulling in a font package
};

export const Spacing = {
  half: 2,
  one: 4,
  two: 8,
  three: 14,
  four: 22,
  five: 32,
} as const;

export const Radius = {
  sm: 8,
  md: 11,
  lg: 14,
  xl: 20,
} as const;
