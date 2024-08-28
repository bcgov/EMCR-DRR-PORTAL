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
import { SummaryItemComponent } from '../../summary-item/summary-item.component';
import { EOIApplicationForm } from '../drif-eoi-form';

@Component({
  selector: 'drr-summary',
  standalone: true,
  imports: [
    CommonModule,
    MatInputModule,
    TranslocoModule,
    NgxMaskPipe,
    SummaryItemComponent,
    MatCardModule,
  ],
  templateUrl: './drif-eoi-summary.component.html',
  styleUrl: './drif-eoi-summary.component.scss',
})
export class DrifEoiSummaryComponent {
  private _eoiApplicationForm?: IFormGroup<EOIApplicationForm>;

  @Input()
  showSubmitterInfo = true;

  @Input()
  set eoiApplicationForm(eoiApplicationForm: IFormGroup<EOIApplicationForm>) {
    this._eoiApplicationForm = eoiApplicationForm;
  }

  get eoiApplicationForm(): IFormGroup<EOIApplicationForm> {
    return this._eoiApplicationForm!;
  }

  objectKeys(obj: any) {
    const excludeKeys = [
      'partneringProponentsArray',
      'infrastructureImpactedArray',
      'declaration',
    ];
    return Object.keys(obj?.controls).filter(
      (key) => !excludeKeys.includes(key)
    );
  }

  objectValues(obj: any) {
    return Object.values(obj?.controls);
  }

  getGroup(groupName: string): RxFormGroup {
    return this.eoiApplicationForm?.get(groupName) as RxFormGroup;
  }

  getGroupControl(groupName: string, controlName: string) {
    return this.getGroup(groupName)?.get(controlName);
  }

  getRemainingAmountAbs() {
    return Math.abs(
      this._eoiApplicationForm
        ?.get('fundingInformation')
        ?.get('remainingAmount')?.value ?? 0
    );
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
}
