import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormArray, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { EstimatedNumberOfPeople, Hazards } from '../../../../model';
import { DrrInputComponent } from '../../../shared/controls/drr-input/drr-input.component';
import { DrrSelectComponent } from '../../../shared/controls/drr-select/drr-select.component';
import { DrrTextareaComponent } from '../../../shared/controls/drr-textarea/drr-textarea.component';
import { ImpactedInfrastructureForm, ProjectAreaForm } from '../drif-fp-form';

@Component({
  selector: 'drif-fp-step-3',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslocoModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    DrrTextareaComponent,
    DrrInputComponent,
    DrrSelectComponent,
  ],
  templateUrl: './drif-fp-step-3.component.html',
  styleUrl: './drif-fp-step-3.component.scss',
})
export class DrifFpStep3Component {
  formBuilder = inject(RxFormBuilder);

  @Input() projectAreaForm!: IFormGroup<ProjectAreaForm>;

  unitOptions = ['m2', 'ha']; // TODO: use enum
  estimatedPeopleImpactedOptions = Object.values(EstimatedNumberOfPeople);
  hazardsOptions = Object.values(Hazards);

  getInfrastructureImpacted() {
    return this.projectAreaForm.get('infrastructureImpacted') as FormArray;
  }

  addInfrastructureImpacted() {
    this.getInfrastructureImpacted().push(
      this.formBuilder.formGroup(ImpactedInfrastructureForm)
    );
  }

  removeInfrastructureImpacted(index: number) {
    this.getInfrastructureImpacted().removeAt(index);
  }

  otherHazardSelected() {
    return this.projectAreaForm.get('relatedHazards')?.value?.includes('Other');
  }
}
