export interface MaintenanceRecord {
  id: number;
  toolId: number;
  toolName: string;
  date: string;
  description: string;
  performedBy?: string;
  cost?: number;
  nextMaintenanceDate?: string;
}

export interface CreateMaintenanceRecord {
  toolId: number;
  date: string;
  description: string;
  performedBy?: string;
  cost?: number;
  nextMaintenanceDate?: string;
}
