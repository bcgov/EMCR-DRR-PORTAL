import { prop, propObject, required } from '@rxweb/reactive-form-validators';
import { PeriodType } from '../../../../model';

export class InterimReportConfigurationForm {
  @prop()
  @required()
  periodType?: PeriodType;

  constructor(values: InterimReportConfigurationForm) {
    Object.assign(this, values);
  }
}

export class InterimReportForm {
  @propObject(InterimReportConfigurationForm)
  configuration?: InterimReportConfigurationForm =
    new InterimReportConfigurationForm({});

  @prop()
  createDate?: Date;

  @prop()
  dueDate?: Date;

  @prop()
  year?: number;

  @prop()
  quarter?: string;

  @prop()
  description?: string;

  constructor(values: InterimReportForm) {
    Object.assign(this, values);
  }
}
