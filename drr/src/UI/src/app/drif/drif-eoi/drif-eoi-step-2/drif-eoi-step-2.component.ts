import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import {
  FormArray,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup, RxFormControl } from '@rxweb/reactive-form-validators';
import { DateTime } from 'luxon';
import { Hazards } from '../../../../model';
import { DrrDatepickerComponent } from '../../../shared/controls/drr-datepicker/drr-datepicker.component';
import { DrrInputComponent } from '../../../shared/controls/drr-input/drr-input.component';
import { DrrSelectComponent } from '../../../shared/controls/drr-select/drr-select.component';
import { ProjectInformationForm } from '../drif-eoi-form';

@Component({
  selector: 'drif-eoi-step-2',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatRadioModule,
    MatSelectModule,
    MatDatepickerModule,
    TranslocoModule,
    DrrInputComponent,
    DrrSelectComponent,
    DrrDatepickerComponent,
  ],
  templateUrl: './drif-eoi-step-2.component.html',
  styleUrl: './drif-eoi-step-2.component.scss',
})
export class DrifEoiStep2Component {
  @Input()
  projectInformationForm!: IFormGroup<ProjectInformationForm>;

  minStartDate = new Date();
  minEndDate = new Date();

  hazardsOptions = Object.values(Hazards);

  ngOnInit() {
    this.projectInformationForm
      .get('startDate')
      ?.valueChanges.subscribe((date) => {
        if (!DateTime.isDateTime(date)) {
          date = DateTime.fromISO(date.toString());
        }

        this.minEndDate = date?.plus({ days: 1 });
      });

    this.projectInformationForm
      .get('relatedHazards')
      ?.valueChanges.subscribe((hazards) => {
        const otherHazardsDescriptionControl = this.projectInformationForm.get(
          'otherHazardsDescription'
        );
        if (hazards?.includes('Other')) {
          otherHazardsDescriptionControl?.addValidators(Validators.required);
        } else {
          otherHazardsDescriptionControl?.clearValidators();
          otherHazardsDescriptionControl?.reset();
        }

        otherHazardsDescriptionControl?.updateValueAndValidity();
      });
  }

  getFormArray(formArrayName: string) {
    return this.projectInformationForm.get(formArrayName) as FormArray;
  }

  getFormControl(name: string): RxFormControl {
    return this.projectInformationForm.get(name) as RxFormControl;
  }

  otherHazardSelected() {
    return this.getFormArray('relatedHazards').value?.includes('Other');
  }
}
