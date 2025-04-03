import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { IFormGroup } from '@rxweb/reactive-form-validators';
import { distinctUntilChanged } from 'rxjs';
import { DrrChipAutocompleteComponent } from '../../../shared/controls/drr-chip-autocomplete/drr-chip-autocomplete.component';
import { DrrRadioButtonComponent } from '../../../shared/controls/drr-radio-button/drr-radio-button.component';
import { DrrTextareaComponent } from '../../../shared/controls/drr-textarea/drr-textarea.component';
import { OptionsStore } from '../../../store/options.store';
import { ClimateAdaptationForm } from '../drif-fp-form';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drif-fp-step-6',
  standalone: true,
  imports: [
    CommonModule,
    TranslocoModule,
    FormsModule,
    ReactiveFormsModule,
    MatInputModule,
    DrrRadioButtonComponent,
    DrrTextareaComponent,
    DrrChipAutocompleteComponent,
  ],
  templateUrl: './drif-fp-step-6.component.html',
  styleUrl: './drif-fp-step-6.component.scss',
})
export class DrifFpStep6Component {
  optionsStore = inject(OptionsStore);

  climateAssessmentToolOptions = this.optionsStore
    .getOptions()
    ?.climateAssessmentToolOptions?.();

  @Input()
  climateAdaptationForm!: IFormGroup<ClimateAdaptationForm>;

  ngOnInit() {
    this.climateAdaptationForm
      .get('incorporateFutureClimateConditions')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        const climateAdaptationControl =
          this.climateAdaptationForm.get('climateAdaptation');
        if (value) {
          climateAdaptationControl?.addValidators(Validators.required);
        } else {
          climateAdaptationControl?.clearValidators();
          climateAdaptationControl?.reset();
        }
        climateAdaptationControl?.updateValueAndValidity();
      });

    this.climateAdaptationForm
      .get('climateAssessment')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        const climateAssessmentToolsControl = this.climateAdaptationForm.get(
          'climateAssessmentTools',
        );
        const climateAssessmentCommentsControl = this.climateAdaptationForm.get(
          'climateAssessmentComments',
        );
        if (value) {
          climateAssessmentToolsControl?.addValidators(Validators.required);
          climateAssessmentCommentsControl?.addValidators(Validators.required);
        } else {
          climateAssessmentToolsControl?.reset();
          climateAssessmentToolsControl?.clearValidators();
          climateAssessmentCommentsControl?.reset();
          climateAssessmentCommentsControl?.clearValidators();
        }
        climateAssessmentToolsControl?.updateValueAndValidity();
        climateAssessmentCommentsControl?.updateValueAndValidity();
      });
  }
}
