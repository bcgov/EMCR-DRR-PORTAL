import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { ProjectService } from '../../../../../api/project/project.service';
import { DraftForecast } from '../../../../../model';
import { ForecastForm } from '../drif-forecast-form';
import { DrifForecastSummaryComponent } from '../drif-forecast-summary/drif-forecast-summary.component';

@Component({
  selector: 'drr-drif-forecast',
  standalone: true,
  imports: [
    CommonModule,
    DrifForecastSummaryComponent,
    TranslocoModule,
    MatButtonModule,
  ],
  templateUrl: './drif-forecast-view.component.html',
  styleUrl: './drif-forecast-view.component.scss',
  providers: [RxFormBuilder],
})
export class DrifForecastViewComponent {
  route = inject(ActivatedRoute);
  router = inject(Router);
  projectService = inject(ProjectService);
  formBuilder = inject(RxFormBuilder);

  projectId?: string;
  reportId?: string;
  forecastId?: string;

  reportName?: string;

  forecastForm?: IFormGroup<ForecastForm>;

  ngOnInit() {
    this.route.params.subscribe((params) => {
      this.projectId = params['projectId'];
      this.reportId = params['reportId'];
      this.forecastId = params['forecastId'];

      this.load();
    });
  }

  load(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.projectService
        .projectGetForecastReport(
          this.projectId!,
          this.reportId!,
          this.forecastId!,
        )
        .subscribe({
          next: (forecast: DraftForecast) => {
            this.reportName = `${forecast.reportPeriod} Forecast`;

            const formValue = new ForecastForm({
              budgetForecast: {
                yearForecasts: forecast.forecastItems,
                totalProjectedExpenditure: forecast.total,
                originalForecast: forecast.originalForecast,
              },
              attachments: {
                attachments: forecast.attachments,
              },
              declaration: {
                authorizedRepresentative: forecast.authorizedRepresentative,
              },
            });

            this.forecastForm = this.formBuilder.formGroup(
              ForecastForm,
              formValue,
            ) as IFormGroup<ForecastForm>;

            resolve();
          },
          error: (error) => {
            console.error('Error loading forecast', error);
            reject();
          },
        });
    });
  }

  goBack() {
    this.router.navigate(['drif-projects', this.projectId]);
  }
}
