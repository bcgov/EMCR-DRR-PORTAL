import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup } from '@rxweb/reactive-form-validators';
import { DrrRadioButtonComponent } from '../../../shared/controls/drr-radio-button/drr-radio-button.component';
import { DrrTextareaComponent } from '../../../shared/controls/drr-textarea/drr-textarea.component';
import { ClimateAdaptationForm } from '../drif-fp-form';

@Component({
  selector: 'drif-fp-step-6',
  standalone: true,
  imports: [
    CommonModule,
    DrrRadioButtonComponent,
    DrrTextareaComponent,
    TranslocoModule,
    FormsModule,
    ReactiveFormsModule,
    MatInputModule,
  ],
  templateUrl: './drif-fp-step-6.component.html',
  styleUrl: './drif-fp-step-6.component.scss',
})
export class DrifFpStep6Component {
  @Input()
  climateAdaptationForm!: IFormGroup<ClimateAdaptationForm>;
}
