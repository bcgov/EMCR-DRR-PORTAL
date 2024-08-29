import { Routes } from '@angular/router';
import { AuthenticationGuard } from './core/guards/authentication.guard';
import { DashboardComponent } from './dashboard/dashboard.component';
import { DrifSubmissionDetailsComponent } from './drif/drif-eoi-summary/drif-eoi-summary.component';
import { EOIApplicationComponent } from './drif/drif-eoi/drif-eoi.component';
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
    path: 'submission-details/:id',
    component: DrifSubmissionDetailsComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'success',
    component: SuccessPageComponent,
    canActivate: [AuthenticationGuard],
  },
];
