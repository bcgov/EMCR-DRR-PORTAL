import { email, prop, required } from '@rxweb/reactive-form-validators';
import { ContactDetails } from '../../../model';

export class AuthorizedRepresentativeForm implements ContactDetails {
  @prop()
  @required()
  firstName?: string;

  @prop()
  @required()
  lastName?: string;

  @prop()
  @required()
  title?: string;

  @prop()
  @required()
  department?: string;

  @prop()
  @required()
  phone?: string;

  @prop()
  @required()
  @email()
  email?: string;

  constructor(values: AuthorizedRepresentativeForm) {
    Object.assign(this, values);
  }
}
