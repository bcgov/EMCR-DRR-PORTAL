import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup } from '@rxweb/reactive-form-validators';
import { DrrTextareaComponent } from '../../shared/controls/drr-textarea/drr-textarea.component';
import { EngagementPlanForm } from '../drif-eoi/drif-eoi-form';

@Component({
  selector: 'drif-eoi-step-6',
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
  templateUrl: './drif-eoi-step-6.component.html',
  styleUrl: './drif-eoi-step-6.component.scss',
})
export class DrifEoiStep6Component {
  @Input()
  engagementPlanForm!: IFormGroup<EngagementPlanForm>;

  getFormControl(name: string): FormControl {
    return this.engagementPlanForm.get(name) as FormControl;
  }
}
