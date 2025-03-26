import {
  prop,
  propArray,
  propObject,
  required,
} from '@rxweb/reactive-form-validators';
import {
  Attachment,
  CostCategory,
  DocumentType,
  Invoice,
} from '../../../../model';
import { DeclarationForm } from '../../../shared/drr-declaration/drr-declaration-form';
import { AttachmentForm } from '../../drif-fp/drif-fp-form';

export class InvoiceAttachmentForm implements Attachment {
  @prop()
  @required()
  id?: string;

  @prop()
  @required()
  name?: string;

  @prop()
  documentType?: DocumentType | undefined;

  constructor(values: AttachmentForm) {
    Object.assign(this, values);
  }
}

export class InvoiceForm implements Invoice {
  @prop()
  id?: string;

  @prop()
  @required()
  invoiceNumber?: string;

  /** invoice date */
  @prop()
  @required()
  date?: string;

  @prop()
  @required()
  workStartDate?: string;

  @prop()
  @required()
  workEndDate?: string;

  @prop()
  @required()
  paymentDate?: string;

  @prop()
  @required()
  costCategory?: CostCategory;

  @prop()
  @required()
  supplierName?: string;

  @prop()
  @required()
  description?: string;

  @prop()
  @required()
  grossAmount?: number;

  @prop()
  taxRebate?: number;

  @prop()
  @required()
  claimAmount?: number;

  @prop()
  totalPST?: number;

  @prop()
  totalGST?: number;

  @propArray(InvoiceAttachmentForm)
  attachments?: InvoiceAttachmentForm[] = [];

  constructor(value: InvoiceForm) {
    Object.assign(this, value);
  }
}

export class ExpenditureForm {
  @prop()
  @required()
  haveClaimExpenses?: boolean;

  @propArray(InvoiceForm)
  invoices?: InvoiceForm[] = [];

  @prop()
  claimComment?: string;

  @prop()
  totalClaimed?: number;

  @prop()
  totalProjectAmount?: number;

  constructor(values: ExpenditureForm) {
    Object.assign(this, values);
  }
}

export class ClaimForm {
  @propObject(ExpenditureForm)
  expenditure: ExpenditureForm = new ExpenditureForm({});

  @propObject(DeclarationForm)
  declaration: DeclarationForm = new DeclarationForm({});

  constructor(values: ClaimForm) {
    Object.assign(this, values);
  }
}
