import { prop } from '@rxweb/reactive-form-validators';
import { ContactDetails } from '../../../model';

// TODO: replace with API enum
export enum ProjectRoleType {
  Head = 'Head',
  ProgressReporting = 'ProgressReporting',
  ForecastReporting = 'ForecastReporting',
  ClaimsReporting = 'ClaimsReporting',
}

// TODO: potentially change parent class
export class ProjectContactForm implements ContactDetails {
  @prop()
  id?: string | undefined;

  @prop()
  firstName?: string | undefined;

  @prop()
  lastName?: string | undefined;

  @prop()
  title?: string | undefined;

  @prop()
  department?: string | undefined;

  @prop()
  phone?: string | undefined;

  @prop()
  email?: string | undefined;

  @prop()
  projectRole?: ProjectRoleType | undefined; // TODO: change to enum
}
