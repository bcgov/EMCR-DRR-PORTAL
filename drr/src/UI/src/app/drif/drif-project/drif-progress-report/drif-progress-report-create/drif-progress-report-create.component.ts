import { CommonModule } from '@angular/common';
import { Component, HostListener, inject, ViewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import {
  MatStepper,
  MatStepperModule,
  StepperOrientation,
} from '@angular/material/stepper';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import {
  IFormGroup,
  RxFormBuilder,
  RxFormGroup,
  RxReactiveFormsModule,
} from '@rxweb/reactive-form-validators';
import {
  ActivityType,
  DeclarationType,
  Delay,
  DocumentType,
  FormType,
  InterimProjectType,
  ProgressReport,
  ProjectProgressStatus,
  RecordType,
  SignageType,
  WorkplanStatus,
  YesNoOption,
} from '../../../../../model';

import { StepperSelectionEvent } from '@angular/cdk/stepper';
import {
  AbstractControl,
  FormArray,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ActivatedRoute, Router } from '@angular/router';
import { HotToastService } from '@ngxpert/hot-toast';
import { distinctUntilChanged, pairwise, startWith, Subscription } from 'rxjs';
import { AttachmentService } from '../../../../../api/attachment/attachment.service';
import { ProjectService } from '../../../../../api/project/project.service';
import { DrrDatepickerComponent } from '../../../../shared/controls/drr-datepicker/drr-datepicker.component';
import { DrrFileUploadComponent } from '../../../../shared/controls/drr-file-upload/drr-file-upload.component';
import { DrrInputComponent } from '../../../../shared/controls/drr-input/drr-input.component';
import { DrrNumericInputComponent } from '../../../../shared/controls/drr-number-input/drr-number-input.component';
import {
  DrrRadioButtonComponent,
  DrrRadioOption,
} from '../../../../shared/controls/drr-radio-button/drr-radio-button.component';
import {
  DrrSelectComponent,
  DrrSelectOption,
} from '../../../../shared/controls/drr-select/drr-select.component';
import { DrrTextareaComponent } from '../../../../shared/controls/drr-textarea/drr-textarea.component';
import { FileService } from '../../../../shared/services/file.service';
import { OptionsStore } from '../../../../store/options.store';
import { ProfileStore } from '../../../../store/profile.store';
import { AttachmentForm } from '../../../drif-fp/drif-fp-form';
import { DrrAttahcmentComponent } from '../../../drif-fp/drif-fp-step-11/drif-fp-attachment.component';
import {
  DeclarationForm,
  EventInformationForm,
  EventProgressType,
  FundingSignageForm,
  PastEventForm,
  ProgressReportForm,
  ProjectEventForm,
  WorkplanActivityForm,
  WorkplanForm,
} from '../drif-progress-report-form';
import { DrifProgressReportSummaryComponent } from '../drif-progress-report-summary/drif-progress-report-summary.component';

@Component({
  selector: 'drr-drif-progress-report-create',
  standalone: true,
  imports: [
    CommonModule,
    MatStepperModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatCardModule,
    MatCheckboxModule,
    MatDividerModule,
    TranslocoModule,
    DrrInputComponent,
    DrrDatepickerComponent,
    DrrInputComponent,
    DrrNumericInputComponent,
    DrrSelectComponent,
    DrrRadioButtonComponent,
    DrrTextareaComponent,
    DrrAttahcmentComponent,
    DrrFileUploadComponent,
    RxReactiveFormsModule,
    DrifProgressReportSummaryComponent,
  ],
  templateUrl: './drif-progress-report-create.component.html',
  styleUrl: './drif-progress-report-create.component.scss',
  providers: [RxFormBuilder],
})
export class DrifProgressReportCreateComponent {
  formBuilder = inject(RxFormBuilder);
  route = inject(ActivatedRoute);
  router = inject(Router);
  projectService = inject(ProjectService);
  translocoService = inject(TranslocoService);
  toastService = inject(HotToastService);
  attachmentsService = inject(AttachmentService);
  fileService = inject(FileService);
  optionsStore = inject(OptionsStore);
  profileStore = inject(ProfileStore);

  projectId!: string;
  reportId!: string;
  progressReportId!: string;

  @ViewChild(MatStepper) stepper!: MatStepper;
  stepperOrientation: StepperOrientation = 'horizontal';
  private formToStepMap: Record<string, string> = {
    workplan: 'Step 2',
    eventInformation: 'Step 3',
    attachments: 'Step 4',
    declaration: 'Step 5',
  };

  progressReportForm = this.formBuilder.formGroup(
    ProgressReportForm,
  ) as IFormGroup<ProgressReportForm>;
  formChanged = false;
  lastSavedAt?: Date;

  authorizedRepresentativeText?: string;
  accuracyOfInformationText?: string;

  private allActivityTypeOptions: DrrSelectOption[] = Object.values(
    ActivityType,
  ).map((activity) => ({
    value: activity,
    label: this.translocoService.translate(`activityType.${activity}`),
  }));

  optionalActivityStatusOptions: DrrSelectOption[] = Object.values(
    WorkplanStatus,
  )
    .filter(
      (s) => s !== WorkplanStatus.NotAwarded && s !== WorkplanStatus.Awarded,
    )
    .map((value) => ({
      label: this.translocoService.translate(`workplanStatus.${value}`),
      value,
    }));

  necessaryActivityStatusOptions: DrrRadioOption[] =
    this.optionalActivityStatusOptions.filter(
      (option) => option.value !== WorkplanStatus.NoLongerNeeded,
    );

  milestoneStatusOptions: DrrSelectOption[] = Object.values(WorkplanStatus)
    .filter(
      (s) => s === WorkplanStatus.NotAwarded || s === WorkplanStatus.Awarded,
    )
    .map((value) => ({
      label: this.translocoService.translate(`workplanStatus.${value}`),
      value,
    }));

  yesNoRadioOptions: DrrRadioOption[] = [
    {
      label: 'Yes',
      value: true,
    },
    {
      label: 'No',
      value: false,
    },
  ];

  yesNoSelectOptions: DrrSelectOption[] = [
    {
      label: 'Yes',
      value: YesNoOption.Yes,
    },
    {
      label: 'No',
      value: YesNoOption.No,
    },
  ];

  eventProgressOptions: DrrRadioOption[] = Object.values(EventProgressType).map(
    (value) => ({
      label: value,
      value,
    }),
  );

  projectProgressOptions = Object.keys(ProjectProgressStatus).map((key) => ({
    label: this.translocoService.translate(`projectProgress.${key}`),
    value: key,
  }));

  delayReasonOptions: DrrSelectOption[] = Object.values(Delay).map((value) => ({
    label: this.translocoService.translate(`delayReason.${value}`),
    value,
  }));

  signageTypeOptions: DrrSelectOption[] = Object.values(SignageType).map(
    (value) => ({
      label: this.translocoService.translate(`signageType.${value}`),
      value,
    }),
  );

  get workplanForm(): IFormGroup<WorkplanForm> {
    return this.progressReportForm.get('workplan') as IFormGroup<WorkplanForm>;
  }

  get workplanActivitiesArray(): FormArray {
    return this.workplanForm?.get('workplanActivities') as FormArray;
  }

  get eventInformationForm(): IFormGroup<EventInformationForm> {
    return this.progressReportForm.get(
      'eventInformation',
    ) as IFormGroup<EventInformationForm>;
  }

  autoSaveCountdown = 0;
  autoSaveTimer: any;
  autoSaveInterval = 60;

  @HostListener('window:mousemove')
  @HostListener('window:mousedown')
  @HostListener('window:keypress')
  @HostListener('window:scroll')
  @HostListener('window:touchmove')
  resetAutoSaveTimer() {
    if (!this.formChanged) {
      this.autoSaveCountdown = 0;
      clearInterval(this.autoSaveTimer);
      return;
    }

    this.autoSaveCountdown = this.autoSaveInterval;
    clearInterval(this.autoSaveTimer);
    this.autoSaveTimer = setInterval(() => {
      this.autoSaveCountdown -= 1;
      if (this.autoSaveCountdown === 0) {
        this.save();
        clearInterval(this.autoSaveTimer);
      }
    }, 1000);
  }

  ngOnInit() {
    this.route.params.subscribe((params) => {
      this.projectId = params['projectId'];
      this.reportId = params['reportId'];
      this.progressReportId = params['progressReportId'];

      this.authorizedRepresentativeText = this.optionsStore.getDeclarations?.(
        DeclarationType.AuthorizedRepresentative,
        FormType.Report,
      );

      this.accuracyOfInformationText = this.optionsStore.getDeclarations?.(
        DeclarationType.AccuracyOfInformation,
        FormType.Report,
      );

      const profileData = this.profileStore.getProfile();

      const submitterForm = this.progressReportForm.get(
        'declaration.submitter',
      );
      if (profileData.firstName?.()) {
        submitterForm
          ?.get('firstName')
          ?.setValue(profileData.firstName(), { emitEvent: false });
        submitterForm?.get('firstName')?.disable();
      }
      if (profileData.lastName?.()) {
        submitterForm
          ?.get('lastName')
          ?.setValue(profileData.lastName(), { emitEvent: false });
        submitterForm?.get('lastName')?.disable();
      }
      if (profileData.title?.()) {
        submitterForm?.get('title')?.setValue(profileData.title(), {
          emitEvent: false,
        });
      }
      if (profileData.department?.()) {
        submitterForm?.get('department')?.setValue(profileData.department(), {
          emitEvent: false,
        });
      }
      if (profileData.phone?.()) {
        submitterForm?.get('phone')?.setValue(profileData.phone(), {
          emitEvent: false,
        });
      }
      if (profileData.email?.()) {
        submitterForm?.get('email')?.setValue(profileData.email(), {
          emitEvent: false,
        });
      }

      this.load().then(() => {
        this.formChanged = false;
        setTimeout(() => {
          this.progressReportForm.valueChanges
            .pipe(
              startWith(this.progressReportForm.value),
              pairwise(),
              distinctUntilChanged((a, b) => {
                // compare objects but ignore declaration changes
                delete a[1].declaration.authorizedRepresentativeStatement;
                delete a[1].declaration.informationAccuracyStatement;
                delete b[1].declaration.authorizedRepresentativeStatement;
                delete b[1].declaration.informationAccuracyStatement;

                return JSON.stringify(a[1]) == JSON.stringify(b[1]);
              }),
            )
            .subscribe(([prev, curr]) => {
              if (
                prev.declaration.authorizedRepresentativeStatement !==
                  curr.declaration.authorizedRepresentativeStatement ||
                prev.declaration.informationAccuracyStatement !==
                  curr.declaration.informationAccuracyStatement
              ) {
                return;
              }

              this.progressReportForm
                ?.get('declaration.authorizedRepresentativeStatement')
                ?.reset();

              this.progressReportForm
                ?.get('declaration.informationAccuracyStatement')
                ?.reset();

              this.formChanged = true;
              this.resetAutoSaveTimer();
            });
        }, 1000);
      });
    });
  }

  load(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.projectService
        .projectGetProgressReport(
          this.projectId,
          this.reportId,
          this.progressReportId,
        )
        .subscribe({
          next: (report: ProgressReport) => {
            report.workplan?.workplanActivities?.map((activity) => {
              const activityForm = this.formBuilder.formGroup(
                new WorkplanActivityForm(activity),
              );

              this.setValidationsForActivity(activityForm);
              this.workplanActivitiesArray?.push(activityForm);
            });

            if (
              report?.workplan?.fundingSignage
                ? report?.workplan?.fundingSignage.length > 0
                : false
            ) {
              this.getSignageFormArray().clear();
            }
            report.workplan?.fundingSignage?.map((signage) => {
              const signageForm = this.formBuilder.formGroup(
                new FundingSignageForm(signage),
              );

              this.getSignageFormArray()?.push(signageForm);
            });

            this.progressReportForm
              .get('workplan.fundingSourcesChanged')
              ?.valueChanges.subscribe((value) => {
                const comment = this.progressReportForm.get(
                  'workplan.fundingSourcesChangedComment',
                );

                if (value) {
                  comment?.addValidators(Validators.required);
                } else {
                  comment?.removeValidators(Validators.required);
                }

                comment?.updateValueAndValidity();
              });

            this.progressReportForm
              .get('workplan.outstandingIssues')
              ?.valueChanges.subscribe((value) => {
                const comment = this.progressReportForm.get(
                  'workplan.outstandingIssuesComments',
                );

                if (value) {
                  comment?.addValidators(Validators.required);
                } else {
                  comment?.removeValidators(Validators.required);
                }

                comment?.updateValueAndValidity();
              });

            this.progressReportForm
              .get('workplan.mediaAnnouncement')
              ?.valueChanges.subscribe((value) => {
                const date = this.progressReportForm.get(
                  'workplan.mediaAnnouncementDate',
                );
                const comment = this.progressReportForm.get(
                  'workplan.mediaAnnouncementComment',
                );

                if (value) {
                  date?.addValidators(Validators.required);
                  comment?.addValidators(Validators.required);
                } else {
                  date?.removeValidators(Validators.required);
                  comment?.removeValidators(Validators.required);
                }

                date?.updateValueAndValidity();
                comment?.updateValueAndValidity();
              });

            report.eventInformation?.pastEvents?.map((event) => {
              this.getPastEventsArray()?.push(
                this.formBuilder.formGroup(new PastEventForm(event)),
              );
            });

            report.eventInformation?.upcomingEvents?.map((event) => {
              this.getUpcomingEventsArray()?.push(
                this.formBuilder.formGroup(new ProjectEventForm(event)),
              );
            });

            this.eventInformationForm
              ?.get('eventsOccurredSinceLastReport')
              ?.valueChanges.subscribe((value) => {
                if (value === true && this.getPastEventsArray()?.length === 0) {
                  this.addPastEvent();
                }
                if (value === false) {
                  this.getPastEventsArray()?.clear();
                }
              });

            this.eventInformationForm
              ?.get('anyUpcomingEvents')
              ?.valueChanges.subscribe((value) => {
                if (
                  value === true &&
                  this.getUpcomingEventsArray()?.length === 0
                ) {
                  this.addFutureEvent();
                }
                if (value === false) {
                  this.getUpcomingEventsArray()?.clear();
                }
              });

            report.attachments?.map((attachment) => {
              const attachmentForm = this.formBuilder.formGroup(
                new AttachmentForm(attachment),
              );

              this.getAttachmentsFormArray().push(attachmentForm);
            });

            this.progressReportForm
              .get('workplan.projectProgress')
              ?.valueChanges.subscribe((value) => {
                const delayReason = this.progressReportForm.get(
                  'workplan.delayReason',
                );
                const otherDelayReason = this.progressReportForm.get(
                  'workplan.otherDelayReason',
                );
                const behindScheduleMitigatingComments =
                  this.progressReportForm.get(
                    'workplan.behindScheduleMitigatingComments',
                  );
                const aheadOfScheduleComments = this.progressReportForm.get(
                  'workplan.aheadOfScheduleComments',
                );

                let delayReasonSub: Subscription | undefined;

                if (value === ProjectProgressStatus.BehindSchedule) {
                  delayReason?.addValidators(Validators.required);
                  delayReasonSub = delayReason?.valueChanges.subscribe(
                    (reason) => {
                      if (reason === Delay.Other) {
                        otherDelayReason?.addValidators(Validators.required);
                      } else {
                        otherDelayReason?.removeValidators(Validators.required);
                        otherDelayReason?.reset();
                      }
                    },
                  );
                  behindScheduleMitigatingComments?.addValidators(
                    Validators.required,
                  );
                } else {
                  delayReason?.removeValidators(Validators.required);
                  delayReason?.reset();
                  delayReasonSub?.unsubscribe();
                  behindScheduleMitigatingComments?.removeValidators(
                    Validators.required,
                  );
                  behindScheduleMitigatingComments?.reset();
                }

                if (value === ProjectProgressStatus.AheadOfSchedule) {
                  aheadOfScheduleComments?.addValidators(Validators.required);
                } else {
                  aheadOfScheduleComments?.removeValidators(
                    Validators.required,
                  );
                  aheadOfScheduleComments?.reset();
                }

                delayReason?.updateValueAndValidity();
                otherDelayReason?.updateValueAndValidity();
                behindScheduleMitigatingComments?.updateValueAndValidity();
                aheadOfScheduleComments?.updateValueAndValidity();
              });

            this.progressReportForm.patchValue(report);
            resolve();
          },
          error: () => {
            reject();
          },
        });
    });
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

  save() {
    if (!this.formChanged) {
      return;
    }

    this.lastSavedAt = undefined;

    this.projectService
      .projectUpdateProgressReport(
        this.projectId,
        this.reportId,
        this.progressReportId,
        this.progressReportForm.getRawValue(),
      )
      .subscribe({
        next: () => {
          this.lastSavedAt = new Date();

          this.toastService.close();
          this.toastService.success('Report saved successfully');

          this.formChanged = false;
          this.resetAutoSaveTimer();
        },
        error: () => {
          this.toastService.close();
          this.toastService.error('Failed to save report');
        },
      });
  }

  goBack() {
    this.save();

    this.router.navigate(['drif-projects', this.projectId]);
  }

  submit() {
    this.progressReportForm.markAllAsTouched();
    this.stepper.steps.forEach((step) => step._markAsInteracted());
    this.stepper._stateChanged();

    if (this.progressReportForm.invalid) {
      const invalidSteps = Object.keys(this.progressReportForm.controls)
        .filter((key) => this.progressReportForm.get(key)?.invalid)
        .map((key) => this.formToStepMap[key]);

      const lastStep = invalidSteps.pop();

      const stepsErrorMessage =
        invalidSteps.length > 0
          ? `${invalidSteps.join(', ')} and ${lastStep}`
          : lastStep;

      this.toastService.close();
      this.toastService.error(
        `Please fill all the required fields in ${stepsErrorMessage}.`,
      );

      return;
    }

    this.projectService
      .projectSubmitProgressReport(
        this.projectId,
        this.reportId,
        this.progressReportId,
        this.progressReportForm.getRawValue(),
      )
      .subscribe({
        next: (response) => {
          this.toastService.close();
          this.toastService.success('Your report has been received.');

          this.goBack();
        },
        error: (error) => {
          this.toastService.close();
          this.toastService.error('Failed to submit report');
        },
      });
  }

  getActivitiesFormArray() {
    return this.workplanForm?.get('workplanActivities') as FormArray;
  }

  getPreDefinedActivitiesArray() {
    return this.workplanActivitiesArray?.controls.filter(
      (control) =>
        control.get('preCreatedActivity')?.value &&
        control.get('activity')?.value !== ActivityType.PermitToConstruct &&
        control.get('activity')?.value !==
          ActivityType.ConstructionContractAward,
    );
  }

  getMilestoneActivitiesArray() {
    return this.workplanActivitiesArray?.controls.filter(
      (control) =>
        control.get('preCreatedActivity')?.value &&
        (control.get('activity')?.value === ActivityType.PermitToConstruct ||
          control.get('activity')?.value ===
            ActivityType.ConstructionContractAward),
    );
  }

  getPreDefinedActivityStatusOptions(preDefinedActivity: ActivityType) {
    if (
      preDefinedActivity === ActivityType.ConstructionContractAward ||
      preDefinedActivity === ActivityType.PermitToConstruct
    ) {
      return this.milestoneStatusOptions;
    }

    return this.necessaryActivityStatusOptions;
  }

  getAdditionalActivitiesArray() {
    return this.workplanActivitiesArray?.controls
      .filter((control) => !control.get('preCreatedActivity')?.value)
      .sort((a, b) => {
        const aMandatory = a.get('isMandatory')?.value;
        const bMandatory = b.get('isMandatory')?.value;

        if (aMandatory && !bMandatory) {
          return -1;
        }

        if (!aMandatory && bMandatory) {
          return 1;
        }

        return 0;
      });
  }

  addAdditionalActivity() {
    const newActivity = this.formBuilder.formGroup(
      new WorkplanActivityForm({
        isMandatory: false,
      }),
    );
    this.setValidationsForActivity(newActivity);
    this.workplanActivitiesArray?.push(newActivity);
  }

  setValidationsForActivity(activityControl: AbstractControl) {
    activityControl.get('status')?.valueChanges.subscribe((value) => {
      const plannedStartDate = activityControl.get('plannedStartDate');
      const plannedCompletionDate = activityControl.get(
        'plannedCompletionDate',
      );
      const actualStartDate = activityControl.get('actualStartDate');
      const actualCompletionDate = activityControl.get('actualCompletionDate');

      switch (value) {
        case WorkplanStatus.NotStarted:
          plannedStartDate?.addValidators(Validators.required);
          plannedCompletionDate?.addValidators(Validators.required);
          actualStartDate?.clearValidators();
          actualCompletionDate?.clearValidators();
          break;
        case WorkplanStatus.InProgress:
          plannedCompletionDate?.addValidators(Validators.required);
          actualStartDate?.addValidators(Validators.required);
          plannedStartDate?.clearValidators();
          actualCompletionDate?.clearValidators();
          break;
        case WorkplanStatus.Completed:
          plannedStartDate?.addValidators(Validators.required);
          plannedCompletionDate?.addValidators(Validators.required);
          actualStartDate?.addValidators(Validators.required);
          actualCompletionDate?.addValidators(Validators.required);
          break;
        case WorkplanStatus.Awarded:
          actualStartDate?.addValidators(Validators.required);
          plannedStartDate?.clearValidators();
          plannedCompletionDate?.clearValidators();
          actualCompletionDate?.clearValidators();
          break;
        case WorkplanStatus.NotAwarded:
          plannedStartDate?.addValidators(Validators.required);
          plannedCompletionDate?.clearValidators();
          actualStartDate?.clearValidators();
          actualCompletionDate?.clearValidators();
          break;
        default:
          plannedStartDate?.clearValidators();
          plannedCompletionDate?.clearValidators();
          actualStartDate?.clearValidators();
          actualCompletionDate?.clearValidators();
          break;
      }

      plannedStartDate?.updateValueAndValidity();
      plannedCompletionDate?.updateValueAndValidity();
      actualStartDate?.updateValueAndValidity();
      actualCompletionDate?.updateValueAndValidity();
    });
  }

  getAdditionalActivityOptions(activityControl: AbstractControl) {
    if (!this.isAdditionalActivityMandatory(activityControl)) {
      return this.necessaryActivityStatusOptions;
    }

    return this.optionalActivityStatusOptions;
  }

  isAdditionalActivityMandatory(activityControl: AbstractControl) {
    return !!activityControl.get('isMandatory')?.value;
  }

  removeAdditionalActivity(index: number) {
    this.workplanActivitiesArray?.removeAt(index);
  }

  showPlannedStartDate(activityControl: AbstractControl<WorkplanActivityForm>) {
    const status = activityControl?.get('status')?.value as WorkplanStatus;
    return (
      status === WorkplanStatus.NotStarted ||
      status === WorkplanStatus.NotAwarded
    );
  }

  showPlannedCompletionDate(
    activityControl: AbstractControl<WorkplanActivityForm>,
  ) {
    const status = activityControl?.get('status')?.value as WorkplanStatus;
    return (
      status === WorkplanStatus.NotStarted ||
      status === WorkplanStatus.InProgress
    );
  }

  showActualStartDate(activityControl: AbstractControl<WorkplanActivityForm>) {
    const status = activityControl?.get('status')?.value as WorkplanStatus;
    return (
      status === WorkplanStatus.InProgress ||
      status === WorkplanStatus.Completed ||
      status === WorkplanStatus.Awarded
    );
  }

  showActualCompletionDate(
    activityControl: AbstractControl<WorkplanActivityForm>,
  ) {
    const status = activityControl?.get('status')?.value as WorkplanStatus;
    return status === WorkplanStatus.Completed;
  }

  getAvailableOptionsForActivity(selectedActivity: ActivityType) {
    const selectedActivities = this.getActivitiesFormArray()?.controls.map(
      (control) => control.get('activity')?.value,
    );

    const availableOptions = this.allActivityTypeOptions.filter(
      (option) => !selectedActivities.includes(option.value),
    );

    if (selectedActivity) {
      const selectedActivityOption = this.allActivityTypeOptions.find(
        (option) => option.value === selectedActivity,
      );

      availableOptions.push(selectedActivityOption!);
      availableOptions.sort((a, b) => a.label.localeCompare(b.label));
    }

    return availableOptions;
  }

  hasAvailableOptionsForActivity() {
    const selectedActivities = this.getActivitiesFormArray()?.controls.map(
      (control) => control.get('activity')?.value,
    );

    return this.allActivityTypeOptions.length !== selectedActivities.length;
  }

  isProjectDelayed() {
    return (
      this.workplanForm?.get('projectProgress')?.value ===
      ProjectProgressStatus.BehindSchedule
    );
  }

  isProjectAhead() {
    return (
      this.workplanForm?.get('projectProgress')?.value ===
      ProjectProgressStatus.AheadOfSchedule
    );
  }

  isOtherDelayReasonSelected() {
    return this.workplanForm?.get('delayReason')?.value === Delay.Other;
  }

  isStructuralProject() {
    return (
      this.progressReportForm.get('projectType')?.value ===
      InterimProjectType.Stream2
    );
  }

  getSignageFormArray() {
    return this.workplanForm?.get('fundingSignage') as FormArray;
  }

  addSignage() {
    this.getSignageFormArray()?.push(
      this.formBuilder.formGroup(new FundingSignageForm({})),
    );
  }

  removeSignage(id: string) {
    const index = this.getSignageFormArray()?.controls.findIndex(
      (control) => control.get('id')?.value === id,
    );

    this.getSignageFormArray()?.removeAt(index!);
  }

  getPastEventsArray() {
    return this.eventInformationForm?.get('pastEvents') as FormArray;
  }

  addPastEvent() {
    this.getPastEventsArray()?.push(
      this.formBuilder.formGroup(new PastEventForm({})),
    );
  }

  removePastEvent(index: number) {
    this.getPastEventsArray()?.removeAt(index);
  }

  getUpcomingEventsArray() {
    return this.eventInformationForm?.get('upcomingEvents') as FormArray;
  }

  addFutureEvent() {
    this.getUpcomingEventsArray()?.push(
      this.formBuilder.formGroup(new ProjectEventForm({})),
    );
  }

  removeFutureEvent(index: number) {
    this.getUpcomingEventsArray()?.removeAt(index);
  }

  getAttachmentsFormArray(): FormArray {
    return this.progressReportForm.get('attachments') as FormArray;
  }

  async uploadFiles(files: File[]) {
    files.forEach(async (file) => {
      if (file == null) {
        return;
      }

      const base64Content = await this.fileService.fileToBase64(file);

      this.attachmentsService
        .attachmentUploadAttachment({
          recordId: this.progressReportId,
          recordType: RecordType.ProgressReport,
          documentType: DocumentType.ProgressReport,
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
            } as AttachmentForm;

            this.getAttachmentsFormArray().push(
              this.formBuilder.formGroup(AttachmentForm, attachmentFormData),
            );
          },
          error: () => {
            this.toastService.close();
            this.toastService.error('File upload failed');
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
        recordId: this.progressReportId,
        id: fileId,
      })
      .subscribe({
        next: () => {
          const attachmentsArray = this.progressReportForm.get(
            'attachments',
          ) as FormArray;
          const fileIndex = attachmentsArray.controls.findIndex(
            (control) => control.value.id === fileId,
          );

          const documentType = attachmentsArray.controls[fileIndex].value
            .documentType as DocumentType;

          attachmentsArray.removeAt(fileIndex);
        },
        error: () => {
          this.toastService.close();
          this.toastService.error('File deletion failed');
        },
      });
  }

  getDelcarationForm() {
    return this.progressReportForm.get(
      'declaration',
    ) as IFormGroup<DeclarationForm>;
  }

  getFormGroup(groupName: string) {
    return this.progressReportForm?.get(groupName) as RxFormGroup;
  }
}
