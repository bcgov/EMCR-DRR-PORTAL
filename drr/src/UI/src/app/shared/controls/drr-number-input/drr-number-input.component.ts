import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  HostListener,
  inject,
  Input,
} from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { RxFormBuilder, RxFormControl } from '@rxweb/reactive-form-validators';
import { NgxMaskDirective } from 'ngx-mask';

export type NumericInputType = 'integer' | 'decimal' | 'percentage';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-numeric-input',
  template: `
    <div>
      <mat-label>{{ label }}{{ getMandatoryMark() }}</mat-label>
      <mat-form-field class="drr-input" *transloco="let t">
        <input
          id="{{ id }}"
          matInput
          [formControl]="rxFormControl"
          [maxlength]="maxlength ?? null"
          required="{{ isRequired() }}"
          type="text"
          [min]="min"
          [max]="max"
          (focus)="onFocus()"
          (blur)="onBlur()"
          [mask]="getMask()"
          [decimalMarker]="'.'"
          [thousandSeparator]="''"
          [ngStyle]="{
            'text-align': numericType === 'percentage' ? 'right' : 'left',
          }"
        />
        @if (this.numericType === 'percentage') {
          <span matTextSuffix>%&nbsp;</span>
        }
        <mat-hint *ngIf="maxlength && isFocused" align="end"
          >{{ getCount() }} / {{ maxlength }}</mat-hint
        >
        @if (getCount() > maxlength!) {
          <mat-error>{{ t('maxLengthError') }}</mat-error>
        }
        <mat-error
          *ngIf="
            numericType === 'percentage' && rxFormControl.hasError('maxNumber')
          "
          >{{ t('percentageMaxValueError') }}</mat-error
        >
        <mat-error *ngIf="rxFormControl.hasError('required')">
          Field is required
        </mat-error>
        <mat-hint *ngIf="hasMaxValueError()" class="max-number-error">
          The value is too large.
        </mat-hint>
      </mat-form-field>
    </div>
  `,
  styles: [
    `
      .drr-input {
        width: 100%;
      }

      .drr-input input[type='number']::-webkit-inner-spin-button,
      .drr-input input[type='number']::-webkit-outer-spin-button {
        -webkit-appearance: none;
        margin: 0;
      }

      .drr-input input[type='number'] {
        -moz-appearance: textfield;
      }

      :host {
        .drr-input
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
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatInputModule,
    NgxMaskDirective,
    TranslocoModule,
  ],
})
export class DrrNumericInputComponent {
  formBuilder = inject(RxFormBuilder);
  breakpointObserver = inject(BreakpointObserver);

  isFocused = false;
  isMobile = false;
  MAX_VALUE = 999999999.99;

  @Input() label = '';
  @Input() id = '';
  @Input() min: number = 0;
  @Input() max: number | null = null;
  @Input() maxlength?: number | null;
  @Input() numericType: NumericInputType = 'integer';
  @Input() customMask: string | null = null;

  ngOnInit() {
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .subscribe(({ matches }) => {
        this.isMobile = !matches;
      });
  }

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

  changeDetector = inject(ChangeDetectorRef);

  ngAfterViewInit() {
    this.changeDetector.detectChanges();
  }

  getCount(): number {
    const inputValue = this.rxFormControl?.value ?? '';
    return inputValue.toString().length;
  }

  getMandatoryMark() {
    return !!this.rxFormControl?.validator?.({})?.required ? ' (required)' : '';
  }

  isRequired(): boolean {
    return this.isMobile
      ? false
      : !!this.rxFormControl?.validator?.({})?.required;
  }

  onFocus() {
    this.isFocused = true;
  }

  onBlur() {
    this.isFocused = false;
  }

  getMask() {
    if (this.customMask) {
      return this.customMask;
    }

    if (this.numericType === 'percentage' || this.numericType === 'integer') {
      return 'separator.0';
    }

    return 'separator.2';
  }

  @HostListener('keydown', ['$event'])
  handleArrowKeyEvent(event: KeyboardEvent) {
    if (event.key === 'ArrowUp' || event.key === 'ArrowDown') {
      event.preventDefault();
    }
  }

  @HostListener('keypress', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    const inputChar = String.fromCharCode(event.charCode);
    this.handleInputEvent(event, inputChar);
  }

  @HostListener('paste', ['$event'])
  handlePasteEvent(event: ClipboardEvent) {
    const value = event.clipboardData?.getData('text') ?? '';
    this.handleInputEvent(event, value);
  }

  private handleInputEvent(event: Event, inputValue: string) {
    // number input doesn't not support maxlength by default
    // so we need to add it manually to match text input behavior,
    // but allow decimal point to be moved around
    if (
      this.maxlength
        ? this.getCount() + inputValue.length > Number(this.maxlength) &&
          inputValue !== '.'
        : false
    ) {
      event.preventDefault();
    }

    if (this.numericType === 'decimal') {
      // Allow positive numbers and decimals
      const pattern = /^\d*\.?\d*$/;

      if (!pattern.test(inputValue)) {
        // Invalid character, prevent input
        event.preventDefault();
      }
    }

    if (this.numericType === 'integer') {
      // Allow positive numbers
      const pattern = /^\d*$/;

      if (!pattern.test(inputValue)) {
        // Invalid character, prevent input
        event.preventDefault();
      }
    }

    const newValue = parseFloat(this.rxFormControl.value + inputValue);
    if (newValue > this.MAX_VALUE) {
      event.preventDefault();
    }
  }

  hasMaxValueError() {
    return this.rxFormControl.value > this.MAX_VALUE;
  }
}
