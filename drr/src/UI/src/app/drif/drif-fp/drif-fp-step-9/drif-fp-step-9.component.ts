import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup } from '@rxweb/reactive-form-validators';
import { DrrChipAutocompleteComponent } from '../../../shared/controls/drr-chip-autocomplete/drr-chip-autocomplete.component';
import { DrrRadioButtonComponent } from '../../../shared/controls/drr-radio-button/drr-radio-button.component';
import { DrrTextareaComponent } from '../../../shared/controls/drr-textarea/drr-textarea.component';
import {
  CapacityRisks,
  ComplexityRisks,
  ProjectRisksForm,
  ReadinessRisks,
  SensitivityRisks,
  TransferRisks,
} from '../drif-fp-form';

@Component({
  selector: 'drif-fp-step-9',
  standalone: true,
  imports: [
    CommonModule,
    MatInputModule,
    TranslocoModule,
    DrrChipAutocompleteComponent,
    DrrTextareaComponent,
    DrrRadioButtonComponent,
    FormsModule,
    ReactiveFormsModule,
    MatListModule,
  ],
  templateUrl: './drif-fp-step-9.component.html',
  styleUrl: './drif-fp-step-9.component.scss',
})
export class DrifFpStep9Component {
  @Input()
  projectRisksForm!: IFormGroup<ProjectRisksForm>;

  complexityRiskOptions = Object.values(ComplexityRisks);
  readinessRiskOptions = Object.values(ReadinessRisks);
  sensitivityRiskOptions = Object.values(SensitivityRisks);
  capacityRiskOptions = Object.values(CapacityRisks);
  transferRisksOptions = Object.values(TransferRisks);

  ngOnInit() {
    this.projectRisksForm
      .get('complexityRiskMitigated')
      ?.valueChanges.subscribe((value) => {
        if (value === false) {
          this.projectRisksForm.get('complexityRisks')?.disable();
        } else {
          this.projectRisksForm.get('complexityRisks')?.enable();
        }
      });

    this.projectRisksForm
      .get('readinessRiskMitigated')
      ?.valueChanges.subscribe((value) => {
        if (value === false) {
          this.projectRisksForm.get('readinessRisks')?.disable();
        } else {
          this.projectRisksForm.get('readinessRisks')?.enable();
        }
      });

    this.projectRisksForm
      .get('sensitivityRiskMitigated')
      ?.valueChanges.subscribe((value) => {
        if (value === false) {
          this.projectRisksForm.get('sensitivityRisks')?.disable();
        } else {
          this.projectRisksForm.get('sensitivityRisks')?.enable();
        }
      });

    this.projectRisksForm
      .get('capacityRiskMitigated')
      ?.valueChanges.subscribe((value) => {
        if (value === false) {
          this.projectRisksForm.get('capacityRisks')?.disable();
        } else {
          this.projectRisksForm.get('capacityRisks')?.enable();
        }
      });

    this.projectRisksForm
      .get('riskTransferMigigated')
      ?.valueChanges.subscribe((value) => {
        if (value === false) {
          this.projectRisksForm.get('transferRisks')?.setValue([]);
          this.projectRisksForm.get('transferRisks')?.disable();
        } else {
          this.projectRisksForm.get('transferRisks')?.enable();
        }
      });
  }

  getFormControl(name: string) {
    return this.projectRisksForm.get(name) as FormControl;
  }
}
