import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatRadioModule } from '@angular/material/radio';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { IFormGroup } from '@rxweb/reactive-form-validators';
import { YesNoOption } from '../../../../model';
import { DrrInputComponent } from '../../../shared/controls/drr-input/drr-input.component';
import {
  DrrRadioButtonComponent,
  RadioOption,
} from '../../../shared/controls/drr-radio-button/drr-radio-button.component';
import { DrrTextareaComponent } from '../../../shared/controls/drr-textarea/drr-textarea.component';
import { OwnershipAndAuthorizationForm } from '../drif-fp-form';

@Component({
  selector: 'drif-fp-step-2',
  standalone: true,
  imports: [
    CommonModule,
    TranslocoModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatRadioModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    DrrTextareaComponent,
    DrrRadioButtonComponent,
    DrrInputComponent,
  ],
  templateUrl: './drif-fp-step-2.component.html',
  styleUrl: './drif-fp-step-2.component.scss',
})
export class DrifFpStep2Component {
  tranlocoService = inject(TranslocoService);

  @Input()
  ownershipAndAuthorizationForm!: IFormGroup<OwnershipAndAuthorizationForm>;

  yesNoBoolOptions: RadioOption[] = [
    { value: true, label: 'Yes' },
    { value: false, label: 'No' },
  ];

  allYesNoOptions: RadioOption[] = [
    { value: YesNoOption.Yes, label: 'Yes' },
    { value: YesNoOption.No, label: 'No' },
    { value: YesNoOption.NotApplicable, label: 'Not Applicable' },
  ];

  ngOnInit() {
    const ownershipDescription = this.ownershipAndAuthorizationForm.get(
      'ownershipDescription'
    );

    // check if comments should be mandatory
    if (
      !this.ownershipAndAuthorizationForm.get('ownershipDeclaration')!.value
    ) {
      ownershipDescription?.addValidators(Validators.required);
      ownershipDescription?.updateValueAndValidity();
    }

    this.ownershipAndAuthorizationForm
      .get('ownershipDeclaration')!
      .valueChanges.subscribe((value) => {
        if (!value) {
          ownershipDescription?.addValidators(Validators.required);
        } else {
          ownershipDescription?.clearValidators();
        }

        ownershipDescription?.updateValueAndValidity();
      });
  }
}
