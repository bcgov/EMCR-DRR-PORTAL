import {
  prop,
  propObject,
  required,
  requiredTrue,
} from '@rxweb/reactive-form-validators';
import { AuthorizedRepresentativeForm } from '../drr-auth-rep/auth-rep-form';

export class DeclarationForm {
  @required()
  @propObject(AuthorizedRepresentativeForm)
  authorizedRepresentative?: AuthorizedRepresentativeForm =
    new AuthorizedRepresentativeForm({});

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
