import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup } from '@rxweb/reactive-form-validators';
import { YesNoOption } from '../../../../model';
import { DrrChipAutocompleteComponent } from '../../../shared/controls/drr-chip-autocomplete/drr-chip-autocomplete.component';
import {
  DrrRadioButtonComponent,
  RadioOption,
} from '../../../shared/controls/drr-radio-button/drr-radio-button.component';
import { DrrTextareaComponent } from '../../../shared/controls/drr-textarea/drr-textarea.component';
import { EntitiesStore } from '../../../store/entities.store';
import { ProjectEngagementForm } from '../drif-fp-form';

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
  entitiesStore = inject(EntitiesStore);

  @Input() projectEngagementForm!: IFormGroup<ProjectEngagementForm>;

  otherEngagementOptions: RadioOption[] = [
    { value: YesNoOption.Yes, label: 'Yes' },
    { value: YesNoOption.No, label: 'No' },
    {
      value: YesNoOption.NotApplicable,
      label: 'Not Applicable - not impacted or affected parties',
    },
  ];

  affectedParties = this.entitiesStore.getEntities()?.affectedParties?.();

  ngOnInit() {
    this.projectEngagementForm
      .get('otherEngagement')
      ?.valueChanges.subscribe((value) => {
        value === YesNoOption.NotApplicable
          ? this.projectEngagementForm.get('affectedParties')?.disable()
          : this.projectEngagementForm.get('affectedParties')?.enable();
      });
  }
}
