import { Routes } from '@angular/router';
import { AuthenticationGuard } from './core/guards/authentication.guard';
import { DashboardComponent } from './dashboard/dashboard.component';
import { DrifEoiViewComponent } from './drif/drif-eoi/drif-eoi-view/drif-eoi-view.component';
import { EOIApplicationComponent } from './drif/drif-eoi/drif-eoi.component';
import { DrifFpInstructionsComponent } from './drif/drif-fp/drif-fp-instructions/drif-fp-instructions.component';
import { DrifFpComponent } from './drif/drif-fp/drif-fp.component';
import { SuccessPageComponent } from './success-page/success-page.component';

export const routes: Routes = [
  { path: '', component: DashboardComponent },
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'drif-eoi',
    component: EOIApplicationComponent,
    canActivate: [AuthenticationGuard],
    children: [
      {
        path: ':id',
        component: EOIApplicationComponent,
        canActivate: [AuthenticationGuard],
      },
    ],
  },
  {
    path: 'drif-fp-instructions/:eoiId',
    component: DrifFpInstructionsComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'drif-fp/:id',
    component: DrifFpComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'eoi-submission-details/:id',
    component: DrifEoiViewComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'success',
    component: SuccessPageComponent,
    canActivate: [AuthenticationGuard],
  },
];
