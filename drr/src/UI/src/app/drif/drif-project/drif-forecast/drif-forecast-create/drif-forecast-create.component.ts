import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormArray } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import {
  MatStepperModule,
  StepperOrientation,
} from '@angular/material/stepper';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { ProjectService } from '../../../../../api/project/project.service';
import { DrrCurrencyInputComponent } from '../../../../shared/controls/drr-currency-input/drr-currency-input.component';
import { DrrDatepickerComponent } from '../../../../shared/controls/drr-datepicker/drr-datepicker.component';
import { DrrInputComponent } from '../../../../shared/controls/drr-input/drr-input.component';
import { DrrRadioButtonComponent } from '../../../../shared/controls/drr-radio-button/drr-radio-button.component';
import { DrrSelectComponent } from '../../../../shared/controls/drr-select/drr-select.component';
import { DrrTextareaComponent } from '../../../../shared/controls/drr-textarea/drr-textarea.component';
import { ForecastForm, YearForecastForm } from '../drif-forecast-form';

@Component({
  selector: 'drr-drif-forecast-create',
  standalone: true,
  imports: [
    CommonModule,
    MatStepperModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatCardModule,
    TranslocoModule,
    DrrDatepickerComponent,
    DrrInputComponent,
    DrrSelectComponent,
    DrrRadioButtonComponent,
    DrrTextareaComponent,
    DrrCurrencyInputComponent,
  ],
  templateUrl: './drif-forecast-create.component.html',
  styleUrl: './drif-forecast-create.component.scss',
  providers: [RxFormBuilder],
})
export class DrifForecastCreateComponent {
  formBuilder = inject(RxFormBuilder);
  route = inject(ActivatedRoute);
  router = inject(Router);
  projectService = inject(ProjectService);

  stepperOrientation: StepperOrientation = 'horizontal';

  projectId?: string;
  reportId?: string;
  forecastId?: string;

  reportName?: string;

  forecastForm = this.formBuilder.formGroup(
    ForecastForm,
  ) as IFormGroup<ForecastForm>;

  ngOnInit() {
    this.route.params.subscribe((params) => {
      this.projectId = params['projectId'];
      this.reportId = params['reportId'];
      this.forecastId = params['forecastId'];
    });

    this.load().then(() => {
      // TODO: after init logic, auto save, etc
    });

    // TODO: temp add init values
    this.getYearForecastFormArray().controls.push(
      this.formBuilder.formGroup(YearForecastForm, {
        fiscalYear: 2021,
        originalForecast: 1000,
        projectedExpenditure: 900,
        paidClaimsAmount: 800,
        outstandingClaimsAmount: 100,
        remainingClaimsAmount: 100,
      }),
    );
    this.getYearForecastFormArray().controls.push(
      this.formBuilder.formGroup(YearForecastForm, {
        fiscalYear: 2022,
        originalForecast: 2000,
        projectedExpenditure: 1900,
        paidClaimsAmount: 1800,
        outstandingClaimsAmount: 200,
        remainingClaimsAmount: 200,
      }),
    );
    this.getYearForecastFormArray().disable();
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
          next: (forecast) => {
            this.reportName = `${forecast.reportPeriod} Forecast`;
          },
          error: (error) => {
            console.error('Error loading forecast', error);
            reject();
          },
        });
    });
  }

  getYearForecastFormArray() {
    return this.forecastForm.get('yearForecasts') as FormArray;
  }

  stepperSelectionChange(event: any) {}

  goBack() {
    // TODO: save

    this.router.navigate(['drif-projects', this.projectId]);
  }

  save() {}
}
