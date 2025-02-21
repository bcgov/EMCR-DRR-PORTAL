import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoModule } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import {
  RxFormBuilder,
  RxReactiveFormsModule,
} from '@rxweb/reactive-form-validators';
import { YesNoOption } from '../../../../model';
import {
  DrrRadioButtonComponent,
  DrrRadioOption,
} from '../../../shared/controls/drr-radio-button/drr-radio-button.component';
import { DrrAlertComponent } from '../../../shared/drr-alert/drr-alert.component';
import { ScreenerQuestionsForm } from './drif-fp-screener-form';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drr-drif-fp-screener',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RxReactiveFormsModule,
    TranslocoModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatCardModule,
    MatDividerModule,
    DrrRadioButtonComponent,
    DrrAlertComponent,
  ],
  templateUrl: './drif-fp-screener.component.html',
  styleUrl: './drif-fp-screener.component.scss',
})
export class DrifFpScreenerComponent {
  formBuilder = inject(RxFormBuilder);
  router = inject(Router);
  route = inject(ActivatedRoute);

  eoiId?: string;
  projectTitle?: string;

  screenerForm = this.formBuilder.formGroup(ScreenerQuestionsForm);

  foundationWorkCompletedOptions: DrrRadioOption[] = [
    { value: YesNoOption.Yes, label: 'Yes' },
    { value: YesNoOption.No, label: 'No' },
    {
      value: YesNoOption.NotApplicable,
      label: 'Not applicable (if not applying for foundational project)',
    },
  ];

  yesNoNotRequiredOptions: DrrRadioOption[] = [
    { value: YesNoOption.Yes, label: 'Yes' },
    { value: YesNoOption.No, label: 'No' },
    { value: YesNoOption.NotApplicable, label: 'Not Required' },
  ];

  yesNoNotApplicableOptions: DrrRadioOption[] = [
    { value: YesNoOption.Yes, label: 'Yes' },
    { value: YesNoOption.No, label: 'No' },
    { value: YesNoOption.NotApplicable, label: 'Not Applicable' },
  ];

  ngOnInit() {
    this.eoiId = this.route.snapshot.params['eoiId'];
    this.projectTitle = this.route.snapshot.params['projectTitle'];
  }

  hasNegativeAnswers() {
    return Object.values(this.screenerForm.value).some(
      (value) => value === false || value === YesNoOption.No,
    );
  }

  allQuestionsAnswered() {
    return Object.values(this.screenerForm.value).every(
      (value) => value !== null,
    );
  }

  skip() {
    this.router.navigate([
      '/drif-fp-instructions',
      this.eoiId,
      this.projectTitle,
    ]);
  }

  cancel() {
    this.router.navigate(['/dashboard']);
  }

  continue() {
    const screenerAnswers = this.screenerForm.value;
    this.router.navigate(
      ['/drif-fp-instructions', this.eoiId, this.projectTitle],
      {
        queryParams: {
          ...screenerAnswers,
        },
      },
    );
  }

  getRelatedEOILink() {
    return `/eoi-submission-details/${this.eoiId}`;
  }
}
