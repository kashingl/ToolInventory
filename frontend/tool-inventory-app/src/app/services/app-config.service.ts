import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, tap } from 'rxjs/operators';

interface AppConfig {
  apiBaseUrl: string;
}

@Injectable({ providedIn: 'root' })
export class AppConfigService {
  private readonly http = inject(HttpClient);
  private config: AppConfig = { apiBaseUrl: '' };

  get apiBaseUrl(): string {
    return this.config.apiBaseUrl;
  }

  load() {
    return this.http.get<AppConfig>('/assets/config.json').pipe(
      map(cfg => this.validate(cfg)),
      tap(cfg => (this.config = cfg))
    );
  }

  private validate(cfg: AppConfig): AppConfig {
    const apiBaseUrl = cfg.apiBaseUrl?.trim();
    if (!apiBaseUrl) {
      throw new Error('Invalid app config: apiBaseUrl is required');
    }

    return { apiBaseUrl: apiBaseUrl.replace(/\/+$/, '') };
  }
}
