import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule, CurrencyPipe } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  HostListener,
  Input,
  ViewChild,
  inject,
} from '@angular/core';
import { FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import {
  RxFormBuilder,
  RxFormControl,
  RxwebValidators,
} from '@rxweb/reactive-form-validators';
import { NgxMaskDirective } from 'ngx-mask';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-currency-input',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    NgxMaskDirective,
    TranslocoModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
  ],
  providers: [CurrencyPipe],
  template: `<mat-label *ngIf="isMobile"
      >{{ label }}{{ getMandatoryMark() }}</mat-label
    >
    <mat-form-field class="drr-currency-input" *transloco="let t">
      <mat-label *ngIf="!isMobile && label">{{ label }}</mat-label>
      <input
        id="{{ id }}"
        #currencyInput
        matInput
        [formControl]="rxFormControl"
        required="{{ isRequired() }}"
        [min]="min"
        [max]="max"
        (focus)="onFocus()"
        (blur)="onBlur()"
        [mask]="'separator.2'"
        [decimalMarker]="'.'"
        [thousandSeparator]="','"
        [allowNegativeNumbers]="allowNegativeNumbers"
      />
      @if (rxFormControl.disabled && allowEnabling) {
        <button
          style="margin-right: 0.5rem"
          matSuffix
          mat-icon-button
          aria-label="edit"
          (click)="enableInput()"
        >
          <mat-icon>edit</mat-icon>
        </button>
      }
      <span matTextPrefix>$&nbsp;</span>
      <mat-hint *ngIf="max && isFocused" align="end"
        >Max: {{ max | currency: '' : 'symbol' : '1.0-0' }}</mat-hint
      >
      <mat-error *ngIf="rxFormControl.hasError('max')">{{
        maxValueCustomErrorMessage
      }}</mat-error>
      <mat-error *ngIf="rxFormControl.hasError('minNumber')">{{
        t('minValueError', { min: min | currency: '' : 'symbol' : '1.0-0' })
      }}</mat-error>
      <mat-error *ngIf="rxFormControl.hasError('required')">
        Field is required
      </mat-error>
      <mat-hint *ngIf="hasMaxValueError()" class="max-number-error">
        {{ maxValueCustomErrorMessage }}
      </mat-hint>
    </mat-form-field> `,
  styles: `
    .drr-currency-input {
      width: 100%;
    }

    .drr-currency-input input[type='number']::-webkit-inner-spin-button,
    .drr-currency-input input[type='number']::-webkit-outer-spin-button {
      -webkit-appearance: none;
      margin: 0;
    }

    .drr-currency-input input[type='number'] {
      -moz-appearance: textfield;
    }

    .max-number-error {
      color: red !important;
    }

    :host {
      .drr-currency-input
        ::ng-deep
        .mat-mdc-text-field-wrapper
        .mat-mdc-form-field-flex {
        left: -10px;
      }

      // .mat-mdc-form-field-error {
      //   text-wrap: nowrap;
      // }

      .drr-currency-input
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
})
export class DrrCurrencyInputComponent {
  formBuilder = inject(RxFormBuilder);
  breakpointObserver = inject(BreakpointObserver);
  translocoService = inject(TranslocoService);
  currency = inject(CurrencyPipe);

  isFocused = false;
  isMobile = false;
  MAX_ALLOWED_VALUE = 999999999.99;

  @Input() label = '';
  @Input() id = '';

  @Input()
  set value(val: number | undefined) {
    // do not set value if the control is pristine since loading the value
    // unless control value is null or undefined
    if (
      this.rxFormControl.pristine &&
      this.rxFormControl.value !== null &&
      this.rxFormControl.value !== undefined
    ) {
      return;
    }

    // if there is a valid value and control is not touched, set the value
    if (val !== undefined && !this.rxFormControl.touched) {
      this.rxFormControl.setValue(val, { emitEvent: false });
    }
  }

  private _allowNegativeNumbers = false;
  @Input() set allowNegativeNumbers(value: boolean) {
    this._allowNegativeNumbers = value;

    // if need to allow negative numbers
    // unless min value is provided, use the max allowed value as negative min value
    if (value) {
      this.min === undefined ? -this.MAX_ALLOWED_VALUE : this.min;
    }
  }
  get allowNegativeNumbers() {
    return this._allowNegativeNumbers;
  }

  @Input() min?: number;

  private _max?: number;
  @Input()
  set max(value: number) {
    if (value !== undefined && value !== null) {
      const validators = this.isRequired()
        ? [RxwebValidators.required(), Validators.max(value)]
        : [Validators.max(value)];
      this.rxFormControl.setValidators(validators);
    } else {
      this.rxFormControl.removeValidators([Validators.max]);
    }

    this.rxFormControl.updateValueAndValidity();
    this._max = value;
  }
  get max(): number | undefined {
    return this._max;
  }

  @Input() maxValueCustomErrorMessage = this.translocoService.translate(
    'maxValueError',
    { max: this.currency.transform(this.MAX_ALLOWED_VALUE) },
  );

  @Input() allowEnabling = false;

  @ViewChild('currencyInput') private currencyInput: any;

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

  handleInputEvent(event: Event, value: string) {
    const newValue = parseFloat(this.rxFormControl.value + value);
    if (newValue > this.MAX_ALLOWED_VALUE) {
      event.preventDefault();
    }
  }

  getMandatoryMark() {
    return !!this.rxFormControl?.validator?.({})?.required ? '*' : '';
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

  enableInput() {
    this.allowEnabling = false;
    this.rxFormControl.enable();
  }

  hasMaxValueError() {
    return this.rxFormControl.value > this.MAX_ALLOWED_VALUE;
  }

  focusOnInput() {
    const inputElement = this.currencyInput.nativeElement;
    if (inputElement) {
      inputElement.focus();
    }
  }
}
