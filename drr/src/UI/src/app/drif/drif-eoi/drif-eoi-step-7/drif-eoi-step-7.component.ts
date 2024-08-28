import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup } from '@rxweb/reactive-form-validators';
import { DrrTextareaComponent } from '../../../shared/controls/drr-textarea/drr-textarea.component';
import { OtherSupportingInformationForm } from '../drif-eoi-form';

@Component({
  selector: 'drif-eoi-step-7',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    TranslocoModule,
    DrrTextareaComponent,
  ],
  templateUrl: './drif-eoi-step-7.component.html',
  styleUrl: './drif-eoi-step-7.component.scss',
})
export class DrifEoiStep7Component {
  @Input()
  otherSupportingInformationForm!: IFormGroup<OtherSupportingInformationForm>;

  getFormControl(name: string): FormControl {
    return this.otherSupportingInformationForm.get(name) as FormControl;
  }
}
