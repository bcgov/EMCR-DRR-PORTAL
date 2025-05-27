import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { ProjectService } from '../../../../../api/project/project.service';
import { DraftProjectClaim } from '../../../../../model';
import { ClaimForm } from '../drif-claim-form';
import { DrifClaimSummaryComponent } from '../drif-claim-summary/drif-claim-summary.component';

@Component({
  selector: 'drr-drif-claim',
  standalone: true,
  imports: [
    CommonModule,
    DrifClaimSummaryComponent,
    TranslocoModule,
    MatButtonModule,
  ],
  templateUrl: './drif-claim-view.component.html',
  styleUrl: './drif-claim-view.component.scss',
  providers: [RxFormBuilder],
})
export class DrifClaimViewComponent {
  route = inject(ActivatedRoute);
  router = inject(Router);
  projectService = inject(ProjectService);
  formBuilder = inject(RxFormBuilder);

  projectId?: string;
  reportId?: string;
  claimId?: string;

  reportName?: string;
  reportingPeriod?: string;

  claimForm?: IFormGroup<ClaimForm>;

  ngOnInit() {
    this.route.paramMap.subscribe((params) => {
      this.projectId = params.get('projectId')!;
      this.reportId = params.get('reportId')!;
      this.claimId = params.get('claimId')!;

      this.load();
    });
  }

  load(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.projectService
        .projectGetClaim(this.projectId!, this.reportId!, this.claimId!)
        .subscribe({
          next: (claim: DraftProjectClaim) => {
            this.reportName = `${claim.reportPeriod} Claim`;
            this.reportingPeriod = claim.reportPeriod;

            const formData = new ClaimForm({
              expenditure: {
                haveClaimExpenses: claim.haveClaimExpenses,
                claimComment: claim.claimComment,
                invoices: claim.invoices,
                totalClaimed: claim.totalClaimed,
                totalProjectAmount: claim.totalProjectAmount,
                upFrontPaymentInterest: claim.upFrontPaymentInterest,
              },
              declaration: {
                authorizedRepresentativeStatement: false,
                informationAccuracyStatement: false,
                authorizedRepresentative: claim.authorizedRepresentative,
              },
            } as ClaimForm);

            this.claimForm = this.formBuilder.formGroup(
              ClaimForm,
              formData,
            ) as IFormGroup<ClaimForm>;

            this.configureInterestControls();

            resolve();
          },
          error: (error) => {
            reject(error);
          },
        });
    });
  }

  goBack() {
    this.router.navigate(['drif-projects', this.projectId]);
  }

  configureInterestControls() {
    if (this.showEarnedInterestControls()) {
      const upFrontPaymentInterestControl = this.claimForm?.get(
        'expenditure.upFrontPaymentInterest',
      );
      upFrontPaymentInterestControl?.setValidators([Validators.required]);
      upFrontPaymentInterestControl?.updateValueAndValidity();
    }
  }

  showEarnedInterestControls() {
    const q2 = 'Q2';
    const q4 = 'Q4';

    if (!this.reportingPeriod) {
      return false;
    }

    return (
      this.reportingPeriod.includes(q2) || this.reportingPeriod.includes(q4)
    );
  }
}
