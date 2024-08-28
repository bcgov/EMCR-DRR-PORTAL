import { AsyncPipe, CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup } from '@rxweb/reactive-form-validators';
import { DrrChipAutocompleteComponent } from '../../../shared/controls/drr-chip-autocomplete/drr-chip-autocomplete.component';
import {
  DrrRadioButtonComponent,
  RadioOption,
} from '../../../shared/controls/drr-radio-button/drr-radio-button.component';
import { DrrTextareaComponent } from '../../../shared/controls/drr-textarea/drr-textarea.component';
import { EntitiesStore } from '../../../store/entities.store';
import { PermitsRegulationsAndStandardsForm, Standards } from '../drif-fp-form';

@Component({
  selector: 'drif-fp-step-7',
  standalone: true,
  imports: [
    CommonModule,
    MatChipsModule,
    MatIconModule,
    TranslocoModule,
    MatFormFieldModule,
    FormsModule,
    ReactiveFormsModule,
    MatAutocompleteModule,
    AsyncPipe,
    DrrChipAutocompleteComponent,
    DrrRadioButtonComponent,
    DrrTextareaComponent,
  ],
  templateUrl: './drif-fp-step-7.component.html',
  styleUrl: './drif-fp-step-7.component.scss',
})
export class DrifFpStep7Component {
  entitiesStore = inject(EntitiesStore);

  @Input()
  permitsRegulationsAndStandardsForm!: IFormGroup<PermitsRegulationsAndStandardsForm>;

  professionalGuidanceOptions = this.entitiesStore
    .getEntities()
    ?.professionals?.();
  standardsAcceptableOptions: RadioOption[] = [
    { value: 1, label: 'Yes' },
    { value: 2, label: 'No' },
    { value: 3, label: 'Not Applicable' },
  ];
  provincialStandardsOptions = Object.values(Standards);

  ngOnInit() {}
}
