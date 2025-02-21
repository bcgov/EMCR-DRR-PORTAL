import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
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
import { MatSelectModule } from '@angular/material/select';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { IFormGroup } from '@rxweb/reactive-form-validators';
import { distinctUntilChanged } from 'rxjs';
import { FundingStream, Hazards, ProjectType } from '../../../../model';
import { DrrDatepickerComponent } from '../../../shared/controls/drr-datepicker/drr-datepicker.component';
import { DrrInputComponent } from '../../../shared/controls/drr-input/drr-input.component';
import {
  DrrRadioButtonComponent,
  DrrRadioOption,
} from '../../../shared/controls/drr-radio-button/drr-radio-button.component';
import {
  DrrSelectComponent,
  DrrSelectOption,
} from '../../../shared/controls/drr-select/drr-select.component';
import { ProjectInformationForm } from '../drif-eoi-form';

@UntilDestroy({ checkProperties: true })
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
    MatSelectModule,
    MatDatepickerModule,
    TranslocoModule,
    DrrInputComponent,
    DrrSelectComponent,
    DrrDatepickerComponent,
    DrrRadioButtonComponent,
  ],
  templateUrl: './drif-eoi-step-2.component.html',
  styleUrl: './drif-eoi-step-2.component.scss',
})
export class DrifEoiStep2Component {
  translocoService = inject(TranslocoService);

  @Input()
  projectInformationForm!: IFormGroup<ProjectInformationForm>;

  minStartDate = new Date();

  fundingStreamOptions: DrrRadioOption[] = Object.values(FundingStream).map(
    (value) => ({
      value,
      label: this.translocoService.translate(value),
    }),
  );

  hazardsOptions: DrrSelectOption[] = Object.values(Hazards).map((value) => ({
    value,
    label: this.translocoService.translate(value),
  }));

  streamOptions: DrrRadioOption[] = Object.values(ProjectType).map((value) => ({
    value,
    label: this.translocoService.translate(value),
  }));

  ngOnInit() {
    this.projectInformationForm
      .get('relatedHazards')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((hazards) => {
        const otherHazardsDescriptionControl = this.projectInformationForm.get(
          'otherHazardsDescription',
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

  getNextDayAfterStartDate() {
    const startDate = this.projectInformationForm.get('startDate')?.value;
    if (startDate) {
      const nextDay = new Date(startDate);
      nextDay.setDate(nextDay.getDate() + 1);
      return nextDay;
    }
    return this.minStartDate;
  }

  getFormArray(formArrayName: string) {
    return this.projectInformationForm.get(formArrayName) as FormArray;
  }

  otherHazardSelected() {
    return this.getFormArray('relatedHazards').value?.includes('Other');
  }
}
