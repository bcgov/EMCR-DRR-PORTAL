import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import {
  MatStepperModule,
  StepperOrientation,
} from '@angular/material/stepper';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import { ProjectService } from '../../../../../api/project/project.service';

@Component({
  selector: 'drif-condition-clear',
  standalone: true,
  imports: [
    CommonModule,
    MatStepperModule,
    MatButtonModule,
    MatIconModule,
    TranslocoModule,
  ],
  templateUrl: './drif-condition-clear.component.html',
  styleUrl: './drif-condition-clear.component.scss',
})
export class DrifConditionClearComponent {
  route = inject(ActivatedRoute);
  router = inject(Router);
  projectService = inject(ProjectService);

  projectId?: string;
  conditionId?: string;

  conditionName?: string;

  stepperOrientation: StepperOrientation = 'horizontal';

  ngOnInit() {
    this.route.params.subscribe((params) => {
      this.projectId = params['projectId'];
      this.conditionId = params['conditionId'];

      // TODO: use contion % or description from API
      this.conditionName = `Request to Clear Condition for ProjectName`;
    });
  }

  stepperSelectionChange(event: StepperSelectionEvent) {}

  goBack() {
    this.save();
    this.router.navigate(['/drif-projects', this.projectId]);
  }

  save() {}
}
