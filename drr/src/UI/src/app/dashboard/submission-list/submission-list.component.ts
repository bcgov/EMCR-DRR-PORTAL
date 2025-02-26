import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { Router, RouterModule } from '@angular/router';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { prop, RxFormBuilder } from '@rxweb/reactive-form-validators';
import {
  ConditionalOperator,
  GridifyQueryBuilder,
  IGridifyQuery,
} from 'gridify-client';
import { distinctUntilChanged } from 'rxjs';
import { DrifapplicationService } from '../../../api/drifapplication/drifapplication.service';
import {
  ApplicationType,
  ProgramType,
  Submission,
  SubmissionPortalStatus,
} from '../../../model';
import { DrrSelectComponent } from '../../shared/controls/drr-select/drr-select.component';
import { ConfigurationStore } from '../../store/configuration.store';

class SubmissionFilter {
  @prop()
  programType?: ProgramType;

  @prop()
  applicationType?: ApplicationType;

  @prop()
  status?: SubmissionPortalStatus[];
}

export enum DialogResponse {
  Yes = 'Yes',
  No = 'No',
}

@UntilDestroy()
@Component({
  selector: 'drr-dialog',
  standalone: true,
  template: `<h2 mat-dialog-title class="dialog-title">
      <mat-icon style="color: #e6a700">warning</mat-icon>
      <span style="margin-left: 1rem;">{{ data.title }}</span>
    </h2>
    <mat-divider></mat-divider>
    <mat-dialog-content style="color: inherit;">{{
      data.text
    }}</mat-dialog-content>
    <mat-divider></mat-divider>
    <mat-dialog-actions class="dialog-actions">
      <button mat-stroked-button [mat-dialog-close]="confirm">Yes</button>
      <button mat-button [mat-dialog-close]="cancel" cdkFocusInitial>No</button>
    </mat-dialog-actions>`,
  styles: [
    `
      .dialog-title {
        display: flex;
        align-items: flex-end;
      }

      .dialog-actions {
        justify-content: flex-end;
      }
    `,
  ],
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
  ],
})
export class DrrDialogComponent {
  dialogRef = inject(MatDialogRef<DrrDialogComponent>);
  data = inject(MAT_DIALOG_DATA);

  confirm = DialogResponse.Yes;
  cancel = DialogResponse.No;
}

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-submission-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    TranslocoModule,
    RouterModule,
    DrrSelectComponent,
  ],
  templateUrl: './submission-list.component.html',
  styleUrl: './submission-list.component.scss',
  providers: [RxFormBuilder, GridifyQueryBuilder],
})
export class SubmissionListComponent {
  router = inject(Router);
  applicationService = inject(DrifapplicationService);
  formbuilder = inject(RxFormBuilder);
  translocoService = inject(TranslocoService);
  matDialog = inject(MatDialog);
  breakpointObserver = inject(BreakpointObserver);
  configStore = inject(ConfigurationStore);

  programTypeOptions = Object.values(ProgramType).map((value) => ({
    value,
    label: this.translocoService.translate(value),
  }));
  applicationTypeOptions = Object.values(ApplicationType).map((value) => ({
    value,
    label: this.translocoService.translate(value),
  }));
  exludedStatuses: SubmissionPortalStatus[] = [
    SubmissionPortalStatus.ApprovedInPrinciple,
    SubmissionPortalStatus.Deleted,
  ];
  statusOptions = Object.values(SubmissionPortalStatus)
    .filter((status) => !this.exludedStatuses.includes(status))
    .map((value) => ({
      value,
      label: this.translocoService.translate(value),
    }))
    .sort((a, b) => a.label.localeCompare(b.label));

  filterApplied = false;
  filterForm = this.formbuilder.formGroup(SubmissionFilter);

  submissions?: Submission[];
  submissionListColumns = [
    'id',
    'programType',
    'applicationType',
    'projectTitle',
    'status',
    'fundingRequest',
    'modifiedDate',
    'submittedDate',
    'partneringProponents',
    'actions',
  ];
  submissionListDataSource = new MatTableDataSource<Submission>();

  paginator = {
    length: 0,
    pageSize: 10,
    pageSizeOptions: [5, 10, 25, 100],
    pageIndex: 0,
  };
  sort: Sort = {
    active: 'id',
    direction: 'desc',
  };

  isMobile = false;

  ngOnInit() {
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .subscribe(({ matches }) => {
        this.isMobile = !matches;
      });

    this.load();

    this.filterForm.valueChanges.pipe(distinctUntilChanged()).subscribe(() => {
      this.filterApplied = false;
    });
  }

  load() {
    const query = this.getQuery();

    this.applicationService
      .dRIFApplicationGet({
        Filter: query.filter,
        OrderBy: query.orderBy,
        Page: query.page,
        PageSize: query.pageSize,
      })
      .subscribe((response) => {
        this.submissions = response.submissions;
        this.submissionListDataSource = new MatTableDataSource(
          this.submissions,
        );
        this.paginator.length = response.length!;
      });
  }

  onPageChange(event: PageEvent) {
    this.paginator.pageIndex = event.pageIndex;
    this.paginator.pageSize = event.pageSize;

    this.load();
  }

  onSortSubmissionTable(sort: Sort) {
    if (sort.direction === '') {
      this.sort.active = 'id';
      this.sort.direction = 'desc';
    } else {
      this.sort = sort;
    }

    this.paginator.pageIndex = 0;

    this.load();
  }

  onCreateFormClick() {
    this.router.navigate(['/drif-eoi']);
  }

  onViewFormClick(submission: Submission, event: Event) {
    event.preventDefault();

    submission.applicationType === 'EOI'
      ? this.router.navigate([
          submission.status == 'Draft' || submission.status == 'Withdrawn'
            ? '/drif-eoi'
            : '/eoi-submission-details',
          submission.id,
        ])
      : this.router.navigate([
          submission.status == 'Draft' || submission.status == 'Withdrawn'
            ? '/drif-fp'
            : '/fp-submission-details',
          submission.id,
        ]);
  }

  onDeleteClick(submission: Submission, event: Event) {
    event.preventDefault();

    this.matDialog
      .open(DrrDialogComponent, {
        data: {
          title: this.translocoService.translate('submission-list.deleteTitle'),
          text: this.translocoService.translate('submission-list.deleteText'),
        },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result) {
          if (result === DialogResponse.Yes) {
            this.deleteApplication(submission);
          }
        }
      });
  }

  deleteApplication(submission: Submission) {
    switch (submission.applicationType) {
      case ApplicationType.EOI:
        this.applicationService
          .dRIFApplicationDeleteApplication(submission.id!)
          .subscribe(() => {
            this.load();
          });
        break;
      case ApplicationType.FP:
        this.applicationService
          .dRIFApplicationDeleteFPApplication(submission.id!)
          .subscribe(() => {
            this.load();
          });
        break;
    }
  }

  onWithdrawClick(submission: Submission, event: Event) {
    event.preventDefault();

    this.matDialog
      .open(DrrDialogComponent, {
        data: {
          title: this.translocoService.translate(
            'submission-list.withdrawTitle',
          ),
          text: this.translocoService.translate(
            submission.applicationType === ApplicationType.EOI
              ? 'submission-list.withdrawTextEoi'
              : 'submission-list.withdrawTextFp',
          ),
        },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result) {
          if (result === DialogResponse.Yes) {
            this.withdrawApplication(submission);
          }
        }
      });
  }

  withdrawApplication(submission: Submission) {
    switch (submission.applicationType) {
      case ApplicationType.EOI:
        this.applicationService
          .dRIFApplicationWithdrawApplication(submission.id!)
          .subscribe(() => {
            this.load();
          });
        break;
      case ApplicationType.FP:
        this.applicationService
          .dRIFApplicationWithdrawFPApplication(submission.id!)
          .subscribe(() => {
            this.load();
          });
        break;
    }
  }

  createFullProposal(submission: Submission, event: Event) {
    event.preventDefault();

    this.router.navigate([
      '/drif-fp-screener',
      submission.id,
      submission.projectTitle,
    ]);
  }

  getQuery(): IGridifyQuery {
    const filters = this.filterForm.value as SubmissionFilter;

    const query = new GridifyQueryBuilder()
      .setPage(this.paginator.pageIndex)
      .setPageSize(this.paginator.pageSize)
      .addOrderBy(this.sort.active, this.sort.direction === 'desc');

    if (filters.programType) {
      query.addCondition(
        'programType',
        ConditionalOperator.Equal,
        filters.programType ?? '',
      );
    }

    if (filters.programType && filters.applicationType) {
      query.and();
    }

    if (filters.applicationType) {
      query.addCondition(
        'applicationType',
        ConditionalOperator.Equal,
        filters.applicationType ?? '',
      );
    }

    if (
      (filters.applicationType || filters.programType) &&
      filters.status &&
      filters.status.length > 0
    ) {
      query.and();
    }

    if (filters.status && filters.status.length > 0) {
      query.addCondition(
        'status',
        ConditionalOperator.Equal,
        filters.status.join('|'),
      );
    }

    return query.build();
  }

  applyFilters() {
    if (this.filterForm.pristine) {
      return;
    }

    this.filterApplied = true;
    this.paginator.pageIndex = 0;
    this.load();
  }

  clearFilters() {
    if (this.filterForm.pristine) {
      return;
    }

    this.paginator.pageIndex = 0;
    this.filterForm.reset({ emitEvent: false });
    this.filterForm.markAsPristine();
    this.filterApplied = false;
    this.load();
  }

  showGenerateTestSubmissionControls() {
    return this.configStore.configuration()!.testDataEndpointsEnabled;
  }

  generateTestEOI() {
  }

  generateTestFP() {
  }
}
