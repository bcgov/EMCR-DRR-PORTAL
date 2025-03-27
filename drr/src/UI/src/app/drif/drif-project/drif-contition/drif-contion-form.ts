import { prop, propArray } from '@rxweb/reactive-form-validators';
import { AttachmentForm } from '../../drif-fp/drif-fp-form';

export class ConditionForm {
  @prop()
  name?: string;

  @prop()
  limit?: number;

  @prop()
  date?: string;

  @prop()
  description?: string;

  @propArray(AttachmentForm)
  attachments?: AttachmentForm[] = [];

  constructor(data?: Partial<ConditionForm>) {
    Object.assign(this, data);
  }
}
