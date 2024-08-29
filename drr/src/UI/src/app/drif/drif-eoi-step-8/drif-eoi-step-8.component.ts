import { CommonModule } from '@angular/common';
import { Component, Input, inject, isDevMode } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule } from '@ngneat/transloco';
import { IFormGroup, RxFormControl } from '@rxweb/reactive-form-validators';
import { DrifapplicationService } from '../../../api/drifapplication/drifapplication.service';
import { DeclarationType } from '../../../model';
import { DrrInputComponent } from '../../shared/controls/drr-input/drr-input.component';
import { ProfileStore } from '../../store/profile.store';
import { DeclarationForm, EOIApplicationForm } from '../drif-eoi/drif-eoi-form';
import { SummaryComponent } from '../summary/summary.component';

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
    SummaryComponent,
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
          (d) => d.type === DeclarationType.AuthorizedRepresentative
        )?.text;
        this.accuracyOfInformationText = declarations.items?.find(
          (d) => d.type === DeclarationType.AccuracyOfInformation
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

  getGroupFormControl(controlName: string, groupName: string): RxFormControl {
    return this.declarationForm
      .get(groupName)
      ?.get(controlName) as RxFormControl;
  }

  get eoiApplicationForm(): IFormGroup<EOIApplicationForm> {
    return this._formGroup;
  }

  declarationForm!: IFormGroup<DeclarationForm>;
}
