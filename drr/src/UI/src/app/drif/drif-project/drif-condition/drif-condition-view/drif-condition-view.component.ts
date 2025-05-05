import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { ProjectService } from '../../../../../api/project/project.service';
import { DraftConditionRequest } from '../../../../../model';
import { ConditionForm } from '../drif-condition-form';
import { DrifConditionSummaryComponent } from '../drif-condition-summary/drif-condition-summary.component';

@Component({
  selector: 'drif-condition-view',
  standalone: true,
  imports: [
    CommonModule,
    DrifConditionSummaryComponent,
    TranslocoModule,
    MatButtonModule,    
  ],
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
  requestId?: string;

  conditionForm: IFormGroup<ConditionForm> = this.formBuilder.formGroup(
    ConditionForm,
    {},
  ) as IFormGroup<ConditionForm>;

  conditionName?: string;

  ngOnInit() {
    this.route.params.subscribe((params) => {
      this.projectId = params['projectId'];
      this.requestId = params['requestId'];

      this.load();
    });
  }

  load(): Promise<void> {
    return new Promise((resolve) => {
      this.projectService
        .projectGetConditionRequest(this.projectId!, this.requestId!)
        .subscribe({
          next: (response: DraftConditionRequest) => {
            this.conditionName = `Request to Clear ${response.limit}% Condition`;

            const conditionFormValue = new ConditionForm({
              conditionRequest: {
                name: response.conditionName,
                limit: response.limit,
                date: response.dateMet,
                description: response.explanation,
                attachments: response.attachments,
              },
              declaration: {
                authorizedRepresentative: response.authorizedRepresentative,
              },
            });
            this.conditionForm = this.formBuilder.formGroup(
              ConditionForm,
              conditionFormValue,
            ) as IFormGroup<ConditionForm>;
            this.conditionForm.get('conditionRequest.name')?.disable();
            this.conditionForm.get('conditionRequest.limit')?.disable();

            if (response?.attachments?.length! > 0) {
              this.conditionForm
                .get('conditionRequest.attachmentsAdded')
                ?.setValue(true, { emitEvent: false });
            }

            resolve();
          },
        });
    });
  }

  goBack() {
    this.router.navigate(['drif-projects', this.projectId]);
  }
}
