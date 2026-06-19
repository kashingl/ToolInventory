import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Category, CreateCategory } from '../models/category.model';
import { AppConfigService } from './app-config.service';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly http = inject(HttpClient);
  private readonly cfg = inject(AppConfigService);
  private get base() { return `${this.cfg.apiBaseUrl}/api/categories`; }

  getAll(): Observable<Category[]> { return this.http.get<Category[]>(this.base); }
  create(cat: CreateCategory): Observable<Category> { return this.http.post<Category>(this.base, cat); }
  update(id: number, cat: CreateCategory): Observable<void> { return this.http.put<void>(`${this.base}/${id}`, cat); }
  delete(id: number): Observable<void> { return this.http.delete<void>(`${this.base}/${id}`); }
}
