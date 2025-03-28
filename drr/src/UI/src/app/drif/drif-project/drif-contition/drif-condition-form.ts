import { prop, propArray, required } from '@rxweb/reactive-form-validators';
import { DocumentType } from '../../../../model';
import { AttachmentForm } from '../../drif-fp/drif-fp-form';

export class CondtionRequestAttachmentForm {
  @prop()
  @required()
  id?: string;

  @prop()
  @required()
  name?: string;

  @prop()
  comments?: string;

  @prop()
  documentType?: DocumentType | undefined;

  constructor(values: AttachmentForm) {
    Object.assign(this, values);
  }
}

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

  @propArray(CondtionRequestAttachmentForm)
  attachments?: CondtionRequestAttachmentForm[] = [];

  constructor(data?: Partial<ConditionForm>) {
    Object.assign(this, data);
  }
}
