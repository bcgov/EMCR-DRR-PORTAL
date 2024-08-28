import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormArray, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import {
  IFormGroup,
  RxFormBuilder,
  RxFormGroup,
} from '@rxweb/reactive-form-validators';
import { DrifapplicationService } from '../../../../api/drifapplication/drifapplication.service';
import { ProfileStore } from '../../../store/profile.store';
import {
  EOIApplicationForm,
  FundingInformationItemForm,
  StringItem,
} from '../drif-eoi-form';
import { DrifEoiSummaryComponent } from '../drif-eoi-summary/drif-eoi-summary.component';

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
    EOIApplicationForm
  ) as IFormGroup<EOIApplicationForm>;
  fpId?: string;

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    this.id = id;

    this.applicationService
      .dRIFApplicationGetEOI(id)
      .subscribe((application) => {
        this.fpId = application.fpId;

        // transform application into step forms
        const eoiApplicationForm: EOIApplicationForm = {
          proponentInformation: {
            proponentType: application.proponentType,
            additionalContacts: application.additionalContacts,
            partneringProponents: application.partneringProponents,
            projectContact: application.projectContact,
          },
          projectInformation: {
            projectType: application.projectType,
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
            drifProgramGoalAlignment: application.drifProgramGoalAlignment,
            estimatedPeopleImpacted: application.estimatedPeopleImpacted,
            communityImpact: application.communityImpact,
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
            submitter: application.submitter,
          },
        };

        this.eoiApplicationForm.patchValue(eoiApplicationForm, {
          emitEvent: false,
        });

        this.eoiApplicationForm
          .get('proponentInformation')
          ?.get('proponentName')
          ?.setValue(this.profileStore.organization(), { emitEvent: false });

        const partneringProponentsArray = this.getFormGroup(
          'proponentInformation'
        ).get('partneringProponentsArray') as FormArray;
        partneringProponentsArray.clear({ emitEvent: false });
        application.partneringProponents?.forEach((proponent) => {
          partneringProponentsArray?.push(
            this.formBuilder.formGroup(new StringItem({ value: proponent })),
            { emitEvent: false }
          );
        });

        const fundingInformationItemFormArray = this.getFormGroup(
          'fundingInformation'
        ).get('otherFunding') as FormArray;
        fundingInformationItemFormArray.clear({ emitEvent: false });
        application.otherFunding?.forEach((funding) => {
          fundingInformationItemFormArray?.push(
            this.formBuilder.formGroup(new FundingInformationItemForm(funding)),
            { emitEvent: false }
          );
        });

        const infrastructureImpactedArray = this.getFormGroup(
          'projectDetails'
        ).get('infrastructureImpactedArray') as FormArray;
        infrastructureImpactedArray.clear({ emitEvent: false });
        application.infrastructureImpacted?.forEach((infrastructure) => {
          infrastructureImpactedArray?.push(
            this.formBuilder.formGroup(
              new StringItem({ value: infrastructure })
            ),
            { emitEvent: false }
          );
        });
      });
  }

  getFormGroup(groupName: string) {
    return this.eoiApplicationForm?.get(groupName) as RxFormGroup;
  }

  createFP() {
    this.router.navigate(['/drif-fp-instructions', this.id], {
      queryParams: {
        fundingStream:
          this.eoiApplicationForm?.value?.projectInformation?.fundingStream,
      },
    });
  }

  goBack() {
    this.router.navigate(['/dashboard']);
  }
}
