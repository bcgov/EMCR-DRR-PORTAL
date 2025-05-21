import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import {
  Component,
  inject,
  Input,
  QueryList,
  ViewChildren,
} from '@angular/core';
import {
  AbstractControl,
  FormArray,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectChange } from '@angular/material/select';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { RxFormBuilder } from '@rxweb/reactive-form-validators';
import { FundingType } from '../../../model';
import { DrrCurrencyInputComponent } from '../../shared/controls/drr-currency-input/drr-currency-input.component';
import { DrrInputComponent } from '../../shared/controls/drr-input/drr-input.component';
import { DrrSelectComponent } from '../../shared/controls/drr-select/drr-select.component';
import { FundingInformationItemForm } from '../drif-eoi/drif-eoi-form';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-funding-list',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatDividerModule,
    DrrCurrencyInputComponent,
    DrrInputComponent,
    DrrSelectComponent,
    TranslocoModule,
    FormsModule,
    ReactiveFormsModule,
  ],
  templateUrl: './drr-funding-list.component.html',
  styleUrl: './drr-funding-list.component.scss',
})
export class DrrFundingListComponent {
  breakpointObserver = inject(BreakpointObserver);
  formBuilder = inject(RxFormBuilder);
  translocoService = inject(TranslocoService);

  isMobile = false;
  fundingTypeOptions = Object.values(FundingType).map((value) => ({
    value,
    label: this.translocoService.translate(value),
  }));

  @Input()
  fundingFormArray!: FormArray;

  @ViewChildren('amountInput')
  amountInputs!: QueryList<DrrCurrencyInputComponent>;

  ngOnInit() {
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .subscribe(({ matches }) => {
        this.isMobile = !matches;
      });
  }

  setFundingTypeDesctiption(
    event: MatSelectChange,
    descriptionControl: AbstractControl,
  ) {
    // check if value contains FundingType.OtherGrants
    if (this.hasOtherGrants(event.value)) {
      descriptionControl?.addValidators(Validators.required);
    } else {
      descriptionControl?.clearValidators();
    }

    descriptionControl?.reset();
    descriptionControl?.updateValueAndValidity();
  }

  hasOtherGrants(selectValue: FundingType[]) {
    return selectValue?.includes(FundingType.OtherGrants);
  }

  addOtherFunding() {
    this.fundingFormArray.push(
      this.formBuilder.formGroup(FundingInformationItemForm),
    );

    this.focusOnLastAmountInput();
  }

  focusOnLastAmountInput() {
    setTimeout(() => {
      const amountInput = this.amountInputs.last;
      if (amountInput) {
        amountInput.focusOnInput();
      }
    }, 0);
  }

  removeOtherSource(index: number) {
    this.fundingFormArray.removeAt(index);
  }
}
