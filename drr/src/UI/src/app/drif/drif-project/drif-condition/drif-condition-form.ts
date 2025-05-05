import {
  prop,
  propArray,
  propObject,
  required,
  requiredTrue,
} from '@rxweb/reactive-form-validators';
import { DocumentType } from '../../../../model';
import { DeclarationForm } from '../../../shared/drr-declaration/drr-declaration-form';
import { AttachmentForm } from '../../drif-fp/drif-fp-form';

export class CondtionRequestAttachmentForm implements AttachmentForm {
  @prop()
  @required()
  id?: string;

  @prop()
  @required()
  name?: string;

  @prop()
  @required()
  comments?: string;

  @prop()
  documentType?: DocumentType | undefined;

  constructor(values: AttachmentForm) {
    Object.assign(this, values);
  }
}

export class ConditionRequestForm {
  @prop()
  name?: string;

  @prop()
  limit?: number;

  @prop()
  date?: string;

  @prop()
  @required()
  description?: string;

  // TODO: temp fix to fail form validation if attachments are not provided
  @prop()
  @requiredTrue()
  attachmentsAdded?: boolean = false;

  @propArray(CondtionRequestAttachmentForm)
  attachments?: CondtionRequestAttachmentForm[] = [];

  constructor(data?: Partial<ConditionRequestForm>) {
    Object.assign(this, data);
  }
}

export class ConditionForm {
  @propObject(ConditionRequestForm)
  conditionRequest?: ConditionRequestForm = new ConditionRequestForm({});

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
