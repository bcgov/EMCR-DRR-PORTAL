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
import { ApplicationType, DeclarationType, FormType } from '../../../../model';
import { DrrInputComponent } from '../../../shared/controls/drr-input/drr-input.component';
import { AuthorizedRepresentativeForm } from '../../../shared/drr-auth-rep/auth-rep-form';
import { DrrAuthRepComponent } from '../../../shared/drr-auth-rep/drr-auth-rep.component';
import { DeclarationForm } from '../../../shared/drr-declaration/drr-declaration-form';
import { OptionsStore } from '../../../store/options.store';
import { ProfileStore } from '../../../store/profile.store';
import { EOIApplicationForm } from '../drif-eoi-form';
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
    DrrAuthRepComponent,
  ],
  templateUrl: './drif-eoi-step-8.component.html',
  styleUrl: './drif-eoi-step-8.component.scss',
})
export class DrifEoiStep8Component {
  profileStore = inject(ProfileStore);
  optionsStore = inject(OptionsStore);

  isDevMode = isDevMode();
  private _formGroup!: IFormGroup<EOIApplicationForm>;

  @Input()
  set eoiApplicationForm(eoiApplicationForm: IFormGroup<EOIApplicationForm>) {
    this._formGroup = eoiApplicationForm;
  }

  authorizedRepresentativeText?: string;
  accuracyOfInformationText?: string;

  ngOnInit() {
    this.authorizedRepresentativeText = this.optionsStore.getDeclarations?.(
      DeclarationType.AuthorizedRepresentative,
      FormType.Application,
      ApplicationType.EOI,
    );

    this.accuracyOfInformationText = this.optionsStore.getDeclarations?.(
      DeclarationType.AccuracyOfInformation,
      FormType.Application,
      ApplicationType.EOI,
    );

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
    if (profileData.title?.()) {
      authorizedRepresentativeForm
        ?.get('title')
        ?.setValue(profileData.title(), {
          emitEvent: false,
        });
    }
    if (profileData.department?.()) {
      authorizedRepresentativeForm
        ?.get('department')
        ?.setValue(profileData.department(), {
          emitEvent: false,
        });
    }
    if (profileData.phone?.()) {
      authorizedRepresentativeForm
        ?.get('phone')
        ?.setValue(profileData.phone(), {
          emitEvent: false,
        });
    }
    if (profileData.email?.()) {
      authorizedRepresentativeForm
        ?.get('email')
        ?.setValue(profileData.email(), {
          emitEvent: false,
        });
    }
  }

  get eoiApplicationForm(): IFormGroup<EOIApplicationForm> {
    return this._formGroup;
  }

  get declarationForm() {
    return this.eoiApplicationForm.get(
      'declaration',
    ) as IFormGroup<DeclarationForm>;
  }

  get authorizedRepresentativeForm() {
    return this.declarationForm?.get(
      'authorizedRepresentative',
    ) as IFormGroup<AuthorizedRepresentativeForm>;
  }
}
