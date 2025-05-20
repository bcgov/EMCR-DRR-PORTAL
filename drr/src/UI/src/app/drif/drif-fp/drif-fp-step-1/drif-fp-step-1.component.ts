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
  FormArray,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { distinctUntilChanged } from 'rxjs';
import { DrrInputComponent } from '../../../shared/controls/drr-input/drr-input.component';
import { DrrRadioButtonComponent } from '../../../shared/controls/drr-radio-button/drr-radio-button.component';
import { DrrTextareaComponent } from '../../../shared/controls/drr-textarea/drr-textarea.component';
import { ContactDetailsForm, StringItem } from '../../drif-eoi/drif-eoi-form';
import { ProponentAndProjectInformationForm } from '../drif-fp-form';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drif-fp-step-1',
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
    DrrTextareaComponent,
    DrrRadioButtonComponent,
  ],
  templateUrl: './drif-fp-step-1.component.html',
  styleUrl: './drif-fp-step-1.component.scss',
})
export class DrifFpStep1Component {
  formBuilder = inject(RxFormBuilder);
  breakpointObserver = inject(BreakpointObserver);

  isMobile = false;

  @Input()
  proponentAndProjectInformationForm!: IFormGroup<ProponentAndProjectInformationForm>;

  @ViewChildren('contactFirstNameInput')
  contactFirstNameInput!: QueryList<DrrInputComponent>;

  @ViewChildren('proponentInput')
  proponentInput!: QueryList<DrrInputComponent>;

  ngOnInit() {
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .subscribe(({ matches }) => {
        this.isMobile = !matches;
      });

    this.proponentAndProjectInformationForm
      .get('partneringProponentsArray')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((proponents: StringItem[]) => {
        this.proponentAndProjectInformationForm
          .get('partneringProponents')
          ?.patchValue(
            proponents.map((proponent) => proponent.value),
            { emitEvent: false },
          );
      });

    const regionalProjectComments = this.proponentAndProjectInformationForm.get(
      'regionalProjectComments',
    );
    this.proponentAndProjectInformationForm
      .get('regionalProject')!
      .valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        if (value) {
          regionalProjectComments?.addValidators(Validators.required);
        } else {
          regionalProjectComments?.clearValidators();
        }

        regionalProjectComments?.reset();
        regionalProjectComments?.updateValueAndValidity();
      });
  }

  showRegionalProjectComments() {
    return this.proponentAndProjectInformationForm.get('regionalProject')
      ?.value;
  }

  getFormArray(name: string) {
    return this.proponentAndProjectInformationForm.get(name) as FormArray;
  }

  addAdditionalContact() {
    this.getFormArray('additionalContacts').push(
      this.formBuilder.formGroup(ContactDetailsForm),
    );

    this.focusOnLastContact();
  }

  focusOnLastContact() {
    setTimeout(() => {
      const lastContactInput = this.contactFirstNameInput.last;
      if (lastContactInput) {
        lastContactInput.focusOnInput();
      }
    }, 0);
  }

  removeAdditionalContact(index: number) {
    const additionalContacts = this.getFormArray('additionalContacts');
    additionalContacts.removeAt(index);
  }

  addProponent() {
    const proponents = this.getFormArray('partneringProponentsArray');
    proponents.push(this.formBuilder.formGroup(StringItem));

    this.focusOnLastProponent();
  }

  focusOnLastProponent() {
    setTimeout(() => {
      const lastProponentInput = this.proponentInput.last;
      if (lastProponentInput) {
        lastProponentInput.focusOnInput();
      }
    }, 0);
  }

  removeProponent(index: number) {
    const proponents = this.getFormArray('partneringProponentsArray');
    proponents.removeAt(index);
  }
}
