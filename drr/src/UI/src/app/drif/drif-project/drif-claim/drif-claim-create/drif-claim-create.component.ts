import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormArray, FormGroup } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import {
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
import { v4 as uuidv4 } from 'uuid';
import { ProjectService } from '../../../../../api/project/project.service';
import {
  CostCategory,
  DeclarationType,
  DraftProjectClaim,
  FormType,
  InterimProjectType,
} from '../../../../../model';
import { DrrCurrencyInputComponent } from '../../../../shared/controls/drr-currency-input/drr-currency-input.component';
import { DrrDatepickerComponent } from '../../../../shared/controls/drr-datepicker/drr-datepicker.component';
import { DrrInputComponent } from '../../../../shared/controls/drr-input/drr-input.component';
import { DrrRadioButtonComponent } from '../../../../shared/controls/drr-radio-button/drr-radio-button.component';
import {
  DrrSelectComponent,
  DrrSelectOption,
} from '../../../../shared/controls/drr-select/drr-select.component';
import { DrrTextareaComponent } from '../../../../shared/controls/drr-textarea/drr-textarea.component';
import { OptionsStore } from '../../../../store/options.store';
import { ProfileStore } from '../../../../store/profile.store';
import { ClaimForm, InvoiceForm } from '../drif-claim-form';

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
  translocoService = inject(TranslocoService);
  toastService = inject(HotToastService);

  projectId?: string;
  reportId?: string;
  claimId?: string;

  reportName?: string;

  authorizedRepresentativeText?: string;
  accuracyOfInformationText?: string;

  claimForm?: RxFormGroup | FormGroup<any> | AppFormGroup<ClaimForm>;

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
        // TODO: after init logic, auto save, etc
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

            console.log(
              this.claimForm.get('expenditure')?.get('invoices')?.value,
            );

            resolve();
          },
          error: (error) => {
            reject(error);
          },
        });
    });
  }

  stepperOrientation: StepperOrientation = 'horizontal';

  stepperSelectionChange(event: any) {}

  save() {
    const claimForm = this.claimForm?.value as ClaimForm;

    this.projectService
      .projectUpdateClaim(this.projectId!, this.reportId!, this.claimId!, {
        claimComment: claimForm.expenditure.claimComment,
        invoices: claimForm.expenditure.invoices,
      })
      .subscribe({
        next: () => {
          this.toastService.close();
          this.toastService.success('Claim saved successfully');
        },
        error: (error) => {
          this.toastService.close();
          this.toastService.error('Failed to save claim');
          console.error(error);
        },
      });
  }

  goBack() {
    // TODO: save

    this.router.navigate(['drif-projects', this.projectId]);
  }

  submit() {}

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
        const invoiceDate = control.get('invoiceDate')?.value;
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
        const invoiceDate = control.get('invoiceDate')?.value;
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
        const startDate = control.get('startDate')?.value;
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
        const endDate = control.get('endDate')?.value;
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
}
