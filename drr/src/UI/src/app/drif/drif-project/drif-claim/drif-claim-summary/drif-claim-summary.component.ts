import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { FormArray } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { FileService } from '../../../../shared/services/file.service';
import { SummaryItemComponent } from '../../../summary-item/summary-item.component';
import { ClaimForm } from '../drif-claim-form';

@Component({
  selector: 'drif-claim-summary',
  standalone: true,
  imports: [
    CommonModule,
    SummaryItemComponent,
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    TranslocoModule,
  ],
  templateUrl: './drif-claim-summary.component.html',
  styleUrl: './drif-claim-summary.component.scss',
})
export class DrifClaimSummaryComponent {
  translocoService = inject(TranslocoService);
  formBuilder = inject(RxFormBuilder);
  fileService = inject(FileService);

  @Input() claimForm?: IFormGroup<ClaimForm>;
  @Input() isReadOnlyView = true;

  getInvoiceFormArray(): FormArray {
    return this.claimForm?.get('expenditure.invoices') as FormArray;
  }

  onDownloadFile(fileId: string) {
    this.fileService.downloadFile(fileId);
  }
}
