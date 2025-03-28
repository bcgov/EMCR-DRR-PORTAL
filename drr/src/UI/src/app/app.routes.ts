import { Routes } from '@angular/router';
import { AuthenticationGuard } from './core/guards/authentication.guard';
import { DashboardComponent } from './dashboard/dashboard.component';
import { DrifEoiViewComponent } from './drif/drif-eoi/drif-eoi-view/drif-eoi-view.component';
import { EOIApplicationComponent } from './drif/drif-eoi/drif-eoi.component';
import { DrifFpInstructionsComponent } from './drif/drif-fp/drif-fp-instructions/drif-fp-instructions.component';
import { DrifFpScreenerComponent } from './drif/drif-fp/drif-fp-screener/drif-fp-screener.component';
import { DrifFpViewComponent } from './drif/drif-fp/drif-fp-view/drif-fp-view.component';
import { DrifFpComponent } from './drif/drif-fp/drif-fp.component';
import { DrifClaimCreateComponent } from './drif/drif-project/drif-claim/drif-claim-create/drif-claim-create.component';
import { DrifClaimViewComponent } from './drif/drif-project/drif-claim/drif-claim-view/drif-claim-view.component';
import { DrifConditionClearComponent } from './drif/drif-project/drif-condition/drif-condition-clear/drif-condition-clear.component';
import { DrifForecastCreateComponent } from './drif/drif-project/drif-forecast/drif-forecast-create/drif-forecast-create.component';
import { DrifForecastViewComponent } from './drif/drif-project/drif-forecast/drif-forecast-view/drif-forecast-view.component';
import { DrifInterimReportCreateComponent } from './drif/drif-project/drif-interim-report/drif-interim-report-create/drif-interim-report-create.component';
import { DrifProgressReportCreateComponent } from './drif/drif-project/drif-progress-report/drif-progress-report-create/drif-progress-report-create.component';
import { DrifProgressReportViewComponent } from './drif/drif-project/drif-progress-report/drif-progress-report-view.component';
import { DrifProjectComponent } from './drif/drif-project/drif-project.component';

export const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'dashboard', component: DashboardComponent },
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
    path: 'drif-fp-screener/:eoiId/:projectTitle',
    component: DrifFpScreenerComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'drif-fp-instructions/:eoiId/:projectTitle',
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
    path: 'fp-submission-details/:id',
    component: DrifFpViewComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'drif-projects/:projectId',
    component: DrifProjectComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'drif-projects/:projectId/interim-reports/create',
    component: DrifInterimReportCreateComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'drif-projects/:projectId/interim-reports/:reportId/progress-reports/:progressReportId/edit',
    component: DrifProgressReportCreateComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'drif-projects/:projectId/interim-reports/:reportId/progress-reports/:progressReportId/view',
    component: DrifProgressReportViewComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'drif-projects/:projectId/interim-reports/:reportId/claims/:claimId/edit',
    component: DrifClaimCreateComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'drif-projects/:projectId/interim-reports/:reportId/claims/:claimId/view',
    component: DrifClaimViewComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'drif-projects/:projectId/interim-reports/:reportId/forecasts/:forecastId/edit',
    component: DrifForecastCreateComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'drif-projects/:projectId/interim-reports/:reportId/forecasts/:forecastId/view',
    component: DrifForecastViewComponent,
    canActivate: [AuthenticationGuard],
  },
  {
    path: 'drif-projects/:projectId/conditions/:conditionId/clear',
    component: DrifConditionClearComponent,
    canActivate: [AuthenticationGuard],
  },
];
