import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import {
  FormControl,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup } from '@rxweb/reactive-form-validators';
import { DrrTextareaComponent } from '../../shared/controls/drr-textarea/drr-textarea.component';
import { LocationInformationForm } from '../drif-eoi/drif-eoi-form';

@Component({
  selector: 'drif-eoi-step-4',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatRadioModule,
    TranslocoModule,
    DrrTextareaComponent,
  ],
  templateUrl: './drif-eoi-step-4.component.html',
  styleUrl: './drif-eoi-step-4.component.scss',
})
export class DrifEoiStep4Component {
  @Input()
  locationInformationForm!: IFormGroup<LocationInformationForm>;

  ngOnInit() {
    const ownershipDescription = this.locationInformationForm.get(
      'ownershipDescription'
    );
    this.locationInformationForm
      .get('ownershipDeclaration')!
      .valueChanges.subscribe((value) => {
        if (!value) {
          ownershipDescription?.addValidators(Validators.required);
        } else {
          ownershipDescription?.clearValidators();
        }

        ownershipDescription?.reset();
        ownershipDescription?.updateValueAndValidity();
      });
  }

  getFormControl(name: string): FormControl {
    return this.locationInformationForm.get(name) as FormControl;
  }
}
