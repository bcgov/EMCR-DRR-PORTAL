import { BreakpointObserver, LayoutModule } from '@angular/cdk/layout';
import {
  STEPPER_GLOBAL_OPTIONS,
  StepperSelectionEvent,
} from '@angular/cdk/stepper';
import { CommonModule } from '@angular/common';
import { Component, HostListener, ViewChild, inject } from '@angular/core';
import {
  FormArray,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import {
  MatStepper,
  MatStepperModule,
  StepperOrientation,
} from '@angular/material/stepper';
import { ActivatedRoute, Router } from '@angular/router';

import { TranslocoModule } from '@ngneat/transloco';
import {
  IFormGroup,
  RxFormBuilder,
  RxFormGroup,
} from '@rxweb/reactive-form-validators';
import { distinctUntilChanged, pairwise, startWith } from 'rxjs/operators';
import { DrifapplicationService } from '../../../api/drifapplication/drifapplication.service';
import {
  ApplicationResult,
  DraftEoiApplication,
  EoiApplication,
  FundingType,
  Hazards,
} from '../../../model';
import { ProfileStore } from '../../store/profile.store';

import { UntilDestroy } from '@ngneat/until-destroy';
import { HotToastService } from '@ngxpert/hot-toast';
import {
  ContactDetailsForm,
  EOIApplicationForm,
  FundingInformationItemForm,
  InfrastructureImpactedForm,
  StringItem,
} from './drif-eoi-form';
import { DrifEoiStep1Component } from './drif-eoi-step-1/drif-eoi-step-1.component';
import { DrifEoiStep2Component } from './drif-eoi-step-2/drif-eoi-step-2.component';
import { DrifEoiStep3Component } from './drif-eoi-step-3/drif-eoi-step-3.component';
import { DrifEoiStep4Component } from './drif-eoi-step-4/drif-eoi-step-4.component';
import { DrifEoiStep5Component } from './drif-eoi-step-5/drif-eoi-step-5.component';
import { DrifEoiStep6Component } from './drif-eoi-step-6/drif-eoi-step-6.component';
import { DrifEoiStep7Component } from './drif-eoi-step-7/drif-eoi-step-7.component';
import { DrifEoiStep8Component } from './drif-eoi-step-8/drif-eoi-step-8.component';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-drif-eoi',
  standalone: true,
  imports: [
    CommonModule,
    LayoutModule,
    MatButtonModule,
    MatStepperModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatDividerModule,
    MatSelectModule,
    MatDatepickerModule,
    MatCheckboxModule,
    DrifEoiStep1Component,
    DrifEoiStep2Component,
    DrifEoiStep3Component,
    DrifEoiStep4Component,
    DrifEoiStep5Component,
    DrifEoiStep6Component,
    DrifEoiStep7Component,
    DrifEoiStep8Component,
    TranslocoModule,
  ],
  templateUrl: './drif-eoi.component.html',
  styleUrl: './drif-eoi.component.scss',
  providers: [
    RxFormBuilder,
    {
      provide: STEPPER_GLOBAL_OPTIONS,
      useValue: { showError: true },
    },
  ],
})
export class EOIApplicationComponent {
  formBuilder = inject(RxFormBuilder);
  applicationService = inject(DrifapplicationService);
  router = inject(Router);
  route = inject(ActivatedRoute);
  hotToast = inject(HotToastService);
  breakpointObserver = inject(BreakpointObserver);
  profileStore = inject(ProfileStore);

  stepperOrientation: StepperOrientation = 'vertical';

  hazardsOptions = Object.values(Hazards);

  eoiApplicationForm = this.formBuilder.formGroup(
    EOIApplicationForm
  ) as IFormGroup<EOIApplicationForm>;

  @ViewChild(MatStepper) stepper!: MatStepper;

  private formToStepMap: Record<string, string> = {
    proponentInformation: 'Step 1',
    projectInformation: 'Step 2',
    fundingInformation: 'Step 3',
    locationInformation: 'Step 4',
    projectDetails: 'Step 5',
    engagementPlan: 'Step 6',
    otherSupportingInformation: 'Step 7',
    declaration: 'Step 8',
  };

  id?: string;
  get isEditMode() {
    return !!this.id;
  }

  autoSaveTimer: any;
  autoSaveCountdown = 0;
  autoSaveInterval = 60;
  formChanged = false;

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
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .subscribe(({ matches }) => {
        this.stepperOrientation = matches ? 'horizontal' : 'vertical';
      });

    // fetch router params to determine if we are editing an existing application
    const id = this.route.snapshot.children[0]?.params['id'];
    if (id) {
      this.id = id;

      this.applicationService
        .dRIFApplicationGetEOI(id)
        .subscribe((response) => {
          const profileData = this.profileStore.getProfile();

          const resetSubmitter =
            profileData.firstName?.() != response.submitter?.firstName ||
            profileData.lastName?.() != response.submitter?.lastName;

          // transform application into step forms
          const eoiApplicationForm: EOIApplicationForm = {
            proponentInformation: {
              proponentType: response.proponentType,
              additionalContacts: response.additionalContacts,
              partneringProponents: response.partneringProponents,
              projectContact: response.projectContact,
            },
            projectInformation: {
              stream: response.stream,
              projectTitle: response.projectTitle,
              scopeStatement: response.scopeStatement,
              fundingStream: response.fundingStream,
              relatedHazards: response.relatedHazards,
              otherHazardsDescription: response.otherHazardsDescription,
              startDate: response.startDate,
              endDate: response.endDate,
            },
            fundingInformation: {
              fundingRequest: response.fundingRequest,
              remainingAmount: response.remainingAmount,
              intendToSecureFunding: response.intendToSecureFunding,
              estimatedTotal: response.estimatedTotal,
              haveOtherFunding: response.haveOtherFunding,
            },
            locationInformation: {
              ownershipDeclaration: response.ownershipDeclaration,
              ownershipDescription: response.ownershipDescription,
              locationDescription: response.locationDescription,
            },
            projectDetails: {
              additionalBackgroundInformation:
                response.additionalBackgroundInformation,
              additionalSolutionInformation:
                response.additionalSolutionInformation,
              addressRisksAndHazards: response.addressRisksAndHazards,
              disasterRiskUnderstanding: response.disasterRiskUnderstanding,
              projectDescription: response.projectDescription,
              estimatedPeopleImpacted: response.estimatedPeopleImpacted,
              communityImpact: response.communityImpact,
              isInfrastructureImpacted: response.isInfrastructureImpacted,
              infrastructureImpacted: response.infrastructureImpacted,
              rationaleForFunding: response.rationaleForFunding,
              rationaleForSolution: response.rationaleForSolution,
            },
            engagementPlan: {
              additionalEngagementInformation:
                response.additionalEngagementInformation,
              firstNationsEngagement: response.firstNationsEngagement,
              neighbourEngagement: response.neighbourEngagement,
            },
            otherSupportingInformation: {
              climateAdaptation: response.climateAdaptation,
              otherInformation: response.otherInformation,
            },
            declaration: {
              submitter: !resetSubmitter
                ? response.submitter
                : {
                    firstName: profileData.firstName?.(),
                    lastName: profileData.lastName?.(),
                    title: profileData.title?.() ?? '',
                    department: profileData.department?.() ?? '',
                    phone: profileData.phone?.() ?? '',
                    email: profileData.email?.() ?? '',
                  },
            },
          };

          this.eoiApplicationForm.patchValue(eoiApplicationForm, {
            emitEvent: false,
          });

          this.initStep1(response);
          this.initStep3(response);
          this.initStep5(response);

          this.eoiApplicationForm.markAsPristine();
          this.formChanged = false;

          if (response.status == 'UnderReview') {
            this.eoiApplicationForm.disable();
          }
        });
    }

    setTimeout(() => {
      this.eoiApplicationForm.valueChanges
        .pipe(
          startWith(this.eoiApplicationForm.getRawValue()),
          pairwise(),
          distinctUntilChanged((a, b) => {
            // compare objects but ignore declaration changes
            delete a[1].declaration.authorizedRepresentativeStatement;
            delete a[1].declaration.informationAccuracyStatement;
            delete b[1].declaration.authorizedRepresentativeStatement;
            delete b[1].declaration.informationAccuracyStatement;

            return JSON.stringify(a[1]) === JSON.stringify(b[1]);
          })
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

          this.eoiApplicationForm
            .get('declaration')
            ?.get('authorizedRepresentativeStatement')
            ?.reset();
          this.eoiApplicationForm
            .get('declaration')
            ?.get('informationAccuracyStatement')
            ?.reset();

          this.formChanged = true;
          this.resetAutoSaveTimer();
        });
    }, 1000); // TOOD: temp workaround to prevent empty form intiation triggering form change
  }

  ngOnDestroy() {
    clearInterval(this.autoSaveTimer);
  }

  initStep1(response: DraftEoiApplication) {
    const partneringProponentsArray = this.eoiApplicationForm.get(
      'proponentInformation.partneringProponentsArray'
    ) as FormArray;
    if (response.partneringProponents?.length! > 0) {
      partneringProponentsArray.clear({ emitEvent: false });
    }
    response.partneringProponents?.forEach((proponent) => {
      partneringProponentsArray?.push(
        this.formBuilder.formGroup(new StringItem({ value: proponent })),
        { emitEvent: false }
      );
    });

    const additionalContactsArray = this.eoiApplicationForm.get(
      'proponentInformation.additionalContacts'
    ) as FormArray;
    if (response.additionalContacts?.length! > 0) {
      additionalContactsArray.clear({ emitEvent: false });
    }
    response.additionalContacts?.forEach((contact) => {
      additionalContactsArray?.push(
        this.formBuilder.formGroup(new ContactDetailsForm(contact)),
        { emitEvent: false }
      );
    });
  }

  initStep3(response: DraftEoiApplication) {
    const fundingInformationItemFormArray = this.eoiApplicationForm.get(
      'fundingInformation.otherFunding'
    ) as FormArray;
    if (response.otherFunding?.length! > 0) {
      fundingInformationItemFormArray.clear({ emitEvent: false });
    }
    response.otherFunding?.forEach((funding) => {
      const fundingInformationItemForm = this.formBuilder.formGroup(
        new FundingInformationItemForm(funding)
      );
      fundingInformationItemFormArray?.push(fundingInformationItemForm, {
        emitEvent: false,
      });
      if (funding.type === FundingType.OtherGrants) {
        fundingInformationItemForm
          .get('otherDescription')
          ?.setValidators([Validators.required]);
      }
      fundingInformationItemFormArray?.push(fundingInformationItemForm);
    });
  }

  initStep5(response: DraftEoiApplication) {
    const infrastructureImpactedArray = this.eoiApplicationForm.get(
      'projectDetails.infrastructureImpacted'
    ) as FormArray;
    if (
      response.isInfrastructureImpacted === false ||
      response.infrastructureImpacted?.length! > 0
    ) {
      infrastructureImpactedArray.clear({ emitEvent: false });
    }

    response.infrastructureImpacted?.forEach((infrastructure) => {
      if (infrastructure) {
        infrastructureImpactedArray?.push(
          this.formBuilder.formGroup(
            new InfrastructureImpactedForm(infrastructure)
          ),
          { emitEvent: false }
        );
      }
    });
  }

  getProjectTitle() {
    return this.eoiApplicationForm.get('projectInformation.projectTitle')
      ?.value;
  }

  getFormGroup(groupName: string) {
    return this.eoiApplicationForm?.get(groupName) as RxFormGroup;
  }

  stepperSelectionChange(event: StepperSelectionEvent) {
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

  lastSavedAt?: Date;

  save() {
    if (!this.formChanged) {
      return;
    }

    const eoiApplicationForm =
      this.eoiApplicationForm.getRawValue() as EOIApplicationForm;

    const drifEoiApplication = {
      ...eoiApplicationForm.proponentInformation,
      ...eoiApplicationForm.projectInformation,
      ...eoiApplicationForm.fundingInformation,
      ...eoiApplicationForm.locationInformation,
      ...eoiApplicationForm.projectDetails,
      ...eoiApplicationForm.engagementPlan,
      ...eoiApplicationForm.otherSupportingInformation,
      submitter: eoiApplicationForm.declaration?.submitter,
    } as DraftEoiApplication;

    this.lastSavedAt = undefined;
    if (this.isEditMode) {
      this.applicationService
        .dRIFApplicationUpdateApplication(this.id!, drifEoiApplication)
        .subscribe({
          next: this.onSaveSuccess,
          error: this.onSaveFailure,
        });
    } else {
      this.applicationService
        .dRIFApplicationCreateEOIApplication(drifEoiApplication)
        .subscribe({
          next: this.onSaveSuccess,
          error: this.onSaveFailure,
        });
    }
  }

  onSaveSuccess = (response: ApplicationResult) => {
    this.lastSavedAt = new Date();

    this.hotToast.close();
    this.hotToast.success('Form saved successfully');

    this.formChanged = false;

    if (!this.isEditMode) {
      this.id = response.id;
      this.router.navigate(['/drif-eoi/', response['id']]);
    }
  };

  onSaveFailure = () => {
    this.hotToast.close();
    this.hotToast.error('Failed to save form');
  };

  submit() {
    this.eoiApplicationForm.markAllAsTouched();
    this.stepper.steps.forEach((step) => step._markAsInteracted());
    this.stepper._stateChanged();
    if (this.eoiApplicationForm.invalid) {
      // select which forms are invalid
      const invalidSteps = Object.keys(this.eoiApplicationForm.controls)
        .filter((key) => this.eoiApplicationForm.get(key)?.invalid)
        .map((key) => this.formToStepMap[key]);

      const lastStep = invalidSteps.pop();

      const stepsErrorMessage =
        invalidSteps.length > 0
          ? `${invalidSteps.join(', ')} and ${lastStep}`
          : lastStep;

      this.hotToast.close();
      this.hotToast.error(
        `Please fill all the required fields in ${stepsErrorMessage}.`
      );

      return;
    }

    const eoiApplicationForm =
      this.eoiApplicationForm.getRawValue() as EOIApplicationForm;

    const drifEoiApplication = {
      ...eoiApplicationForm.proponentInformation,
      ...eoiApplicationForm.projectInformation,
      ...eoiApplicationForm.fundingInformation,
      ...eoiApplicationForm.locationInformation,
      ...eoiApplicationForm.projectDetails,
      ...eoiApplicationForm.engagementPlan,
      ...eoiApplicationForm.otherSupportingInformation,
      ...eoiApplicationForm.declaration,
    } as EoiApplication;

    if (this.isEditMode) {
      this.applicationService
        .dRIFApplicationSubmitApplication2(this.id!, drifEoiApplication)
        .subscribe({
          next: this.onSubmitSuccess,
          error: this.onSubmitFailure,
        });
    } else {
      this.applicationService
        .dRIFApplicationSubmitApplication(drifEoiApplication)
        .subscribe({
          next: this.onSubmitSuccess,
          error: this.onSubmitFailure,
        });
    }
  }

  onSubmitSuccess = (response: ApplicationResult) => {
    this.hotToast.close();
    this.hotToast.success(
      `Your submission has been received. \nID #: ${response.id}.`
    );
    this.router.navigate(['/submissions']);
  };

  onSubmitFailure = () => {
    this.hotToast.close();
    this.hotToast.error('Failed to submit application');
  };

  goBack() {
    this.router.navigate(['/submissions']);
  }
}
