import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { CommonModule } from '@angular/common';
import {
  Component,
  HostListener,
  inject,
  QueryList,
  ViewChild,
  ViewChildren,
} from '@angular/core';
import {
  AbstractControl,
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
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { HotToastService } from '@ngxpert/hot-toast';
import {
  IFormGroup,
  RxFormBuilder,
  RxFormGroup,
  RxReactiveFormsModule,
} from '@rxweb/reactive-form-validators';
import { distinctUntilChanged, pairwise, startWith, Subscription } from 'rxjs';
import { v4 as uuidv4 } from 'uuid';
import { AttachmentService } from '../../../../../api/attachment/attachment.service';
import { ProjectService } from '../../../../../api/project/project.service';
import {
  ActivityType,
  ApplicationType,
  DeclarationType,
  Delay,
  DocumentType,
  DraftProgressReport,
  FormType,
  InterimProjectType,
  ProgressReport,
  ProjectProgressStatus,
  SignageType,
  WorkplanActivity,
  WorkplanStatus,
  YesNoOption,
} from '../../../../../model';
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
import { AuthorizedRepresentativeForm } from '../../../../shared/drr-auth-rep/auth-rep-form';
import { DrrAuthRepComponent } from '../../../../shared/drr-auth-rep/drr-auth-rep.component';
import { DeclarationForm } from '../../../../shared/drr-declaration/drr-declaration-form';
import { DrrDeclarationComponent } from '../../../../shared/drr-declaration/drr-declaration.component';
import {
  FileService,
  RecordType,
} from '../../../../shared/services/file.service';
import { OptionsStore } from '../../../../store/options.store';
import { ProfileStore } from '../../../../store/profile.store';
import { AttachmentForm } from '../../../drif-fp/drif-fp-form';
import { DrrAttahcmentComponent } from '../../../drif-fp/drif-fp-step-11/drif-fp-attachment.component';
import {
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

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-drif-progress-report-create',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RxReactiveFormsModule,
    MatStepperModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatIconModule,
    MatButtonModule,
    MatCardModule,
    MatDividerModule,
    TranslocoModule,
    DrrDatepickerComponent,
    DrrInputComponent,
    DrrNumericInputComponent,
    DrrSelectComponent,
    DrrRadioButtonComponent,
    DrrTextareaComponent,
    DrrAttahcmentComponent,
    DrrFileUploadComponent,
    DrifProgressReportSummaryComponent,
    DrrAuthRepComponent,
    DrrDeclarationComponent,
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

  reportName?: string;

  projectType?: InterimProjectType;

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

  private commonActivityOptions: DrrSelectOption[] = Object.values(ActivityType)
    .filter(
      (activity) =>
        activity === ActivityType.Administration ||
        activity === ActivityType.ProjectPlanning ||
        activity === ActivityType.Assessment ||
        activity === ActivityType.Mapping ||
        activity === ActivityType.LandAcquisition ||
        activity === ActivityType.ApprovalsPermitting ||
        activity === ActivityType.Communications ||
        activity === ActivityType.AffectedPartiesEngagement ||
        activity === ActivityType.CommunityEngagement,
    )
    .map((activity) => ({
      value: activity,
      label: this.translocoService.translate(`activityType.${activity}`),
    }));

  private nonStructuralActivities: ActivityType[] = [
    ActivityType.Project,
    ActivityType.FirstNationsEngagement,
  ];
  private nonStructuralActivityOptions: DrrSelectOption[] = [
    ...this.commonActivityOptions,
    ...this.nonStructuralActivities.map((activity) => ({
      value: activity,
      label: this.translocoService.translate(`activityType.${activity}`),
    })),
  ];

  private structuralActivities: ActivityType[] = [
    ActivityType.Project,
    ActivityType.FirstNationsEngagement,
    ActivityType.Design,
    ActivityType.ConstructionTender,
    ActivityType.Construction,
    ActivityType.ConstructionContractAward,
    ActivityType.PermitToConstruct,
  ];
  private structuralActivityOptions: DrrSelectOption[] = [
    ...this.commonActivityOptions,
    ...this.structuralActivities.map((activity) => ({
      value: activity,
      label: this.translocoService.translate(`activityType.${activity}`),
    })),
  ];

  optionalActivityStatusOptions: DrrSelectOption[] = [];
  necessaryActivityStatusOptions: DrrRadioOption[] = [];
  milestoneStatusOptions: DrrSelectOption[] = [];

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

  projectProgressOptions: DrrSelectOption[] = [];

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

  @ViewChildren('futureEventNameInput')
  futureEventNameInputs!: QueryList<DrrInputComponent>;

  @ViewChildren('pastEventNameInput')
  pastEventNameInputs!: QueryList<DrrInputComponent>;

  ngOnInit() {
    this.route.params.subscribe((params) => {
      this.projectId = params['projectId'];
      this.reportId = params['reportId'];
      this.progressReportId = params['progressReportId'];

      this.translocoService
        .selectTranslateObject('projectProgress')
        .subscribe((translations) => {
          this.projectProgressOptions = Object.keys(ProjectProgressStatus).map(
            (key) => ({
              label: translations[`${key}`] || key,
              value: key,
            }),
          );

          this.optionalActivityStatusOptions = Object.values(WorkplanStatus)
            .filter(
              (s) =>
                s !== WorkplanStatus.NotAwarded && s !== WorkplanStatus.Awarded,
            )
            .map((value) => ({
              label: this.translocoService.translate(`workplanStatus.${value}`),
              value,
            }));

          this.necessaryActivityStatusOptions =
            this.optionalActivityStatusOptions.filter(
              (option) => option.value !== WorkplanStatus.NoLongerNeeded,
            );

          this.milestoneStatusOptions = Object.values(WorkplanStatus)
            .filter(
              (s) =>
                s === WorkplanStatus.NotAwarded || s === WorkplanStatus.Awarded,
            )
            .map((value) => ({
              label: this.translocoService.translate(`workplanStatus.${value}`),
              value,
            }));
        });

      this.authorizedRepresentativeText = this.optionsStore.getDeclarations?.(
        DeclarationType.AuthorizedRepresentative,
        FormType.Report,
        ApplicationType.Progress,
      );

      this.accuracyOfInformationText = this.optionsStore.getDeclarations?.(
        DeclarationType.AccuracyOfInformation,
        FormType.Report,
        ApplicationType.Progress,
      );

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

  ngOnDestroy() {
    clearInterval(this.autoSaveTimer);
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
          next: (report: DraftProgressReport) => {
            this.reportName = `${report.reportPeriod} Progress`;

            this.progressReportForm = this.formBuilder.formGroup(
              new ProgressReportForm({
                workplan: report.workplan,
                eventInformation: report.eventInformation,
                attachments: report.attachments,
                declaration: {
                  authorizedRepresentativeStatement: false,
                  informationAccuracyStatement: false,
                  authorizedRepresentative: report.authorizedRepresentative,
                },
                projectType: report.projectType,
              }),
            ) as IFormGroup<ProgressReportForm>;

            this.projectType = report.projectType;

            this.setAuthorizedRepresentative();

            this.getActivitiesFormArray().controls.forEach((control) => {
              this.setValidationsForActivity(control);
              control.get('status')?.updateValueAndValidity();
            });

            if (report.workplan?.fundingSourcesChanged) {
              this.workplanForm
                .get('fundingSourcesChangedComment')
                ?.setValidators(Validators.required);
            }
            this.workplanForm
              .get('fundingSourcesChanged')
              ?.valueChanges.subscribe((value) => {
                const comment = this.workplanForm.get(
                  'fundingSourcesChangedComment',
                );
                value
                  ? comment?.addValidators(Validators.required)
                  : [
                      comment?.removeValidators(Validators.required),
                      comment?.reset(),
                    ];
                comment?.updateValueAndValidity();
              });

            if (report.workplan?.outstandingIssues) {
              this.workplanForm
                .get('outstandingIssuesComments')
                ?.setValidators(Validators.required);
            }

            this.workplanForm
              .get('outstandingIssues')
              ?.valueChanges.subscribe((value) => {
                const comment = this.workplanForm.get(
                  'outstandingIssuesComments',
                );
                value
                  ? comment?.addValidators(Validators.required)
                  : [
                      comment?.removeValidators(Validators.required),
                      comment?.reset(),
                    ];
                comment?.updateValueAndValidity();
              });

            if (report.workplan?.mediaAnnouncement) {
              this.workplanForm
                .get('mediaAnnouncementDate')
                ?.setValidators(Validators.required);
              this.workplanForm
                .get('mediaAnnouncementComment')
                ?.setValidators(Validators.required);
            }

            this.workplanForm
              .get('mediaAnnouncement')
              ?.valueChanges.subscribe((value) => {
                const date = this.workplanForm.get('mediaAnnouncementDate');
                const comment = this.workplanForm.get(
                  'mediaAnnouncementComment',
                );

                if (value) {
                  date?.addValidators(Validators.required);
                  comment?.addValidators(Validators.required);
                } else {
                  date?.clearValidators();
                  date?.setValue(null, { emitEvent: false });
                  comment?.clearValidators();
                  comment?.setValue(null, { emitEvent: false });
                }

                date?.updateValueAndValidity();
                comment?.updateValueAndValidity();
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

            this.workplanForm
              .get('projectProgress')
              ?.valueChanges.subscribe((value) => {
                const delayReason = this.workplanForm.get('delayReason');
                const otherDelayReason =
                  this.workplanForm.get('otherDelayReason');
                const behindScheduleMitigatingComments = this.workplanForm.get(
                  'behindScheduleMitigatingComments',
                );
                const aheadOfScheduleComments = this.workplanForm.get(
                  'aheadOfScheduleComments',
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

            if (this.isStructuralProject()) {
              const constructionCompletionPercentageControl =
                this.workplanForm?.get('constructionCompletionPercentage');
              constructionCompletionPercentageControl?.addValidators(
                Validators.required,
              );
              constructionCompletionPercentageControl?.updateValueAndValidity();

              const signageRequiredControl =
                this.workplanForm?.get('signageRequired');
              signageRequiredControl?.addValidators(Validators.required);
              signageRequiredControl?.updateValueAndValidity();

              if (!report?.workplan?.signageRequired) {
                this.getSignageFormArray().clear();
                this.workplanForm
                  .get('signageNotRequiredComments')
                  ?.setValidators(Validators.required);
              }

              this.workplanForm
                .get('signageRequired')
                ?.valueChanges.subscribe((value) => {
                  const signageNotRequiredComments =
                    this.progressReportForm.get('signageNotRequiredComments');

                  if (value) {
                    signageNotRequiredComments?.clearValidators();
                    signageNotRequiredComments?.reset();
                    this.addSignage();
                  } else {
                    signageNotRequiredComments?.addValidators(
                      Validators.required,
                    );
                    this.getSignageFormArray()?.clear();
                  }

                  signageNotRequiredComments?.updateValueAndValidity();
                });
            }

            resolve();
          },
          error: () => {
            reject();
          },
        });
    });
  }

  setAuthorizedRepresentative() {
    const profileData = this.profileStore.getProfile();

    const authorizedRepresentativeForm = this.progressReportForm.get(
      'declaration.authorizedRepresentative',
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
    // log all invalid controls form
    // const workplanGroup = this.progressReportForm?.get(
    //   'workplan',
    // ) as RxFormGroup;
    // workplanGroup.markAllAsTouched();
    // Object.keys(workplanGroup.controls).forEach((key) => {
    //   const control = workplanGroup.get(key);
    //   if (control?.invalid) {
    //     console.log(key, control.errors);
    //   }
    // });

    if (!this.formChanged) {
      return;
    }

    this.lastSavedAt = undefined;

    this.projectService
      .projectUpdateProgressReport(
        this.projectId,
        this.reportId,
        this.progressReportId,
        this.getFormValue(),
      )
      .subscribe({
        next: () => {
          this.lastSavedAt = new Date();

          this.toastService.close();
          this.toastService.success('Report saved successfully');

          this.formChanged = false;
          this.resetAutoSaveTimer();
        },
        error: (error) => {
          this.toastService.close();
          this.toastService.error('Failed to save report');
          console.error(error);
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
        this.getFormValue(),
      )
      .subscribe({
        next: (response) => {
          this.toastService.close();
          this.toastService.success('Your report has been received.');

          this.router.navigate(['drif-projects', this.projectId]);
        },
        error: (error) => {
          this.toastService.close();
          this.toastService.error('Failed to submit report');
          console.error(error);
        },
      });
  }

  getFormValue(): ProgressReport {
    const rawValue = this.progressReportForm.getRawValue();
    rawValue.workplan.workplanActivities =
      rawValue.workplan.workplanActivities.filter(
        (activity: WorkplanActivity) => {
          return activity.activity !== null && activity.activity !== undefined;
        },
      );

    return {
      workplan: rawValue.workplan,
      eventInformation: rawValue.eventInformation,
      attachments: rawValue.attachments,
      authorizedRepresentative: rawValue.declaration.authorizedRepresentative,
      authorizedRepresentativeStatement:
        rawValue.declaration.authorizedRepresentativeStatement,
      informationAccuracyStatement:
        rawValue.declaration.informationAccuracyStatement,
    };
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
        id: uuidv4(),
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
          actualStartDate?.setValue(null);
          actualCompletionDate?.clearValidators();
          actualCompletionDate?.setValue(null);
          break;
        case WorkplanStatus.InProgress:
          actualStartDate?.addValidators(Validators.required);
          plannedCompletionDate?.addValidators(Validators.required);

          plannedStartDate?.clearValidators();
          plannedStartDate?.setValue(null);
          actualCompletionDate?.clearValidators();
          actualCompletionDate?.setValue(null);
          break;
        case WorkplanStatus.Completed:
          actualStartDate?.addValidators(Validators.required);
          actualCompletionDate?.addValidators(Validators.required);

          plannedStartDate?.clearValidators();
          plannedStartDate?.setValue(null);
          plannedCompletionDate?.clearValidators();
          plannedCompletionDate?.setValue(null);
          break;
        case WorkplanStatus.Awarded:
          actualStartDate?.addValidators(Validators.required);

          plannedStartDate?.clearValidators();
          plannedStartDate?.setValue(null);
          plannedCompletionDate?.clearValidators();
          plannedCompletionDate?.setValue(null);
          actualCompletionDate?.clearValidators();
          actualCompletionDate?.setValue(null);
          break;
        case WorkplanStatus.NotAwarded:
          plannedStartDate?.addValidators(Validators.required);

          plannedCompletionDate?.clearValidators();
          plannedCompletionDate?.setValue(null);
          actualStartDate?.clearValidators();
          actualStartDate?.setValue(null);
          actualCompletionDate?.clearValidators();
          actualCompletionDate?.setValue(null);
          break;
        default:
          plannedStartDate?.clearValidators();
          plannedStartDate?.setValue(null);
          plannedCompletionDate?.clearValidators();
          plannedCompletionDate?.setValue(null);
          actualStartDate?.clearValidators();
          actualStartDate?.setValue(null);
          actualCompletionDate?.clearValidators();
          actualCompletionDate?.setValue(null);
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

  removeAdditionalActivity(id: string) {
    if (!id) {
      return;
    }

    const index = this.workplanActivitiesArray?.controls.findIndex(
      (control) => control.get('id')?.value === id,
    );

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

    const availableOptions = this.getAvailableOptionsFundingStream().filter(
      (option) => !selectedActivities.includes(option.value),
    );

    if (selectedActivity) {
      const selectedActivityOption =
        this.getAvailableOptionsFundingStream().find(
          (option) => option.value === selectedActivity,
        );

      availableOptions.push(selectedActivityOption!);
      availableOptions.sort((a, b) => a.label.localeCompare(b.label));
    }

    return availableOptions;
  }

  private getAvailableOptionsFundingStream() {
    return this.isStructuralProject()
      ? this.structuralActivityOptions
      : this.nonStructuralActivityOptions;
  }

  hasAvailableOptionsForActivity() {
    const selectedActivities = this.getActivitiesFormArray()?.controls.map(
      (control) => control.get('activity')?.value,
    );

    return (
      this.getAvailableOptionsFundingStream().length !==
      selectedActivities.length
    );
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
    return this.projectType === InterimProjectType.Stream2;
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

    this.focusOnLastPastEvent();
  }

  focusOnLastPastEvent() {
    setTimeout(() => {
      const lastEventInput = this.pastEventNameInputs.last;
      if (lastEventInput) {
        lastEventInput.focusOnInput();
      }
    }, 0);
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

    this.focusOnLastFutureEvent();
  }

  focusOnLastFutureEvent() {
    setTimeout(() => {
      const lastEventInput = this.futureEventNameInputs.last;
      if (lastEventInput) {
        lastEventInput.focusOnInput();
      }
    }, 0);
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

      this.attachmentsService
        .attachmentUploadAttachment({
          RecordId: this.progressReportId,
          RecordType: RecordType.ProgressReport,
          DocumentType: DocumentType.ProgressReport,

          ContentType:
            file.type === ''
              ? this.fileService.getCustomContentType(file)
              : file.type,
          File: file,
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
        error: (error) => {
          this.toastService.close();
          this.toastService.error('File deletion failed');
          console.error(error);
        },
      });
  }

  get declarationForm() {
    return this.progressReportForm.get(
      'declaration',
    ) as IFormGroup<DeclarationForm>;
  }

  get authorizedRepresentativeForm() {
    return this.declarationForm.get(
      'authorizedRepresentative',
    ) as IFormGroup<AuthorizedRepresentativeForm>;
  }

  getFormGroup(groupName: string) {
    return this.progressReportForm?.get(groupName) as RxFormGroup;
  }
}
