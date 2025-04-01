import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { Component, HostListener, inject, ViewChild } from '@angular/core';
import {
  AbstractControl,
  FormArray,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import {
  MatStepper,
  MatStepperModule,
  StepperOrientation,
} from '@angular/material/stepper';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { HotToastService } from '@ngxpert/hot-toast';
import {
  IFormGroup,
  RxFormBuilder,
  RxReactiveFormsModule,
} from '@rxweb/reactive-form-validators';
import { distinctUntilChanged, pairwise, startWith } from 'rxjs';
import { v4 as uuidv4 } from 'uuid';
import { AttachmentService } from '../../../../../api/attachment/attachment.service';
import { ProjectService } from '../../../../../api/project/project.service';
import {
  ActiveCondition,
  CostCategory,
  DeclarationType,
  DocumentType,
  DraftProjectClaim,
  FormType,
  InterimProjectType,
  PreviousClaim,
  ProjectClaim,
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
import { DrrAlertComponent } from '../../../../shared/drr-alert/drr-alert.component';
import { AuthorizedRepresentativeForm } from '../../../../shared/drr-auth-rep/auth-rep-form';
import { DrrAuthRepComponent } from '../../../../shared/drr-auth-rep/drr-auth-rep.component';
import { DeclarationForm } from '../../../../shared/drr-declaration/drr-declaration-form';
import { DrrDeclarationComponent } from '../../../../shared/drr-declaration/drr-declaration.component';
import { FileService } from '../../../../shared/services/file.service';
import { OptionsStore } from '../../../../store/options.store';
import { ProfileStore } from '../../../../store/profile.store';
import {
  ClaimForm,
  InvoiceAttachmentForm,
  InvoiceForm,
} from '../drif-claim-form';
import { DrifClaimSummaryComponent } from '../drif-claim-summary/drif-claim-summary.component';

export class ClaimSummaryItem implements PreviousClaim {
  costCategory?: CostCategory;
  currentClaim?: number;
  totalForProject?: number;
  originalEstimate?: number;
}

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-drif-claim-create',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RxReactiveFormsModule,
    MatFormFieldModule,
    MatStepperModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatCardModule,
    MatTableModule,
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
    DrifClaimSummaryComponent,
    DrrAlertComponent,
    DrrAuthRepComponent,
    DrrDeclarationComponent,
  ],
  templateUrl: './drif-claim-create.component.html',
  styleUrl: './drif-claim-create.component.scss',
  providers: [RxFormBuilder, CurrencyPipe],
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
  currencyPipe = inject(CurrencyPipe);

  invoiceDocumentType = DocumentType.Invoice;
  proofOfPaymentDocumentType = DocumentType.ProofOfPayment;

  projectId?: string;
  reportId?: string;
  claimId?: string;

  reportName?: string;

  authorizedRepresentativeText?: string;
  accuracyOfInformationText?: string;

  @ViewChild(MatStepper) stepper!: MatStepper;
  stepperOrientation: StepperOrientation = 'horizontal';

  claimForm?: IFormGroup<ClaimForm> = this.formBuilder.formGroup(
    ClaimForm,
  ) as IFormGroup<ClaimForm>;
  formChanged = false;

  previousClaimSummaryItems: ClaimSummaryItem[] = [];
  claimSummaryItemsDataSource = new MatTableDataSource<ClaimSummaryItem>();
  displayedColumns: string[] = [
    'costCategory',
    'currentClaim',
    'totalForProject',
    'originalEstimate',
  ];

  previousClaimTotal = 0;
  activeConditionLimit?: ActiveCondition;

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
    .filter((value) => value !== CostCategory.Contingency)
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

              this.calculateClaimSummary();

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

  // TODO: this could be moved to the store, except for the form part
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

            this.previousClaimTotal = claim.previousClaimTotal || 0;
            this.activeConditionLimit = claim.activeCondition;

            const formData = new ClaimForm({
              expenditure: {
                haveClaimExpenses: claim.haveClaimExpenses,
                claimComment: claim.claimComment,
                invoices: claim.invoices,
                totalClaimed: claim.totalClaimed,
                totalProjectAmount: claim.totalProjectAmount,
              },
              declaration: {
                authorizedRepresentativeStatement: false,
                informationAccuracyStatement: false,
                authorizedRepresentative: claim.authorizedRepresentative,
              },
            } as ClaimForm);

            // iterate over invoices and add attachments if they are missing
            formData.expenditure.invoices?.forEach((invoice) => {
              if (!invoice.attachments || invoice.attachments.length === 0) {
                invoice.attachments = [];
              }

              if (
                !invoice.attachments.some(
                  (attachment) =>
                    attachment.documentType === this.invoiceDocumentType,
                )
              ) {
                invoice.attachments.push({
                  documentType: this.invoiceDocumentType,
                } as InvoiceAttachmentForm);
              }

              if (
                !invoice.attachments.some(
                  (attachment) =>
                    attachment.documentType === this.proofOfPaymentDocumentType,
                )
              ) {
                invoice.attachments.push({
                  documentType: this.proofOfPaymentDocumentType,
                } as InvoiceAttachmentForm);
              }
            });

            this.claimForm = this.formBuilder.formGroup(
              ClaimForm,
              formData,
            ) as IFormGroup<ClaimForm>;

            this.setupClaimQuestionnaire(claim.haveClaimExpenses);

            this.claimForm
              ?.get('expenditure.totalClaimed')
              ?.disable({ emitEvent: false });

            this.claimForm
              ?.get('expenditure.totalProjectAmount')
              ?.disable({ emitEvent: false });

            this.setAuthorizedRepresentative();

            this.formChanged = false;

            this.previousClaimSummaryItems =
              claim.previousClaims?.map((claim) => {
                return {
                  costCategory: claim.costCategory,
                  currentClaim: 0,
                  totalForProject: claim.totalForProject,
                  originalEstimate: claim.originalEstimate,
                } as ClaimSummaryItem;
              }) || [];
            this.calculateClaimSummary();

            resolve();
          },
          error: (error) => {
            reject(error);
          },
        });
    });
  }

  setupClaimQuestionnaire(haveClaimExpenses?: boolean) {
    const claimCommentControl = this.claimForm?.get('expenditure.claimComment');
    if (haveClaimExpenses === false) {
      claimCommentControl?.setValidators([Validators.required]);
    }

    this.claimForm
      ?.get('expenditure.haveClaimExpenses')
      ?.valueChanges.subscribe((value) => {
        value
          ? [
              claimCommentControl?.clearValidators(),
              claimCommentControl?.reset(),
            ]
          : claimCommentControl?.setValidators([Validators.required]);

        claimCommentControl?.updateValueAndValidity();
      });
  }

  setAuthorizedRepresentative() {
    const profileData = this.profileStore.getProfile();

    const authorizedRepresentativeForm = this.claimForm?.get(
      'declaration.authorizedRepresentative',
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

  stepperSelectionChange(event: StepperSelectionEvent) {
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

  getFormValue(): ProjectClaim {
    const claimForm = this.claimForm?.getRawValue() as ClaimForm;

    // iterate over invoices and remove attachments if they are empty
    claimForm.expenditure.invoices?.forEach((invoice) => {
      invoice.attachments = [];
    });

    return {
      haveClaimExpenses: claimForm.expenditure.haveClaimExpenses,
      invoices: claimForm.expenditure.invoices,
      claimComment: claimForm.expenditure.claimComment,
      authorizedRepresentative: claimForm.declaration.authorizedRepresentative,
      totalClaimed: claimForm.expenditure.totalClaimed,
      authorizedRepresentativeStatement:
        claimForm.declaration.authorizedRepresentativeStatement,
      informationAccuracyStatement:
        claimForm.declaration.informationAccuracyStatement,
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

    if (this.hasClaimIntentIssue()) {
      this.toastService.error(
        this.translocoService.translate('claim.claimIntentIssueErrorMessage'),
      );
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

  getTotalClaimAmount(): number {
    return (
      this.getInvoiceFormArray()?.controls.reduce((total: number, control) => {
        const claimAmount = control.get('claimAmount')?.value;
        if (!claimAmount) {
          return total;
        }

        return total + claimAmount;
      }, 0) || 0
    );
  }

  uploadFiles(
    files: any,
    invoiceControl: AbstractControl,
    docType: DocumentType,
  ) {
    files.forEach(async (file: any) => {
      if (file == null) {
        return;
      }

      const base64Content = await this.fileService.fileToBase64(file);

      this.attachmentsService
        .attachmentUploadAttachment({
          recordId: invoiceControl.get('id')?.value,
          recordType: RecordType.Invoice,
          documentType: docType,
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
              id: attachment.id,
              documentType: docType,
            } as InvoiceAttachmentForm;

            const attachments = invoiceControl.get('attachments') as FormArray;
            const attachmentControl = attachments.controls.find(
              (control) => control.get('documentType')?.value === docType,
            );

            if (attachmentControl) {
              attachmentControl.patchValue(attachmentFormData);
            } else {
              attachments.push(
                this.formBuilder.formGroup(
                  InvoiceAttachmentForm,
                  attachmentFormData,
                ),
              );
            }
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

  removeFile(fileId: string, invoiceControl: AbstractControl) {
    const invoiceId = invoiceControl.get('id')?.value;

    this.attachmentsService
      .attachmentDeleteAttachment(fileId, {
        id: fileId,
        recordId: invoiceId,
      })
      .subscribe({
        next: () => {
          const attachments = invoiceControl.get('attachments') as FormArray;
          const index = attachments.controls.findIndex(
            (control) => control.get('id')?.value === fileId,
          );
          attachments.removeAt(index);

          this.toastService.close();
          this.toastService.success('File removed successfully');
        },
        error: () => {
          this.toastService.close();
          this.toastService.error('Failed to remove file');
        },
      });
  }

  calculateClaimSummary() {
    const currentClaimSummary: { [key: string]: number } = {};

    this.getInvoiceFormArray()?.controls.forEach((control) => {
      const costCategory = control.get('costCategory')?.value;
      const claimAmount = control.get('claimAmount')?.value;

      if (costCategory && claimAmount) {
        if (!currentClaimSummary[costCategory]) {
          currentClaimSummary[costCategory] = 0;
        }
        currentClaimSummary[costCategory] += claimAmount;
      }
    });

    // combite current claim cost category with previous claims cost categories
    // check if current claim has got the same cost category and add the current claim amount on top of the previous claim amount
    // otherwise insert current claim amount as new cost category
    const previousClaimSummary = this.previousClaimSummaryItems.map(
      (claimSummary) => {
        const currentClaimAmount =
          currentClaimSummary[claimSummary.costCategory!];
        if (currentClaimAmount) {
          claimSummary.currentClaim = currentClaimAmount;
        }
        return claimSummary;
      },
    );

    // add new cost categories from current claim
    Object.keys(currentClaimSummary).forEach((currentCostCategory) => {
      if (
        !previousClaimSummary.some(
          (claimSummary) => claimSummary.costCategory === currentCostCategory,
        )
      ) {
        previousClaimSummary.push({
          costCategory: currentCostCategory as CostCategory,
          currentClaim: currentClaimSummary[currentCostCategory],
          totalForProject: 0,
          originalEstimate: 0,
        });
      }
    });

    this.claimSummaryItemsDataSource.data = previousClaimSummary;

    const currentClaimTotal = this.getTotalClaimAmount();

    this.claimForm
      ?.get('expenditure.totalClaimed')
      ?.setValue(currentClaimTotal + this.previousClaimTotal);
  }

  get declarationForm() {
    return this.claimForm?.get('declaration') as IFormGroup<DeclarationForm>;
  }

  get authorizedRepresentativeForm() {
    return this.declarationForm?.get(
      'authorizedRepresentative',
    ) as IFormGroup<AuthorizedRepresentativeForm>;
  }

  hasClaimIntentIssue() {
    return (
      this.claimForm?.get('expenditure.haveClaimExpenses')?.value === false &&
      this.getInvoiceFormArray()?.length! > 0
    );
  }
}
