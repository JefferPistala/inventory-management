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

// Registro el idioma español de Ecuador para que los pipes de fecha
// y moneda usen el formato local.
registerLocaleData(localeEsEc);

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),

    // withComponentInputBinding: los parámetros de la ruta (:id) y los
    // query params (?productId=3) llegan directo como input() del componente.
    provideRouter(routes, withComponentInputBinding()),

    // Todas las peticiones HTTP pasan por el interceptor de errores.
    provideHttpClient(withInterceptors([errorInterceptor])),

    // Adaptador de fechas para los datepicker de Material.
    provideNativeDateAdapter(),
    { provide: LOCALE_ID, useValue: 'es-EC' },
    { provide: MAT_DATE_LOCALE, useValue: 'es-EC' },

    // Textos del paginador en español.
    { provide: MatPaginatorIntl, useClass: SpanishPaginatorIntl },
  ],
};
