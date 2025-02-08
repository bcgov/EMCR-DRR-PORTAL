import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { AbstractControl, FormArray } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { ActivityType, WorkplanStatus } from '../../../../../model';
import { FileService } from '../../../../shared/services/file.service';
import { SummaryItemComponent } from '../../../summary-item/summary-item.component';
import {
  ProgressReportForm,
  WorkplanActivityForm,
} from '../drif-progress-report-form';

@Component({
  selector: 'drif-progress-report-summary',
  standalone: true,
  imports: [
    CommonModule,
    SummaryItemComponent,
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    TranslocoModule,
  ],
  templateUrl: './drif-progress-report-summary.component.html',
  styleUrl: './drif-progress-report-summary.component.scss',
})
export class DrifProgressReportSummaryComponent {
  translocoService = inject(TranslocoService);
  formBuilder = inject(RxFormBuilder);
  fileService = inject(FileService);

  @Input() progressReportForm!: IFormGroup<ProgressReportForm>;
  @Input() isReadOnlyView = true;

  getWorkplanItems(): FormArray | null {
    return this.progressReportForm.get(
      'workplan.workplanActivities',
    ) as FormArray;
  }

  getPreDefinedActivitiesArray() {
    return this.getWorkplanItems()?.controls.filter(
      (control) =>
        control.get('preCreatedActivity')?.value &&
        control.get('activity')?.value !== ActivityType.PermitToConstruct &&
        control.get('activity')?.value !==
          ActivityType.ConstructionContractAward,
    );
  }

  getMilestoneActivitiesArray() {
    return this.getWorkplanItems()?.controls.filter(
      (control) =>
        control.get('preCreatedActivity')?.value &&
        (control.get('activity')?.value === ActivityType.PermitToConstruct ||
          control.get('activity')?.value ===
            ActivityType.ConstructionContractAward),
    );
  }

  getAdditionalActivitiesArray() {
    return this.getWorkplanItems()
      ?.controls.filter((control) => !control.get('preCreatedActivity')?.value)
      .sort((a, b) => {
        const aMandatory = a.get('isMandatory')?.value;
        const bMandatory = b.get('isMandatory')?.value;

        if (aMandatory && !bMandatory) {
          return -1;
        }

        if (!aMandatory && bMandatory) {
          return 1;
        }

        return 0;
      });
  }

  getSignageFormArray() {
    return this.progressReportForm?.get('workplan.fundingSignage') as FormArray;
  }

  showPlannedStartDate(activityControl: AbstractControl<WorkplanActivityForm>) {
    const status = activityControl?.get('status')?.value as WorkplanStatus;
    return (
      status === WorkplanStatus.NotStarted ||
      status === WorkplanStatus.NotAwarded
    );
  }

  showPlannedCompletionDate(
    activityControl: AbstractControl<WorkplanActivityForm>,
  ) {
    const status = activityControl?.get('status')?.value as WorkplanStatus;
    return (
      status === WorkplanStatus.NotStarted ||
      status === WorkplanStatus.InProgress
    );
  }

  showActualStartDate(activityControl: AbstractControl<WorkplanActivityForm>) {
    const status = activityControl?.get('status')?.value as WorkplanStatus;
    return (
      status === WorkplanStatus.InProgress ||
      status === WorkplanStatus.Completed ||
      status === WorkplanStatus.Awarded
    );
  }

  showActualCompletionDate(
    activityControl: AbstractControl<WorkplanActivityForm>,
  ) {
    const status = activityControl?.get('status')?.value as WorkplanStatus;
    return status === WorkplanStatus.Completed;
  }

  getPastEventsArray() {
    return this.progressReportForm?.get(
      'eventInformation.pastEvents',
    ) as FormArray;
  }

  getFutureEventsArray() {
    return this.progressReportForm?.get(
      'eventInformation.futureEvents',
    ) as FormArray;
  }
}
