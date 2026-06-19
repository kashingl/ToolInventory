import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MaintenanceRecord, CreateMaintenanceRecord } from '../models/maintenance.model';
import { AppConfigService } from './app-config.service';

@Injectable({ providedIn: 'root' })
export class MaintenanceService {
  private readonly http = inject(HttpClient);
  private readonly cfg = inject(AppConfigService);
  private get base() { return `${this.cfg.apiBaseUrl}/api/maintenance`; }

  getAll(): Observable<MaintenanceRecord[]> { return this.http.get<MaintenanceRecord[]>(this.base); }
  getByTool(toolId: number): Observable<MaintenanceRecord[]> { return this.http.get<MaintenanceRecord[]>(`${this.base}/tool/${toolId}`); }
  create(dto: CreateMaintenanceRecord): Observable<MaintenanceRecord> { return this.http.post<MaintenanceRecord>(this.base, dto); }
  delete(id: number): Observable<void> { return this.http.delete<void>(`${this.base}/${id}`); }
}
