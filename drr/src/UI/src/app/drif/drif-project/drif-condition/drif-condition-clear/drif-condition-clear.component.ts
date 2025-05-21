import { BreakpointObserver } from '@angular/cdk/layout';
import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { CommonModule } from '@angular/common';
import { Component, HostListener, inject, viewChild } from '@angular/core';
import { FormArray, FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import {
  MatStepper,
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
import { distinctUntilChanged, pairwise, startWith } from 'rxjs';
import { AttachmentService } from '../../../../../api/attachment/attachment.service';
import { ProjectService } from '../../../../../api/project/project.service';
import {
  ApplicationType,
  ConditionRequest,
  DeclarationType,
  DocumentType,
  DraftConditionRequest,
  FormType,
} from '../../../../../model';
import { DrrDatepickerComponent } from '../../../../shared/controls/drr-datepicker/drr-datepicker.component';
import { DrrFileUploadComponent } from '../../../../shared/controls/drr-file-upload/drr-file-upload.component';
import { DrrInputComponent } from '../../../../shared/controls/drr-input/drr-input.component';
import { DrrNumericInputComponent } from '../../../../shared/controls/drr-number-input/drr-number-input.component';
import { DrrTextareaComponent } from '../../../../shared/controls/drr-textarea/drr-textarea.component';
import { DrrAlertComponent } from '../../../../shared/drr-alert/drr-alert.component';
import { AuthorizedRepresentativeForm } from '../../../../shared/drr-auth-rep/auth-rep-form';
import { DrrAuthRepComponent } from '../../../../shared/drr-auth-rep/drr-auth-rep.component';
import { DeclarationForm } from '../../../../shared/drr-declaration/drr-declaration-form';
import { DrrDeclarationComponent } from '../../../../shared/drr-declaration/drr-declaration.component';
import {
  FileService,
  RecordType,
} from '../../../../shared/services/file.service';
import { OptionsStore } from '../../../../store/options.store';
import { ProfileStore } from '../../../../store/profile.store';
import { DrrAttahcmentComponent } from '../../../drif-fp/drif-fp-step-11/drif-fp-attachment.component';
import {
  ConditionDMAPMessageForm,
  ConditionForm,
  CondtionRequestAttachmentForm,
} from '../drif-condition-form';
import { DrifConditionSummaryComponent } from '../drif-condition-summary/drif-condition-summary.component';

@Component({
  selector: 'drif-condition-clear',
  standalone: true,
  imports: [
    CommonModule,
    MatStepperModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
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
    DrrAlertComponent,
    DrifConditionSummaryComponent,
    DrrAuthRepComponent,
    DrrDeclarationComponent,
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
  requestId?: string;
  conditionId?: string;

  conditionName?: string;

  stepper = viewChild.required(MatStepper);
  breakpointObserver = inject(BreakpointObserver);
  stepperOrientation: StepperOrientation = 'horizontal';

  conditionForm: IFormGroup<ConditionForm> = this.formBuilder.formGroup(
    ConditionForm,
    {},
  ) as IFormGroup<ConditionForm>;
  conditionDMAPMessageForm?: IFormGroup<ConditionDMAPMessageForm>;

  todayDate = new Date();

  authorizedRepresentativeText?: string;
  accuracyOfInformationText?: string;

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
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .subscribe(({ matches }) => {
        this.stepperOrientation = matches ? 'horizontal' : 'vertical';
      });

    this.route.params.subscribe((params) => {
      this.projectId = params['projectId'];
      this.requestId = params['requestId'];

      this.authorizedRepresentativeText = this.optionsStore.getDeclarations?.(
        DeclarationType.AuthorizedRepresentative,
        FormType.Application,
        ApplicationType.ConditionRequest,
      );
      this.accuracyOfInformationText = this.optionsStore.getDeclarations?.(
        DeclarationType.AccuracyOfInformation,
        FormType.Application,
        ApplicationType.ConditionRequest,
      );

      this.load().then(() => {
        this.formChanged = false;
        setTimeout(() => {
          this.conditionForm?.valueChanges
            .pipe(
              startWith(this.conditionForm.value),
              pairwise(),
              distinctUntilChanged((a, b) => {
                // compare objects but ignore declaration changes
                delete a[1].declaration.authorizedRepresentativeStatement;
                delete a[1].declaration.informationAccuracyStatement;
                delete b[1].declaration.authorizedRepresentativeStatement;
                delete b[1].declaration.informationAccuracyStatement;

                return JSON.stringify(a[1]) == JSON.stringify(b[1]);
              }),
            )
            .subscribe(([prev, curr]) => {
              if (
                prev.declaration.authorizedRepresentativeStatement !==
                  curr.declaration.authorizedRepresentativeStatement ||
                prev.declaration.informationAccuracyStatement !==
                  curr.declaration.informationAccuracyStatement
              ) {
                return;
              }

              this.declarationForm
                ?.get('authorizedRepresentativeStatement')
                ?.reset();

              this.declarationForm
                ?.get('informationAccuracyStatement')
                ?.reset();

              this.formChanged = true;
              this.resetAutoSaveTimer();
            });
        }, 1000);
      });
    });
  }

  ngOnDestroy() {
    clearInterval(this.autoSaveTimer);
  }

  load(): Promise<void> {
    return new Promise((resolve) => {
      this.projectService
        .projectGetConditionRequest(this.projectId!, this.requestId!)
        .subscribe({
          next: (response: DraftConditionRequest) => {
            // TODO: if (hasCondidionMessage in response)
            // const conditionDMAPMessageFormValue = new ConditionDMAPMessageForm({
            //   author: 'John Doe',
            //   date: new Date().toISOString(),
            //   message:
            //     'Lorem ipsum dolor sit amet, consectetur adipiscing elit.',
            // });
            // this.conditionDMAPMessageForm = this.formBuilder.formGroup(
            //   ConditionDMAPMessageForm,
            //   conditionDMAPMessageFormValue,
            // ) as IFormGroup<ConditionDMAPMessageForm>;

            this.conditionName = `Request to Clear ${response.limit}% Condition`;
            this.conditionId = response.conditionId;

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

            this.conditionForm
              .get('conditionRequest.attachments')
              ?.valueChanges.subscribe((attachments) => {
                if (attachments.length > 0) {
                  this.conditionForm
                    .get('conditionRequest.attachmentsAdded')
                    ?.setValue(true, { emitEvent: false });
                } else {
                  this.conditionForm
                    .get('conditionRequest.attachmentsAdded')
                    ?.setValue(false, { emitEvent: false });
                }
              });

            this.setAuthorizedRepresentative();

            resolve();
          },
        });
    });
  }

  setAuthorizedRepresentative() {
    const profileData = this.profileStore.getProfile();

    const authorizedRepresentativeForm = this.declarationForm.get(
      'authorizedRepresentative',
    );
    if (profileData.firstName?.()) {
      authorizedRepresentativeForm
        ?.get('firstName')
        ?.setValue(profileData.firstName(), { emitEvent: false });
      authorizedRepresentativeForm?.get('firstName')?.disable();
    }
    if (profileData.lastName?.()) {
      authorizedRepresentativeForm
        ?.get('lastName')
        ?.setValue(profileData.lastName(), { emitEvent: false });
      authorizedRepresentativeForm?.get('lastName')?.disable();
    }
    if (profileData.title?.() && !authorizedRepresentativeForm?.value?.title) {
      authorizedRepresentativeForm
        ?.get('title')
        ?.setValue(profileData.title(), {
          emitEvent: false,
        });
    }
    if (
      profileData.department?.() &&
      !authorizedRepresentativeForm?.value?.department
    ) {
      authorizedRepresentativeForm
        ?.get('department')
        ?.setValue(profileData.department(), {
          emitEvent: false,
        });
    }
    if (profileData.phone?.() && !authorizedRepresentativeForm?.value?.phone) {
      authorizedRepresentativeForm
        ?.get('phone')
        ?.setValue(profileData.phone(), {
          emitEvent: false,
        });
    }
    if (profileData.email?.() && !authorizedRepresentativeForm?.value?.email) {
      authorizedRepresentativeForm
        ?.get('email')
        ?.setValue(profileData.email(), {
          emitEvent: false,
        });
    }
  }

  hasConditionDMAPMessage() {
    return true;
  }

  stepperSelectionChange(event: StepperSelectionEvent) {
    this.save();

    event.previouslySelectedStep.stepControl.markAllAsTouched();

    if (this.stepperOrientation === 'horizontal') {
      return;
    }

    const stepId = this.stepper()._getStepLabelId(event.selectedIndex);
    const stepElement = document.getElementById(stepId);
    if (stepElement) {
      setTimeout(() => {
        stepElement.scrollIntoView({
          block: 'start',
          inline: 'nearest',
          behavior: 'smooth',
        });
      }, 250);
    }
  }

  goBack() {
    this.save();
    this.router.navigate(['/drif-projects', this.projectId]);
  }

  private getFormValue(): ConditionRequest {
    const formValue = this.conditionForm?.getRawValue() as ConditionForm;

    const conditionRequestDraft: ConditionRequest = {
      id: this.requestId,
      conditionId: this.conditionId,
      conditionName: formValue?.conditionRequest?.name,
      limit: formValue?.conditionRequest?.limit,
      explanation: formValue?.conditionRequest?.description,
      attachments: formValue?.conditionRequest?.attachments,
      authorizedRepresentative:
        formValue?.declaration?.authorizedRepresentative,
      authorizedRepresentativeStatement:
        formValue?.declaration?.authorizedRepresentativeStatement,
      informationAccuracyStatement:
        formValue?.declaration?.informationAccuracyStatement,
    };

    return conditionRequestDraft;
  }

  save() {
    if (!this.formChanged) {
      return;
    }

    const conditionRequestDraft = this.getFormValue();

    this.lastSavedAt = undefined;

    this.projectService
      .projectUpdateConditionRequest(
        this.projectId!,
        this.requestId!,
        conditionRequestDraft,
      )
      .subscribe({
        next: () => {
          this.lastSavedAt = new Date();

          this.toastService.close();
          this.toastService.success('Condition request saved successfully');

          this.formChanged = false;
          this.resetAutoSaveTimer();
        },
        error: (error) => {
          this.toastService.close();
          this.toastService.error('Failed to save condition request');
          console.error(error);
        },
      });
  }

  submit() {
    this.conditionForm?.markAllAsTouched();
    this.stepper().steps.forEach((step) => step._markAsInteracted());
    this.stepper()._stateChanged();

    if (this.conditionForm?.invalid) {
      this.toastService.close();
      this.toastService.error('Please fill in all required fields');
      return;
    }

    const conditionFormValue = this.getFormValue();

    this.projectService
      .projectSubmitConditionRequest(this.projectId!, this.requestId!, {
        ...conditionFormValue,
      })
      .subscribe({
        next: () => {
          this.toastService.close();
          this.toastService.success('Request submitted successfully.');

          this.router.navigate(['drif-projects', this.projectId]);
        },
        error: (error) => {
          this.toastService.close();
          this.toastService.error('Condition clear request submission failed');
          console.error(error);
        },
      });
  }

  get declarationForm() {
    return this.conditionForm?.get(
      'declaration',
    ) as IFormGroup<DeclarationForm>;
  }

  get authorizedRepresentativeForm() {
    return this.declarationForm?.get(
      'authorizedRepresentative',
    ) as IFormGroup<AuthorizedRepresentativeForm>;
  }

  get attachmentsArray() {
    return this.conditionForm?.get('conditionRequest.attachments') as FormArray;
  }

  async uploadFiles(files: File[]) {
    files.forEach(async (file) => {
      if (file == null) {
        return;
      }

      this.attachmentsService
        .attachmentUploadAttachment({
          RecordId: this.requestId,
          RecordType: RecordType.ConditionRequest,
          DocumentType: DocumentType.ConditionApproval,
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
              documentType: DocumentType.ConditionApproval,
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
        recordId: this.requestId,
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
