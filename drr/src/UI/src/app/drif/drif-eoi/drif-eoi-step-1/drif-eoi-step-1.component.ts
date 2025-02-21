import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormArray, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { distinctUntilChanged, Subscription } from 'rxjs';
import { ProponentType } from '../../../../model';
import { DrrInputComponent } from '../../../shared/controls/drr-input/drr-input.component';
import {
  DrrRadioButtonComponent,
  DrrRadioOption,
} from '../../../shared/controls/drr-radio-button/drr-radio-button.component';
import { ProfileStore } from '../../../store/profile.store';
import {
  ContactDetailsForm,
  ProponentInformationForm,
  StringItem,
} from '../drif-eoi-form';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drif-eoi-step-1',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatDividerModule,
    MatButtonModule,
    MatCheckboxModule,
    TranslocoModule,
    DrrInputComponent,
    DrrRadioButtonComponent,
  ],
  templateUrl: './drif-eoi-step-1.component.html',
  styleUrl: './drif-eoi-step-1.component.scss',
})
export class DrifEoiStep1Component {
  formBuilder = inject(RxFormBuilder);
  breakpointObserver = inject(BreakpointObserver);
  profileStore = inject(ProfileStore);
  translocoService = inject(TranslocoService);

  submitterSub: Subscription | undefined;
  isMobile = false;

  proponentTypeOptions: DrrRadioOption[] = Object.values(ProponentType).map(
    (value) => ({
      value,
      label: this.translocoService.translate(value),
    }),
  );

  @Input()
  proponentInformationForm!: IFormGroup<ProponentInformationForm>;

  ngOnInit() {
    this.proponentInformationForm
      ?.get('proponentName')
      ?.setValue(this.profileStore.organization(), { emitEvent: false });
    this.proponentInformationForm?.get('proponentName')?.disable();

    this.proponentInformationForm
      .get('partneringProponentsArray')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((proponents: StringItem[]) => {
        this.proponentInformationForm.get('partneringProponents')?.patchValue(
          proponents.map((proponent) => proponent.value),
          { emitEvent: false },
        );
      });

    this.breakpointObserver
      .observe('(min-width: 768px)')
      .subscribe(({ matches }) => {
        this.isMobile = !matches;
      });
  }

  getFormArray(formArrayName: string) {
    return this.proponentInformationForm.get(formArrayName) as FormArray;
  }

  addAdditionalContact() {
    this.getFormArray('additionalContacts').push(
      this.formBuilder.formGroup(ContactDetailsForm),
    );
  }

  removeAdditionalContact(index: number) {
    const additionalContacts = this.getFormArray('additionalContacts');
    additionalContacts.removeAt(index);
  }

  addProponent() {
    const proponents = this.getFormArray('partneringProponentsArray');
    proponents.push(this.formBuilder.formGroup(StringItem));
  }

  removeProponent(index: number) {
    const proponents = this.getFormArray('partneringProponentsArray');
    proponents.removeAt(index);
  }
}
