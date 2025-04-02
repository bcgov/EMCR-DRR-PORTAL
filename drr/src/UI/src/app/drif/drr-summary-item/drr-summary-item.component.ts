import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { NgxMaskPipe } from 'ngx-mask';

export type ControlType =
  | 'input'
  | 'textarea'
  | 'select'
  | 'radio'
  | 'checkbox'
  | 'array'
  | 'date'
  | 'currency'
  | 'phone';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-summary-item',
  standalone: true,
  imports: [CommonModule, MatInputModule, TranslocoModule],
  templateUrl: './drr-summary-item.component.html',
  styleUrl: './drr-summary-item.component.scss',
  providers: [DatePipe, CurrencyPipe, NgxMaskPipe],
})
export class DrrSummaryItemComponent {
  translocoService = inject(TranslocoService);
  datePipe = inject(DatePipe);
  currencyPipe = inject(CurrencyPipe);
  maskPipe = inject(NgxMaskPipe);

  @Input() label?: string;

  private _value: string | number | null | undefined;
  @Input()
  set value(value: string | number | null | undefined) {
    this._value = value;
  }
  get value(): string | number | null | undefined {
    if (this._value !== undefined) {
      return this._value;
    }

    const controlValue = this.rxFormControl?.value;
    if (controlValue === undefined || controlValue === null) {
      return controlValue;
    }

    const translateKey = this.translatePrefix
      ? `${this.translatePrefix}.${controlValue}`
      : controlValue;

    switch (this.controlType) {
      case 'input':
      case 'textarea':
        return this.rxFormControl?.value;
      case 'select':
        return this.translocoService.translate(translateKey);
      case 'radio':
        return typeof controlValue === 'boolean'
          ? this.translocoService.translate(controlValue ? 'Yes' : 'No')
          : this.translocoService.translate(controlValue);
      case 'array':
        return (
          this.translate
            ? (this.translocoService.translate(
                this.rxFormControl?.value,
              ) as string[])
            : (this.rxFormControl?.value as string[])
        )?.join(', ');
      case 'date':
        return this.datePipe.transform(controlValue, 'yyyy-MM-dd');
      case 'currency':
        return this.currencyPipe.transform(controlValue);
      case 'phone':
        return this.maskPipe.transform(controlValue, '000-000-0000');

      default:
        return this.rxFormControl?.value;
    }
  }

  @Input() controlType: ControlType = 'input';

  @Input() translate = true;
  @Input() translatePrefix = '';

  @Input() rxFormControl?: AbstractControl | null;

  isRequired(): boolean {
    if (this.rxFormControl && this.rxFormControl.validator) {
      const validator = this.rxFormControl.validator({} as AbstractControl);
      return !!validator && !!validator['required'];
    }
    return false;
  }
}
