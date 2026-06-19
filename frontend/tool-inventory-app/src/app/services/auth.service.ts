import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { AuthResponse, CurrentUser, LoginRequest, RegisterRequest } from '../models/auth.model';
import { AppConfigService } from './app-config.service';

const STORAGE_KEY = 'tool_inventory_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly cfg = inject(AppConfigService);
  private get apiBase() { return `${this.cfg.apiBaseUrl}/api/auth`; }

  currentUser = signal<CurrentUser | null>(this.loadFromStorage());

  get isAuthenticated(): boolean {
    const user = this.currentUser();
    return user !== null && user.expiresAt > new Date();
  }

  login(request: LoginRequest) {
    return this.http.post<AuthResponse>(`${this.apiBase}/login`, request).pipe(
      tap(response => this.setUser(response))
    );
  }

  register(request: RegisterRequest) {
    return this.http.post<AuthResponse>(`${this.apiBase}/register`, request).pipe(
      tap(response => this.setUser(response))
    );
  }

  logout() {
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(STORAGE_KEY);
    }
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  getValidToken(): string | null {
    const user = this.currentUser();
    if (!user) {
      return null;
    }

    if (user.expiresAt <= new Date()) {
      this.logout();
      return null;
    }

    return user.token;
  }

  private setUser(response: AuthResponse) {
    const user: CurrentUser = {
      email: response.email,
      displayName: response.displayName,
      token: response.token,
      expiresAt: new Date(response.expiresAt)
    };

    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(user));
    }

    this.currentUser.set(user);
  }

  private loadFromStorage(): CurrentUser | null {
    if (typeof localStorage === 'undefined') {
      return null;
    }

    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) {
        return null;
      }

      const user = JSON.parse(raw) as Omit<CurrentUser, 'expiresAt'> & { expiresAt: string | Date };
      const hydratedUser: CurrentUser = {
        ...user,
        expiresAt: new Date(user.expiresAt)
      };

      if (hydratedUser.expiresAt <= new Date()) {
        localStorage.removeItem(STORAGE_KEY);
        return null;
      }

      return hydratedUser;
    } catch {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }
  }
}
