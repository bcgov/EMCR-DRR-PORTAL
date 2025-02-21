import {
  animate,
  state,
  style,
  transition,
  trigger,
} from '@angular/animations';
import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import { ProjectService } from '../../../api/project/project.service';
import {
  Attachment,
  ContactDetails,
  DraftDrrProject,
  InterimReport,
  InterimReportStatus,
  PaymentCondition,
} from '../../../model';
import { DrrInputComponent } from '../../shared/controls/drr-input/drr-input.component';

export enum InterimSubReportSection {
  Progress = 'Progress',
  Claim = 'Claim',
  Forecast = 'Forecast',
}

export interface InterimSubReport {
  id?: string;
  parentId?: string;
  section?: InterimSubReportSection;
  status?: string;
  dueDate?: string;
  submittedDate?: string;
  actions?: [];
}

@Component({
  selector: 'drr-drif-project',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatTableModule,
    MatIconModule,
    MatChipsModule,
    MatTabsModule,
    MatDividerModule,
    DrrInputComponent,
    TranslocoModule,
  ],
  templateUrl: './drif-project.component.html',
  styleUrl: './drif-project.component.scss',
  animations: [
    trigger('detailExpand', [
      state('collapsed,void', style({ height: '0px', minHeight: '0' })),
      state('expanded', style({ height: '*' })),
      transition(
        'expanded <=> collapsed',
        animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)'),
      ),
    ]),
  ],
})
export class DrifProjectComponent {
  route = inject(ActivatedRoute);
  router = inject(Router);
  projectService = inject(ProjectService);

  projectId?: string;

  project?: DraftDrrProject;

  conditionsDataSource = new MatTableDataSource<
    PaymentCondition & { actions?: [] }
  >([]);

  expandedInterimReport?: InterimReport | null;

  projectContactsDataSource = new MatTableDataSource<ContactDetails>([]);

  interimReportsDataSource = new MatTableDataSource<InterimSubReport>([]);
  pastReportsDataSource = new MatTableDataSource<InterimReport>([]);

  attachmentsDataSource = new MatTableDataSource<Attachment>([]);

  ngOnInit() {
    this.projectId = this.route.snapshot.params['projectId'];

    this.projectService
      .projectGetProject(this.projectId!)
      .subscribe((project) => {
        this.project = project;

        this.conditionsDataSource.data = [...this.project!.conditions!];

        this.projectContactsDataSource.data = this.project!.contacts!;

        const reportsDue = this.project!.interimReports!.filter(
          (report) =>
            report.status !== InterimReportStatus.Approved &&
            report.status !== InterimReportStatus.Skipped,
        );
        const subReportsDue: InterimSubReport[] = [];
        reportsDue.forEach((report) => {
          subReportsDue.push({
            id: report.id,
          });
          if (report.progressReport) {
            subReportsDue.push({
              id: report.progressReport?.id,
              parentId: report.id,
              section: InterimSubReportSection.Progress,
              status: report.progressReport?.status,
              dueDate: report.dueDate,
              submittedDate: report.progressReport?.dateSubmitted,
            });
          }
          if (report.projectClaim) {
            subReportsDue.push({
              id: report.projectClaim?.id,
              parentId: report.id,
              section: InterimSubReportSection.Claim,
              status: report.projectClaim?.status,
              dueDate: report.dueDate,
              // submittedDate: report.projectClaim?.submittedDate,
            });
          }
          if (report.forecast) {
            subReportsDue.push({
              id: report.forecast?.id,
              parentId: report.id,
              section: InterimSubReportSection.Forecast,
              status: report.forecast?.status,
              dueDate: report.dueDate,
              // submittedDate: report.forecast?.submittedDate,
            });
          }
        });

        this.interimReportsDataSource.data = subReportsDue;

        this.pastReportsDataSource.data = this.project!.interimReports!.filter(
          (report) =>
            report.status === InterimReportStatus.Approved ||
            report.status === InterimReportStatus.Skipped,
        );

        this.attachmentsDataSource.data = this.project!.attachments!;
      });
  }

  addInterimReport() {
    this.router.navigate([
      'drif-projects',
      this.projectId,
      'interim-reports',
      'create',
    ]);
  }

  getSubReportRoute(subReport: InterimSubReport) {
    const sectionToRouteMap = {
      [InterimSubReportSection.Progress]: 'progress-reports',
      [InterimSubReportSection.Claim]: 'claims',
      [InterimSubReportSection.Forecast]: 'forecasts',
    };

    return [
      '/drif-projects',
      this.projectId,
      'interim-reports',
      subReport.parentId,
      sectionToRouteMap[subReport.section!],
      subReport.id,
      'edit',
    ];
  }

  addProjectContact() {}

  editClaim() {}

  deleteClaim() {}

  editProgressReport() {}

  deleteProgressReport() {}

  editForecast() {}

  deleteForecast() {}

  viewCondition() {}

  clearCondition() {}
}
