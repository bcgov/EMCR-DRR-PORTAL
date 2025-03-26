import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import { IFormGroup } from '@rxweb/reactive-form-validators';
import { ApplicationType, DeclarationType, FormType } from '../../../../model';
import { AuthorizedRepresentativeForm } from '../../../shared/drr-auth-rep/auth-rep-form';
import { DrrAuthRepComponent } from '../../../shared/drr-auth-rep/drr-auth-rep.component';
import { OptionsStore } from '../../../store/options.store';
import { ProfileStore } from '../../../store/profile.store';
import { DeclarationForm, DrifFpForm } from '../drif-fp-form';
import { DrifFpSummaryComponent } from '../drif-fp-summary/drif-fp-summary.component';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drif-fp-step-12',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslocoModule,
    MatInputModule,
    MatCheckboxModule,
    DrrAuthRepComponent,
    DrifFpSummaryComponent,
  ],
  templateUrl: './drif-fp-step-12.component.html',
  styleUrl: './drif-fp-step-12.component.scss',
})
export class DrifFpStep12Component {
  profileStore = inject(ProfileStore);
  optionsStore = inject(OptionsStore);

  @Input()
  fullProposalForm!: IFormGroup<DrifFpForm>;

  get declarationForm(): IFormGroup<DeclarationForm> {
    return this.fullProposalForm.get(
      'declaration',
    ) as IFormGroup<DeclarationForm>;
  }

  authorizedRepresentativeText?: string;
  accuracyOfInformationText?: string;

  get authorizedRepresentativeForm() {
    return this.declarationForm?.get(
      'authorizedRepresentative',
    ) as IFormGroup<AuthorizedRepresentativeForm>;
  }

  ngOnInit() {
    this.authorizedRepresentativeText = this.optionsStore.getDeclarations?.(
      DeclarationType.AuthorizedRepresentative,
      FormType.Application,
      ApplicationType.FP,
    );

    this.accuracyOfInformationText = this.optionsStore.getDeclarations?.(
      DeclarationType.AccuracyOfInformation,
      FormType.Application,
      ApplicationType.FP,
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
}
