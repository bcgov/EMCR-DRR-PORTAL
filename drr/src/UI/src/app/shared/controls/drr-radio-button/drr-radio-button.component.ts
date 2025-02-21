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
  templateUrl: './drr-radio-button.component.html',
  styleUrl: './drr-radio-button.component.scss',
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
}
