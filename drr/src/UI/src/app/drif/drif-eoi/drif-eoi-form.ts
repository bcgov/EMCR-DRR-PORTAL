import {
  email,
  maxNumber,
  minLength,
  minNumber,
  prop,
  propArray,
  propObject,
  required,
  requiredTrue,
} from '@rxweb/reactive-form-validators';
import {
  ContactDetails,
  EstimatedNumberOfPeople,
  FundingInformation,
  FundingStream,
  FundingType,
  Hazards,
  ProjectType,
  ProponentType,
} from '../../../model';

export class FundingInformationItemForm implements FundingInformation {
  @prop()
  @required()
  @minNumber({ value: 0 })
  @maxNumber({ value: 999999999.99 })
  amount?: number;

  @prop()
  @required()
  name?: string;

  @prop()
  @required()
  @minLength({ value: 1 })
  type?: FundingType;

  @prop()
  otherDescription?: string;

  constructor(values: FundingInformationItemForm) {
    Object.assign(this, values);
  }
}

export class StringItem {
  @prop()
  value: string = '';

  constructor(values: StringItem) {
    Object.assign(this, values);
  }
}

export class StringItemRequired {
  @prop()
  @required()
  value: string = '';

  constructor(values: StringItemRequired) {
    Object.assign(this, values);
  }
}

export class ContactDetailsForm implements ContactDetails {
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

  constructor(values: ContactDetailsForm) {
    Object.assign(this, values);
  }
}

export class ProponentInformationForm {
  @prop()
  @required()
  proponentType?: ProponentType;

  @prop()
  @required()
  proponentName?: string;

  @required()
  @propObject(ContactDetailsForm)
  projectContact?: ContactDetailsForm = new ContactDetailsForm({});

  @required()
  @propArray(ContactDetailsForm)
  additionalContacts?: ContactDetailsForm[] = [{}];

  @prop()
  partneringProponents?: string[] = [];

  @propArray(StringItem)
  partneringProponentsArray?: StringItem[] = [{ value: '' }];

  constructor(values: ProponentInformationForm) {
    Object.assign(this, values);
  }
}

export class ProjectInformationForm {
  @prop()
  @required()
  fundingStream?: FundingStream;

  @prop()
  @required()
  projectTitle?: string;

  @prop()
  @required()
  scopeStatement?: string;

  @prop()
  @required()
  projectType?: ProjectType;

  @prop()
  @required()
  @minLength({ value: 1 })
  relatedHazards?: Hazards[];

  @prop()
  otherHazardsDescription?: string;

  @prop()
  @required()
  startDate?: string;

  @prop()
  @required()
  endDate?: string;

  constructor(values: ProjectInformationForm) {
    Object.assign(this, values);
  }
}

export class FundingInformationForm {
  @prop()
  @required()
  @minNumber({ value: 0 })
  @maxNumber({ value: 999999999.99 })
  estimatedTotal?: number;

  @prop()
  @required()
  @minNumber({ value: 0 })
  @maxNumber({ value: 999999999.99 })
  fundingRequest?: number;

  @prop()
  @required()
  haveOtherFunding?: boolean;

  @propArray(FundingInformationItemForm)
  otherFunding?: FundingInformationItemForm[] = [];

  @prop()
  @minNumber({ value: 0 })
  remainingAmount?: number;

  @prop()
  intendToSecureFunding?: string;

  constructor(values: FundingInformationForm) {
    Object.assign(this, values);
  }
}

export class LocationInformationForm {
  @prop()
  @required()
  ownershipDeclaration?: boolean;

  @prop()
  ownershipDescription?: string;

  @prop()
  @required()
  locationDescription?: string;

  constructor(values: LocationInformationForm) {
    Object.assign(this, values);
  }
}

export class ProjectDetailsForm {
  @prop()
  @required()
  rationaleForFunding?: string;

  @prop()
  @required()
  estimatedPeopleImpacted?: EstimatedNumberOfPeople;

  @prop()
  @required()
  communityImpact?: string;

  @prop()
  @required()
  @minLength({ value: 1 })
  infrastructureImpacted?: string[] = [];

  @propArray(StringItemRequired)
  infrastructureImpactedArray?: StringItemRequired[] = [{ value: '' }];

  @prop()
  @required()
  disasterRiskUnderstanding?: string;

  @prop()
  additionalBackgroundInformation?: string;

  @prop()
  @required()
  addressRisksAndHazards?: string;

  @prop()
  @required()
  drifProgramGoalAlignment?: string;

  @prop()
  additionalSolutionInformation?: string;

  @prop()
  @required()
  rationaleForSolution?: string;

  constructor(values: ProjectDetailsForm) {
    Object.assign(this, values);
  }
}

export class EngagementPlanForm {
  @prop()
  @required()
  firstNationsEngagement?: string;

  @prop()
  @required()
  neighbourEngagement?: string;

  @prop()
  additionalEngagementInformation?: string;

  constructor(values: EngagementPlanForm) {
    Object.assign(this, values);
  }
}

export class OtherSupportingInformationForm {
  @prop()
  @required()
  climateAdaptation?: string;

  @prop()
  otherInformation?: string;

  constructor(values: OtherSupportingInformationForm) {
    Object.assign(this, values);
  }
}

export class DeclarationForm {
  @required()
  @propObject(ContactDetailsForm)
  submitter?: ContactDetailsForm = new ContactDetailsForm({});

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

export class EOIApplicationForm {
  @propObject(ProponentInformationForm)
  proponentInformation?: ProponentInformationForm =
    new ProponentInformationForm({});

  @propObject(ProjectInformationForm)
  projectInformation?: ProjectInformationForm = new ProjectInformationForm({});

  @propObject(FundingInformationForm)
  fundingInformation?: FundingInformationForm = new FundingInformationForm({});

  @propObject(LocationInformationForm)
  locationInformation?: LocationInformationForm = new LocationInformationForm(
    {}
  );

  @propObject(ProjectDetailsForm)
  projectDetails?: ProjectDetailsForm = new ProjectDetailsForm({});

  @propObject(EngagementPlanForm)
  engagementPlan?: EngagementPlanForm = new EngagementPlanForm({});

  @propObject(OtherSupportingInformationForm)
  otherSupportingInformation?: OtherSupportingInformationForm =
    new OtherSupportingInformationForm({});

  @propObject(DeclarationForm)
  declaration?: DeclarationForm = new DeclarationForm({});

  // TODO: have a factory method to create a new instance of this form
}
