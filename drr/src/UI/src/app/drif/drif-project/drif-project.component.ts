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
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { ProjectService } from '../../../api/project/project.service';
import {
  Attachment,
  ClaimStatus,
  ContactDetails,
  DraftDrrProject,
  ForecastStatus,
  InterimReport,
  InterimReportStatus,
  PaymentCondition,
  ProgressReportStatus,
} from '../../../model';
import { DrrInputComponent } from '../../shared/controls/drr-input/drr-input.component';
import { DrifProjectContactDialogComponent } from './drif-project-contact-dialog.component';

export class CostProjectionItem {
  fiscalYear?: string;
  originalForecast?: number;
  currentForecast?: number;
}

export enum InterimSubReportSection {
  Progress = 'Progress',
  Claim = 'Claim',
  Forecast = 'Forecast',
}

export interface InterimSubReport {
  id?: string;
  name?: string;
  parentId?: string;
  section?: InterimSubReportSection;
  status?: string;
  dueDate?: string;
  submittedDate?: string;
  actions?: string[];
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
    MatDialogModule,
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
  matDialog = inject(MatDialog);
  translocoService = inject(TranslocoService);

  projectId?: string;

  project?: DraftDrrProject;

  conditionsDataSource = new MatTableDataSource<
    PaymentCondition & { actions?: [] }
  >([]);

  costProjectionsDataSource = new MatTableDataSource<CostProjectionItem>([]);

  expandedInterimReport?: InterimReport | null;

  projectContactsDataSource = new MatTableDataSource<ContactDetails>([]);

  interimReportsDataSource = new MatTableDataSource<InterimSubReport>([]);
  pastReportsDataSource = new MatTableDataSource<InterimReport>([]);

  attachmentsDataSource = new MatTableDataSource<Attachment>([]);

  private progressReadonlyStatuses: ProgressReportStatus[] = [
    ProgressReportStatus.Submitted,
    ProgressReportStatus.Approved,
    ProgressReportStatus.Skipped,
  ];

  private claimReadonlyStatuses: ClaimStatus[] = [
    ClaimStatus.Submitted,
    ClaimStatus.Approved,
    ClaimStatus.Skipped,
  ];

  private forecastReadonlyStatuses: ForecastStatus[] = [
    ForecastStatus.Submitted,
    ForecastStatus.Approved,
    ForecastStatus.Skipped,
  ];

  ngOnInit() {
    this.projectId = this.route.snapshot.params['projectId'];

    this.projectService
      .projectGetProject(this.projectId!)
      .subscribe((project) => {
        this.project = project;

        this.conditionsDataSource.data = [...this.project!.conditions!];

        // TODO: Remove hardcoded data after API provides this information
        this.costProjectionsDataSource.data = [
          {
            fiscalYear: '2021/2022',
            originalForecast: 100000,
            currentForecast: 120000,
          },
          {
            fiscalYear: '2022/2023',
            originalForecast: 150000,
            currentForecast: 180000,
          },
          {
            fiscalYear: '2023/2024',
            originalForecast: 200000,
            currentForecast: 220000,
          },
        ];

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
            name: `${report.reportPeriod} Interim Report`,
          });
          if (report.progressReport) {
            subReportsDue.push({
              id: report.progressReport?.id,
              name: `${report.reportPeriod} Progress`,
              parentId: report.id,
              section: InterimSubReportSection.Progress,
              status: this.translocoService.translate(
                `project.${report.progressReport?.status!}`,
              ),
              dueDate: report.dueDate,
              submittedDate: report.progressReport?.dateSubmitted,
              actions: this.progressReadonlyStatuses.includes(
                report.progressReport?.status!,
              )
                ? ['view']
                : ['edit'],
            });
          }
          if (report.projectClaim) {
            subReportsDue.push({
              id: report.projectClaim?.id,
              name: `${report.reportPeriod} Claim`,
              parentId: report.id,
              section: InterimSubReportSection.Claim,
              status: this.translocoService.translate(
                `project.${report.projectClaim?.status!}`,
              ),
              dueDate: report.dueDate,
              submittedDate: report.projectClaim?.dateSubmitted,
              actions: this.claimReadonlyStatuses.includes(
                report.projectClaim?.status!,
              )
                ? ['view']
                : ['edit'],
            });
          }
          if (report.forecast) {
            subReportsDue.push({
              id: report.forecast?.id,
              name: `${report.reportPeriod} Forecast`,
              parentId: report.id,
              section: InterimSubReportSection.Forecast,
              status: this.translocoService.translate(
                `project.${report.forecast?.status!}`,
              ),
              dueDate: report.dueDate,
              submittedDate: report.forecast?.dateSubmitted,
              actions: this.forecastReadonlyStatuses.includes(
                report.forecast?.status!,
              )
                ? ['view']
                : ['edit'],
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

  getSubReportRoute(subReport: InterimSubReport, action: string) {
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
      action,
    ];
  }

  addProjectContact() {
    this.matDialog
      .open(DrifProjectContactDialogComponent, {
        data: {
          firstName: 'Vasa',
        },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result) {
          console.log(result);
        }
      });
  }

  editClaim() {}

  deleteClaim() {}

  editProgressReport() {}

  deleteProgressReport() {}

  editForecast() {}

  deleteForecast() {}
}
