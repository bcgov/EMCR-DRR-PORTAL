import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { DrrInputComponent } from '../../shared/controls/drr-input/drr-input.component';
import { DrrSelectComponent } from '../../shared/controls/drr-select/drr-select.component';
import {
  ProjectContactForm,
  ProjectRoleType,
} from './drif-project-contact-form';

@Component({
  selector: 'drif-project-contact-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatDividerModule,
    DrrInputComponent,
    DrrSelectComponent,
    TranslocoModule,
  ],
  providers: [RxFormBuilder],
  template: `
    <div *transloco="let t">
      <h2 mat-dialog-title>Contact Information</h2>
      <mat-divider></mat-divider>
      <mat-dialog-content>
        <div class="dialog-content">
          <drr-select
            [label]="t('projectRole')"
            [rxFormControl]="contactForm.get('projectRole')"
            [options]="projectRoleTypeOptions"
          ></drr-select>
          <div class="dialog-content__body">
            <drr-input
              [label]="t('firstName')"
              [rxFormControl]="contactForm.get('firstName')"
              [maxlength]="40"
            ></drr-input>
            <drr-input
              [label]="t('lastName')"
              [rxFormControl]="contactForm.get('lastName')"
              [maxlength]="40"
            ></drr-input>
            <drr-input
              [label]="t('title')"
              [rxFormControl]="contactForm.get('title')"
              [maxlength]="40"
            ></drr-input>
            <drr-input
              [label]="t('department')"
              [rxFormControl]="contactForm.get('department')"
              [maxlength]="40"
            ></drr-input>
            <drr-input
              [label]="t('phone')"
              [rxFormControl]="contactForm.get('phone')"
              type="tel"
            ></drr-input>
            <drr-input
              [label]="t('email')"
              [rxFormControl]="contactForm.get('email')"
              type="email"
            ></drr-input>
          </div>
        </div>
      </mat-dialog-content>
      <mat-divider></mat-divider>
      <mat-dialog-actions align="end">
        <button
          mat-raised-button
          color="primary"
          [disabled]="!contactForm.valid"
        >
          Save
        </button>
        <button mat-stroked-button mat-dialog-close>Close</button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [
    `
      .dialog-content {
        display: flex;
        flex-direction: column;
        gap: 1rem;
        width: 50rem;
      }

      .dialog-content__body {
        display: grid;
        grid-template-columns: repeat(2, 1fr);
        gap: 1rem;
      }
    `,
  ],
})
export class DrifProjectContactDialogComponent {
  formBuilder = inject(RxFormBuilder);
  dialogRef = inject(MatDialogRef<DrifProjectContactDialogComponent>);
  translocoService = inject(TranslocoService);

  projectRoleTypeOptions = Object.keys(ProjectRoleType).map((key) => ({
    value: key,
    label: this.translocoService.translate(`projectRoleType.${key}`),
  }));

  contactForm = this.formBuilder.formGroup(
    ProjectContactForm,
  ) as IFormGroup<ProjectContactForm>;
}
