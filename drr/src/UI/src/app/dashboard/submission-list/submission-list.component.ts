import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { Router, RouterModule } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import { DrifapplicationService } from '../../../api/drifapplication/drifapplication.service';
import { FundingStream, Submission } from '../../../model';

@Component({
  selector: 'drr-submission-list',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    TranslocoModule,
    RouterModule,
  ],
  templateUrl: './submission-list.component.html',
  styleUrl: './submission-list.component.scss',
})
export class SubmissionListComponent {
  router = inject(Router);
  applicationService = inject(DrifapplicationService);

  submissions?: Submission[];

  submissionListColumns = [
    'id',
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
    showPaginator: false,
  };
  sort: Sort = {
    active: '',
    direction: '',
  };

  ngOnInit() {
    this.applicationService.dRIFApplicationGet().subscribe((submissions) => {
      this.submissions = submissions;
      this.submissionListDataSource = new MatTableDataSource(this.submissions);
      this.paginator.length = submissions.length;
    });
  }

  onPageChange(event: PageEvent) {
    this.paginator.pageIndex = event.pageIndex;
    this.paginator.pageSize = event.pageSize;

    // TODO: call API with
  }

  onSortSubmissionTable(sort: Sort) {
    this.sort = sort;

    // TODO: call API with
  }

  onCreateFormClick() {
    this.router.navigate(['/drif-eoi']);
  }

  onViewFormClick(submission: Submission, event: Event) {
    event.preventDefault();

    submission.applicationType === 'EOI'
      ? this.router.navigate([
          submission.status == 'Draft'
            ? '/drif-eoi'
            : '/eoi-submission-details',
          submission.id,
        ])
      : this.router.navigate([
          submission.status == 'Draft' ? '/drif-fp' : '/fp-submission-details',
          submission.id,
        ]);
  }

  canCreateFullProposal(submission: Submission) {
    return submission.status === 'EligibleInvited' && !submission.existingFpId;
  }

  createFullProposal(submission: Submission, event: Event) {
    event.preventDefault();

    this.router.navigate(['/drif-fp-instructions', submission.id], {
      queryParams: {
        fundingStream: submission.fundingStream,
      },
    });
  }
}
