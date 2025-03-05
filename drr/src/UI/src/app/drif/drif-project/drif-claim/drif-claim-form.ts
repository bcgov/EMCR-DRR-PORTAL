import {
  prop,
  propArray,
  propObject,
  required,
  requiredTrue,
} from '@rxweb/reactive-form-validators';
import { Attachment, CostCategory, Invoice } from '../../../../model';
import { ContactDetailsForm } from '../../drif-eoi/drif-eoi-form';
import { AttachmentForm } from '../../drif-fp/drif-fp-form';

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

  @propArray(AttachmentForm)
  attachments?: Attachment[] = [];

  constructor(value: InvoiceForm) {
    // TODO: check if I can initiate formArray from within the constructor
    // perhaps I should reinstantiate the whole form instead of patching value after API call
    Object.assign(this, value);
  }
}

export class ExpenditureForm {
  @prop()
  @required()
  skipClaimReport?: boolean;

  @propArray(InvoiceForm)
  invoices?: InvoiceForm[] = [];

  @prop()
  claimComment?: string;

  constructor(values: ExpenditureForm) {
    Object.assign(this, values);
  }
}

export class DeclarationForm {
  @required()
  @propObject(ContactDetailsForm)
  submitter?: ContactDetailsForm = new ContactDetailsForm({});

  @prop()
  @required()
  @requiredTrue()
  authorizedRepresentativeStatement?: boolean;

  @prop()
  @required()
  @requiredTrue()
  informationAccuracyStatement?: boolean;

  constructor(values: DeclarationForm) {
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
function requiredFalse(): (
  target: InvoiceForm,
  propertyKey: 'paymentDate',
) => void {
  throw new Error('Function not implemented.');
}
