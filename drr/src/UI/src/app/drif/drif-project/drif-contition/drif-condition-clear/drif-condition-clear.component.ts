import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { CommonModule } from '@angular/common';
import { Component, HostListener, inject } from '@angular/core';
import { FormArray, FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import {
  MatStepperModule,
  StepperOrientation,
} from '@angular/material/stepper';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import { HotToastService } from '@ngxpert/hot-toast';
import {
  IFormGroup,
  RxFormBuilder,
  RxReactiveFormsModule,
} from '@rxweb/reactive-form-validators';
import { AttachmentService } from '../../../../../api/attachment/attachment.service';
import { ProjectService } from '../../../../../api/project/project.service';
import { DocumentType, RecordType } from '../../../../../model';
import { DrrDatepickerComponent } from '../../../../shared/controls/drr-datepicker/drr-datepicker.component';
import { DrrFileUploadComponent } from '../../../../shared/controls/drr-file-upload/drr-file-upload.component';
import { DrrInputComponent } from '../../../../shared/controls/drr-input/drr-input.component';
import { DrrNumericInputComponent } from '../../../../shared/controls/drr-number-input/drr-number-input.component';
import { DrrTextareaComponent } from '../../../../shared/controls/drr-textarea/drr-textarea.component';
import { FileService } from '../../../../shared/services/file.service';
import { OptionsStore } from '../../../../store/options.store';
import { ProfileStore } from '../../../../store/profile.store';
import { DrrAttahcmentComponent } from '../../../drif-fp/drif-fp-step-11/drif-fp-attachment.component';
import {
  ConditionForm,
  CondtionRequestAttachmentForm,
} from '../drif-condition-form';

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
    DrrAttahcmentComponent,
    DrrFileUploadComponent,
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
  optionsStore = inject(OptionsStore);
  profileStore = inject(ProfileStore);
  fileService = inject(FileService);
  attachmentsService = inject(AttachmentService);
  toastService = inject(HotToastService);

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

  get attachmentsArray() {
    return this.conditionForm?.get('attachments') as FormArray;
  }

  async uploadFiles(files: File[]) {
    files.forEach(async (file) => {
      if (file == null) {
        return;
      }

      const base64Content = await this.fileService.fileToBase64(file);

      this.attachmentsService
        .attachmentUploadAttachment({
          recordId: this.conditionId,
          recordType: RecordType.ConditionRequest,
          documentType: DocumentType.ConditionRequest,
          name: file.name,
          contentType:
            file.type === ''
              ? this.fileService.getCustomContentType(file)
              : file.type,
          content: base64Content.split(',')[1],
        })
        .subscribe({
          next: (attachment) => {
            const attachmentFormData = {
              name: file.name,
              comments: '',
              id: attachment.id,
              documentType: DocumentType.ConditionRequest,
            } as CondtionRequestAttachmentForm;

            this.attachmentsArray.push(
              this.formBuilder.formGroup(
                CondtionRequestAttachmentForm,
                attachmentFormData,
              ),
            );
          },
          error: (error) => {
            this.toastService.close();
            this.toastService.error('File upload failed');
            console.error(error);
          },
        });
    });
  }

  downloadFile(fileId: string) {
    this.fileService.downloadFile(fileId);
  }

  removeFile(fileId: string) {
    this.attachmentsService
      .attachmentDeleteAttachment(fileId, {
        recordId: this.conditionId,
        id: fileId,
      })
      .subscribe({
        next: () => {
          const fileIndex = this.attachmentsArray.controls.findIndex(
            (control) => control.value.id === fileId,
          );

          const documentType = this.attachmentsArray.controls[fileIndex].value
            .documentType as DocumentType;

          this.attachmentsArray.removeAt(fileIndex);
        },
        error: (error) => {
          this.toastService.close();
          this.toastService.error('File deletion failed');
          console.error(error);
        },
      });
  }
}
