// Static placeholder content mirroring prototype/sadalene-prototype.html — no backend calls yet.
// Swap these for the real src/api/catalog.ts + src/hooks/use-catalog-queries.ts once wired up.

export interface MockDivision {
  id: string;
  name: string;
}

export const MOCK_DIVISIONS: MockDivision[] = [
  { id: "58panna", name: "58 PANNA DIVISION" },
  { id: "36panna", name: "36 PANNA DIVISION" },
  { id: "pieces", name: "PIECES PACKING" },
];

export const CATEGORY_TABS = ["MASTER CARD", "DIGITAL", "PRINT"] as const;
export type CategoryTab = (typeof CATEGORY_TABS)[number];

export const MOCK_PRODUCTS_BY_TAB: Record<CategoryTab, string[]> = {
  "MASTER CARD": ["Product 1", "Product 2", "Product 3", "Product 4", "Product 5", "Product 6", "Product 7", "Product 8", "Product 9"],
  DIGITAL: ["Digital Jacquard -1", "Digital Jacquard -2", "Digital Print -1"],
  PRINT: ["Print Design -1", "Print Design -2"],
};

export const MOCK_PRODUCT_OPTIONS = ["Plain", "Checks", "Lining"];

export interface MockCartLine {
  sNo: number;
  product: string;
  qty: number;
}

export const MOCK_CART_LINES: MockCartLine[] = [
  { sNo: 1, product: "Digital Jacquard -1", qty: 12 },
  { sNo: 2, product: "Digital Jacquard -2", qty: 8 },
];

export const MOCK_SEARCH_RESULTS = ["Digital Jacquard -1", "Digital Jacquard -2"];

export const MOCK_ADVANCE_BOOKING_PRODUCTS = ["Product 13", "Product 14", "Product 15"];

export const MOCK_GALLERY_PRODUCTS = [
  "Product 1", "Product 2", "Product 3", "Product 4",
  "Product 5", "Product 6", "Product 7", "Product 8",
];
