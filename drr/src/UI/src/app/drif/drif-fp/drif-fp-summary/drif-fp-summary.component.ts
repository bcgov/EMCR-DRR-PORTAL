import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import {
  IFormGroup,
  RxFormArray,
  RxFormControl,
  RxFormGroup,
} from '@rxweb/reactive-form-validators';
import { NgxMaskPipe } from 'ngx-mask';
import { DrifFpForm } from '../drif-fp-form';
import { SummaryItemComponent } from '../../summary-item/summary-item.component';

@Component({
  selector: 'drif-fp-summary',
  standalone: true,
  imports: [
    CommonModule,
    SummaryItemComponent,
    MatCardModule,
    MatInputModule,
    TranslocoModule,
    NgxMaskPipe,
  ],
  templateUrl: './drif-fp-summary.component.html',
  styleUrl: './drif-fp-summary.component.scss',
})
export class DrifFpSummaryComponent {
  private _drifFpForm?: IFormGroup<DrifFpForm>;

  @Input()
  showSubmitterInfo = true;

  @Input()
  set DrifFpForm(DrifFpForm: IFormGroup<DrifFpForm>) {
    this._drifFpForm = DrifFpForm;
  }

  get DrifFpForm(): IFormGroup<DrifFpForm> {
    return this._drifFpForm!;
  }

  getGroup(groupName: string): RxFormGroup {
    return this.DrifFpForm?.get(groupName) as RxFormGroup;
  }

  getFormArray(groupName: string, controlName: string): any[] {
    return this.getGroup(groupName)?.get(controlName)?.value ?? [];
  }

  getRxFormControl(groupName: string, controlName: string) {
    return this.getGroup(groupName)?.get(controlName) as RxFormControl;
  }

  getRxGroupFormControl(
    groupName: string,
    nestedGroup: string,
    controlName: string
  ) {
    return this.getGroup(groupName)
      ?.get(nestedGroup)
      ?.get(controlName) as RxFormControl;
  }

  getRxFormArrayControls(groupName: string, controlName: string) {
    return (this.getGroup(groupName)?.get(controlName) as RxFormArray).controls;
  }

  convertRxFormControl(formControl: AbstractControl<any, any> | null) {
    return formControl as RxFormControl;
  }

  objectHasValues(obj: any): boolean {
    // if array - check length, if value - check if truthy
    return (
      obj &&
      Object.values(obj).some((value) => {
        if (Array.isArray(value)) {
          return this.arrayHasValues(value);
        } else {
          return !!value;
        }
      })
    );
  }

  arrayHasValues(array: any[]): boolean {
    return (
      array &&
      array.length > 0 &&
      array.some((value) => this.objectHasValues(value))
    );
  }
}
