import {
  prop,
  propArray,
  propObject,
  required,
  requiredTrue,
} from '@rxweb/reactive-form-validators';
import { CostCategory } from '../../../../model';
import { ContactDetailsForm } from '../../drif-eoi/drif-eoi-form';

export class InvoiceForm {
  @prop()
  id?: string;

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
  supplierName?: string;

  @prop()
  claimCategory?: CostCategory;

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

export class ExpenditureForm {
  @prop()
  @required()
  skipClaimReport?: boolean;

  @propArray(InvoiceForm)
  invoices?: InvoiceForm[] = [];

  @prop()
  @required()
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
}
