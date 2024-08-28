import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup } from '@rxweb/reactive-form-validators';
import { DrifapplicationService } from '../../../../api/drifapplication/drifapplication.service';
import { ApplicationType, DeclarationType } from '../../../../model';
import { DrrInputComponent } from '../../../shared/controls/drr-input/drr-input.component';
import { DeclarationsForm } from '../drif-fp-form';
import { ProfileStore } from '../../../store/profile.store';

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
    DrrInputComponent,
  ],
  templateUrl: './drif-fp-step-12.component.html',
  styleUrl: './drif-fp-step-12.component.scss',
})
export class DrifFpStep12Component {
  drifAppService = inject(DrifapplicationService);
  profileStore = inject(ProfileStore);

  @Input() declarationsForm!: IFormGroup<DeclarationsForm>;

  authorizedRepresentativeText?: string;
  accuracyOfInformationText?: string;

  ngOnInit() {
    this.drifAppService
      .dRIFApplicationGetDeclarations()
      .subscribe((declarations) => {
        this.authorizedRepresentativeText = declarations.items?.find(
          (d) =>
            d.type === DeclarationType.AuthorizedRepresentative &&
            d.applicationType === ApplicationType.FP
        )?.text;
        this.accuracyOfInformationText = declarations.items?.find(
          (d) =>
            d.type === DeclarationType.AccuracyOfInformation &&
            d.applicationType === ApplicationType.FP
        )?.text;
      });

    const profileData = this.profileStore.getProfile();

    const submitterForm = this.declarationsForm.get('submitter');
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
}
