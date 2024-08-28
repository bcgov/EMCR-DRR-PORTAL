import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatRadioModule } from '@angular/material/radio';
import { TranslocoModule } from '@ngneat/transloco';
import { RxFormBuilder, RxFormControl } from '@rxweb/reactive-form-validators';

export class RadioOption {
  value!: string | boolean | number;
  label!: string;
}

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

  @Input()
  options?: RadioOption[] = [
    { value: true, label: 'Yes' },
    { value: false, label: 'No' },
  ];

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
