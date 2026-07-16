// Mirrors Sadalene.API's C# DTOs field-for-field. Keep in sync with:
// Sadalene.API/DTOs/{Auth,Catalog,Cart,Orders,Customers}/*.cs

export type IdentityType = "Customer" | "Agent" | "Staff";
export type UnitType = "Full" | "Half";
export type OrderStatus =
  | "Pending"
  | "Confirmed"
  | "Processing"
  | "Dispatched"
  | "Delivered"
  | "Cancelled";
export type CartStatus = "Active" | "CheckedOut" | "Abandoned";

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// --- Auth ---

export interface AuthResponse {
  token: string;
  identityType: IdentityType;
  id: number;
  displayName: string;
  phone: string;
}

// --- Catalog ---

export interface DivisionDto {
  id: number;
  name: string;
  code: string | null;
}

export interface CategoryDto {
  id: number;
  name: string;
}

export interface SubCategoryDto {
  id: number;
  name: string;
  uomMasterId: number | null;
}

export interface ProductListItemDto {
  id: number;
  name: string;
  productCode: string | null;
  marketName: string | null;
  rate: number | null;
  uom: string;
  stock: number;
  primaryImageUrl: string | null;
  divisionId: number;
  divisionName: string;
}

export interface ProductDetailDto {
  id: number;
  name: string;
  marketName: string | null;
  description: string | null;
  productCode: string | null;
  divisionId: number;
  divisionName: string;
  categoryId: number;
  categoryName: string;
  subCategoryId: number;
  subCategoryName: string;
  rate: number | null;
  ratePer: string | null;
  uom: string;
  allowsHalfUnit: boolean;
  stock: number;
  grade: string | null;
  fabricComposition: string | null;
  width: string | null;
  weight: string | null;
  color: string | null;
  designNo: string | null;
  design: string | null;
  brand: string | null;
  imageUrls: string[];
}

// --- Cart ---

export interface CartItemDto {
  id: number;
  productId: number;
  productName: string;
  productCode: string | null;
  quantity: number;
  unitType: UnitType;
  unitOfMeasure: string;
  effectiveQuantity: number;
  displayUnitPrice: number;
  lineTotal: number;
  addedByBarcodeScan: boolean;
}

export interface CartDto {
  id: number;
  forCustomerId: number;
  divisionId: number | null;
  divisionName: string | null;
  status: CartStatus;
  items: CartItemDto[];
  grandTotal: number;
}

export interface AddCartItemRequest {
  productId: number;
  quantity: number;
  unitType?: UnitType;
  addedByBarcodeScan?: boolean;
  scannedBarcodeValue?: string | null;
}

export interface UpdateCartItemRequest {
  quantity: number;
  unitType: UnitType;
}

// --- Orders ---

export interface OrderItemDto {
  productId: number;
  productName: string;
  productCode: string | null;
  quantity: number;
  unitType: UnitType;
  unitOfMeasure: string;
  effectiveQuantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface OrderSummaryDto {
  id: number;
  orderNumber: string;
  customerName: string;
  orderDate: string;
  status: OrderStatus;
  itemCount: number;
  grandTotal: number;
}

export interface OrderDetailDto {
  id: number;
  orderNumber: string;
  customerName: string;
  agentName: string | null;
  placedByName: string | null;
  orderDate: string;
  status: OrderStatus;
  notes: string | null;
  items: OrderItemDto[];
  grandTotal: number;
}

export interface CheckoutRequest {
  notes?: string | null;
}

// --- Customers ---

export interface CustomerListItemDto {
  id: number;
  fullName: string;
  phone: string;
  city: string | null;
}
