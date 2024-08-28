import {
  provideHttpClient,
  withFetch,
  withInterceptors,
} from '@angular/common/http';
import {
  APP_INITIALIZER,
  ApplicationConfig,
  importProvidersFrom,
  isDevMode,
} from '@angular/core';
import { provideLuxonDateAdapter } from '@angular/material-luxon-adapter';
import { MAT_DATE_FORMATS, MatDateFormats } from '@angular/material/core';
import { MAT_FORM_FIELD_DEFAULT_OPTIONS } from '@angular/material/form-field';
import { provideClientHydration } from '@angular/platform-browser';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { provideHotToastConfig } from '@ngneat/hot-toast';
import { provideTransloco } from '@ngneat/transloco';
import { provideOAuthClient } from 'angular-oauth2-oidc';
import { provideNgxMask } from 'ngx-mask';
import { NgxSpinnerModule } from 'ngx-spinner';
import { DrifapplicationService } from '../api/drifapplication/drifapplication.service';
import { routes } from './app.routes';
import { AuthService } from './core/auth/auth.service';
import { AppConfigurationService } from './core/configuration/app-configuration.service';
import { ErrorInterceptor } from './core/interceptors/error.interceptor';
import { LoadingInterceptor } from './core/interceptors/loading.interceptor';
import { TokenInterceptor } from './core/interceptors/token.interceptor';
import { TranslocoHttpLoader } from './transloco-loader';

export const DRR_DATE_FORMATS: MatDateFormats = {
  parse: {
    dateInput: 'yyyy-MM-dd',
  },
  display: {
    dateInput: 'yyyy-MM-dd',
    monthYearLabel: 'yyyy',
    dateA11yLabel: 'yyyy-MM-dd',
    monthYearA11yLabel: 'yyyy',
  },
};

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideClientHydration(),
    provideHttpClient(
      withFetch(),
      withInterceptors([LoadingInterceptor, TokenInterceptor, ErrorInterceptor])
    ),
    provideOAuthClient(),
    provideAnimations(),
    provideLuxonDateAdapter(),
    {
      provide: MAT_DATE_FORMATS,
      useValue: DRR_DATE_FORMATS,
    },
    provideHotToastConfig({
      autoClose: false,
      dismissible: true,
    }),
    DrifapplicationService,
    provideHttpClient(),
    provideTransloco({
      config: {
        availableLangs: ['en'],
        defaultLang: 'en',
        // Remove this option if your application doesn't support changing language in runtime.
        reRenderOnLangChange: true,
        prodMode: !isDevMode(),
      },
      loader: TranslocoHttpLoader,
    }),
    {
      provide: APP_INITIALIZER,
      deps: [AppConfigurationService, AuthService],
      useFactory:
        (
          appConfigurationService: AppConfigurationService,
          authService: AuthService
        ) =>
        async () => {
          await appConfigurationService.loadConfiguration();
          await authService.init();
          await appConfigurationService.loadEntites();
        },
      multi: true,
    },
    importProvidersFrom(NgxSpinnerModule),
    {
      provide: MAT_FORM_FIELD_DEFAULT_OPTIONS,
      useValue: {
        hideRequiredMarker: false,
        floatLabel: 'always',
        appearance: 'outline',
      },
    },
    provideNgxMask(),
  ],
};
