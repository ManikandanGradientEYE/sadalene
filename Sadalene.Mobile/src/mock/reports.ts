// Static placeholder content mirroring prototype/sadalene-prototype.html's Reports screens.

export interface MockPendingOrder {
  id: string;
  label: string;
  quality: string[];
}

export const MOCK_PENDING_ORDERS: MockPendingOrder[] = [
  { id: "10021", label: "Order No 10021", quality: ["Digital Jacquard -1", "Digital Jacquard -2"] },
  { id: "10022", label: "Order No 10022", quality: ["Product 3"] },
  { id: "10023", label: "Order No 10023", quality: ["Product 7", "Product 8"] },
];

export interface MockChallanRow {
  date: string;
  number: string;
}

export const MOCK_CHALLANS: MockChallanRow[] = [
  { date: "01-07-1984", number: "00056" },
  { date: "02-07-1984", number: "00057" },
];

export const MOCK_INVOICES: MockChallanRow[] = [
  { date: "01-07-1984", number: "00056" },
  { date: "02-07-1984", number: "00057" },
];

export interface MockQA {
  question: string;
  answer: string;
}

export const MOCK_QUERIES: MockQA[] = [
  { question: "When will my order 10021 ship?", answer: "Answered by admin staff — dispatch expected in 3 working days." },
];
