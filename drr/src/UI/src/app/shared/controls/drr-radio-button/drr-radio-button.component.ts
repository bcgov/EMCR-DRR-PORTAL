import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatRadioModule } from '@angular/material/radio';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { RxFormBuilder, RxFormControl } from '@rxweb/reactive-form-validators';

export class DrrRadioOption {
  value!: string | boolean | number;
  label!: string;
}

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-radio-button',
  standalone: true,
  imports: [
    CommonModule,
    MatRadioModule,
    MatFormFieldModule,
    FormsModule,
    ReactiveFormsModule,
    TranslocoModule,
  ],
  template: `
    <ng-container *transloco="let t">
      <div class="drr-radio-button">
        <mat-label
          *ngIf="label"
          [class]="hasRequiredError() ? 'drr-label--error' : ''"
          >{{ label }}{{ getMandatoryMark() }}</mat-label
        >
        <mat-error
          style="color: #f44336; font-size: 12px;"
          *ngIf="hasRequiredError()"
          >Selection is required</mat-error
        >
        <mat-radio-group
          class="drr-radio-group"
          aria-label="Select an option"
          [formControl]="rxFormControl"
        >
          @for (option of options; track $index) {
            <mat-radio-button [value]="option.value">{{
              option.label
            }}</mat-radio-button>
          }
        </mat-radio-group>
      </div>
    </ng-container>
  `,
  styles: [``],
})
export class DrrRadioButtonComponent {
  formBuilder = inject(RxFormBuilder);

  @Input()
  label = '';

  private _options: DrrRadioOption[] = [
    { value: true, label: 'Yes' },
    { value: false, label: 'No' },
  ];
  @Input()
  set options(options: DrrRadioOption[]) {
    if (options) {
      this._options = options;
    }
  }
  get options() {
    return this._options;
  }

  private _formControl = this.formBuilder.control('', []) as RxFormControl;
  @Input()
  set rxFormControl(rxFormControl: any) {
    this._formControl = rxFormControl as RxFormControl;
  }
  get rxFormControl() {
    return this._formControl;
  }

  getMandatoryMark() {
    return !!this.rxFormControl?.validator?.({})?.required ? '*' : '';
  }

  hasRequiredError(): boolean {
    return (
      this.rxFormControl.hasError('required') &&
      !this.rxFormControl.disabled &&
      this.rxFormControl.touched
    );
  }
}
