import { CommonModule } from '@angular/common';
import { Component, Input, inject, isDevMode } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { IFormGroup } from '@rxweb/reactive-form-validators';
import { DrifapplicationService } from '../../../../api/drifapplication/drifapplication.service';
import { ApplicationType, DeclarationType } from '../../../../model';
import { DrrInputComponent } from '../../../shared/controls/drr-input/drr-input.component';
import { ProfileStore } from '../../../store/profile.store';
import { DeclarationForm, EOIApplicationForm } from '../drif-eoi-form';
import { DrifEoiSummaryComponent } from '../drif-eoi-summary/drif-eoi-summary.component';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drif-eoi-step-8',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    DrifEoiSummaryComponent,
    TranslocoModule,
    DrrInputComponent,
  ],
  templateUrl: './drif-eoi-step-8.component.html',
  styleUrl: './drif-eoi-step-8.component.scss',
})
export class DrifEoiStep8Component {
  drifAppService = inject(DrifapplicationService);
  profileStore = inject(ProfileStore);

  isDevMode = isDevMode();
  private _formGroup!: IFormGroup<EOIApplicationForm>;

  @Input()
  set eoiApplicationForm(eoiApplicationForm: IFormGroup<EOIApplicationForm>) {
    this._formGroup = eoiApplicationForm;
    this.declarationForm = eoiApplicationForm.get(
      'declaration'
    ) as IFormGroup<DeclarationForm>;
  }

  authorizedRepresentativeText?: string;
  accuracyOfInformationText?: string;

  ngOnInit() {
    this.drifAppService
      .dRIFApplicationGetDeclarations()
      .subscribe((declarations) => {
        this.authorizedRepresentativeText = declarations.items?.find(
          (d) =>
            d.type === DeclarationType.AuthorizedRepresentative &&
            d.applicationType === ApplicationType.EOI
        )?.text;
        this.accuracyOfInformationText = declarations.items?.find(
          (d) =>
            d.type === DeclarationType.AccuracyOfInformation &&
            d.applicationType === ApplicationType.EOI
        )?.text;
      });

    const profileData = this.profileStore.getProfile();

    const submitterForm = this.declarationForm.get('submitter');
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
  }

  get eoiApplicationForm(): IFormGroup<EOIApplicationForm> {
    return this._formGroup;
  }

  declarationForm!: IFormGroup<DeclarationForm>;
}
