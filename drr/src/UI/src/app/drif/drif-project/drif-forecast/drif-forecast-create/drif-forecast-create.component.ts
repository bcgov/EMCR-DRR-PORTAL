import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { CommonModule } from '@angular/common';
import { Component, inject, ViewChild } from '@angular/core';
import {
  FormArray,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
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
import { UntilDestroy } from '@ngneat/until-destroy';
import { HotToastService } from '@ngxpert/hot-toast';
import {
  IFormGroup,
  RxFormBuilder,
  RxReactiveFormsModule,
} from '@rxweb/reactive-form-validators';
import { AttachmentService } from '../../../../../api/attachment/attachment.service';
import { ProjectService } from '../../../../../api/project/project.service';
import {
  DeclarationType,
  DocumentType,
  DraftForecast,
  FormType,
} from '../../../../../model';
import { DrrCurrencyInputComponent } from '../../../../shared/controls/drr-currency-input/drr-currency-input.component';
import { DrrDatepickerComponent } from '../../../../shared/controls/drr-datepicker/drr-datepicker.component';
import { DrrFileUploadComponent } from '../../../../shared/controls/drr-file-upload/drr-file-upload.component';
import { DrrInputComponent } from '../../../../shared/controls/drr-input/drr-input.component';
import { DrrRadioButtonComponent } from '../../../../shared/controls/drr-radio-button/drr-radio-button.component';
import { DrrSelectComponent } from '../../../../shared/controls/drr-select/drr-select.component';
import { DrrTextareaComponent } from '../../../../shared/controls/drr-textarea/drr-textarea.component';
import { FileService } from '../../../../shared/services/file.service';
import { OptionsStore } from '../../../../store/options.store';
import { ProfileStore } from '../../../../store/profile.store';
import { DrrAttahcmentComponent } from '../../../drif-fp/drif-fp-step-11/drif-fp-attachment.component';
import {
  BudgetForecastForm,
  ForecastAttachmentsForm,
  ForecastDeclarationForm,
  ForecastForm,
} from '../drif-forecast-form';
import { DrifForecastSummaryComponent } from '../drif-forecast-summary/drif-forecast-summary.component';

@UntilDestroy({ checkProperties: true })
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
    DrrAttahcmentComponent,
    DrrFileUploadComponent,
    DrifForecastSummaryComponent,
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
  fileService = inject(FileService);
  attachmentsService = inject(AttachmentService);
  toastService = inject(HotToastService);

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

  get attachmentsArray() {
    return this.attachmentsForm?.get('attachments') as FormArray;
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

      this.getYearForecastFormArray().controls.forEach((control) => {
        control.get('totalProjectedExpenditure')?.valueChanges.subscribe(() => {
          this.calculateTotalProjectedExpenditure();
        });
      });

      // disable total controls
      this.budgetForecastForm?.get('totalProjectedExpenditure')?.disable();
      this.budgetForecastForm?.get('originalForecast')?.disable();
      this.budgetForecastForm?.get('variance')?.disable();
    });
  }

  calculateTotalProjectedExpenditure() {
    const totalProjectedExpenditure =
      this.getYearForecastFormArray().controls.reduce((total, control) => {
        return total + control.get('totalProjectedExpenditure')?.value;
      }, 0);

    this.budgetForecastForm
      ?.get('totalProjectedExpenditure')
      ?.setValue(totalProjectedExpenditure, { emitEvent: false });

    // calculate variance
    const originalForecast =
      this.budgetForecastForm?.get('originalForecast')?.value;
    const variance = totalProjectedExpenditure - originalForecast;
    this.budgetForecastForm?.get('variance')?.setValue(variance, {
      emitEvent: false,
    });

    const varianceComments = this.budgetForecastForm?.get('varianceComment');

    if (variance != 0) {
      varianceComments?.addValidators(Validators.required);
    } else {
      varianceComments?.clearValidators();
    }
    varianceComments?.updateValueAndValidity();
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

            this.setAuthorizedRepresentative();

            resolve();
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
    if (profileData.firstName?.()) {
      authorizedRepresentativeForm
        ?.get('firstName')
        ?.setValue(profileData.firstName(), { emitEvent: false });
      authorizedRepresentativeForm?.get('firstName')?.disable();
    }
    if (profileData.lastName?.()) {
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

  getFormValue(): DraftForecast {
    const formValue = this.forecastForm?.getRawValue();

    return {
      forecastItems: formValue.budgetForecast.yearForecasts,
      total: formValue.budgetForecast.totalProjectedExpenditure,
      variance: formValue.budgetForecast.variance,
      varianceComment: formValue.budgetForecast.varianceComment,
      attachments: formValue.attachments.attachments,
      authorizedRepresentative: formValue.declaration.authorizedRepresentative,
    };
  }

  save() {
    // TODO: temp
    // if (!this.formChanged) {
    //   return;
    // }

    const forecastFormValue = this.getFormValue();

    // TODO: temp
    console.log('Saving forecast', forecastFormValue);

    // this.lastSavedAt = undefined;

    this.projectService
      .projectUpdateForecastReport(
        this.projectId!,
        this.reportId!,
        this.forecastId!,
        forecastFormValue,
      )
      .subscribe({
        next: () => {
          // this.lastSavedAt = new Date();

          this.toastService.close();
          this.toastService.success('Claim saved successfully');

          this.formChanged = false;
          // this.resetAutoSaveTimer();
        },
        error: (error) => {
          this.toastService.close();
          this.toastService.error('Failed to save claim');
          console.error(error);
        },
      });
  }

  async uploadFiles(files: File[]) {
    files.forEach(async (file) => {
      if (file == null) {
        return;
      }

      const base64Content = await this.fileService.fileToBase64(file);

      this.attachmentsService
        .attachmentUploadAttachment({
          recordId: this.forecastId,
          // TODO: recordType: RecordType.Forecast,
          // TODO: documentType: DocumentType.Forecast,
          name: file.name,
          contentType:
            file.type === ''
              ? this.fileService.getCustomContentType(file)
              : file.type,
          content: base64Content.split(',')[1],
        })
        .subscribe({
          next: (attachment) => {
            const attachmentFormData = {
              name: file.name,
              comments: '',
              id: attachment.id,
              documentType: DocumentType.ProgressReport,
            } as ForecastAttachmentsForm;

            this.attachmentsArray.push(
              this.formBuilder.formGroup(
                ForecastAttachmentsForm,
                attachmentFormData,
              ),
            );
          },
          error: (error) => {
            this.toastService.close();
            this.toastService.error('File upload failed');
            console.error(error);
          },
        });
    });
  }

  downloadFile(fileId: string) {
    this.fileService.downloadFile(fileId);
  }

  removeFile(fileId: string) {
    this.attachmentsService
      .attachmentDeleteAttachment(fileId, {
        recordId: this.forecastId,
        id: fileId,
      })
      .subscribe({
        next: () => {
          const fileIndex = this.attachmentsArray.controls.findIndex(
            (control) => control.value.id === fileId,
          );

          const documentType = this.attachmentsArray.controls[fileIndex].value
            .documentType as DocumentType;

          this.attachmentsArray.removeAt(fileIndex);
        },
        error: (error) => {
          this.toastService.close();
          this.toastService.error('File deletion failed');
          console.error(error);
        },
      });
  }
}
