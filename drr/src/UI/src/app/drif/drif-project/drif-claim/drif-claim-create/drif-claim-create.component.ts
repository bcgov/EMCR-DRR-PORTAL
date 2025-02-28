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
import {
  AppFormGroup,
  RxFormBuilder,
  RxFormGroup,
} from '@rxweb/reactive-form-validators';
import { ProjectService } from '../../../../../api/project/project.service';
import {
  CostCategory,
  DeclarationType,
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
          next: (claim) => {
            this.reportName = `${claim.reportPeriod} Claim`;

            // TODO: change when API is ready
            // temp set to month before and month after
            this.plannedStartDate = new Date();
            this.plannedStartDate.setMonth(
              this.plannedStartDate.getMonth() - 1,
            );
            this.plannedEndDate = new Date();
            this.plannedEndDate.setMonth(this.plannedEndDate.getMonth() + 1);
            this.projectType = InterimProjectType.Stream1;

            if (this.projectType === InterimProjectType.Stream1) {
              this.costCategoryOptions = this.costCategoryOptions.filter(
                (option) => option.value !== CostCategory.Contingency,
              );
            }

            const formData = new ClaimForm({
              expenditure: {
                skipClaimReport: false,
                claimComment: 'claim comment text goes here',
                invoices: [
                  {
                    invoiceNumber: 'N123-456',
                    date: '2025-02-24T21:43:47Z',
                    workStartDate: '2025-02-24T21:43:47Z',
                    workEndDate: '2025-02-24T21:43:47Z',
                    paymentDate: '2025-02-24T21:43:47Z',
                    supplierName: 'Supplier of Tools Inc.',
                    costCategory: CostCategory.ConstructionMaterials,
                    description: 'Tools for construction: hammer, nails, etc.',
                    grossAmount: 1000,
                    taxRebate: 50,
                    claimAmount: 950,
                    totalPST: 1,
                    totalGST: 2,
                  },
                  {
                    invoiceNumber: 'N987-457',
                    date: '2025-03-24T21:43:47Z',
                    workStartDate: '2025-03-24T21:43:47Z',
                    workEndDate: '2025-03-24T21:43:47Z',
                    paymentDate: '2025-03-24T21:43:47Z',
                    supplierName: 'Design Inc.',
                    costCategory: CostCategory.Design,
                    description: 'Design of the building.',
                    grossAmount: 2000,
                    taxRebate: 100,
                    claimAmount: 1900,
                    totalPST: 4,
                    totalGST: 5,
                  },
                ],
              },
            } as ClaimForm);

            this.claimForm = this.formBuilder.formGroup(ClaimForm, formData);

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

  save() {}

  goBack() {
    // TODO: save

    this.router.navigate(['drif-projects', this.projectId]);
  }

  submit() {}

  addInvoice() {
    this.getInvoiceFormArray()?.push(this.formBuilder.formGroup(InvoiceForm));
  }

  removeInvoice(index: number) {
    this.getInvoiceFormArray()?.removeAt(index);
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
