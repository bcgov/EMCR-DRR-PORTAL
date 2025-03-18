import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { CommonModule } from '@angular/common';
import { Component, inject, ViewChild } from '@angular/core';
import { FormArray, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import {
  MatStepper,
  MatStepperModule,
  StepperOrientation,
} from '@angular/material/stepper';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import {
  IFormGroup,
  RxFormBuilder,
  RxReactiveFormsModule,
} from '@rxweb/reactive-form-validators';
import { ProjectService } from '../../../../../api/project/project.service';
import { DeclarationType, FormType } from '../../../../../model';
import { DrrCurrencyInputComponent } from '../../../../shared/controls/drr-currency-input/drr-currency-input.component';
import { DrrDatepickerComponent } from '../../../../shared/controls/drr-datepicker/drr-datepicker.component';
import { DrrInputComponent } from '../../../../shared/controls/drr-input/drr-input.component';
import { DrrRadioButtonComponent } from '../../../../shared/controls/drr-radio-button/drr-radio-button.component';
import { DrrSelectComponent } from '../../../../shared/controls/drr-select/drr-select.component';
import { DrrTextareaComponent } from '../../../../shared/controls/drr-textarea/drr-textarea.component';
import { OptionsStore } from '../../../../store/options.store';
import { ProfileStore } from '../../../../store/profile.store';
import {
  BudgetForecastForm,
  ForecastAttachmentsForm,
  ForecastDeclarationForm,
  ForecastForm,
  YearForecastForm,
} from '../drif-forecast-form';

@Component({
  selector: 'drr-drif-forecast-create',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RxReactiveFormsModule,
    MatStepperModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatCardModule,
    TranslocoModule,
    MatFormFieldModule,
    MatCheckboxModule,
    MatTableModule,
    MatDividerModule,
    RouterModule,
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
  optionsStore = inject(OptionsStore);
  profileStore = inject(ProfileStore);

  @ViewChild(MatStepper) stepper!: MatStepper;
  stepperOrientation: StepperOrientation = 'horizontal';

  projectId?: string;
  reportId?: string;
  forecastId?: string;

  reportName?: string;

  authorizedRepresentativeText?: string;
  accuracyOfInformationText?: string;

  forecastForm?: IFormGroup<ForecastForm> = this.formBuilder.formGroup(
    ForecastForm,
  ) as IFormGroup<ForecastForm>;
  formChanged = false;

  get budgetForecastForm() {
    return this.forecastForm?.get(
      'budgetForecast',
    ) as IFormGroup<BudgetForecastForm>;
  }

  get attachmentsForm() {
    return this.forecastForm?.get(
      'attachments',
    ) as IFormGroup<ForecastAttachmentsForm>;
  }

  get declarationForm() {
    return this.forecastForm?.get(
      'declaration',
    ) as IFormGroup<ForecastDeclarationForm>;
  }

  ngOnInit() {
    this.route.params.subscribe((params) => {
      this.projectId = params['projectId'];
      this.reportId = params['reportId'];
      this.forecastId = params['forecastId'];
    });

    this.authorizedRepresentativeText = this.optionsStore.getDeclarations?.(
      DeclarationType.AuthorizedRepresentative,
      FormType.Report,
    );

    this.accuracyOfInformationText = this.optionsStore.getDeclarations?.(
      DeclarationType.AccuracyOfInformation,
      FormType.Report,
    );

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

            this.setAuthorizedRepresentative();
          },
          error: (error) => {
            console.error('Error loading forecast', error);
            reject();
          },
        });
    });
  }

  setAuthorizedRepresentative() {
    const profileData = this.profileStore.getProfile();

    const authorizedRepresentativeForm = this.declarationForm.get(
      'authorizedRepresentative',
    );
    if (
      profileData.firstName?.() &&
      !authorizedRepresentativeForm?.value?.firstName
    ) {
      authorizedRepresentativeForm
        ?.get('firstName')
        ?.setValue(profileData.firstName(), { emitEvent: false });
      authorizedRepresentativeForm?.get('firstName')?.disable();
    }
    if (
      profileData.lastName?.() &&
      !authorizedRepresentativeForm?.value?.lastName
    ) {
      authorizedRepresentativeForm
        ?.get('lastName')
        ?.setValue(profileData.lastName(), { emitEvent: false });
      authorizedRepresentativeForm?.get('lastName')?.disable();
    }
    if (profileData.title?.() && !authorizedRepresentativeForm?.value?.title) {
      authorizedRepresentativeForm
        ?.get('title')
        ?.setValue(profileData.title(), {
          emitEvent: false,
        });
    }
    if (
      profileData.department?.() &&
      !authorizedRepresentativeForm?.value?.department
    ) {
      authorizedRepresentativeForm
        ?.get('department')
        ?.setValue(profileData.department(), {
          emitEvent: false,
        });
    }
    if (profileData.phone?.() && !authorizedRepresentativeForm?.value?.phone) {
      authorizedRepresentativeForm
        ?.get('phone')
        ?.setValue(profileData.phone(), {
          emitEvent: false,
        });
    }
    if (profileData.email?.() && !authorizedRepresentativeForm?.value?.email) {
      authorizedRepresentativeForm
        ?.get('email')
        ?.setValue(profileData.email(), {
          emitEvent: false,
        });
    }
  }

  getYearForecastFormArray() {
    return this.budgetForecastForm.get('yearForecasts') as FormArray;
  }

  stepperSelectionChange(event: StepperSelectionEvent) {
    if (event.previouslySelectedIndex === 0) {
      return;
    }

    this.save();

    event.previouslySelectedStep.stepControl.markAllAsTouched();

    if (this.stepperOrientation === 'horizontal') {
      return;
    }

    const stepId = this.stepper._getStepLabelId(event.selectedIndex);
    const stepElement = document.getElementById(stepId);
    if (stepElement) {
      setTimeout(() => {
        stepElement.scrollIntoView({
          block: 'start',
          inline: 'nearest',
          behavior: 'smooth',
        });
      }, 250);
    }
  }

  goBack() {
    this.save();
    this.router.navigate(['drif-projects', this.projectId]);
  }

  getFormValue() {}

  save() {
    if (!this.formChanged) {
      return;
    }

    const forecastFormValue = this.getFormValue();

    // this.lastSavedAt = undefined;

    // update forecast
    // .subscribe({
    //   next: () => {
    //     this.lastSavedAt = new Date();

    //     this.toastService.close();
    //     this.toastService.success('Claim saved successfully');

    //     this.formChanged = false;
    //     this.resetAutoSaveTimer();
    //   },
    //   error: (error) => {
    //     this.toastService.close();
    //     this.toastService.error('Failed to save claim');
    //     console.error(error);
    //   },
    // });
  }
}
