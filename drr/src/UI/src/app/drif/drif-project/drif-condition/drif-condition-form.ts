import {
  prop,
  propArray,
  propObject,
  required,
} from '@rxweb/reactive-form-validators';
import { DocumentType } from '../../../../model';
import { DeclarationForm } from '../../../shared/drr-declaration/drr-declaration-form';
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

  @propObject(DeclarationForm)
  declaration?: DeclarationForm = new DeclarationForm({});

  constructor(data?: Partial<ConditionForm>) {
    Object.assign(this, data);
  }
}

export class ConditionDMAPMessageForm {
  @prop()
  author?: string;

  @prop()
  date?: string;

  @prop()
  message?: string;

  constructor(data?: Partial<ConditionDMAPMessageForm>) {
    Object.assign(this, data);
  }
}
