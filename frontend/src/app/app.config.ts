import { registerLocaleData } from '@angular/common';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import localeEsEc from '@angular/common/locales/es-EC';
import { ApplicationConfig, LOCALE_ID, provideBrowserGlobalErrorListeners } from '@angular/core';
import { MAT_DATE_LOCALE, provideNativeDateAdapter } from '@angular/material/core';
import { MatPaginatorIntl } from '@angular/material/paginator';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { routes } from './app.routes';
import { SpanishPaginatorIntl } from './core/i18n/spanish-paginator-intl';
import { errorInterceptor } from './core/interceptors/error.interceptor';

registerLocaleData(localeEsEc);

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),

    // Los parámetros de ruta llegan como input() del componente.
    provideRouter(routes, withComponentInputBinding()),

    provideHttpClient(withInterceptors([errorInterceptor])),

    provideNativeDateAdapter(),
    { provide: LOCALE_ID, useValue: 'es-EC' },
    { provide: MAT_DATE_LOCALE, useValue: 'es-EC' },

    { provide: MatPaginatorIntl, useClass: SpanishPaginatorIntl },
  ],
};
