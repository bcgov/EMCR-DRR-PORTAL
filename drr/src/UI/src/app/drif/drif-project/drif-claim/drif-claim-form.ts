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
  date?: string;

  @prop()
  workStartDate?: string;

  @prop()
  workEndDate?: string;

  @prop()
  paymentDate?: string;

  @prop()
  supplierName?: string;

  @prop()
  costCategory?: CostCategory;

  @prop()
  description?: string;

  @prop()
  grossAmount?: number;

  @prop()
  taxRebate?: number;

  @prop()
  claimAmount?: number;

  @prop()
  totalPST?: number;

  @prop()
  totalGST?: number;

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

  constructor(values: ClaimForm) {
    Object.assign(this, values);
  }
}
