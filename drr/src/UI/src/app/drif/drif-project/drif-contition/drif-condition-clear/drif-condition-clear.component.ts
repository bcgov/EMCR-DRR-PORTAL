import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { CommonModule } from '@angular/common';
import { Component, HostListener, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import {
  MatStepperModule,
  StepperOrientation,
} from '@angular/material/stepper';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import {
  IFormGroup,
  RxFormBuilder,
  RxReactiveFormsModule,
} from '@rxweb/reactive-form-validators';
import { ProjectService } from '../../../../../api/project/project.service';
import { DrrDatepickerComponent } from '../../../../shared/controls/drr-datepicker/drr-datepicker.component';
import { DrrInputComponent } from '../../../../shared/controls/drr-input/drr-input.component';
import { DrrNumericInputComponent } from '../../../../shared/controls/drr-number-input/drr-number-input.component';
import { DrrTextareaComponent } from '../../../../shared/controls/drr-textarea/drr-textarea.component';
import { ConditionForm } from '../drif-condition-form';

@Component({
  selector: 'drif-condition-clear',
  standalone: true,
  imports: [
    CommonModule,
    MatStepperModule,
    MatButtonModule,
    MatIconModule,
    TranslocoModule,
    RxReactiveFormsModule,
    FormsModule,
    MatFormFieldModule,
    DrrInputComponent,
    DrrNumericInputComponent,
    DrrDatepickerComponent,
    DrrTextareaComponent,
  ],
  templateUrl: './drif-condition-clear.component.html',
  styleUrl: './drif-condition-clear.component.scss',
  providers: [RxFormBuilder],
})
export class DrifConditionClearComponent {
  route = inject(ActivatedRoute);
  router = inject(Router);
  projectService = inject(ProjectService);
  formBuilder = inject(RxFormBuilder);

  projectId?: string;
  conditionId?: string;

  conditionName?: string;

  stepperOrientation: StepperOrientation = 'horizontal';

  conditionForm?: IFormGroup<ConditionForm>;

  formChanged = false;

  lastSavedAt?: Date;

  autoSaveCountdown = 0;
  autoSaveTimer: any;
  autoSaveInterval = 60;

  @HostListener('window:mousemove')
  @HostListener('window:mousedown')
  @HostListener('window:keypress')
  @HostListener('window:scroll')
  @HostListener('window:touchmove')
  resetAutoSaveTimer() {
    if (!this.formChanged) {
      this.autoSaveCountdown = 0;
      clearInterval(this.autoSaveTimer);
      return;
    }

    this.autoSaveCountdown = this.autoSaveInterval;
    clearInterval(this.autoSaveTimer);
    this.autoSaveTimer = setInterval(() => {
      this.autoSaveCountdown -= 1;
      if (this.autoSaveCountdown === 0) {
        this.save();
        clearInterval(this.autoSaveTimer);
      }
    }, 1000);
  }

  ngOnInit() {
    this.route.params.subscribe((params) => {
      this.projectId = params['projectId'];
      this.conditionId = params['conditionId'];

      // TODO: use contion % or description from API
      this.conditionName = `Request to Clear Condition for ProjectName`;

      this.load().then(() => {
        this.resetAutoSaveTimer();
      });
    });
  }

  load(): Promise<void> {
    return new Promise((resolve) => {
      const conditionFormValue = new ConditionForm({
        name: 'Condition_Name',
        limit: 23,
        attachments: [],
      });
      this.conditionForm = this.formBuilder.formGroup(
        ConditionForm,
        conditionFormValue,
      ) as IFormGroup<ConditionForm>;
      resolve();
    });
  }

  stepperSelectionChange(event: StepperSelectionEvent) {}

  goBack() {
    this.save();
    this.router.navigate(['/drif-projects', this.projectId]);
  }

  save() {
    // TODO: temp
    this.lastSavedAt = new Date();
  }
}
