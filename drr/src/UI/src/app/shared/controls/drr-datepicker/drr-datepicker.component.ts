import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { RxFormBuilder, RxFormControl } from '@rxweb/reactive-form-validators';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-datepicker',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatInputModule,
    MatDatepickerModule,
    TranslocoModule,
  ],
  template: `
    <div class="drr-input-container">
      <mat-label [class]="hasRequiredError() ? 'drr-label--error' : ''"
        >{{ label }}{{ getMandatoryMark() }}</mat-label
      >
      <mat-form-field class="drr-datepicker" *transloco="let t">
        <input
          [id]="id"
          required="{{ isRequired() }}"
          matInput
          [matDatepicker]="date_picker"
          [formControl]="rxFormControl"
          [min]="min"
          [max]="max"
        />
        <mat-hint>YYYY-MM-DD</mat-hint>
        <mat-error *ngIf="rxFormControl.hasError('matDatepickerMin')">{{
          minErrorLabel
        }}</mat-error>
        <mat-error *ngIf="rxFormControl.hasError('matDatepickerMax')">{{
          maxErrorLabel
        }}</mat-error>
        <mat-error *ngIf="rxFormControl.hasError('required')">
          Date is required
        </mat-error>
        <mat-error *ngIf="rxFormControl.hasError('matDatepickerParse')">{{
          t('matDatepickerParseError')
        }}</mat-error>
        <mat-datepicker-toggle
          matIconSuffix
          [for]="date_picker"
        ></mat-datepicker-toggle>
        <mat-datepicker #date_picker></mat-datepicker>
      </mat-form-field>
    </div>
  `,
  styles: [
    `
      .drr-datepicker {
        width: 100%;
      }

      :host {
        .drr-datepicker
          ::ng-deep
          .mdc-text-field--outlined.mdc-text-field--disabled
          .mdc-text-field__input {
          color: var(--mdc-outlined-text-field-input-text-color);
        }

        ::ng-deep .mdc-text-field--outlined.mdc-text-field--disabled {
          .mdc-floating-label,
          .mdc-floating-label--float-above {
            color: var(--mdc-outlined-text-field-label-text-color);
          }
        }
      }
    `,
  ],
})
export class DrrDatepickerComponent {
  formBuilder = inject(RxFormBuilder);
  breakpointObserver = inject(BreakpointObserver);

  @Input() label = '';
  @Input() id = '';
  @Input() min?: Date | string;
  @Input() max?: Date | string;
  @Input() minErrorLabel = '';
  @Input() maxErrorLabel = '';

  private _formControl = this.formBuilder.control('', []) as RxFormControl;
  @Input()
  set rxFormControl(rxFormControl: any) {
    this._formControl = rxFormControl as RxFormControl;
  }
  get rxFormControl() {
    return this._formControl;
  }

  @Input()
  set disabled(disabled: boolean) {
    disabled ? this.rxFormControl.disable() : this.rxFormControl.enable();
  }

  isMobile = false;

  ngOnInit() {
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .subscribe(({ matches }) => {
        this.isMobile = !matches;
      });
  }

  getMandatoryMark() {
    return !!this.rxFormControl?.validator?.({})?.required ? '*' : '';
  }

  isRequired(): boolean {
    return this.isMobile
      ? false
      : !!this.rxFormControl?.validator?.({})?.required;
  }

  hasRequiredError(): boolean {
    return (
      this.rxFormControl.hasError('required') &&
      this.rxFormControl.touched &&
      this.rxFormControl.invalid &&
      !this.rxFormControl.disabled
    );
  }
}
