import { BreakpointObserver, LayoutModule } from '@angular/cdk/layout';
import {
  STEPPER_GLOBAL_OPTIONS,
  StepperSelectionEvent,
} from '@angular/cdk/stepper';
import { CommonModule } from '@angular/common';
import { Component, HostListener, inject, ViewChild } from '@angular/core';
import { FormArray, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import {
  MatStepper,
  MatStepperModule,
  StepperOrientation,
} from '@angular/material/stepper';
import { ActivatedRoute, Router } from '@angular/router';
import { HotToastService } from '@ngneat/hot-toast';
import { TranslocoModule } from '@ngneat/transloco';
import {
  IFormGroup,
  RxFormBuilder,
  RxFormGroup,
} from '@rxweb/reactive-form-validators';
import { DrifFpStep1Component } from './drif-fp-step-1/drif-fp-step-1.component';

import { distinctUntilChanged } from 'rxjs/operators';
import { DrifapplicationService } from '../../../api/drifapplication/drifapplication.service';
import { DraftFpApplication } from '../../../model';
import {
  ContactDetailsForm,
  FundingInformationItemForm,
  StringItem,
} from '../drif-eoi/drif-eoi-form';

import {
  DrifFpForm,
  ImpactedInfrastructureForm,
  ProposedActivityForm,
  YearOverYearFundingForm,
} from './drif-fp-form';
import { DrifFpStep10Component } from './drif-fp-step-10/drif-fp-step-10.component';
import { DrifFpStep11Component } from './drif-fp-step-11/drif-fp-step-11.component';
import { DrifFpStep12Component } from './drif-fp-step-12/drif-fp-step-12.component';
import { DrifFpStep2Component } from './drif-fp-step-2/drif-fp-step-2.component';
import { DrifFpStep3Component } from './drif-fp-step-3/drif-fp-step-3.component';
import { DrifFpStep4Component } from './drif-fp-step-4/drif-fp-step-4.component';
import { DrifFpStep5Component } from './drif-fp-step-5/drif-fp-step-5.component';
import { DrifFpStep6Component } from './drif-fp-step-6/drif-fp-step-6.component';
import { DrifFpStep7Component } from './drif-fp-step-7/drif-fp-step-7.component';
import { DrifFpStep8Component } from './drif-fp-step-8/drif-fp-step-8.component';
import { DrifFpStep9Component } from './drif-fp-step-9/drif-fp-step-9.component';

@Component({
  selector: 'drr-drif-fp',
  standalone: true,
  imports: [
    CommonModule,
    MatStepperModule,
    MatButtonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatIconModule,
    TranslocoModule,
    LayoutModule,
    DrifFpStep1Component,
    DrifFpStep2Component,
    DrifFpStep3Component,
    DrifFpStep4Component,
    DrifFpStep5Component,
    DrifFpStep6Component,
    DrifFpStep7Component,
    DrifFpStep8Component,
    DrifFpStep9Component,
    DrifFpStep10Component,
    DrifFpStep11Component,
    DrifFpStep12Component,
  ],
  providers: [
    RxFormBuilder,
    HotToastService,
    {
      provide: STEPPER_GLOBAL_OPTIONS,
      useValue: { showError: true },
    },
  ],
  templateUrl: './drif-fp.component.html',
  styleUrl: './drif-fp.component.scss',
})
export class DrifFpComponent {
  formBuilder = inject(RxFormBuilder);
  breakpointObserver = inject(BreakpointObserver);
  router = inject(Router);
  route = inject(ActivatedRoute);
  appService = inject(DrifapplicationService);
  hotToast = inject(HotToastService);

  stepperOrientation: StepperOrientation = 'vertical';

  autoSaveTimer: any;
  autoSaveCountdown = 0;
  autoSaveInterval = 60;
  lastSavedAt?: Date;
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

  @ViewChild(MatStepper) stepper!: MatStepper;

  id?: string;
  get isEditMode() {
    return !!this.id;
  }

  drifFpForm = this.formBuilder.formGroup(DrifFpForm) as IFormGroup<DrifFpForm>;

  ngOnInit() {
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .subscribe(({ matches }) => {
        this.stepperOrientation = matches ? 'horizontal' : 'vertical';
      });

    this.id = this.route.snapshot.params['id'];

    if (this.isEditMode) {
      this.load();
      this.formChanged = false;
    }

    setTimeout(() => {
      this.drifFpForm.valueChanges
        .pipe(
          distinctUntilChanged((a, b) => {
            return JSON.stringify(a) == JSON.stringify(b);
          })
        )
        .subscribe((val) => {
          this.formChanged = true;
          this.resetAutoSaveTimer();
        });
    }, 1000);
  }

  ngOnDestroy() {
    clearInterval(this.autoSaveTimer);
  }

  load() {
    this.appService.dRIFApplicationGetFP(this.id!).subscribe({
      next: (response) => {
        const formData: DrifFpForm = {
          eoiId: response.eoiId,
          fundingStream: response.fundingStream,
          projectType: response.projectType,
          proponentAndProjectInformationForm: {
            projectContact: response.projectContact,
            projectTitle: response.projectTitle,
            scopeStatement: response.scopeStatement,
            regionalProject: response.regionalProject,
            regionalProjectComments: response.regionalProjectComments,
          },
          ownershipAndAuthorization: {
            ownershipDeclaration: response.ownershipDeclaration,
            ownershipDescription: response.ownershipDescription,
            projectAuthority: response.projectAuthority,
            projectAuthorityComments: response.projectAuthorityComments,
            operationAndMaintenance: response.operationAndMaintenance,
            operationAndMaintenanceComments:
              response.operationAndMaintenanceComments,
            firstNationsEndorsement: response.firstNationsEndorsement,
            localGovernmentEndorsement: response.localGovernmentEndorsement,
            authorizationOrEndorsementComments:
              response.authorizationOrEndorsementComments,
          },
          projectArea: {
            // area: response.area,
            // areaDescription: response.areaDescription,
            communityImpact: response.communityImpact,
            estimatedPeopleImpacted: response.estimatedPeopleImpacted,
            // infrastructureImpacted: response.infrastructureImpacted,
            locationDescription: response.locationDescription,
            // units: response.units,
            relatedHazards: response.relatedHazards,
            otherHazardsDescription: response.otherHazardsDescription,
          },
          projectPlan: {
            startDate: response.startDate,
            endDate: response.endDate,
            projectAlternateOptions: response.projectAlternateOptions,
            projectDescription: response.projectDescription,
            proposedActivities: response.proposedActivities,
            verificationMethods: response.verificationMethods,
            verificationMethodsComments: response.verificationMethodsComments,
          },
          projectEngagement: {
            affectedParties: response.affectedParties,
            collaborationComments: response.collaborationComments,
            engagedWithFirstNations: response.engagedWithFirstNations,
            engagedWithFirstNationsComments:
              response.engagedWithFirstNationsComments,
            otherEngagement: response.otherEngagement,
            otherEngagementComments: response.otherEngagementComments,
          },
          climateAdaptation: {
            climateAdaptationScreener: response.climateAdaptationScreener,
            climateAdaptation: response.climateAdaptation,
          },
          permitsRegulationsAndStandards: {
            regulations: response.regulations,
            regulationsComments: response.regulationsComments,
            standards: response.standards,
            standardsAcceptable: response.standardsAcceptable,
            standardsComments: response.standardsComments,
            professionals: response.professionals,
            professionalGuidanceComments: response.professionalGuidanceComments,
            professionalGuidance: response.professionalGuidance,
            approvalsComments: response.approvalsComments,
            approvals: response.approvals,
          },
          projectOutcomes: {
            publicBenefit: response.publicBenefit,
            publicBenefitComments: response.publicBenefitComments,
            coBenefits: response.coBenefits,
            coBenefitComments: response.coBenefitComments,
            costReductions: response.costReductions,
            costReductionComments: response.costReductionComments,
            futureCostReduction: response.futureCostReduction,
            increasedResiliency: response.increasedResiliency,
            increasedResiliencyComments: response.increasedResiliencyComments,
            produceCoBenefits: response.produceCoBenefits,
          },
          projectRisks: {
            capacityRiskComments: response.capacityRiskComments,
            capacityRiskMitigated: response.capacityRiskMitigated,
            capacityRisks: response.capacityRisks,
            complexityRiskComments: response.complexityRiskComments,
            complexityRiskMitigated: response.complexityRiskMitigated,
            complexityRisks: response.complexityRisks,
            readinessRiskComments: response.readinessRiskComments,
            readinessRiskMitigated: response.readinessRiskMitigated,
            readinessRisks: response.readinessRisks,
            riskTransferMigigated: response.riskTransferMigigated,
            sensitivityRiskComments: response.sensitivityRiskComments,
            sensitivityRiskMitigated: response.sensitivityRiskMitigated,
            sensitivityRisks: response.sensitivityRisks,
            transferRisks: response.transferRisks,
            transferRisksComments: response.transferRisksComments,
          },
          budget: {
            haveOtherFunding: response.haveOtherFunding,
            estimatedTotal: response.estimatedTotal,
            fundingRequest: response.fundingRequest,
            remainingAmount: response.remainingAmount,
            discrepancyComment: response.discrepancyComment,
            totalDrifFundingRequest: response.totalDrifFundingRequest,
            activityCostEffectiveness: response.activityCostEffectiveness,
            costConsiderations: response.costConsiderations,
            costConsiderationsApplied: response.costConsiderationsApplied,
            costConsiderationsComments: response.costConsiderationsComments,
            costEffective: response.costEffective,
            costEffectiveComments: response.costEffectiveComments,
            previousResponse: response.previousResponse,
            previousResponseComments: response.previousResponseComments,
            previousResponseCost: response.previousResponseCost,
          },
          attachments: {
            // TODO: attachments: response.attachments,
          },
          declarations: {},
        };

        this.drifFpForm.patchValue(formData, { emitEvent: false });

        const partneringProponentsArray = this.getFormGroup(
          'proponentAndProjectInformationForm'
        ).get('partneringProponentsArray') as FormArray;
        if (response.partneringProponents?.length! > 0) {
          partneringProponentsArray.clear({ emitEvent: false });
        }
        response.partneringProponents?.forEach((proponent) => {
          partneringProponentsArray?.push(
            this.formBuilder.formGroup(new StringItem({ value: proponent })),
            { emitEvent: false }
          );
        });

        const additionalContactsArray = this.getFormGroup(
          'proponentAndProjectInformationForm'
        ).get('additionalContacts') as FormArray;
        if (response.additionalContacts?.length! > 0) {
          additionalContactsArray.clear({ emitEvent: false });
        }
        response.additionalContacts?.forEach((contact) => {
          additionalContactsArray?.push(
            this.formBuilder.formGroup(new ContactDetailsForm(contact)),
            { emitEvent: false }
          );
        });

        const infrastructureImpactedArray = this.getFormGroup(
          'projectArea'
        ).get('infrastructureImpacted') as FormArray;
        if (response.infrastructureImpacted?.length! > 0) {
          infrastructureImpactedArray.clear({ emitEvent: false });
        }
        response.infrastructureImpacted?.forEach((infrastructure) => {
          infrastructureImpactedArray?.push(
            this.formBuilder.formGroup(
              new ImpactedInfrastructureForm(
                // TODO: change to API model when available
                {
                  impact: '',
                  infrastructure: '',
                }
              )
            ),
            { emitEvent: false }
          );
        });

        const proposedActivitiesArray = this.getFormGroup('projectPlan').get(
          'proposedActivities'
        ) as FormArray;
        if (response.proposedActivities?.length! > 0) {
          proposedActivitiesArray.clear({ emitEvent: false });
        }
        response.proposedActivities?.forEach((activity) => {
          proposedActivitiesArray?.push(
            this.formBuilder.formGroup(new ProposedActivityForm(activity)),
            { emitEvent: false }
          );
        });

        const fundingInformationItemFormArray = this.getFormGroup('budget').get(
          'otherFunding'
        ) as FormArray;
        if (response.otherFunding?.length! > 0) {
          fundingInformationItemFormArray.clear({ emitEvent: false });
        }
        response.otherFunding?.forEach((funding) => {
          fundingInformationItemFormArray?.push(
            this.formBuilder.formGroup(new FundingInformationItemForm(funding)),
            { emitEvent: false }
          );
        });

        const yearOverYearFormArray = this.getFormGroup('budget').get(
          'yearOverYearFunding'
        ) as FormArray;
        if (response.yearOverYearFunding?.length! > 0) {
          yearOverYearFormArray.clear({ emitEvent: false });
        }
        response.yearOverYearFunding?.forEach((funding) => {
          yearOverYearFormArray?.push(
            this.formBuilder.formGroup(new YearOverYearFundingForm(funding)),
            { emitEvent: false }
          );
        });

        this.drifFpForm.markAsPristine();
        this.formChanged = false;
      },
      error: (error) => {
        this.hotToast.error('Failed to load application');
      },
    });
  }

  getFormGroup(groupName: string) {
    return this.drifFpForm?.get(groupName) as RxFormGroup;
  }

  getProjectTitle() {
    return this.drifFpForm
      ?.get('proponentAndProjectInformationForm')
      ?.get('projectTitle')?.value;
  }

  getRelatedEOILink() {
    return `/eoi-submission-details/${this.getRelatedEOI()}`;
  }

  getRelatedEOI() {
    return this.drifFpForm?.get('eoiId')?.value;
  }

  onEoiClick(event: Event) {
    event.preventDefault();
    this.router.navigate(['/eoi-submission-details', this.getRelatedEOI()]);
  }

  goBack() {
    this.router.navigate(['/dashboard']);
  }

  save() {
    if (!this.formChanged) {
      return;
    }

    const drifFpForm = this.drifFpForm.getRawValue() as DrifFpForm;

    // TODO: remove when API is updated
    drifFpForm!.projectArea!.infrastructureImpacted = [];

    const fpDraft = {
      ...drifFpForm.proponentAndProjectInformationForm,
      ...drifFpForm.ownershipAndAuthorization,
      ...drifFpForm.projectArea,
      ...drifFpForm.projectPlan,
      ...drifFpForm.projectEngagement,
      ...drifFpForm.climateAdaptation,
      ...drifFpForm.permitsRegulationsAndStandards,
      ...drifFpForm.projectOutcomes,
      ...drifFpForm.projectRisks,
      ...drifFpForm.budget,
      ...drifFpForm.attachments,
      ...drifFpForm.declarations,
    } as DraftFpApplication;

    this.lastSavedAt = undefined;
    this.appService
      .dRIFApplicationUpdateFPApplication(this.id!, fpDraft)
      .subscribe({
        next: (response) => {
          this.lastSavedAt = new Date();

          this.hotToast.close();
          this.hotToast.success('Application saved successfully', {
            duration: 5000,
            autoClose: true,
          });

          this.formChanged = false;
          this.resetAutoSaveTimer();
        },
        error: (error) => {
          this.hotToast.close();
          this.hotToast.error('Failed to save application');
        },
      });
  }

  submit() {}

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
}
