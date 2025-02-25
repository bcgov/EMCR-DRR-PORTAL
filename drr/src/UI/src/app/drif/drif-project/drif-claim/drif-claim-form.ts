import {
  prop,
  propArray,
  propObject,
  required,
  requiredTrue,
} from '@rxweb/reactive-form-validators';
import { ContactDetailsForm } from '../../drif-eoi/drif-eoi-form';

export class InvoiceForm {
  @prop()
  invoiceNumber?: string;

  @prop()
  invoiceDate?: string;

  @prop()
  startDate?: string;

  @prop()
  endDate?: string;

  @prop()
  paymentDate?: string;

  @prop()
  claimCategory?: string; // TODO: enum

  @prop()
  supplierName?: string;

  @prop()
  description?: string;

  @prop()
  grossAmount?: number;

  @prop()
  taxRebate?: number;

  @prop()
  claimAmount?: number;

  @prop()
  pstPaid?: number;

  @prop()
  gstPaid?: number;

  constructor(value: InvoiceForm) {
    Object.assign(this, value);
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
  @propArray(InvoiceForm)
  invoices: InvoiceForm[] = [];

  @propObject(DeclarationForm)
  declaration: DeclarationForm = new DeclarationForm({});
}
