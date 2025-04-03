import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormArray, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { DrifapplicationService } from '../../../../api/drifapplication/drifapplication.service';
import { SubmissionPortalStatus } from '../../../../model';
import { ROUTE_PATH } from '../../../app.routes';
import { ProfileStore } from '../../../store/profile.store';
import {
  ContactDetailsForm,
  EOIApplicationForm,
  FundingInformationItemForm,
  InfrastructureImpactedForm,
  StringItem,
} from '../drif-eoi-form';
import { DrifEoiSummaryComponent } from '../drif-eoi-summary/drif-eoi-summary.component';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drif-eoi-summary',
  standalone: true,
  imports: [
    CommonModule,
    DrifEoiSummaryComponent,
    FormsModule,
    ReactiveFormsModule,
    MatButtonModule,
    TranslocoModule,
  ],
  providers: [RxFormBuilder],
  templateUrl: './drif-eoi-view.component.html',
  styleUrl: './drif-eoi-view.component.scss',
})
export class DrifEoiViewComponent {
  applicationService = inject(DrifapplicationService);
  route = inject(ActivatedRoute);
  router = inject(Router);
  formBuilder = inject(RxFormBuilder);
  profileStore = inject(ProfileStore);

  id!: string;
  eoiApplicationForm = this.formBuilder.formGroup(
    EOIApplicationForm,
  ) as IFormGroup<EOIApplicationForm>;
  fpId?: string;
  status?: SubmissionPortalStatus;

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    this.id = id;

    this.applicationService
      .dRIFApplicationGetEOI(id)
      .subscribe((application) => {
        this.fpId = application.fpId;
        this.status = application.status;

        // transform application into step forms
        const eoiApplicationForm: EOIApplicationForm = {
          proponentInformation: {
            proponentType: application.proponentType,
            additionalContacts: application.additionalContacts,
            partneringProponents: application.partneringProponents,
            projectContact: application.projectContact,
          },
          projectInformation: {
            stream: application.stream,
            projectTitle: application.projectTitle,
            scopeStatement: application.scopeStatement,
            fundingStream: application.fundingStream,
            relatedHazards: application.relatedHazards,
            otherHazardsDescription: application.otherHazardsDescription,
            startDate: application.startDate,
            endDate: application.endDate,
          },
          fundingInformation: {
            fundingRequest: application.fundingRequest,
            remainingAmount: application.remainingAmount,
            intendToSecureFunding: application.intendToSecureFunding,
            estimatedTotal: application.estimatedTotal,
          },
          locationInformation: {
            ownershipDeclaration: application.ownershipDeclaration,
            ownershipDescription: application.ownershipDescription,
            locationDescription: application.locationDescription,
          },
          projectDetails: {
            additionalBackgroundInformation:
              application.additionalBackgroundInformation,
            additionalSolutionInformation:
              application.additionalSolutionInformation,
            addressRisksAndHazards: application.addressRisksAndHazards,
            disasterRiskUnderstanding: application.disasterRiskUnderstanding,
            projectDescription: application.projectDescription,
            estimatedPeopleImpacted: application.estimatedPeopleImpacted,
            communityImpact: application.communityImpact,
            isInfrastructureImpacted: application.isInfrastructureImpacted,
            infrastructureImpacted: application.infrastructureImpacted,
            rationaleForFunding: application.rationaleForFunding,
            rationaleForSolution: application.rationaleForSolution,
          },
          engagementPlan: {
            additionalEngagementInformation:
              application.additionalEngagementInformation,
            firstNationsEngagement: application.firstNationsEngagement,
            neighbourEngagement: application.neighbourEngagement,
          },
          otherSupportingInformation: {
            climateAdaptation: application.climateAdaptation,
            otherInformation: application.otherInformation,
          },
          declaration: {
            authorizedRepresentative: application.authorizedRepresentative,
          },
        };

        this.eoiApplicationForm.patchValue(eoiApplicationForm, {
          emitEvent: false,
        });

        this.eoiApplicationForm
          .get('proponentInformation')
          ?.get('proponentName')
          ?.setValue(this.profileStore.organization(), { emitEvent: false });

        const additionalContactsArray = this.eoiApplicationForm.get(
          'proponentInformation.additionalContacts',
        ) as FormArray;
        additionalContactsArray.clear({ emitEvent: false });
        application.additionalContacts?.forEach((contact) => {
          additionalContactsArray?.push(
            this.formBuilder.formGroup(new ContactDetailsForm(contact)),
            { emitEvent: false },
          );
        });

        const partneringProponentsArray = this.eoiApplicationForm.get(
          'proponentInformation.partneringProponentsArray',
        ) as FormArray;
        partneringProponentsArray.clear({ emitEvent: false });
        application.partneringProponents?.forEach((proponent) => {
          partneringProponentsArray?.push(
            this.formBuilder.formGroup(new StringItem({ value: proponent })),
            { emitEvent: false },
          );
        });

        const fundingInformationItemFormArray = this.eoiApplicationForm.get(
          'fundingInformation.otherFunding',
        ) as FormArray;
        fundingInformationItemFormArray.clear({ emitEvent: false });
        application.otherFunding?.forEach((funding) => {
          fundingInformationItemFormArray?.push(
            this.formBuilder.formGroup(new FundingInformationItemForm(funding)),
            { emitEvent: false },
          );
        });

        const infrastructureImpacted = this.eoiApplicationForm.get(
          'projectDetails.infrastructureImpacted',
        ) as FormArray;
        infrastructureImpacted.clear({ emitEvent: false });
        application.infrastructureImpacted?.forEach((infrastructure) => {
          infrastructureImpacted?.push(
            this.formBuilder.formGroup(
              new InfrastructureImpactedForm(infrastructure),
            ),
            { emitEvent: false },
          );
        });
      });
  }

  createFP() {
    this.router.navigate([
      '/drif-fp-screener',
      this.id,
      this.eoiApplicationForm?.value?.projectInformation?.projectTitle,
    ]);
  }

  goBack() {
    this.router.navigate([ROUTE_PATH.SUBMISSIONS]);
  }

  canCreateFP() {
    return this.status === SubmissionPortalStatus.EligibleInvited && !this.fpId;
  }
}
