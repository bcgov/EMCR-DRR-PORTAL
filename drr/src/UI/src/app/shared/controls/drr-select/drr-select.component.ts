import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectChange, MatSelectModule } from '@angular/material/select';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { RxFormBuilder, RxFormControl } from '@rxweb/reactive-form-validators';

export interface DrrSelectOption {
  value: string;
  label: string;
}

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-select',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatSelectModule,
    TranslocoModule,
  ],
  template: `
    <div class="drr-input-container">
      <mat-label [class]="hasRequiredError() ? 'drr-label--error' : ''"
        >{{ label }}{{ getMandatoryMark() }}</mat-label
      >
      <mat-form-field *transloco="let t" class="drr-select">
        <mat-select
          id="{{ id }}"
          required="{{ isRequired() }}"
          [formControl]="rxFormControl"
          multiple="{{ isMultiple }}"
          (selectionChange)="onSelectionChange($event)"
        >
          @for (option of options; track option.value) {
            <mat-option [value]="option.value">{{ option.label }}</mat-option>
          }
        </mat-select>
        <mat-error *ngIf="rxFormControl.hasError('required')">
          Selection is required
        </mat-error>
      </mat-form-field>
    </div>
  `,
  styles: [
    `
      .drr-select {
        width: 100%;
        padding-bottom: 1rem;
      }
    `,
  ],
  providers: [RxFormBuilder],
})
export class DrrSelectComponent {
  formBuilder = inject(RxFormBuilder);
  breakpointObserver = inject(BreakpointObserver);

  isMobile = false;

  private _formControl = this.formBuilder.control('', []) as RxFormControl;
  @Input()
  set rxFormControl(rxFormControl: any) {
    this._formControl = rxFormControl as RxFormControl;
  }
  get rxFormControl() {
    return this._formControl;
  }

  @Input() isMultiple = false;
  @Input() label = '';
  @Input() id = '';
  @Input() options?: DrrSelectOption[] = [];

  @Output()
  selectionChange = new EventEmitter<MatSelectChange>();

  ngOnInit() {
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .subscribe(({ matches }) => {
        this.isMobile = !matches;
      });
  }

  onSelectionChange(event: MatSelectChange) {
    this.selectionChange.emit(event);
  }

  getMandatoryMark() {
    return !!this.rxFormControl?.validator?.({})?.required ? ' (required)' : '';
  }

  isRequired(): boolean {
    return (
      !!this.rxFormControl?.validator?.({})?.required ||
      !!this.rxFormControl?.validator?.({})?.minLength
    );
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
