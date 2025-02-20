import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { AbstractControl, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { TranslocoModule, TranslocoService } from '@ngneat/transloco';
import { UntilDestroy } from '@ngneat/until-destroy';
import {
  IFormGroup,
  RxFormArray,
  RxFormBuilder,
  RxFormControl,
  RxFormGroup,
} from '@rxweb/reactive-form-validators';
import { NgxMaskPipe } from 'ngx-mask';
import { DocumentType, YesNoOption } from '../../../../model';
import { FileService } from '../../../shared/services/file.service';
import { DrifEoiSummaryComponent } from '../../drif-eoi/drif-eoi-summary/drif-eoi-summary.component';
import { SummaryItemComponent } from '../../summary-item/summary-item.component';
import { DrifFpForm } from '../drif-fp-form';

@UntilDestroy({ checkProperties: true })
@Component({
  selector: 'drif-fp-summary',
  standalone: true,
  imports: [
    CommonModule,
    SummaryItemComponent,
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    TranslocoModule,
    NgxMaskPipe,
    DrifEoiSummaryComponent,
  ],
  templateUrl: './drif-fp-summary.component.html',
  styleUrl: './drif-fp-summary.component.scss',
})
export class DrifFpSummaryComponent {
  translocoService = inject(TranslocoService);
  formBuilder = inject(RxFormBuilder);
  fileService = inject(FileService);
  private _fullProposalForm?: IFormGroup<DrifFpForm>;

  @Input()
  isReadOnlyView = true;

  @Input()
  set fullProposalForm(DrifFpForm: IFormGroup<DrifFpForm>) {
    this._fullProposalForm = DrifFpForm;
  }
  get fullProposalForm(): IFormGroup<DrifFpForm> {
    return this._fullProposalForm!;
  }

  getGroup(groupName: string): RxFormGroup {
    return this.fullProposalForm?.get(groupName) as RxFormGroup;
  }

  getFormArray(groupName: string, controlName: string): any[] {
    return this.getGroup(groupName)?.get(controlName)?.value ?? [];
  }

  getRxFormControl(controlFullName: string) {
    return this.fullProposalForm.get(controlFullName) as RxFormControl;
  }

  getRxGroupFormControl(
    groupName: string,
    nestedGroup: string,
    controlName: string,
  ) {
    return this.getGroup(groupName)
      ?.get(nestedGroup)
      ?.get(controlName) as RxFormControl;
  }

  getRxFormArrayControls(groupName: string, controlName: string) {
    return (this.getGroup(groupName)?.get(controlName) as RxFormArray).controls;
  }

  getAttachmentByDocumentType(documentType: DocumentType) {
    const attachmentsArray = this.fullProposalForm.get(
      'attachments.attachments',
    ) as RxFormArray;
    const attahcment = attachmentsArray.controls.find(
      (control) => control.get('documentType')?.value === documentType,
    ) as RxFormGroup;

    return attahcment;
  }

  getOtherAttachmentsArrayControls() {
    const attachmentsArray = this.fullProposalForm.get(
      'attachments.attachments',
    ) as RxFormArray;
    return attachmentsArray.controls.filter(
      (control) =>
        control.get('documentType')?.value ===
        DocumentType.OtherSupportingDocument,
    );
  }

  gethaveResolutionDocument() {
    return (
      this.fullProposalForm.get('attachments.haveResolution')?.value === true
    );
  }

  isAttachmentRequired(documentType: DocumentType) {
    return documentType === DocumentType.DetailedCostEstimate;
  }

  convertRxFormControl(formControl: AbstractControl<any, any> | null) {
    return formControl as RxFormControl;
  }

  objectHasValues(obj: any): boolean {
    // if array - check length, if value - check if truthy
    return (
      obj &&
      Object.values(obj).some((value) => {
        if (Array.isArray(value)) {
          return this.arrayHasValues(value);
        } else {
          return !!value;
        }
      })
    );
  }

  arrayHasValues(array: any[]): boolean {
    return (
      array &&
      array.length > 0 &&
      array.some((value) => this.objectHasValues(value))
    );
  }

  getRemainingAmountAbs() {
    return Math.abs(
      this.fullProposalForm?.get('budget.remainingAmount')?.value ?? 0,
    );
  }

  getPreviousResponseValue() {
    return this.fullProposalForm.get('budget.previousResponse')?.value ===
      YesNoOption.NotApplicable
      ? this.translocoService.translate('costUnknown')
      : this.translocoService.translate(
          this.fullProposalForm.get('budget.previousResponse')?.value,
        );
  }

  onDownloadFile(fileId: string) {
    this.fileService.downloadFile(fileId);
  }

  getProfessionalGuidanceAnswer() {
    const professionalGuidance = this.fullProposalForm.get(
      'permitsRegulationsAndStandards.professionalGuidance',
    )?.value;

    switch (professionalGuidance) {
      case true:
        return this.translocoService.translate(YesNoOption.Yes);

      case false:
        return this.translocoService.translate(YesNoOption.NotApplicable);

      default:
        return professionalGuidance;
    }
  }

  isProjectTotalCostChangedCommentRequired() {
    return this.fullProposalForm
      .get('budget.totalProjectCostChangeComments')
      ?.hasValidator(Validators.required);
  }
}
