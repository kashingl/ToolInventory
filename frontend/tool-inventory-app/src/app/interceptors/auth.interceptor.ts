import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getValidToken();
  const requestUrl = req.url.toLowerCase();
  const isApiRequest = requestUrl.includes('/api/');
  const isAuthEndpoint = requestUrl.includes('/api/auth/login') || requestUrl.includes('/api/auth/register');
  const shouldHandleUnauthorized = isApiRequest && !isAuthEndpoint;
  const request = token && shouldHandleUnauthorized
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(request).pipe(
    catchError(error => {
      if (shouldHandleUnauthorized && error.status === 401) {
        authService.logout();
      }

      return throwError(() => error);
    })
  );
};
