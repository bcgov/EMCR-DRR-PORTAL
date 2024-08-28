import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { FormArray, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { TranslocoModule } from '@ngneat/transloco';
import {
  IFormGroup,
  RxFormBuilder,
  RxFormControl,
} from '@rxweb/reactive-form-validators';
import { Subscription } from 'rxjs';
import { DrrInputComponent } from '../../../shared/controls/drr-input/drr-input.component';
import { ProfileStore } from '../../../store/profile.store';
import {
  ContactDetailsForm,
  ProponentInformationForm,
  StringItem,
} from '../drif-eoi-form';

@Component({
  selector: 'drif-eoi-step-1',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatRadioModule,
    MatIconModule,
    MatDividerModule,
    MatButtonModule,
    MatCheckboxModule,
    TranslocoModule,
    DrrInputComponent,
  ],
  templateUrl: './drif-eoi-step-1.component.html',
  styleUrl: './drif-eoi-step-1.component.scss',
})
export class DrifEoiStep1Component {
  formBuilder = inject(RxFormBuilder);
  breakpointObserver = inject(BreakpointObserver);
  profileStore = inject(ProfileStore);

  submitterSub: Subscription | undefined;
  isMobile = false;

  @Input()
  proponentInformationForm!: IFormGroup<ProponentInformationForm>;

  ngOnInit() {
    this.proponentInformationForm
      ?.get('proponentName')
      ?.setValue(this.profileStore.organization(), { emitEvent: false });
    this.proponentInformationForm?.get('proponentName')?.disable();

    this.proponentInformationForm
      .get('partneringProponentsArray')
      ?.valueChanges.subscribe((proponents: StringItem[]) => {
        this.proponentInformationForm.get('partneringProponents')?.patchValue(
          proponents.map((proponent) => proponent.value),
          { emitEvent: false }
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

  getFormControl(name: string): RxFormControl {
    return this.proponentInformationForm.get(name) as RxFormControl;
  }

  getGroupFormControl(controlName: string, groupName: string): RxFormControl {
    return this.proponentInformationForm
      .get(groupName)
      ?.get(controlName) as RxFormControl;
  }

  getArrayFormControl(
    controlName: string,
    arrayName: string,
    index: number
  ): RxFormControl {
    return this.getFormArray(arrayName)?.controls[index]?.get(
      controlName
    ) as RxFormControl;
  }

  addAdditionalContact() {
    this.getFormArray('additionalContacts').push(
      this.formBuilder.formGroup(ContactDetailsForm)
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
