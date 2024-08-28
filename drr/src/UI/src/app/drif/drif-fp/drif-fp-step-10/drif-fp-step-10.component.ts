import { CommonModule, CurrencyPipe } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import {
  FormArray,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectChange } from '@angular/material/select';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { distinctUntilChanged } from 'rxjs';
import { FundingType, YesNoOption } from '../../../../model';
import { DrrChipAutocompleteComponent } from '../../../shared/controls/drr-chip-autocomplete/drr-chip-autocomplete.component';
import { DrrCurrencyInputComponent } from '../../../shared/controls/drr-currency-input/drr-currency-input.component';
import { DrrInputComponent } from '../../../shared/controls/drr-input/drr-input.component';
import { DrrRadioButtonComponent } from '../../../shared/controls/drr-radio-button/drr-radio-button.component';
import { DrrSelectComponent } from '../../../shared/controls/drr-select/drr-select.component';
import { DrrTextareaComponent } from '../../../shared/controls/drr-textarea/drr-textarea.component';
import { FundingInformationItemForm } from '../../drif-eoi/drif-eoi-form';
import { DrrFundingListComponent } from '../../drr-funding-list/drr-funding-list.component';
import {
  BudgetForm,
  CostConsiderations,
  YearOverYearFundingForm,
} from '../drif-fp-form';

@Component({
  selector: 'drif-fp-step-10',
  standalone: true,
  imports: [
    CommonModule,
    MatInputModule,
    DrrInputComponent,
    DrrCurrencyInputComponent,
    DrrSelectComponent,
    DrrTextareaComponent,
    TranslocoModule,
    FormsModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    DrrFundingListComponent,
    DrrRadioButtonComponent,
    DrrChipAutocompleteComponent,
  ],
  providers: [CurrencyPipe],
  templateUrl: './drif-fp-step-10.component.html',
  styleUrl: './drif-fp-step-10.component.scss',
})
export class DrifFpStep10Component {
  formBuilder = inject(RxFormBuilder);

  @Input()
  budgetForm!: IFormGroup<BudgetForm>;

  isMobile = false;

  fiscalYearsOptions: string[] = [];
  fundingTypeOptions = Object.values(FundingType);
  previousResponseOptions = [
    { value: YesNoOption.Yes, label: 'Yes' },
    { value: YesNoOption.NotApplicable, label: 'Yes, but costs unknown' },
    { value: YesNoOption.No, label: 'No' },
  ];
  costConsiderationsOptions = Object.values(CostConsiderations);

  ngOnInit() {
    const currentYear = new Date().getFullYear();
    const currentMonth = new Date().getMonth();
    const startFiscalYear = currentMonth >= 6 ? currentYear : currentYear - 1; // Assuming fiscal year starts in July

    this.fiscalYearsOptions = Array.from({ length: 10 }, (_, i) => {
      const startYear = startFiscalYear + i;
      const endYear = startYear + 1;
      return `${startYear}/${endYear}`;
    });

    this.budgetForm
      .get('yearOverYearFunding')!
      .valueChanges.subscribe((years: YearOverYearFundingForm[]) => {
        const total = years.reduce((acc, year) => acc + Number(year.amount), 0);
        this.budgetForm.get('totalDrifFundingRequest')?.setValue(total);
      });

    this.budgetForm
      .get('otherFunding')!
      .valueChanges.pipe(distinctUntilChanged())
      .subscribe(() => {
        this.calculateRemainingAmount();
      });

    this.budgetForm
      .get('fundingRequest')!
      .valueChanges.pipe(distinctUntilChanged())
      .subscribe(() => {
        this.calculateRemainingAmount();
      });

    this.budgetForm
      .get('estimatedTotal')!
      .valueChanges.pipe(distinctUntilChanged())
      .subscribe(() => {
        this.calculateRemainingAmount();
      });

    if (this.budgetForm.get('haveOtherFunding')?.value !== true) {
      this.getFormArray('otherFunding').clear({ emitEvent: false });
      this.getFormArray('otherFunding').disable();
    }
    this.budgetForm.get('haveOtherFunding')?.valueChanges.subscribe((value) => {
      if (value) {
        this.getFormArray('otherFunding').enable();
        if (this.getFormArray('otherFunding').length === 0) {
          this.getFormArray('otherFunding').push(
            this.formBuilder.formGroup(FundingInformationItemForm)
          );
        }
      } else {
        this.getFormArray('otherFunding').clear();
        this.getFormArray('otherFunding').disable();
      }
    });

    this.budgetForm
      .get('costConsiderationsApplied')
      ?.valueChanges.subscribe((value) => {
        if (value) {
          this.budgetForm.get('costConsiderations')?.enable();
          this.budgetForm
            .get('costConsiderationsComments')
            ?.setValidators(Validators.required);
        } else {
          this.budgetForm.get('costConsiderations')?.disable();
          this.budgetForm.get('costConsiderationsComments')?.clearValidators();
        }

        this.budgetForm
          .get('costConsiderationsComments')
          ?.updateValueAndValidity();
      });
  }

  calculateRemainingAmount() {
    const estimatedTotal = this.budgetForm.get('estimatedTotal')?.value ?? 0;

    let otherFundingSum = this.getFormArray('otherFunding').controls.reduce(
      (total, funding) => total + Number(funding.value.amount),
      0
    );
    // check if number
    if (isNaN(otherFundingSum)) {
      otherFundingSum = 0;
    }

    const fundingRequest = this.budgetForm.get('fundingRequest')?.value ?? 0;

    let remainingAmount = estimatedTotal - otherFundingSum - fundingRequest;

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

  getFormArray(formArrayName: string) {
    return this.budgetForm.get(formArrayName) as FormArray;
  }

  getArrayFormControl(
    formArrayName: string,
    controlName: string,
    index: number
  ) {
    return this.getFormArray(formArrayName)?.at(index).get(controlName);
  }

  addYear() {
    this.getFormArray('yearOverYearFunding').push(
      this.formBuilder.formGroup(YearOverYearFundingForm)
    );
  }

  removeYear(index: number) {
    this.getFormArray('years').removeAt(index);
  }

  hasOtherGrants(selectValue: FundingType[]) {
    return selectValue?.includes(FundingType.OtherGrants);
  }

  setFundingTypeDesctiption(event: MatSelectChange, index: number) {
    const descriptionControl = this.getArrayFormControl(
      'otherDescription',
      'otherFunding',
      index
    );

    // check if value contains FundingType.OtherGrants
    if (this.hasOtherGrants(event.value)) {
      descriptionControl?.addValidators(Validators.required);
    } else {
      descriptionControl?.clearValidators();
    }

    descriptionControl?.reset();
    descriptionControl?.updateValueAndValidity();
  }

  removeOtherSource(index: number) {
    this.getFormArray('otherFunding').removeAt(index);
  }

  addOtherFunding() {
    this.getFormArray('otherFunding').push(
      this.formBuilder.formGroup(FundingInformationItemForm)
    );
  }

  getRemainingAmount() {
    return this.budgetForm.get('remainingAmount')?.value;
  }

  getRemainingAmountAbs() {
    return Math.abs(this.getRemainingAmount());
  }
}
