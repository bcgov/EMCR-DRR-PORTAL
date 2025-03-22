import {
  prop,
  propArray,
  propObject,
  required,
  requiredTrue,
} from '@rxweb/reactive-form-validators';
import { ContactDetailsForm } from '../../drif-eoi/drif-eoi-form';
import { AttachmentForm } from '../../drif-fp/drif-fp-form';

export class YearForecastForm {
  @prop()
  id?: string;

  @prop()
  fiscalYear?: string;

  @prop()
  originalForecast?: number;

  @prop()
  @required()
  projectedExpenditure?: number;

  @prop()
  paidClaimsAmount?: number;

  @prop()
  notPaidClaimsAmount?: number;

  @prop()
  @required()
  outstandingClaimsAmount?: number;

  @prop()
  remainingClaimsAmount?: number;

  constructor(data?: Partial<YearForecastForm>) {
    Object.assign(this, data);
  }
}

export class BudgetForecastForm {
  @propArray(YearForecastForm)
  yearForecasts?: YearForecastForm[] = [];

  @prop()
  @required()
  totalProjectedExpenditure?: number;

  @prop()
  @required()
  originalTotalForecast?: number;

  @prop()
  @required()
  variance?: number;

  @prop()
  varianceComment?: string;

  constructor(data?: Partial<BudgetForecastForm>) {
    Object.assign(this, data);
  }
}

export class ForecastAttachmentsForm {
  @propArray(AttachmentForm)
  attachments?: AttachmentForm[] = [];

  constructor(data?: Partial<ForecastAttachmentsForm>) {
    Object.assign(this, data);
  }
}

export class ForecastDeclarationForm {
  @required()
  @propObject(ContactDetailsForm)
  authorizedRepresentative?: ContactDetailsForm = new ContactDetailsForm({});

  @prop()
  @required()
  @requiredTrue()
  authorizedRepresentativeStatement?: boolean;

  @prop()
  @required()
  @requiredTrue()
  informationAccuracyStatement?: boolean;

  constructor(values: ForecastDeclarationForm) {
    Object.assign(this, values);
  }
}

export class ForecastForm {
  @propObject(BudgetForecastForm)
  budgetForecast = new BudgetForecastForm({});

  @propObject(ForecastAttachmentsForm)
  attachments = new ForecastAttachmentsForm({});

  @propObject(ForecastDeclarationForm)
  declaration = new ForecastDeclarationForm({});

  constructor(data: ForecastForm) {
    Object.assign(this, data);
  }
}
