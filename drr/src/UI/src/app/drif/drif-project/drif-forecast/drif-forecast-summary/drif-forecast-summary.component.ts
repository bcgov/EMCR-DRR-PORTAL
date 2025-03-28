import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormArray } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { DeclarationForm } from '../../../../shared/drr-declaration/drr-declaration-form';
import { FileService } from '../../../../shared/services/file.service';
import { DrrSummaryItemComponent } from '../../../drr-summary-item/drr-summary-item.component';
import {
  BudgetForecastForm,
  ForecastAttachmentsForm,
  ForecastForm,
} from '../drif-forecast-form';

@Component({
  selector: 'drif-forecast-summary',
  standalone: true,
  imports: [
    CommonModule,
    TranslocoModule,
    MatCardModule,
    DrrSummaryItemComponent,
    MatInputModule,
  ],
  templateUrl: './drif-forecast-summary.component.html',
  styleUrl: './drif-forecast-summary.component.scss',
})
export class DrifForecastSummaryComponent {
  translocoService = inject(TranslocoService);
  formBuilder = inject(RxFormBuilder);
  fileService = inject(FileService);

  @Input() forecastForm?: IFormGroup<ForecastForm>;
  @Input() isReadOnlyView = true;

  get budgetForecastForm() {
    return this.forecastForm?.get(
      'budgetForecast',
    ) as IFormGroup<BudgetForecastForm>;
  }

  get yearForecastFormArray() {
    return this.budgetForecastForm?.get('yearForecasts') as FormArray;
  }

  get attachmentsForm() {
    return this.forecastForm?.get(
      'attachments',
    ) as IFormGroup<ForecastAttachmentsForm>;
  }

  get attachmentsArray() {
    return this.attachmentsForm?.get('attachments') as FormArray;
  }

  hasAttachments(): boolean {
    return this.attachmentsArray.length > 0;
  }

  get declarationForm() {
    return this.forecastForm?.get('declaration') as IFormGroup<DeclarationForm>;
  }

  onDownloadFile(fileId: string) {
    this.fileService.downloadFile(fileId);
  }
}
