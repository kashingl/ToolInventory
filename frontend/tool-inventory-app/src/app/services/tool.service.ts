import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Tool, CreateTool, UpdateTool } from '../models/tool.model';
import { AppConfigService } from './app-config.service';

@Injectable({ providedIn: 'root' })
export class ToolService {
  private readonly http = inject(HttpClient);
  private readonly cfg = inject(AppConfigService);
  private get base() { return `${this.cfg.apiBaseUrl}/api/tools`; }

  getAll(): Observable<Tool[]> { return this.http.get<Tool[]>(this.base); }
  getById(id: number): Observable<Tool> { return this.http.get<Tool>(`${this.base}/${id}`); }
  getByBarcode(code: string): Observable<Tool> { return this.http.get<Tool>(`${this.base}/barcode/${code}`); }
  create(tool: CreateTool): Observable<Tool> { return this.http.post<Tool>(this.base, tool); }
  update(id: number, tool: UpdateTool): Observable<void> { return this.http.put<void>(`${this.base}/${id}`, tool); }
  delete(id: number): Observable<void> { return this.http.delete<void>(`${this.base}/${id}`); }
}
