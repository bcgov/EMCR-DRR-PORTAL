import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { ProjectService } from '../../../../../api/project/project.service';
import { DraftProjectClaim } from '../../../../../model';
import { ClaimForm } from '../drif-claim-form';
import { DrifClaimSummaryComponent } from '../drif-claim-summary/drif-claim-summary.component';

@Component({
  selector: 'drr-drif-claim',
  standalone: true,
  imports: [CommonModule, DrifClaimSummaryComponent],
  templateUrl: './drif-claim.component-view.html',
  styleUrl: './drif-claim.component-view.scss',
  providers: [RxFormBuilder],
})
export class DrifClaimComponent {
  route = inject(ActivatedRoute);
  router = inject(Router);
  projectService = inject(ProjectService);
  formBuilder = inject(RxFormBuilder);

  projectId?: string;
  reportId?: string;
  claimId?: string;

  reportName?: string;

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

            // this.projectType = claim.projectType!;

            const formData = new ClaimForm({
              expenditure: {
                skipClaimReport: claim.skipClaim,
                claimComment: claim.claimComment,
                invoices: claim.invoices,
                totalClaimed: claim.totalClaimed,
                totalProjectAmount: claim.totalProjectAmount,
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

            resolve();
          },
          error: (error) => {
            reject(error);
          },
        });
    });
  }

  goBack() {
    this.router.navigate(['projects', this.projectId]);
  }
}
