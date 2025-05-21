import { CommonModule } from '@angular/common';
import {
  Component,
  Input,
  QueryList,
  ViewChildren,
  inject,
} from '@angular/core';
import { FormArray, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { distinctUntilChanged } from 'rxjs';
import { EstimatedNumberOfPeople } from '../../../../model';
import { DrrInputComponent } from '../../../shared/controls/drr-input/drr-input.component';
import { DrrRadioButtonComponent } from '../../../shared/controls/drr-radio-button/drr-radio-button.component';
import { DrrSelectComponent } from '../../../shared/controls/drr-select/drr-select.component';
import { DrrTextareaComponent } from '../../../shared/controls/drr-textarea/drr-textarea.component';
import {
  InfrastructureImpactedForm,
  ProjectDetailsForm,
} from '../drif-eoi-form';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drif-eoi-step-5',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    TranslocoModule,
    DrrTextareaComponent,
    DrrInputComponent,
    DrrSelectComponent,
    DrrRadioButtonComponent,
  ],
  templateUrl: './drif-eoi-step-5.component.html',
  styleUrl: './drif-eoi-step-5.component.scss',
})
export class DrifEoiStep5Component {
  formBuilder = inject(RxFormBuilder);
  translocoService = inject(TranslocoService);

  estimatedNumberOfPeopleOptions = Object.values(EstimatedNumberOfPeople).map(
    (value) => ({
      value,
      label: this.translocoService.translate(value),
    }),
  );

  @Input()
  projectDetailsForm!: IFormGroup<ProjectDetailsForm>;

  @ViewChildren('infrastructureInput')
  infrastructureInputs!: QueryList<DrrInputComponent>;

  ngOnInit() {
    this.projectDetailsForm
      .get('isInfrastructureImpacted')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        const infrastructureImpacted = this.projectDetailsForm.get(
          'infrastructureImpacted',
        ) as FormArray;
        if (!value) {
          infrastructureImpacted?.clear();
        } else {
          if (infrastructureImpacted?.length === 0) {
            this.addInfrastructure();
          }
        }
      });
  }

  getFormArray(formArrayName: string) {
    return this.projectDetailsForm.get(formArrayName) as FormArray;
  }

  addInfrastructure() {
    this.getFormArray('infrastructureImpacted').push(
      this.formBuilder.formGroup(InfrastructureImpactedForm),
    );

    this.focusOnLastInfrastructure();
  }

  focusOnLastInfrastructure() {
    setTimeout(() => {
      const lastInfrastructureInput = this.infrastructureInputs.last;
      if (lastInfrastructureInput) {
        lastInfrastructureInput.focusOnInput();
      }
    }, 0);
  }

  removeInfrastructure(index: number) {
    this.getFormArray('infrastructureImpacted').removeAt(index);
  }
}
