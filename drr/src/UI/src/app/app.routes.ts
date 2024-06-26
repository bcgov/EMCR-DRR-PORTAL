import { Routes } from '@angular/router';
import { AuthenticationGuard } from './core/guards/authentication.guard';
import { DashboardComponent } from './dashboard/dashboard.component';
import { DrifSubmissionDetailsComponent } from './drif/drif-submission-details/drif-submission-details.component';
import { EOIApplicationComponent } from './drif/eoi-application/eoi-application.component';
import { LandingPageComponent } from './landing-page/landing-page.component';
import { SuccessPageComponent } from './success-page/success-page.component';

export const routes: Routes = [
  { path: '', component: LandingPageComponent },
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'eoi-application',
    component: EOIApplicationComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'eoi-application/:id',
    component: EOIApplicationComponent,
    canActivate: [AuthenticationGuard],
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
