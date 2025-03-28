import { prop, propArray, required } from '@rxweb/reactive-form-validators';
import { AttachmentForm } from '../../drif-fp/drif-fp-form';

export class ConditionForm {
  @prop()
  name?: string;

  @prop()
  limit?: number;

  @prop()
  @required()
  date?: string;

  @prop()
  @required()
  description?: string;

  @propArray(AttachmentForm)
  attachments?: AttachmentForm[] = [];

  constructor(data?: Partial<ConditionForm>) {
    Object.assign(this, data);
  }
}
