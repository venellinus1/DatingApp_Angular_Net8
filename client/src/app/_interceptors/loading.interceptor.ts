import { HttpInterceptorFn } from '@angular/common/http';
import { BusyService } from '../_services/busy.service';
import { inject } from '@angular/core';
import { delay, finalize, identity } from 'rxjs';
import { environment } from '../../environments/environment';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const busyService = inject(BusyService);
  busyService.busy();

  return next(req).pipe(
    // kind of a hack - cannot use null in here therefore using identity which will return itself if called
    // the goal is to have a delay only in dev not in prod
    (environment.production ? identity : delay(1000)), 
    finalize(() => busyService.idle())
  );
};
