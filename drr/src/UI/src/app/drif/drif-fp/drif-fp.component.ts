import { BreakpointObserver, LayoutModule } from '@angular/cdk/layout';
import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { CommonModule } from '@angular/common';
import { Component, HostListener, inject, ViewChild } from '@angular/core';
import {
  FormArray,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import {
  MatStepper,
  MatStepperModule,
  StepperOrientation,
} from '@angular/material/stepper';
import { ActivatedRoute, Router } from '@angular/router';

import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import {
  IFormGroup,
  RxFormBuilder,
  RxFormGroup,
  RxwebValidators,
} from '@rxweb/reactive-form-validators';
import { DrifFpStep1Component } from './drif-fp-step-1/drif-fp-step-1.component';

import { distinctUntilChanged, pairwise, startWith } from 'rxjs/operators';
import { DrifapplicationService } from '../../../api/drifapplication/drifapplication.service';
import {
  ActivityType,
  DocumentType,
  DraftFpApplication,
  FpApplication,
  YesNoOption,
} from '../../../model';
import {
  ContactDetailsForm,
  FundingInformationItemForm,
  StringItem,
} from '../drif-eoi/drif-eoi-form';

import { UntilDestroy } from '@ngneat/until-destroy';
import { HotToastService } from '@ngxpert/hot-toast';
import { OptionsStore } from '../../store/options.store';
import { ProfileStore } from '../../store/profile.store';
import {
  AttachmentForm,
  CostEstimateForm,
  DrifFpForm,
  ImpactedInfrastructureForm,
  ProposedActivityForm,
  StandardInfoForm,
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

@UntilDestroy({ checkProperties: true })
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
  providers: [RxFormBuilder],
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
  optionsStore = inject(OptionsStore);
  profileStore = inject(ProfileStore);
  translocoService = inject(TranslocoService);

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

  private formToStepMap: Record<string, string> = {
    proponentAndProjectInformation: 'Step 1',
    ownershipAndAuthorization: 'Step 2',
    projectArea: 'Step 3',
    projectPlan: 'Step 4',
    projectEngagement: 'Step 5',
    climateAdaptation: 'Step 6',
    permitsRegulationsAndStandards: 'Step 7',
    projectOutcomes: 'Step 8',
    projectRisks: 'Step 9',
    budget: 'Step 10',
    attachments: 'Step 11',
    declaration: 'Step 12',
  };

  id?: string;

  fullProposalForm = this.formBuilder.formGroup(
    DrifFpForm,
  ) as IFormGroup<DrifFpForm>;

  async ngOnInit() {
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .subscribe(({ matches }) => {
        this.stepperOrientation = matches ? 'horizontal' : 'vertical';
      });

    this.id = this.route.snapshot.params['id'];

    await this.load().then(() => {
      this.formChanged = false;
      setTimeout(() => {
        this.fullProposalForm.valueChanges
          .pipe(
            startWith(this.fullProposalForm.value),
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

            this.fullProposalForm
              .get('declaration')
              ?.get('authorizedRepresentativeStatement')
              ?.reset();

            this.fullProposalForm
              .get('declaration')
              ?.get('informationAccuracyStatement')
              ?.reset();

            this.formChanged = true;
            this.resetAutoSaveTimer();
          });
      }, 1000);
    });
  }

  ngOnDestroy() {
    clearInterval(this.autoSaveTimer);
  }

  load(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.appService.dRIFApplicationGetFP(this.id!).subscribe({
        next: (response) => {
          const profileData = this.profileStore.getProfile();

          const resetSubmitter =
            profileData.firstName?.() != response.submitter?.firstName ||
            profileData.lastName?.() != response.submitter?.lastName;

          const formData: DrifFpForm = {
            eoiId: response.eoiId,
            fundingStream: response.fundingStream,
            proponentAndProjectInformation: {
              projectContact: response.projectContact,
              projectTitle: response.projectTitle,
              mainDeliverable: response.mainDeliverable,
              regionalProject: response.regionalProject,
              regionalProjectComments: response.regionalProjectComments,
            },
            ownershipAndAuthorization: {
              ownershipDeclaration: response.ownershipDeclaration,
              ownershipDescription: response.ownershipDescription,
              haveAuthorityToDevelop: response.haveAuthorityToDevelop,
              operationAndMaintenance: response.operationAndMaintenance,
              operationAndMaintenanceComments:
                response.operationAndMaintenanceComments,
              firstNationsAuthorizedByPartners:
                response.firstNationsAuthorizedByPartners,
              localGovernmentAuthorizedByPartners:
                response.localGovernmentAuthorizedByPartners,
              authorizationOrEndorsementComments:
                response.authorizationOrEndorsementComments,
            },
            projectArea: {
              area: response.area,
              areaDescription: response.areaDescription,
              communityImpact: response.communityImpact,
              estimatedPeopleImpactedFP: response.estimatedPeopleImpactedFP,
              isInfrastructureImpacted: response.isInfrastructureImpacted,
              locationDescription: response.locationDescription,
              units: response.units,
              relatedHazards: response.relatedHazards,
              otherHazardsDescription: response.otherHazardsDescription,
            },
            projectPlan: {
              startDate: response.startDate,
              endDate: response.endDate,
              projectAlternateOptions: response.projectAlternateOptions,
              projectDescription: response.projectDescription,
              proposedActivities: response.proposedActivities,
              foundationalOrPreviousWorks: response.foundationalOrPreviousWorks,
              addressRisksAndHazards: response.addressRisksAndHazards,
              disasterRiskUnderstanding: response.disasterRiskUnderstanding,
              rationaleForFunding: response.rationaleForFunding,
              howWasNeedIdentified: response.howWasNeedIdentified,
            },
            projectEngagement: {
              affectedParties: response.affectedParties,
              collaborationComments: response.collaborationComments,
              engagedWithFirstNationsOccurred:
                response.engagedWithFirstNationsOccurred,
              engagedWithFirstNationsComments:
                response.engagedWithFirstNationsComments,
              otherEngagement: response.otherEngagement,
              otherEngagementComments: response.otherEngagementComments,
            },
            climateAdaptation: {
              incorporateFutureClimateConditions:
                response.incorporateFutureClimateConditions,
              climateAdaptation: response.climateAdaptation,
              climateAssessment: response.climateAssessment,
              climateAssessmentComments: response.climateAssessmentComments,
              climateAssessmentTools: response.climateAssessmentTools,
            },
            permitsRegulationsAndStandards: {
              meetsRegulatoryRequirements: response.meetsRegulatoryRequirements,
              meetsRegulatoryComments: response.meetsRegulatoryComments,
              standards: response.standards,
              standardsAcceptable: response.standardsAcceptable,
              standardsComments: response.standardsComments,
              professionals: response.professionals,
              professionalGuidanceComments:
                response.professionalGuidanceComments,
              knowledgeHolders: response.knowledgeHolders,
              professionalGuidance: response.professionalGuidance,
              meetsEligibilityComments: response.meetsEligibilityComments,
              meetsEligibilityRequirements:
                response.meetsEligibilityRequirements,
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
              increasedOrTransferred: response.increasedOrTransferred,
              increasedOrTransferredComments:
                response.increasedOrTransferredComments,
            },
            budget: {
              haveOtherFunding: response.haveOtherFunding,
              eligibleFundingRequest: response.eligibleFundingRequest,
              totalProjectCost: response.totalProjectCost,
              remainingAmount: response.remainingAmount,
              discrepancyComment: response.discrepancyComment,
              totalDrifFundingRequest: response.totalDrifFundingRequest,
              costConsiderations: response.costConsiderations,
              costConsiderationsApplied: response.costConsiderationsApplied,
              costConsiderationsComments: response.costConsiderationsComments,
              costEffectiveComments: response.costEffectiveComments,
              previousResponse: response.previousResponse,
              previousResponseComments: response.previousResponseComments,
              previousResponseCost: response.previousResponseCost,
              intendToSecureFunding: response.intendToSecureFunding,
              contingency: response.contingency,
              costEstimateClass: response.costEstimateClass,
              totalProjectCostChangeComments:
                response.totalProjectCostChangeComments,
            },
            attachments: {
              haveResolution: response.haveResolution,
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

          this.fullProposalForm.patchValue(formData, { emitEvent: false });

          this.initStep1(response);
          this.initStep2();
          this.initStep3(response);
          this.initStep4(response);
          this.initStep6(response);
          this.initStep7(response);
          this.initStep8(response);
          this.initStep9(response);
          this.initStep10(response);
          this.initStep11(response);

          this.fullProposalForm.markAsPristine();
          this.formChanged = false;
          resolve();
        },
        error: (error) => {
          reject();
          this.hotToast.close();
          this.hotToast.error('Failed to load application');
        },
      });
    });
  }

  initStep1(response: DraftFpApplication) {
    const partneringProponentsArray = this.fullProposalForm.get(
      'proponentAndProjectInformation.partneringProponentsArray',
    ) as FormArray;
    if (response.partneringProponents?.length! > 0) {
      partneringProponentsArray.clear({ emitEvent: false });
    }
    response.partneringProponents?.forEach((proponent) => {
      partneringProponentsArray?.push(
        this.formBuilder.formGroup(new StringItem({ value: proponent })),
        { emitEvent: false },
      );
    });

    const additionalContactsArray = this.fullProposalForm.get(
      'proponentAndProjectInformation.additionalContacts',
    ) as FormArray;
    if (response.additionalContacts?.length! > 0) {
      additionalContactsArray.clear({ emitEvent: false });
    }
    response.additionalContacts?.forEach((contact) => {
      additionalContactsArray?.push(
        this.formBuilder.formGroup(new ContactDetailsForm(contact)),
        { emitEvent: false },
      );
    });

    if (response.regionalProject === true) {
      this.fullProposalForm
        .get('proponentAndProjectInformation.regionalProjectComments')
        ?.addValidators(Validators.required);
    }
  }

  initStep2() {
    if (
      this.fullProposalForm
        .get('ownershipAndAuthorization')
        ?.get('ownershipDeclaration')?.value === false
    ) {
      this.fullProposalForm
        .get('ownershipAndAuthorization')
        ?.get('ownershipDescription')
        ?.addValidators(Validators.required);
      this.fullProposalForm
        .get('ownershipAndAuthorization')
        ?.get('ownershipDescription')
        ?.updateValueAndValidity();
    }
  }

  initStep3(response: DraftFpApplication) {
    const infrastructureImpactedArray = this.fullProposalForm.get(
      'projectArea.infrastructureImpacted',
    ) as FormArray;
    if (
      response.isInfrastructureImpacted === false ||
      response.infrastructureImpacted?.length! > 0
    ) {
      infrastructureImpactedArray.clear({ emitEvent: false });
    }
    response.infrastructureImpacted?.forEach((infrastructure) => {
      infrastructureImpactedArray?.push(
        this.formBuilder.formGroup(
          new ImpactedInfrastructureForm(infrastructure),
        ),
        { emitEvent: false },
      );
    });
  }

  initStep4(response: DraftFpApplication) {
    const proposedActivitiesArray = this.fullProposalForm.get(
      'projectPlan.proposedActivities',
    ) as FormArray;
    if (response.proposedActivities?.length! > 0) {
      proposedActivitiesArray.clear({ emitEvent: false });
    }
    response.proposedActivities?.forEach((activity) => {
      const proposedActivity = this.formBuilder.formGroup(
        new ProposedActivityForm(activity),
      );
      if (
        activity.activity !== ActivityType.ConstructionContractAward &&
        activity.activity !== ActivityType.PermitToConstruct
      ) {
        proposedActivity.get('startDate')?.addValidators(Validators.required);
      }
      proposedActivitiesArray?.push(proposedActivity, { emitEvent: false });
    });
  }

  initStep6(response: DraftFpApplication) {
    if (response.incorporateFutureClimateConditions === true) {
      this.fullProposalForm
        .get('climateAdaptation')
        ?.get('climateAdaptation')
        ?.addValidators(Validators.required);
    }

    if (response.climateAssessment === true) {
      this.fullProposalForm
        .get('climateAdaptation')
        ?.get('climateAssessmentTools')
        ?.addValidators(Validators.required);
      this.fullProposalForm
        .get('climateAdaptation')
        ?.get('climateAssessmentComments')
        ?.addValidators(Validators.required);
    }
  }

  initStep7(response: DraftFpApplication) {
    const standardsFormArray = this.fullProposalForm.get(
      'permitsRegulationsAndStandards.standards',
    ) as FormArray;
    this.optionsStore.options.standards?.()?.forEach((standard) => {
      const standardsInfo = response.standards?.find(
        (s) => s.category === standard.category,
      );
      const standardInfo = new StandardInfoForm({
        isCategorySelected: standardsInfo?.isCategorySelected,
        category: standard.category,
        standards: standardsInfo?.isCategorySelected
          ? (standardsInfo?.standards ?? [])
          : [],
      });
      const standardInfoForm = this.formBuilder.formGroup(standardInfo);

      if (standardsInfo?.isCategorySelected === false) {
        standardInfoForm.get('standards')?.disable();
      }

      standardInfoForm
        .get('isCategorySelected')
        ?.valueChanges.pipe(distinctUntilChanged())
        .subscribe((value) => {
          const standardsControl = standardInfoForm.get('standards');
          if (value === false) {
            standardsControl?.setValue([], {
              emitEvent: false,
            });
            standardsControl?.disable();
            standardsControl?.clearValidators();
          } else {
            standardsControl?.enable();
            standardsControl?.setValidators(Validators.required);
          }
          standardsControl?.updateValueAndValidity();
        });

      standardsFormArray?.push(standardInfoForm, { emitEvent: false });
    });

    if (response.standardsAcceptable === 'Yes') {
      const standardsValidControl = this.fullProposalForm.get(
        'permitsRegulationsAndStandards.standardsValid',
      );
      standardsValidControl?.addValidators(RxwebValidators.requiredTrue());
      standardsValidControl?.updateValueAndValidity();
    }

    if (response.professionalGuidance === true) {
      this.fullProposalForm
        .get('permitsRegulationsAndStandards')
        ?.get('professionals')
        ?.addValidators(Validators.required);
    }

    if (response.meetsRegulatoryRequirements === true) {
      this.fullProposalForm
        .get('permitsRegulationsAndStandards.meetsRegulatoryComments')
        ?.addValidators(Validators.required);
    }

    if (response.meetsEligibilityRequirements === true) {
      this.fullProposalForm
        .get('permitsRegulationsAndStandards.meetsEligibilityComments')
        ?.addValidators(Validators.required);
    }

    const permitsArray = this.fullProposalForm.get(
      'permitsRegulationsAndStandards.permitsArray',
    ) as FormArray;
    if (
      response.permits?.length! > 0 ||
      response.meetsEligibilityRequirements === false
    ) {
      permitsArray.clear({ emitEvent: false });
    }
    response.permits?.forEach((permit) => {
      permitsArray?.push(
        this.formBuilder.formGroup(new StringItem({ value: permit })),
        { emitEvent: false },
      );
    });
  }

  initStep8(response: DraftFpApplication) {
    if (response.futureCostReduction === true) {
      this.fullProposalForm
        .get('projectOutcomes.costReductions')
        ?.addValidators(Validators.required);
      this.fullProposalForm
        .get('projectOutcomes.costReductionComments')
        ?.addValidators(Validators.required);
    }

    if (response.produceCoBenefits === true) {
      this.fullProposalForm
        .get('projectOutcomes.coBenefits')
        ?.addValidators(Validators.required);
      this.fullProposalForm
        .get('projectOutcomes.coBenefitComments')
        ?.addValidators(Validators.required);
    }
  }

  initStep9(response: DraftFpApplication) {
    if (response.complexityRiskMitigated === true) {
      this.fullProposalForm
        .get('projectRisks.complexityRisks')
        ?.addValidators(Validators.required);
      this.fullProposalForm
        .get('projectRisks.complexityRiskComments')
        ?.addValidators(Validators.required);
    }

    if (response.readinessRiskMitigated === true) {
      this.fullProposalForm
        .get('projectRisks.readinessRisks')
        ?.addValidators(Validators.required);
      this.fullProposalForm
        .get('projectRisks.readinessRiskComments')
        ?.addValidators(Validators.required);
    }

    if (response.sensitivityRiskMitigated === true) {
      this.fullProposalForm
        .get('projectRisks.sensitivityRisks')
        ?.addValidators(Validators.required);
      this.fullProposalForm
        .get('projectRisks.sensitivityRiskComments')
        ?.addValidators(Validators.required);
    }

    if (response.capacityRiskMitigated === true) {
      this.fullProposalForm
        .get('projectRisks.capacityRisks')
        ?.addValidators(Validators.required);
      this.fullProposalForm
        .get('projectRisks.capacityRiskComments')
        ?.addValidators(Validators.required);
    }

    if (response.riskTransferMigigated === true) {
      this.fullProposalForm
        .get('projectRisks.increasedOrTransferred')
        ?.addValidators(Validators.required);
      this.fullProposalForm
        .get('projectRisks.increasedOrTransferredComments')
        ?.addValidators(Validators.required);
    }
  }

  initStep10(response: DraftFpApplication) {
    const otherFundingArray = this.fullProposalForm.get(
      'budget.otherFunding',
    ) as FormArray;
    if (
      response.haveOtherFunding === false ||
      response.otherFunding?.length! > 0
    ) {
      otherFundingArray.clear({ emitEvent: false });
    }
    response.otherFunding?.forEach((funding) => {
      otherFundingArray?.push(
        this.formBuilder.formGroup(new FundingInformationItemForm(funding)),
        { emitEvent: false },
      );
    });

    const yearOverYearFormArray = this.fullProposalForm.get(
      'budget.yearOverYearFunding',
    ) as FormArray;
    if (response.yearOverYearFunding?.length! > 0) {
      yearOverYearFormArray.clear({ emitEvent: false });
    }
    response.yearOverYearFunding?.forEach((funding) => {
      yearOverYearFormArray?.push(
        this.formBuilder.formGroup(new YearOverYearFundingForm(funding)),
        { emitEvent: false },
      );
    });

    const costEstimatesArray = this.fullProposalForm.get(
      'budget.costEstimates',
    ) as FormArray;
    if (response.costEstimates?.length! > 0) {
      costEstimatesArray.clear({ emitEvent: false });
    }
    response.costEstimates?.forEach((costEstimate) => {
      costEstimatesArray?.push(
        this.formBuilder.formGroup(new CostEstimateForm(costEstimate)),
        { emitEvent: false },
      );
    });

    const previousResponseCost = this.fullProposalForm.get(
      'budget.previousResponseCost',
    );
    const previousResponseComments = this.fullProposalForm.get(
      'budget.previousResponseComments',
    );

    switch (response.previousResponse) {
      case YesNoOption.Yes:
        previousResponseCost?.setValidators(Validators.required);
        previousResponseComments?.setValidators(Validators.required);
        break;
      case YesNoOption.NotApplicable:
        previousResponseCost?.clearValidators();
        previousResponseComments?.setValidators(Validators.required);
        break;
      case YesNoOption.No:
        previousResponseCost?.clearValidators();
        previousResponseComments?.clearValidators();
        break;

      default:
        break;
    }

    previousResponseCost?.updateValueAndValidity();
    previousResponseComments?.updateValueAndValidity();

    const costConsiderations = this.fullProposalForm.get(
      'budget.costConsiderations',
    );
    const costConsiderationsComments = this.fullProposalForm.get(
      'budget.costConsiderationsComments',
    );

    if (response.costConsiderationsApplied === true) {
      costConsiderations?.setValidators(Validators.required);
      costConsiderationsComments?.setValidators(Validators.required);
    } else {
      costConsiderations?.clearValidators();
      costConsiderationsComments?.clearValidators();
    }

    costConsiderations?.updateValueAndValidity();
    costConsiderationsComments?.updateValueAndValidity();
  }

  initStep11(response: DraftFpApplication) {
    const attachmentsArray = this.fullProposalForm.get(
      'attachments.attachments',
    ) as FormArray;

    if (response.haveResolution === true) {
      attachmentsArray?.push(
        this.formBuilder.formGroup(
          new AttachmentForm({ documentType: DocumentType.Resolution }),
        ),
        { emitEvent: false },
      );
    }

    response.attachments?.forEach((attachment) => {
      if (attachment.documentType === DocumentType.DetailedCostEstimate) {
        attachmentsArray.controls
          .find(
            (control) =>
              control.value.documentType === DocumentType.DetailedCostEstimate,
          )
          ?.patchValue(attachment, { emitEvent: false });
        return;
      }

      if (attachment.documentType === DocumentType.Resolution) {
        attachmentsArray.controls
          .find(
            (control) => control.value.documentType === DocumentType.Resolution,
          )
          ?.patchValue(attachment, { emitEvent: false });
        return;
      }

      attachmentsArray?.push(
        this.formBuilder.formGroup(new AttachmentForm(attachment)),
        { emitEvent: false },
      );
    });
  }

  getFormGroup(groupName: string) {
    return this.fullProposalForm?.get(groupName) as RxFormGroup;
  }

  getProjectTitle() {
    return this.fullProposalForm
      ?.get('proponentAndProjectInformation')
      ?.get('projectTitle')?.value;
  }

  getRelatedEOILink() {
    return `/eoi-submission-details/${this.getRelatedEOI()}`;
  }

  getRelatedEOI() {
    return this.fullProposalForm?.get('eoiId')?.value;
  }

  onEoiClick(event: Event) {
    event.preventDefault();
    this.router.navigate(['/eoi-submission-details', this.getRelatedEOI()]);
  }

  goBack() {
    this.router.navigate(['/dashboard']);
  }

  getFormMaxNumberInvalidControlKeysRecursively(
    formGroup: RxFormGroup,
  ): string[] {
    const errorFieldKeys: string[] = [];

    Object.keys(formGroup.controls).forEach((key) => {
      const control = formGroup.get(key);
      if (control instanceof FormArray) {
        control.controls.forEach((nestedControl) => {
          errorFieldKeys.push(
            ...this.getFormMaxNumberInvalidControlKeysRecursively(
              nestedControl as RxFormGroup,
            ),
          );
        });
      } else if (control instanceof RxFormGroup) {
        errorFieldKeys.push(
          ...this.getFormMaxNumberInvalidControlKeysRecursively(control),
        );
      } else {
        let controlWasDisabled = false;
        if (control && control.disabled) {
          control.enable();
          controlWasDisabled = true;
        }

        if (control && control.hasError('maxNumber')) {
          errorFieldKeys.push(key);
        }

        if (controlWasDisabled) {
          control!.disable();
        }
      }
    });

    return errorFieldKeys;
  }

  getFormValue() {
    const fpForm = this.fullProposalForm.getRawValue() as DrifFpForm;

    // filter out empty attachment forms
    const attachmentsForm = { ...fpForm.attachments };
    attachmentsForm.attachments = attachmentsForm.attachments?.filter(
      (attachment: AttachmentForm) => attachment.id,
    );
    attachmentsForm.haveResolution = fpForm.attachments?.haveResolution;

    const fpApp = {
      ...fpForm.proponentAndProjectInformation,
      ...fpForm.ownershipAndAuthorization,
      ...fpForm.projectArea,
      ...fpForm.projectPlan,
      ...fpForm.projectEngagement,
      ...fpForm.climateAdaptation,
      ...fpForm.permitsRegulationsAndStandards,
      ...fpForm.projectOutcomes,
      ...fpForm.projectRisks,
      ...fpForm.budget,
      ...attachmentsForm,
      ...fpForm.declaration,
    } as FpApplication;

    return fpApp;
  }

  isFormMaxNumberConditionValid() {
    const invalidControlKeys =
      this.getFormMaxNumberInvalidControlKeysRecursively(
        this.fullProposalForm.get('budget') as RxFormGroup,
      );

    if (invalidControlKeys.length > 0) {
      this.hotToast.close();
      this.hotToast.error(
        'Unable to save because one of the currency fields exceeds the maximum allowed amount. Please review the Total Cost field for each task, as well as the Total Eligible Costs field.',
      );
      return false;
    }

    return true;
  }

  save() {
    if (!this.formChanged) {
      return;
    }

    if (!this.isFormMaxNumberConditionValid()) {
      return;
    }

    const fpApp = this.getFormValue();

    this.lastSavedAt = undefined;
    this.appService
      .dRIFApplicationUpdateFPApplication(this.id!, fpApp)
      .subscribe({
        next: (response) => {
          this.lastSavedAt = new Date();

          this.hotToast.close();
          this.hotToast.success('Application saved successfully');

          this.formChanged = false;
          this.resetAutoSaveTimer();
        },
        error: (error) => {
          this.hotToast.close();
          this.hotToast.error('Failed to save application');
        },
      });
  }

  getBudgetStepErrorMessageKey() {
    const budgetForm = this.getFormGroup('budget');

    const invalidControls = Object.keys(budgetForm?.controls).filter(
      (key) => budgetForm?.get(key)?.invalid,
    );
    if (budgetForm.get('remainingAmount')?.invalid) {
      return 'excessFundingError';
    }

    if (budgetForm?.get('estimatesMatchFundingRequest')?.invalid) {
      return 'totalDrifFundingRequestError';
    }

    return 'stepError';
  }

  submit() {
    this.fullProposalForm.markAllAsTouched();
    this.stepper.steps.forEach((step) => step._markAsInteracted());
    this.stepper._stateChanged();

    if (!this.isFormMaxNumberConditionValid()) {
      return;
    }

    if (this.fullProposalForm.invalid) {
      const invalidSteps = Object.keys(this.fullProposalForm.controls)
        .filter((key) => this.fullProposalForm.get(key)?.invalid)
        .map((key) => this.formToStepMap[key]);

      if (
        invalidSteps.length === 1 &&
        invalidSteps[0] === 'Step 10' &&
        this.fullProposalForm.get('budget.remainingAmount')?.value! < 0
      ) {
        this.hotToast.close();
        this.hotToast.error('Cannot submit with excess funding.');
        return;
      }

      if (
        this.fullProposalForm.get('budget.estimatesMatchFundingRequest')
          ?.invalid
      ) {
        this.hotToast.close();
        this.hotToast.error(
          'Detailed cost estimates do not match your funding request in Step 10.',
        );
        return;
      }

      const lastStep = invalidSteps.pop();

      const stepsErrorMessage =
        invalidSteps.length > 0
          ? `${invalidSteps.join(', ')} and ${lastStep}`
          : lastStep;

      this.hotToast.close();
      this.hotToast.error(
        `Please fill all the required fields in ${stepsErrorMessage}.`,
      );

      return;
    }

    const fpApp = this.getFormValue();

    this.appService
      .dRIFApplicationSubmitFPApplication(this.id!, fpApp)
      .subscribe({
        next: (response) => {
          this.hotToast.close();
          this.hotToast.success(
            `Your submission has been received. \nID #: ${response.id}.`,
          );

          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.hotToast.close();
          this.hotToast.error('Failed to submit application');
        },
      });
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
}
