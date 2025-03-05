import { CommonModule } from '@angular/common';
import { Component, HostListener, inject, ViewChild } from '@angular/core';
import { AbstractControl, FormArray, FormGroup } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import {
  MatStepper,
  MatStepperModule,
  StepperOrientation,
} from '@angular/material/stepper';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { HotToastService } from '@ngxpert/hot-toast';
import {
  AppFormGroup,
  RxFormBuilder,
  RxFormGroup,
} from '@rxweb/reactive-form-validators';
import { distinctUntilChanged, pairwise, startWith } from 'rxjs';
import { v4 as uuidv4 } from 'uuid';
import { AttachmentService } from '../../../../../api/attachment/attachment.service';
import { ProjectService } from '../../../../../api/project/project.service';
import {
  CostCategory,
  DeclarationType,
  DraftProjectClaim,
  FormType,
  InterimProjectType,
  RecordType,
} from '../../../../../model';
import { DrrCurrencyInputComponent } from '../../../../shared/controls/drr-currency-input/drr-currency-input.component';
import { DrrDatepickerComponent } from '../../../../shared/controls/drr-datepicker/drr-datepicker.component';
import { DrrFileUploadComponent } from '../../../../shared/controls/drr-file-upload/drr-file-upload.component';
import { DrrInputComponent } from '../../../../shared/controls/drr-input/drr-input.component';
import { DrrRadioButtonComponent } from '../../../../shared/controls/drr-radio-button/drr-radio-button.component';
import {
  DrrSelectComponent,
  DrrSelectOption,
} from '../../../../shared/controls/drr-select/drr-select.component';
import { DrrTextareaComponent } from '../../../../shared/controls/drr-textarea/drr-textarea.component';
import { FileService } from '../../../../shared/services/file.service';
import { OptionsStore } from '../../../../store/options.store';
import { ProfileStore } from '../../../../store/profile.store';
import { AttachmentForm } from '../../../drif-fp/drif-fp-form';
import { DrrAttahcmentComponent } from '../../../drif-fp/drif-fp-step-11/drif-fp-attachment.component';
import { ClaimForm, InvoiceForm } from '../drif-claim-form';

export enum InvoiceDocumentType {
  Invoice = 'Invoice',
  ProofOfPayment = 'ProofOfPayment',
}

@Component({
  selector: 'drr-drif-claim-create',
  standalone: true,
  imports: [
    CommonModule,
    MatStepperModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatCardModule,
    MatDividerModule,
    TranslocoModule,
    RouterModule,
    DrrDatepickerComponent,
    DrrInputComponent,
    DrrSelectComponent,
    DrrRadioButtonComponent,
    DrrTextareaComponent,
    DrrCurrencyInputComponent,
    DrrFileUploadComponent,
    DrrAttahcmentComponent,
  ],
  templateUrl: './drif-claim-create.component.html',
  styleUrl: './drif-claim-create.component.scss',
  providers: [RxFormBuilder],
})
export class DrifClaimCreateComponent {
  formBuilder = inject(RxFormBuilder);
  route = inject(ActivatedRoute);
  router = inject(Router);
  optionsStore = inject(OptionsStore);
  profileStore = inject(ProfileStore);
  projectService = inject(ProjectService);
  attachmentsService = inject(AttachmentService);
  translocoService = inject(TranslocoService);
  toastService = inject(HotToastService);
  fileService = inject(FileService);

  invoiceDocumentType = InvoiceDocumentType.Invoice;
  proofOfPaymentDocumentType = InvoiceDocumentType.ProofOfPayment;

  projectId?: string;
  reportId?: string;
  claimId?: string;

  reportName?: string;

  authorizedRepresentativeText?: string;
  accuracyOfInformationText?: string;

  @ViewChild(MatStepper) stepper!: MatStepper;
  stepperOrientation: StepperOrientation = 'horizontal';

  claimForm?: RxFormGroup | FormGroup<any> | AppFormGroup<ClaimForm>;
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

  today = new Date();
  plannedStartDate!: Date;
  plannedEndDate!: Date;
  projectType!: InterimProjectType;

  costCategoryOptions: DrrSelectOption[] = Object.values(CostCategory)
    .map((value) => ({
      value,
      label: this.translocoService.translate(value),
    }))
    .sort((a, b) => a.label.localeCompare(b.label));

  getInvoiceFormArray(): FormArray | undefined {
    return this.claimForm?.get('expenditure')?.get('invoices') as FormArray;
  }

  ngOnInit() {
    this.route.params.subscribe((params) => {
      this.projectId = params['projectId'];
      this.reportId = params['reportId'];
      this.claimId = params['claimId'];

      this.authorizedRepresentativeText = this.optionsStore.getDeclarations?.(
        DeclarationType.AuthorizedRepresentative,
        FormType.Report,
      );

      this.accuracyOfInformationText = this.optionsStore.getDeclarations?.(
        DeclarationType.AccuracyOfInformation,
        FormType.Report,
      );

      const profileData = this.profileStore.getProfile();

      // const submitterForm = this.claimForm?.get('declaration.submitter');
      // if (profileData.firstName?.()) {
      //   submitterForm
      //     ?.get('firstName')
      //     ?.setValue(profileData.firstName(), { emitEvent: false });
      //   submitterForm?.get('firstName')?.disable();
      // }
      // if (profileData.lastName?.()) {
      //   submitterForm
      //     ?.get('lastName')
      //     ?.setValue(profileData.lastName(), { emitEvent: false });
      //   submitterForm?.get('lastName')?.disable();
      // }
      // if (profileData.title?.()) {
      //   submitterForm?.get('title')?.setValue(profileData.title(), {
      //     emitEvent: false,
      //   });
      // }
      // if (profileData.department?.()) {
      //   submitterForm?.get('department')?.setValue(profileData.department(), {
      //     emitEvent: false,
      //   });
      // }
      // if (profileData.phone?.()) {
      //   submitterForm?.get('phone')?.setValue(profileData.phone(), {
      //     emitEvent: false,
      //   });
      // }
      // if (profileData.email?.()) {
      //   submitterForm?.get('email')?.setValue(profileData.email(), {
      //     emitEvent: false,
      //   });
      // }

      this.load().then(() => {
        this.formChanged = false;
        setTimeout(() => {
          this.claimForm?.valueChanges
            .pipe(
              startWith(this.claimForm.value),
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

              this.claimForm
                ?.get('declaration.authorizedRepresentativeStatement')
                ?.reset();

              this.claimForm
                ?.get('declaration.informationAccuracyStatement')
                ?.reset();

              this.formChanged = true;
              this.resetAutoSaveTimer();
            });
        }, 1000);
      });
    });
  }

  load(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.projectService
        .projectGetClaim(this.projectId!, this.reportId!, this.claimId!)
        .subscribe({
          next: (claim: DraftProjectClaim) => {
            this.reportName = `${claim.reportPeriod} Claim`;

            this.plannedStartDate = new Date(claim.plannedStartDate!);
            this.plannedEndDate = new Date(claim.plannedEndDate!);
            this.projectType = claim.projectType!;

            if (this.projectType === InterimProjectType.Stream1) {
              this.costCategoryOptions = this.costCategoryOptions.filter(
                (option) => option.value !== CostCategory.Contingency,
              );
            }

            const formData = new ClaimForm({
              expenditure: {
                skipClaimReport: false, // claim.skipClaimReport,
                claimComment: claim.claimComment,
                invoices: claim.invoices,
              },
            } as ClaimForm);

            this.claimForm = this.formBuilder.formGroup(ClaimForm, formData);

            this.formChanged = false;

            resolve();
          },
          error: (error) => {
            reject(error);
          },
        });
    });
  }

  stepperSelectionChange(event: any) {
    if (event.previouslySelectedIndex === 0) {
      return;
    }

    this.save();

    event.previouslySelectedStep.stepControl.markAllAsTouched();

    if (this.stepperOrientation === 'horizontal') {
      return;
    }

    const stepId = this.stepper._getStepLabelId(event.selectedIndex);
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

  getFormValue(): DraftProjectClaim {
    const claimForm = this.claimForm?.value as ClaimForm;

    return {
      claimComment: claimForm.expenditure.claimComment,
      invoices: claimForm.expenditure.invoices,
    };
  }

  save() {
    if (!this.formChanged) {
      return;
    }

    const claimValue = this.getFormValue();

    this.lastSavedAt = undefined;

    this.projectService
      .projectUpdateClaim(this.projectId!, this.reportId!, this.claimId!, {
        ...claimValue,
      })
      .subscribe({
        next: () => {
          this.lastSavedAt = new Date();

          this.toastService.close();
          this.toastService.success('Claim saved successfully');

          this.formChanged = false;
          this.resetAutoSaveTimer();
        },
        error: (error) => {
          this.toastService.close();
          this.toastService.error('Failed to save claim');
          console.error(error);
        },
      });
  }

  goBack() {
    this.save();
    this.router.navigate(['drif-projects', this.projectId]);
  }

  submit() {
    this.claimForm?.markAllAsTouched();
    this.stepper.steps.forEach((step) => step._markAsInteracted());
    this.stepper._stateChanged();

    if (this.claimForm?.invalid) {
      this.toastService.error('Please fill in all required fields');
      return;
    }

    const claimValue = this.getFormValue();

    this.projectService
      .projectSubmitClaim(this.projectId!, this.reportId!, this.claimId!, {
        ...claimValue,
      })
      .subscribe({
        next: () => {
          this.toastService.close();
          this.toastService.success('Claim submitted successfully');

          this.router.navigate(['drif-projects', this.projectId]);
        },
        error: (error) => {
          this.toastService.error('Failed to submit claim');
          console.error(error);
        },
      });
  }

  addInvoice() {
    this.projectService
      .projectCreateInvoice(this.projectId!, this.reportId!, this.claimId!, {
        id: uuidv4(),
      })
      .subscribe({
        next: (invoice) => {
          this.getInvoiceFormArray()?.push(
            this.formBuilder.formGroup(InvoiceForm, invoice),
          );
        },
        error: (error) => {
          console.error(error);
        },
      });
  }

  removeInvoice(id: string) {
    this.projectService
      .projectDeleteInvoice(this.projectId!, this.reportId!, this.claimId!, {
        id,
      })
      .subscribe({
        next: () => {
          const index = this.getInvoiceFormArray()?.controls.findIndex(
            (control) => control.get('id')?.value === id,
          );
          this.getInvoiceFormArray()?.removeAt(index!);
        },
        error: (error) => {
          console.error(error);
        },
      });
  }

  getEarliestInvoiceDate() {
    return this.getInvoiceFormArray()?.controls.reduce(
      (earliestDate: Date | null, control) => {
        const invoiceDate = control.get('date')?.value;
        if (!invoiceDate) {
          return earliestDate;
        }

        const date = new Date(invoiceDate);
        if (!earliestDate || date < earliestDate) {
          return date;
        }

        return earliestDate;
      },
      null,
    );
  }

  getLatestInvoiceDate() {
    return this.getInvoiceFormArray()?.controls.reduce(
      (latestDate: Date | null, control) => {
        const invoiceDate = control.get('date')?.value;
        if (!invoiceDate) {
          return latestDate;
        }

        const date = new Date(invoiceDate);
        if (!latestDate || date > latestDate) {
          return date;
        }

        return latestDate;
      },
      null,
    );
  }

  getEarliestGoodsAndServicesWorkStartDate() {
    return this.getInvoiceFormArray()?.controls.reduce(
      (earliestDate: Date | null, control) => {
        const startDate = control.get('workStartDate')?.value;
        if (!startDate) {
          return earliestDate;
        }

        const date = new Date(startDate);
        if (!earliestDate || date < earliestDate) {
          return date;
        }

        return earliestDate;
      },
      null,
    );
  }

  getLatestGoodsAndServicesWorkEndDate() {
    return this.getInvoiceFormArray()?.controls.reduce(
      (latestDate: Date | null, control) => {
        const endDate = control.get('workEndDate')?.value;
        if (!endDate) {
          return latestDate;
        }

        const date = new Date(endDate);
        if (!latestDate || date > latestDate) {
          return date;
        }

        return latestDate;
      },
      null,
    );
  }

  getNumberOfInvoices() {
    return this.getInvoiceFormArray()?.length;
  }

  getTotalClaimAmount() {
    return this.getInvoiceFormArray()?.controls.reduce(
      (total: number, control) => {
        const claimAmount = control.get('claimAmount')?.value;
        if (!claimAmount) {
          return total;
        }

        return total + claimAmount;
      },
      0,
    );
  }

  uploadFiles(
    event: any,
    invoiceControl: AbstractControl,
    docType: InvoiceDocumentType,
  ) {
    event.files.forEach(async (file: any) => {
      if (file == null) {
        return;
      }

      const base64Content = await this.fileService.fileToBase64(file);

      this.attachmentsService
        .attachmentUploadAttachment({
          recordId: invoiceControl.get('id')?.value,
          recordType: RecordType.Invoice,
          documentType: event.documentType,
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
              documentType: event.documentType,
            } as AttachmentForm;

            const attachmentsArray = invoiceControl.get(
              'attachments',
            ) as FormArray;

            attachmentsArray.push(
              this.formBuilder.formGroup(AttachmentForm, attachmentFormData),
            );
          },
          error: () => {
            this.toastService.close();
            this.toastService.error('File upload failed');
          },
        });
    });
  }

  getInvoiceDocument(invoiceControl: AbstractControl) {
    const attachments = invoiceControl.get('attachments') as FormArray;
    return attachments.controls.find(
      (control) =>
        control.get('documentType')?.value === this.invoiceDocumentType,
    );
  }

  getInvoiceProofOfPayment(invoiceControl: AbstractControl) {
    const attachments = invoiceControl.get('attachments') as FormArray;
    return attachments.controls.find(
      (control) =>
        control.get('documentType')?.value === this.proofOfPaymentDocumentType,
    );
  }

  downloadFile(fileId: string) {
    this.fileService.downloadFile(fileId);
  }

  showInvoiceDocumentRequiredError(invoiceControl: AbstractControl) {
    const invoiceDocumentControl = this.getInvoiceDocument(invoiceControl);
    return (
      invoiceDocumentControl?.get('id')?.invalid &&
      invoiceDocumentControl?.touched
    );
  }

  showProofOfPaymentRequiredError(invoiceControl: AbstractControl) {
    const proofOfPaymentControl = this.getInvoiceProofOfPayment(invoiceControl);
    return (
      proofOfPaymentControl?.get('id')?.invalid &&
      proofOfPaymentControl?.touched
    );
  }

  removeFile(fileId: string, invoiceId: string) {
    this.attachmentsService
      .attachmentDeleteAttachment(fileId, {
        id: fileId,
        recordId: invoiceId,
      })
      .subscribe({
        next: () => {
          this.toastService.close();
          this.toastService.success('File removed successfully');
        },
        error: () => {
          this.toastService.close();
          this.toastService.error('Failed to remove file');
        },
      });
  }
}
