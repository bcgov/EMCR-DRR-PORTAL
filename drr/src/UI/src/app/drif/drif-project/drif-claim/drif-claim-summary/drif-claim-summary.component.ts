import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { AbstractControl, FormArray } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { IFormGroup, RxFormBuilder } from '@rxweb/reactive-form-validators';
import { DocumentType } from '../../../../../model';
import { FileService } from '../../../../shared/services/file.service';
import { DrrSummaryItemComponent } from '../../../drr-summary-item/drr-summary-item.component';
import { ClaimForm } from '../drif-claim-form';

@Component({
  selector: 'drif-claim-summary',
  standalone: true,
  imports: [
    CommonModule,
    DrrSummaryItemComponent,
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
  @Input() showEarnedInterestControls!: boolean;

  getInvoiceFormArray(): FormArray {
    return this.claimForm?.get('expenditure.invoices') as FormArray;
  }

  private getInvoiceAttachments(invoiceControl: AbstractControl) {
    return invoiceControl.get('attachments') as FormArray;
  }

  // TODO: not ideal to determine if attachment is mandatory
  getProofOfPaymentAttachment(invoiceControl: AbstractControl) {
    return this.getInvoiceAttachments(invoiceControl).controls.find(
      (control) =>
        control.get('documentType')?.value === DocumentType.ProofOfPayment,
    );
  }

  // TODO: not ideal to determine if attachment is mandatory
  getInvoceAttachment(invoiceControl: AbstractControl) {
    return this.getInvoiceAttachments(invoiceControl).controls.find(
      (control) => control.get('documentType')?.value === DocumentType.Invoice,
    );
  }

  onDownloadFile(fileId: string) {
    this.fileService.downloadFile(fileId);
  }
}
