import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormArray } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import {
  MatStepperModule,
  StepperOrientation,
} from '@angular/material/stepper';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { ProjectService } from '../../../../../api/project/project.service';
import { DeclarationType, FormType } from '../../../../../model';
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

  projectId?: string;
  reportId?: string;
  claimId?: string;

  authorizedRepresentativeText?: string;
  accuracyOfInformationText?: string;

  claimForm = this.formBuilder.formGroup(ClaimForm) as IFormGroup<ClaimForm>;

  claimCategoryOptions: DrrSelectOption[] = [
    { label: 'Option 1', value: 'option1' },
    { label: 'Option 2', value: 'option2' },
  ];

  getInvoiceFormArray() {
    return this.claimForm.get('invoices') as FormArray;
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

      const submitterForm = this.claimForm.get('declaration.submitter');
      if (profileData.firstName?.()) {
        submitterForm
          ?.get('firstName')
          ?.setValue(profileData.firstName(), { emitEvent: false });
        submitterForm?.get('firstName')?.disable();
      }
      if (profileData.lastName?.()) {
        submitterForm
          ?.get('lastName')
          ?.setValue(profileData.lastName(), { emitEvent: false });
        submitterForm?.get('lastName')?.disable();
      }
      if (profileData.title?.()) {
        submitterForm?.get('title')?.setValue(profileData.title(), {
          emitEvent: false,
        });
      }
      if (profileData.department?.()) {
        submitterForm?.get('department')?.setValue(profileData.department(), {
          emitEvent: false,
        });
      }
      if (profileData.phone?.()) {
        submitterForm?.get('phone')?.setValue(profileData.phone(), {
          emitEvent: false,
        });
      }
      if (profileData.email?.()) {
        submitterForm?.get('email')?.setValue(profileData.email(), {
          emitEvent: false,
        });
      }

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
            this.claimForm.patchValue(claim);
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

  addInvoice() {
    this.getInvoiceFormArray().push(this.formBuilder.formGroup(InvoiceForm));
  }

  removeInvoice(index: number) {
    this.getInvoiceFormArray().removeAt(index);
  }
}
