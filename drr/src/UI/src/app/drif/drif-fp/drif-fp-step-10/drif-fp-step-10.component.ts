import { CommonModule, CurrencyPipe } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import {
  FormArray,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { distinctUntilChanged } from 'rxjs';
import { v4 as uuidv4 } from 'uuid';
import {
  CostCategory,
  CostEstimateClass,
  CostUnit,
  FundingStream,
  FundingType,
  ResourceCategory,
  YesNoOption,
} from '../../../../model';
import { DrrChipAutocompleteComponent } from '../../../shared/controls/drr-chip-autocomplete/drr-chip-autocomplete.component';
import { DrrCurrencyInputComponent } from '../../../shared/controls/drr-currency-input/drr-currency-input.component';
import { DrrInputComponent } from '../../../shared/controls/drr-input/drr-input.component';
import {
  DrrRadioButtonComponent,
  RadioOption,
} from '../../../shared/controls/drr-radio-button/drr-radio-button.component';
import {
  DrrSelectComponent,
  DrrSelectOption,
} from '../../../shared/controls/drr-select/drr-select.component';
import { DrrTextareaComponent } from '../../../shared/controls/drr-textarea/drr-textarea.component';
import { OptionsStore } from '../../../store/options.store';
import { FundingInformationItemForm } from '../../drif-eoi/drif-eoi-form';
import { DrrFundingListComponent } from '../../drr-funding-list/drr-funding-list.component';

import { DrrNumericInputComponent } from '../../../shared/controls/drr-number-input/drr-number-input.component';
import {
  BudgetForm,
  CostEstimateForm,
  YearOverYearFundingForm,
} from '../drif-fp-form';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drif-fp-step-10',
  standalone: true,
  imports: [
    CommonModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatDividerModule,
    MatFormFieldModule,
    MatCardModule,
    TranslocoModule,
    FormsModule,
    ReactiveFormsModule,
    DrrInputComponent,
    DrrNumericInputComponent,
    DrrCurrencyInputComponent,
    DrrSelectComponent,
    DrrTextareaComponent,
    DrrFundingListComponent,
    DrrRadioButtonComponent,
    DrrChipAutocompleteComponent,
  ],
  providers: [CurrencyPipe],
  templateUrl: './drif-fp-step-10.component.html',
  styleUrl: './drif-fp-step-10.component.scss',
})
export class DrifFpStep10Component {
  optionsStore = inject(OptionsStore);
  formBuilder = inject(RxFormBuilder);
  translocoService = inject(TranslocoService);

  @Input()
  budgetForm!: IFormGroup<BudgetForm>;

  private _fundingStream!: FundingStream;
  @Input()
  set fundingStream(fundingStream: FundingStream) {
    this._fundingStream = fundingStream;
    if (this.isStrucutralProject()) {
      this.setValidatorsForStructuralProject();
    }
  }
  get fundingStream() {
    return this._fundingStream;
  }

  isMobile = false;

  costEstimateClassOptions: RadioOption[] = Object.keys(CostEstimateClass)
    .map((key) => ({
      value: key,
      label: this.translocoService.translate(`costEstimateClassType.${key}`),
    }))
    .sort((a, b) => a.label.localeCompare(b.label));

  fiscalYearsOptions =
    this.optionsStore.options.fiscalYears?.()?.map((value) => ({
      value,
      label: value,
    })) ?? [];
  previousResponseOptions = [
    { value: YesNoOption.Yes, label: 'Yes' },
    {
      value: YesNoOption.NotApplicable,
      label: this.translocoService.translate('costUnknown'),
    },
    { value: YesNoOption.No, label: 'No' },
  ];
  costConsiderationsOptions =
    this.optionsStore.options.costConsiderations?.() ?? [];

  costCategoriesOptions: DrrSelectOption[] = Object.values(CostCategory)
    .filter((value) =>
      this.isStrucutralProject() ? value !== CostCategory.Contingency : value,
    )
    .map((value) => ({
      value,
      label: this.translocoService.translate(value),
    }))
    .sort((a, b) => a.label.localeCompare(b.label));
  resourcesOptions: DrrSelectOption[] = Object.values(ResourceCategory)
    .map((value) => ({
      value,
      label: this.translocoService.translate(value),
    }))
    .sort((a, b) => a.label.localeCompare(b.label));
  unitsOptions: DrrSelectOption[] = Object.values(CostUnit)
    .map((value) => ({
      value,
      label: this.translocoService.translate(value),
    }))
    .sort((a, b) => a.label.localeCompare(b.label));

  originalTotalProjectCost?: number;

  ngOnInit() {
    this.budgetForm
      .get('yearOverYearFunding')!
      .valueChanges.pipe(distinctUntilChanged())
      .subscribe((years: YearOverYearFundingForm[]) => {
        const total = years.reduce((acc, year) => acc + Number(year.amount), 0);
        this.budgetForm.get('totalDrifFundingRequest')?.setValue(total);

        if (total !== this.budgetForm.get('eligibleFundingRequest')?.value) {
          this.budgetForm
            .get('discrepancyComment')
            ?.setValidators(Validators.required);
        } else {
          this.budgetForm.get('discrepancyComment')?.clearValidators();
        }

        this.budgetForm.get('discrepancyComment')?.updateValueAndValidity();
      });

    this.budgetForm
      .get('totalDrifFundingRequest')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe(() => {
        this.calculateRemainingAmount();
        this.budgetForm.get('costEstimates')?.updateValueAndValidity();
      });

    this.budgetForm
      .get('otherFunding')!
      .valueChanges.pipe(distinctUntilChanged())
      .subscribe(() => {
        this.calculateRemainingAmount();
      });

    this.budgetForm
      .get('totalProjectCost')!
      .valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        this.calculateRemainingAmount();

        if (value === null) {
          return;
        }

        if (
          this.originalTotalProjectCost === undefined &&
          !this.originalTotalProjectCost
        ) {
          this.originalTotalProjectCost = value;
        }

        this.hasTotalProjectCostChanged()
          ? this.budgetForm
              .get('totalProjectCostChangeComments')
              ?.setValidators(Validators.required)
          : this.budgetForm
              .get('totalProjectCostChangeComments')
              ?.clearValidators();
      });

    this.budgetForm
      .get('haveOtherFunding')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        if (value) {
          this.getFormArray('otherFunding').enable();
          if (this.getFormArray('otherFunding').length === 0) {
            this.getFormArray('otherFunding').push(
              this.formBuilder.formGroup(FundingInformationItemForm),
            );
          }
        } else {
          this.getFormArray('otherFunding').clear();
          this.getFormArray('otherFunding').disable();
        }
      });

    this.budgetForm
      .get('previousResponse')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        const previousResponseCost = this.budgetForm.get(
          'previousResponseCost',
        );
        const previousResponseComments = this.budgetForm.get(
          'previousResponseComments',
        );

        switch (value) {
          case YesNoOption.Yes:
            previousResponseCost?.setValidators(Validators.required);
            previousResponseComments?.setValidators(Validators.required);
            break;
          case YesNoOption.NotApplicable:
            previousResponseCost?.clearValidators();
            previousResponseCost?.reset();
            previousResponseComments?.setValidators(Validators.required);
            break;
          case YesNoOption.No:
            previousResponseCost?.clearValidators();
            previousResponseCost?.reset();
            previousResponseComments?.clearValidators();
            previousResponseComments?.reset();
            break;

          default:
            break;
        }

        previousResponseCost?.updateValueAndValidity();
        previousResponseComments?.updateValueAndValidity();
      });

    this.budgetForm
      .get('costConsiderationsApplied')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        const costConsiderations = this.budgetForm.get('costConsiderations');
        const costConsiderationsComments = this.budgetForm.get(
          'costConsiderationsComments',
        );
        if (value) {
          costConsiderations?.setValidators(Validators.required);
          costConsiderationsComments?.setValidators(Validators.required);
        } else {
          costConsiderations?.clearValidators();
          costConsiderations?.reset();
          costConsiderationsComments?.clearValidators();
          costConsiderationsComments?.reset();
        }

        costConsiderations?.updateValueAndValidity();
        costConsiderationsComments?.updateValueAndValidity();
      });

    this.getFormArray('costEstimates').controls.length === 0 && this.addCost();

    this.budgetForm
      .get('costEstimates')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe(() => {
        let totalCost = 0;

        // iterate over cost estimates and calculate total cost
        this.getFormArray('costEstimates').controls.forEach((costEstimate) => {
          const unitRate = costEstimate.get('unitRate')?.value;
          const quantity = costEstimate.get('quantity')?.value;
          const cost = unitRate * quantity;
          costEstimate.get('totalCost')?.setValue(cost, {
            emitEvent: false,
          });

          totalCost += cost;
        });

        if (this.isStrucutralProject()) {
          let contingency = 0;

          // fetch contingency value from costEstimate array and calculate what % of total cost it is
          const contingencyCostEstimate = this.getFormArray(
            'costEstimates',
          ).controls.find(
            (costEstimate) =>
              costEstimate.get('costCategory')?.value ===
              CostCategory.Contingency,
          );
          const contingencyTotalCost =
            contingencyCostEstimate?.get('totalCost')?.value;

          if (contingencyTotalCost) {
            contingency = (contingencyTotalCost / totalCost) * 100;
          }

          this.budgetForm.get('contingency')?.setValue(contingency.toFixed(0));
        }

        this.budgetForm.get('totalEligibleCosts')?.setValue(totalCost);

        this.calculateDiscrepancy();

        const totalDrifFundingRequest = this.budgetForm?.get(
          'totalDrifFundingRequest',
        )?.value;
        this.budgetForm
          .get('estimatesMatchFundingRequest')
          ?.setValue(totalCost === totalDrifFundingRequest);
      });

    this.budgetForm
      .get('contingency')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe(() => {
        this.budgetForm.get('costEstimates')?.updateValueAndValidity();
      });
  }

  setValidatorsForStructuralProject() {
    this.budgetForm
      .get('costEstimateClass')
      ?.addValidators(Validators.required);
    this.budgetForm.get('contingency')?.addValidators(Validators.required);
    this.budgetForm
      .get('isContingencyPercentageThreasholdMet')
      ?.addValidators(Validators.requiredTrue);
  }

  hasTotalProjectCostChanged() {
    return (
      this.budgetForm.get('totalProjectCost')?.value !==
      this.originalTotalProjectCost
    );
  }

  showDiscrepancyComment() {
    return this.budgetForm.get('fundingRequestDiscrepancy')?.value != 0;
  }

  showForecastingDiscrepancy() {
    return (
      this.budgetForm.get('totalDrifFundingRequest')?.value !==
      this.budgetForm.get('totalEligibleCosts')?.value
    );
  }

  calculateRemainingAmount() {
    // how much I'm covering with other funding
    let otherFundingSum = this.getFormArray('otherFunding').controls.reduce(
      (total, funding) => total + Number(funding.value.amount),
      0,
    );
    // check if number
    if (isNaN(otherFundingSum)) {
      otherFundingSum = 0;
    }

    // how much will the project cost, but not how much I'm asking for
    const totalProjectCost =
      this.budgetForm.get('totalProjectCost')?.value ?? 0;
    // how much I'm asking for
    const totalEligibleCosts =
      this.budgetForm.get('totalEligibleCosts')?.value ?? 0;

    const estimatedUnfundedAmount = totalProjectCost - totalEligibleCosts;
    this.budgetForm.patchValue({ estimatedUnfundedAmount });

    // how much is left to cover and I need to explain how I'm going to cover it
    let remainingAmount = estimatedUnfundedAmount - otherFundingSum;
    this.budgetForm.patchValue({ remainingAmount });

    const intendToSecureFunding = this.budgetForm.get('intendToSecureFunding');

    if (remainingAmount > 0) {
      intendToSecureFunding?.addValidators(Validators.required);
    } else {
      intendToSecureFunding?.clearValidators();
      intendToSecureFunding?.reset();
    }

    intendToSecureFunding?.updateValueAndValidity();
  }

  calculateDiscrepancy() {
    const discrepancy =
      this.budgetForm.get('totalEligibleCosts')?.value -
      this.budgetForm.get('eligibleFundingRequest')?.value;

    this.budgetForm.get('fundingRequestDiscrepancy')?.setValue(discrepancy);
  }

  getFormArray(formArrayName: string) {
    return this.budgetForm.get(formArrayName) as FormArray;
  }

  addYear() {
    this.getFormArray('yearOverYearFunding').push(
      this.formBuilder.formGroup(YearOverYearFundingForm),
    );
  }

  removeYear(index: number) {
    this.getFormArray('yearOverYearFunding').removeAt(index);
  }

  hasOtherGrants(selectValue: FundingType[]) {
    return selectValue?.includes(FundingType.OtherGrants);
  }

  removeOtherSource(index: number) {
    this.getFormArray('otherFunding').removeAt(index);
  }

  addOtherFunding() {
    this.getFormArray('otherFunding').push(
      this.formBuilder.formGroup(FundingInformationItemForm),
    );
  }

  getRemainingAmount() {
    return this.budgetForm.get('remainingAmount')?.value;
  }

  isStrucutralProject() {
    return this.fundingStream === FundingStream.Stream2;
  }

  addCost() {
    const newCostEstimateForm = this.formBuilder.formGroup(CostEstimateForm);

    newCostEstimateForm.get('id')?.setValue(uuidv4());

    this.getFormArray('costEstimates').push(newCostEstimateForm);
  }

  removeCost(id: string) {
    this.getFormArray('costEstimates').removeAt(
      this.getFormArray('costEstimates').controls.findIndex(
        (control) => control.get('id')?.value === id,
      ),
    );
  }

  isContingencyPercentageThreasholdBroken() {
    const contingency = this.budgetForm.get('contingency')?.value ?? 0;
    const costEstimateClass = this.budgetForm.get('costEstimateClass')?.value;

    if (costEstimateClass === CostEstimateClass.ClassA && contingency > 15) {
      this.budgetForm
        .get('isContingencyPercentageThreasholdMet')
        ?.setValue(false, { emitEvent: false });
      return true;
    }

    if (costEstimateClass === CostEstimateClass.ClassB && contingency > 25) {
      this.budgetForm
        .get('isContingencyPercentageThreasholdMet')
        ?.setValue(false, { emitEvent: false });
      return true;
    }

    this.budgetForm
      .get('isContingencyPercentageThreasholdMet')
      ?.setValue(true, { emitEvent: false });
    return false;
  }
}
