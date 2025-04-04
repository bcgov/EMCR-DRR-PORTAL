import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormArray, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { ProjectService } from '../../../../api/project/project.service';
import { DraftProgressReport } from '../../../../model';
import { ProgressReportForm } from './drif-progress-report-form';
import { DrifProgressReportSummaryComponent } from './drif-progress-report-summary/drif-progress-report-summary.component';

@Component({
  selector: 'drr-drif-progress-report-view',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatButtonModule,
    TranslocoModule,
    DrifProgressReportSummaryComponent,
  ],
  templateUrl: './drif-progress-report-view.component.html',
  styleUrl: './drif-progress-report-view.component.scss',
  providers: [RxFormBuilder],
})
export class DrifProgressReportViewComponent {
  route = inject(ActivatedRoute);
  router = inject(Router);
  projectService = inject(ProjectService);
  formBuilder = inject(RxFormBuilder);

  projectId!: string;
  reportId!: string;
  progressReportId!: string;

  reportName?: string;

  progressReportForm = this.formBuilder.formGroup(
    ProgressReportForm,
    {},
  ) as IFormGroup<ProgressReportForm>;

  get workplanArray(): FormArray | null {
    return this.progressReportForm?.get(
      'workplan.workplanActivities',
    ) as FormArray;
  }

  get signageArray(): FormArray | null {
    return this.progressReportForm?.get('workplan.fundingSignage') as FormArray;
  }

  get attachmentArray(): FormArray {
    return this.progressReportForm.get('attachments') as FormArray;
  }

  get pastEventsArray(): FormArray | null {
    return this.progressReportForm?.get(
      'eventInformation.pastEvents',
    ) as FormArray;
  }

  get upcomingEventsArray(): FormArray | null {
    return this.progressReportForm?.get(
      'eventInformation.upcomingEvents',
    ) as FormArray;
  }

  ngOnInit() {
    this.route.params.subscribe((params) => {
      this.projectId = params['projectId'];
      this.reportId = params['reportId'];
      this.progressReportId = params['progressReportId'];

      this.projectService
        .projectGetProgressReport(
          this.projectId,
          this.reportId,
          this.progressReportId,
        )
        .subscribe((report: DraftProgressReport) => {
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
        });
    });
  }

  goBack() {
    this.router.navigate(['/drif-projects', this.projectId]);
  }
}
