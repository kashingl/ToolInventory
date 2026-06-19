export interface Checkout {
  id: number;
  toolId: number;
  toolName: string;
  userId: string;
  userName: string;
  checkoutDate: string;
  expectedReturnDate?: string;
  actualReturnDate?: string;
  notes?: string;
}

export interface CreateCheckout {
  toolId: number;
  userId: string;
  expectedReturnDate?: string;
  notes?: string;
}
