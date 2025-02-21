import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import {
  FormArray,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import {
  IFormGroup,
  RxFormBuilder,
  RxwebValidators,
} from '@rxweb/reactive-form-validators';
import { distinctUntilChanged } from 'rxjs';
import { YesNoOption } from '../../../../model';
import { DrrChipAutocompleteComponent } from '../../../shared/controls/drr-chip-autocomplete/drr-chip-autocomplete.component';
import { DrrInputComponent } from '../../../shared/controls/drr-input/drr-input.component';
import {
  DrrRadioButtonComponent,
  DrrRadioOption,
} from '../../../shared/controls/drr-radio-button/drr-radio-button.component';
import {
  DrrSelectComponent,
  DrrSelectOption,
} from '../../../shared/controls/drr-select/drr-select.component';
import { DrrTextareaComponent } from '../../../shared/controls/drr-textarea/drr-textarea.component';
import { OptionsStore } from '../../../store/options.store';
import { StringItem } from '../../drif-eoi/drif-eoi-form';
import {
  PermitsRegulationsAndStandardsForm,
  StandardInfoForm,
} from '../drif-fp-form';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drif-fp-step-7',
  standalone: true,
  imports: [
    CommonModule,
    MatChipsModule,
    MatIconModule,
    TranslocoModule,
    MatFormFieldModule,
    MatCheckboxModule,
    MatButtonModule,
    FormsModule,
    ReactiveFormsModule,
    MatAutocompleteModule,
    DrrInputComponent,
    DrrSelectComponent,
    DrrChipAutocompleteComponent,
    DrrRadioButtonComponent,
    DrrTextareaComponent,
  ],
  templateUrl: './drif-fp-step-7.component.html',
  styleUrl: './drif-fp-step-7.component.scss',
})
export class DrifFpStep7Component {
  optionsStore = inject(OptionsStore);
  formBuilder = inject(RxFormBuilder);

  isMobile = false;

  @Input()
  permitsRegulationsAndStandardsForm!: IFormGroup<PermitsRegulationsAndStandardsForm>;

  professionalGuidanceOptions: DrrRadioOption[] = [
    { value: true, label: 'Yes' },
    { value: false, label: 'Not Applicable' },
  ];

  categories = this.optionsStore
    .getOptions()
    ?.standards?.()
    ?.map((s) => s.category);

  private _professionalOptionsSignal =
    this.optionsStore.getOptions!().professionals;
  get professionalOptions(): DrrSelectOption[] | undefined {
    return this._professionalOptionsSignal?.()?.map((p) => ({
      value: p,
      label: p,
    }));
  }

  standardsAcceptableOptions: DrrRadioOption[] = [
    { value: YesNoOption.Yes, label: 'Yes' },
    { value: YesNoOption.No, label: 'No' },
    { value: YesNoOption.NotApplicable, label: 'Not Applicable' },
  ];

  standardsVisible = true;

  ngOnInit() {
    this.permitsRegulationsAndStandardsForm
      .get('professionalGuidance')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        const professionalsControl =
          this.permitsRegulationsAndStandardsForm.get('professionals');
        if (value === false) {
          professionalsControl?.reset();
          professionalsControl?.clearValidators();
        } else {
          professionalsControl?.addValidators(Validators.required);
        }
        professionalsControl?.updateValueAndValidity();
      });

    this.permitsRegulationsAndStandardsForm
      .get('meetsRegulatoryRequirements')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        const regulatoryRequirementsControl =
          this.permitsRegulationsAndStandardsForm.get(
            'meetsRegulatoryComments',
          );
        if (value === false) {
          regulatoryRequirementsControl?.clearValidators();
        } else {
          regulatoryRequirementsControl?.addValidators(Validators.required);
        }
        regulatoryRequirementsControl?.updateValueAndValidity();
      });

    this.permitsRegulationsAndStandardsForm
      .get('meetsEligibilityRequirements')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        const permitsArray = this.getPermitsFormArray();
        const meetsEligibilityCommentsControl =
          this.permitsRegulationsAndStandardsForm.get(
            'meetsEligibilityComments',
          );
        if (value === false) {
          meetsEligibilityCommentsControl?.clearValidators();
          permitsArray.clear();
        } else {
          meetsEligibilityCommentsControl?.addValidators(Validators.required);
          this.addPermit();
        }
        meetsEligibilityCommentsControl?.updateValueAndValidity();
      });

    const standardsValidControl =
      this.permitsRegulationsAndStandardsForm.get('standardsValid');

    this.permitsRegulationsAndStandardsForm
      .get('standardsAcceptable')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        const standardsControl = this.permitsRegulationsAndStandardsForm.get(
          'standards',
        ) as FormArray;

        switch (value) {
          case YesNoOption.Yes:
            this.standardsVisible = true;
            standardsValidControl?.addValidators(
              RxwebValidators.requiredTrue(),
            );
            break;
          case YesNoOption.No:
            this.standardsVisible = true;
            standardsValidControl?.reset();
            standardsValidControl?.clearValidators();
            break;
          case YesNoOption.NotApplicable:
            standardsControl.controls.forEach((control) => {
              control.get('standards')?.reset();
              control.get('isCategorySelected')?.reset();
            });
            this.standardsVisible = false;
            standardsValidControl?.reset();
            standardsValidControl?.clearValidators();
            break;

          default:
            break;
        }

        standardsValidControl?.updateValueAndValidity();
      });

    this.permitsRegulationsAndStandardsForm
      .get('standards')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        const standardsValidControl =
          this.permitsRegulationsAndStandardsForm.get('standardsValid');
        const isStandardsValid = value.some(
          (s: StandardInfoForm) => s.isCategorySelected,
        );

        standardsValidControl?.patchValue(isStandardsValid);
        standardsValidControl?.markAllAsTouched();
      });

    this.permitsRegulationsAndStandardsForm
      .get('permitsArray')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((permits: StringItem[]) => {
        this.permitsRegulationsAndStandardsForm.get('permits')?.patchValue(
          permits.map((permit) => permit.value),
          { emitEvent: false },
        );
      });
  }

  showStandardsRequiredTrueError() {
    const standardsControl =
      this.permitsRegulationsAndStandardsForm.get('standards');
    const standardsValidControl =
      this.permitsRegulationsAndStandardsForm.get('standardsValid');
    return standardsControl?.touched && standardsValidControl?.errors;
  }

  getStandardsInfoArrayControls() {
    return (
      this.permitsRegulationsAndStandardsForm.get('standards') as FormArray
    ).controls;
  }

  getCategoryControl(category: string) {
    const standardInfoArray = this.permitsRegulationsAndStandardsForm.get(
      'standards',
    ) as FormArray;

    const categoryControl = standardInfoArray.controls.filter(
      (c) => c?.get('category')?.value === category,
    );

    return categoryControl;
  }

  getStandardsOptions(category: string) {
    return this.optionsStore
      .getOptions()
      ?.standards?.()
      ?.find((s) => s.category === category)?.standards;
  }

  getPermitsFormArray() {
    return this.permitsRegulationsAndStandardsForm.get(
      'permitsArray',
    ) as FormArray;
  }

  addPermit() {
    const permitsArray = this.getPermitsFormArray();
    permitsArray.push(this.formBuilder.formGroup(StringItem));
  }

  removePermit(index: number) {
    const permitsArray = this.getPermitsFormArray();
    permitsArray.removeAt(index);
  }
}
