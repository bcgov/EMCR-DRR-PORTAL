import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormArray } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { DeclarationForm } from '../../../../shared/drr-declaration/drr-declaration-form';
import { FileService } from '../../../../shared/services/file.service';
import { SummaryItemComponent } from '../../../summary-item/summary-item.component';
import { ConditionForm } from '../drif-condition-form';

@Component({
  selector: 'drif-condition-summary',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    SummaryItemComponent,
    TranslocoModule,
    MatInputModule,
  ],
  templateUrl: './drif-condition-summary.component.html',
  styleUrl: './drif-condition-summary.component.scss',
})
export class DrifConditionSummaryComponent {
  translocoService = inject(TranslocoService);
  formBuilder = inject(RxFormBuilder);
  fileService = inject(FileService);

  @Input() conditionForm?: IFormGroup<ConditionForm>;
  @Input() isReadOnlyView = true;

  get attachmentsArray() {
    return this.conditionForm?.get('attachments') as FormArray;
  }

  hasAttachments(): boolean {
    return this.attachmentsArray.length > 0;
  }

  get declarationForm() {
    return this.conditionForm?.get(
      'declaration',
    ) as IFormGroup<DeclarationForm>;
  }

  onDownloadFile(fileId: string) {
    this.fileService.downloadFile(fileId);
  }
}
