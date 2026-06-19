import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Checkout, CreateCheckout } from '../models/checkout.model';
import { AppConfigService } from './app-config.service';

@Injectable({ providedIn: 'root' })
export class CheckoutService {
  private readonly http = inject(HttpClient);
  private readonly cfg = inject(AppConfigService);
  private get base() { return `${this.cfg.apiBaseUrl}/api/checkouts`; }

  getAll(): Observable<Checkout[]> { return this.http.get<Checkout[]>(this.base); }
  create(dto: CreateCheckout): Observable<Checkout> { return this.http.post<Checkout>(this.base, dto); }
  checkIn(id: number): Observable<void> { return this.http.put<void>(`${this.base}/${id}/checkin`, {}); }
  checkInByToolId(toolId: number): Observable<void> { return this.http.put<void>(`${this.base}/tool/${toolId}/checkin`, {}); }
}
