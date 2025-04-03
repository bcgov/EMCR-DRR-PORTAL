import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';

import { MatDividerModule } from '@angular/material/divider';
import { UntilDestroy } from '@ngneat/until-destroy';
import { DrifapplicationService } from '../../../../api/drifapplication/drifapplication.service';
import { ScreenerQuestions } from '../../../../model';
import { ROUTE_PATH } from '../../../app.routes';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-drif-fp-instructions',
  standalone: true,
  imports: [CommonModule, MatButtonModule, TranslocoModule, MatDividerModule],
  templateUrl: './drif-fp-instructions.component.html',
  styleUrl: './drif-fp-instructions.component.scss',
})
export class DrifFpInstructionsComponent {
  router = inject(Router);
  route = inject(ActivatedRoute);
  appService = inject(DrifapplicationService);

  eoiId?: string;
  projectTitle?: string;
  screenerQuestions?: ScreenerQuestions;

  ngOnInit() {
    this.eoiId = this.route.snapshot.params['eoiId'];
    this.projectTitle = this.route.snapshot.params['projectTitle'];
    const queryParams = this.route.snapshot.queryParams;
    this.screenerQuestions = {
      costEstimate: this.convertToBoolean(queryParams['costEstimate']),
      engagedWithFirstNationsOccurred: this.convertToBoolean(
        queryParams['engagedWithFirstNationsOccurred'],
      ),
      firstNationsAuthorizedByPartners:
        queryParams['firstNationsAuthorizedByPartners'],
      foundationWorkCompleted: queryParams['foundationWorkCompleted'],
      haveAuthorityToDevelop: this.convertToBoolean(
        queryParams['haveAuthorityToDevelop'],
      ),
      incorporateFutureClimateConditions: this.convertToBoolean(
        queryParams['incorporateFutureClimateConditions'],
      ),
      localGovernmentAuthorizedByPartners:
        queryParams['localGovernmentAuthorizedByPartners'],
      meetsEligibilityRequirements: this.convertToBoolean(
        queryParams['meetsEligibilityRequirements'],
      ),
      meetsRegulatoryRequirements: this.convertToBoolean(
        queryParams['meetsRegulatoryRequirements'],
      ),
      projectSchedule: this.convertToBoolean(queryParams['projectSchedule']),
      projectWorkplan: this.convertToBoolean(queryParams['projectWorkplan']),
      sitePlan: queryParams['sitePlan'],
    };
  }

  private convertToBoolean(queryParam: any): boolean | undefined {
    return queryParam ? Boolean(JSON.parse(queryParam)) : undefined;
  }

  goBack() {
    this.router.navigate([ROUTE_PATH.SUBMISSIONS]);
  }

  continue() {
    this.appService
      .dRIFApplicationCreateFPFromEOI(this.screenerQuestions!, {
        eoiId: this.eoiId!,
      })
      .subscribe((res) => {
        this.router.navigate(['/drif-fp', res.id]);
      });
  }

  getRelatedEOILink() {
    return `/eoi-submission-details/${this.eoiId}`;
  }
}
