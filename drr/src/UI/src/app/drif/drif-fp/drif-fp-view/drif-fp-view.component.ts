import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import {
  FormArray,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { TranslocoModule } from '@ngneat/transloco';

import { ActivatedRoute, Router } from '@angular/router';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { DrifapplicationService } from '../../../../api/drifapplication/drifapplication.service';
import { OptionsStore } from '../../../store/options.store';
import {
  ContactDetailsForm,
  FundingInformationItemForm,
  StringItem,
} from '../../drif-eoi/drif-eoi-form';
import {
  DrifFpForm,
  ImpactedInfrastructureForm,
  ProposedActivityForm,
  StandardInfoForm,
  YearOverYearFundingForm,
} from '../drif-fp-form';
import { DrifFpSummaryComponent } from '../drif-fp-summary/drif-fp-summary.component';

@Component({
  selector: 'drr-drif-fp-view',
  standalone: true,
  imports: [
    CommonModule,
    DrifFpSummaryComponent,
    FormsModule,
    ReactiveFormsModule,
    MatButtonModule,
    TranslocoModule,
  ],
  templateUrl: './drif-fp-view.component.html',
  styleUrl: './drif-fp-view.component.scss',
  providers: [RxFormBuilder],
})
export class DrifFpViewComponent {
  router = inject(Router);
  route = inject(ActivatedRoute);
  appService = inject(DrifapplicationService);
  formBuilder = inject(RxFormBuilder);
  optionsStore = inject(OptionsStore);

  fullProposalForm = this.formBuilder.formGroup(
    DrifFpForm
  ) as IFormGroup<DrifFpForm>;
  id!: string;

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    this.id = id;
    this.appService.dRIFApplicationGetFP(id).subscribe((response) => {
      const formData: DrifFpForm = {
        eoiId: response.eoiId,
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
          verificationMethods: response.verificationMethods,
          addressRisksAndHazards: response.addressRisksAndHazards,
          disasterRiskUnderstanding: response.disasterRiskUnderstanding,
          rationaleForFunding: response.rationaleForFunding,
          rationaleForSolution: response.rationaleForSolution,
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
        },
        permitsRegulationsAndStandards: {
          meetsRegulatoryRequirements: response.meetsRegulatoryRequirements,
          meetsRegulatoryComments: response.meetsRegulatoryComments,
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
          eligibleFundingRequest: response.eligibleFundingRequest,
          fundingRequest: response.fundingRequest,
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
        },
        attachments: {
          // TODO: attachments: response.attachments,
        },
        declarations: {
          submitter: response.submitter,
        },
      };
      this.fullProposalForm.patchValue(formData, { emitEvent: false });
      const partneringProponentsArray = this.fullProposalForm
        .get('proponentAndProjectInformation')
        ?.get('partneringProponentsArray') as FormArray;
      if (response.partneringProponents?.length! > 0) {
        partneringProponentsArray.clear({ emitEvent: false });
      }
      response.partneringProponents?.forEach((proponent) => {
        partneringProponentsArray?.push(
          this.formBuilder.formGroup(new StringItem({ value: proponent })),
          { emitEvent: false }
        );
      });
      const additionalContactsArray = this.fullProposalForm
        .get('proponentAndProjectInformation')
        ?.get('additionalContacts') as FormArray;
      if (response.additionalContacts?.length! > 0) {
        additionalContactsArray.clear({ emitEvent: false });
      }
      response.additionalContacts?.forEach((contact) => {
        additionalContactsArray?.push(
          this.formBuilder.formGroup(new ContactDetailsForm(contact)),
          { emitEvent: false }
        );
      });
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
      const infrastructureImpacted = this.fullProposalForm
        .get('projectArea')
        ?.get('infrastructureImpacted') as FormArray;
      if (response.infrastructureImpacted?.length! > 0) {
        infrastructureImpacted.clear({ emitEvent: false });
      }
      response.infrastructureImpacted?.forEach((infrastructure) => {
        infrastructureImpacted?.push(
          this.formBuilder.formGroup(
            new ImpactedInfrastructureForm(infrastructure)
          ),
          { emitEvent: false }
        );
      });
      const proposedActivitiesArray = this.fullProposalForm
        .get('projectPlan')
        ?.get('proposedActivities') as FormArray;
      if (response.proposedActivities?.length! > 0) {
        proposedActivitiesArray.clear({ emitEvent: false });
      }
      response.proposedActivities?.forEach((activity) => {
        proposedActivitiesArray?.push(
          this.formBuilder.formGroup(new ProposedActivityForm(activity)),
          { emitEvent: false }
        );
      });
      const fundingInformationItemFormArray = this.fullProposalForm
        .get('budget')
        ?.get('otherFunding') as FormArray;
      if (response.otherFunding?.length! > 0) {
        fundingInformationItemFormArray.clear({ emitEvent: false });
      }
      response.otherFunding?.forEach((funding) => {
        fundingInformationItemFormArray?.push(
          this.formBuilder.formGroup(new FundingInformationItemForm(funding)),
          { emitEvent: false }
        );
      });
      const yearOverYearFormArray = this.fullProposalForm
        .get('budget')
        ?.get('yearOverYearFunding') as FormArray;
      if (response.yearOverYearFunding?.length! > 0) {
        yearOverYearFormArray.clear({ emitEvent: false });
      }
      response.yearOverYearFunding?.forEach((funding) => {
        yearOverYearFormArray?.push(
          this.formBuilder.formGroup(new YearOverYearFundingForm(funding)),
          { emitEvent: false }
        );
      });
      const standardsFormArray = this.fullProposalForm
        .get('permitsRegulationsAndStandards')
        ?.get('standards') as FormArray;
      this.optionsStore.standards?.()?.forEach((standard) => {
        const standardsInfo = response.standards?.find(
          (s) => s.category === standard.category
        );
        const standardInfo = new StandardInfoForm({
          isCategorySelected: standardsInfo?.isCategorySelected,
          category: standard.category,
          standards: standardsInfo?.isCategorySelected
            ? standardsInfo?.standards ?? []
            : [],
        });
        const standardInfoForm = this.formBuilder.formGroup(standardInfo);
        standardsFormArray?.push(standardInfoForm, { emitEvent: false });
      });
    });
  }

  goBack() {
    this.router.navigate(['/submissions']);
  }
}
