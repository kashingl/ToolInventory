import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status >= 500) {
        console.error('Server error', {
          url: req.url,
          status: error.status,
          message: error.message
        });
      }

      return throwError(() => error);
    })
  );
};
