import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { ProjectService } from '../../../../../api/project/project.service';
import { ConditionForm } from '../drif-condition-form';
import { DrifConditionSummaryComponent } from '../drif-condition-summary/drif-condition-summary.component';

@Component({
  selector: 'drif-condition-view',
  standalone: true,
  imports: [CommonModule, DrifConditionSummaryComponent],
  templateUrl: './drif-condition-view.component.html',
  styleUrl: './drif-condition-view.component.scss',
  providers: [RxFormBuilder],
})
export class DrifConditionViewComponent {
  route = inject(ActivatedRoute);
  router = inject(Router);
  projectService = inject(ProjectService);
  formBuilder = inject(RxFormBuilder);

  projectId?: string;
  conditionId?: string;

  conditionForm?: IFormGroup<ConditionForm>;

  conditionName?: string;

  ngOnInit() {
    this.route.params.subscribe((params) => {
      this.projectId = params['projectId'];
      this.conditionId = params['conditionId'];
    });
  }

  load(): Promise<void> {
    return new Promise((resolve, reject) => {
      // this.projectService
      //   .projectGetCondition(this.projectId!, this.conditionId!)
      //   .subscribe({
      //     next: (condition) => {
      //       const formValue = new ConditionForm(condition);
      //       this.conditionForm = this.formBuilder.group(formValue);
      //       resolve();
      //     },
      //     error: (error) => {
      //       reject(error);
      //     },
      //   });
    });
  }

  goBack() {
    this.router.navigate(['drif-projects', this.projectId]);
  }
}
