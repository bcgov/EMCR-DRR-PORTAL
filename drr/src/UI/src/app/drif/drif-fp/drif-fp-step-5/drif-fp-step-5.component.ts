import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { IFormGroup } from '@rxweb/reactive-form-validators';
import { distinctUntilChanged } from 'rxjs';
import { YesNoOption } from '../../../../model';
import { DrrChipAutocompleteComponent } from '../../../shared/controls/drr-chip-autocomplete/drr-chip-autocomplete.component';
import {
  DrrRadioButtonComponent,
  DrrRadioOption,
} from '../../../shared/controls/drr-radio-button/drr-radio-button.component';
import { DrrTextareaComponent } from '../../../shared/controls/drr-textarea/drr-textarea.component';
import { OptionsStore } from '../../../store/options.store';
import { ProjectEngagementForm } from '../drif-fp-form';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drif-fp-step-5',
  standalone: true,
  imports: [
    CommonModule,
    MatInputModule,
    TranslocoModule,
    FormsModule,
    ReactiveFormsModule,
    DrrRadioButtonComponent,
    DrrChipAutocompleteComponent,
    DrrTextareaComponent,
  ],
  templateUrl: './drif-fp-step-5.component.html',
  styleUrl: './drif-fp-step-5.component.scss',
})
export class DrifFpStep5Component {
  optionsStore = inject(OptionsStore);

  @Input() projectEngagementForm!: IFormGroup<ProjectEngagementForm>;

  otherEngagementOptions: DrrRadioOption[] = [
    { value: YesNoOption.Yes, label: 'Yes' },
    { value: YesNoOption.No, label: 'No' },
    {
      value: YesNoOption.NotApplicable,
      label: 'Not Applicable - not impacted or affected parties',
    },
  ];

  affectedParties = this.optionsStore.getOptions()?.affectedParties?.();

  ngOnInit() {
    this.projectEngagementForm
      .get('otherEngagement')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        const affectedParties =
          this.projectEngagementForm.get('affectedParties');
        if (value === YesNoOption.NotApplicable || value === YesNoOption.No) {
          affectedParties?.reset();
          affectedParties?.clearValidators();
          affectedParties?.disable();
        } else {
          affectedParties?.addValidators(Validators.required);
          affectedParties?.enable();
        }

        affectedParties?.updateValueAndValidity();
      });
  }
}
