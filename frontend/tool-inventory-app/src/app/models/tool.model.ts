export type ToolStatus = 'Available' | 'CheckedOut' | 'UnderMaintenance' | 'Retired';

export interface Tool {
  id: number;
  name: string;
  description?: string;
  barcode?: string;
  location?: string;
  systainer?: string;
  status: ToolStatus;
  imageUrl?: string;
  categoryId?: number;
  categoryName?: string;
}

export interface CreateTool {
  name: string;
  description?: string;
  barcode?: string;
  location?: string;
  systainer?: string;
  imageUrl?: string;
  categoryId?: number;
}

export interface UpdateTool extends CreateTool {
  status: ToolStatus;
}
