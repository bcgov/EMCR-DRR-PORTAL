import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormArray, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { HotToastService } from '@ngxpert/hot-toast';
import {
  IFormGroup,
  RxFormBuilder,
  RxFormGroup,
} from '@rxweb/reactive-form-validators';
import { distinctUntilChanged } from 'rxjs';
import { AttachmentService } from '../../../../api/attachment/attachment.service';
import { DocumentType } from '../../../../model';
import { DrrFileUploadComponent } from '../../../shared/controls/drr-file-upload/drr-file-upload.component';
import { DrrRadioButtonComponent } from '../../../shared/controls/drr-radio-button/drr-radio-button.component';
import { FileService, RecordType } from '../../../shared/services/file.service';
import { AttachmentForm, AttachmentsForm } from '../drif-fp-form';
import {
  DrrAttahcmentComponent,
  FileUploadEvent,
} from './drif-fp-attachment.component';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drif-fp-step-11',
  standalone: true,
  imports: [
    CommonModule,
    TranslocoModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatDividerModule,
    DrrFileUploadComponent,
    DrrRadioButtonComponent,
    DrrAttahcmentComponent,
  ],
  templateUrl: './drif-fp-step-11.component.html',
  styleUrl: './drif-fp-step-11.component.scss',
})
export class DrifFpStep11Component {
  formBuilder = inject(RxFormBuilder);
  attachmentsService = inject(AttachmentService);
  fileService = inject(FileService);
  hotToast = inject(HotToastService);

  @Input() attachmentsForm!: IFormGroup<AttachmentsForm>;

  @Input() applicationId!: string;

  ngOnInit() {
    this.attachmentsForm
      .get('haveResolution')
      ?.valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        const attachmentsFormArray = this.attachmentsForm.get(
          'attachments',
        ) as FormArray;
        if (value === true) {
          attachmentsFormArray.push(
            this.formBuilder.formGroup(AttachmentForm, {
              documentType: DocumentType.CouncilBoardResolution,
            }),
          );
        } else {
          const resolutionForm = attachmentsFormArray.controls.find(
            (control) =>
              control.value.documentType ===
              DocumentType.CouncilBoardResolution,
          );

          if (resolutionForm && resolutionForm.value.id) {
            this.removeFile(resolutionForm.value.id);
          } else {
            const resolutionIndex = attachmentsFormArray.controls.findIndex(
              (c) =>
                c.value.documentType === DocumentType.CouncilBoardResolution,
            );
            if (resolutionIndex >= 0) {
              attachmentsFormArray.removeAt(resolutionIndex);
            }
          }
        }
      });
  }

  async uploadFiles(event: FileUploadEvent) {
    event.files.forEach(async (file) => {
      if (file == null) {
        return;
      }

      this.attachmentsService
        .attachmentUploadAttachment({
          RecordId: this.applicationId,
          RecordType: RecordType.FullProposal,
          DocumentType: event.documentType,
          ContentType:
            file.type === ''
              ? this.fileService.getCustomContentType(file)
              : file.type,
          File: file,
        })
        .subscribe({
          next: (attachment) => {
            const attachmentFormData = {
              name: file.name,
              comments: '',
              id: attachment.id,
              documentType: event.documentType,
            } as AttachmentForm;

            const attachmentsArray = this.attachmentsForm.get(
              'attachments',
            ) as FormArray;

            // if other supporting document, add a new form
            if (event.documentType === DocumentType.OtherSupportingDocument) {
              this.addAttachmentForm(attachmentFormData);
              return;
            }

            // if it's a mandatory document, update the existing form if it pre-exists
            const mathcingAttachment = attachmentsArray.controls.find(
              (control) => control.value.documentType === event.documentType,
            );
            if (mathcingAttachment) {
              mathcingAttachment.patchValue(attachmentFormData);
            } else {
              this.addAttachmentForm(attachmentFormData);
            }
          },
          error: () => {
            this.hotToast.close();
            this.hotToast.error('File upload failed');
          },
        });
    });
  }

  private addAttachmentForm(attachmentForm: AttachmentForm) {
    const attachmentsArray = this.attachmentsForm.get(
      'attachments',
    ) as FormArray;

    const fileForm = this.formBuilder.formGroup(
      AttachmentForm,
      attachmentForm,
    ) as RxFormGroup;
    attachmentsArray.push(fileForm);
  }

  uploadOtherFiles(event: File[]) {
    this.uploadFiles({
      files: event,
      documentType: 'OtherSupportingDocument',
    });
  }

  removeFile(fileId: string) {
    this.attachmentsService
      .attachmentDeleteAttachment(fileId, {
        recordId: this.applicationId,
        id: fileId,
      })
      .subscribe({
        next: () => {
          const attachmentsArray = this.attachmentsForm.get(
            'attachments',
          ) as FormArray;
          const fileIndex = attachmentsArray.controls.findIndex(
            (control) => control.value.id === fileId,
          );

          const documentType = attachmentsArray.controls[fileIndex].value
            .documentType as DocumentType;

          attachmentsArray.removeAt(fileIndex);

          // if it's a mandatory document add empty form again to enforce validation
          if (
            documentType === DocumentType.CouncilBoardResolution &&
            this.attachmentsForm.get('haveResolution')?.value === true
          ) {
            const attachmentForm = this.formBuilder.formGroup(AttachmentForm, {
              documentType,
            });
            attachmentForm.markAllAsTouched();
            attachmentsArray.push(attachmentForm);
          }
        },
        error: () => {
          this.hotToast.close();
          this.hotToast.error('File deletion failed');
        },
      });
  }

  downloadFile(fileId: string) {
    this.fileService.downloadFile(fileId);
  }

  getFormByDocumentType(documentType: DocumentType) {
    const attachmentsArray = this.attachmentsForm.get(
      'attachments',
    ) as FormArray;

    return attachmentsArray.controls.find(
      (control) => control.value.documentType === documentType,
    ) as IFormGroup<AttachmentForm>;
  }

  getOtherFormArray() {
    const attachmentsArray = this.attachmentsForm.get(
      'attachments',
    ) as FormArray;

    return attachmentsArray.controls.filter(
      (control) =>
        control.value.documentType === DocumentType.OtherSupportingDocument,
    );
  }

  getOtherFormArrayCount() {
    return this.getOtherFormArray().length;
  }

  getOtherDocumentFormGroup(index: number) {
    const otherForms = this.getOtherFormArray();
    return otherForms[index] as IFormGroup<AttachmentForm>;
  }
}
