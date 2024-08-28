import {
  minLength,
  prop,
  propArray,
  propObject,
  required,
} from '@rxweb/reactive-form-validators';
import {
  EstimatedNumberOfPeople,
  FundingStream,
  Hazards,
  ProjectType,
  YesNoOption,
} from '../../../model';
import {
  ContactDetailsForm,
  FundingInformationItemForm,
  ProponentInformationForm,
  StringItem,
} from '../drif-eoi/drif-eoi-form';

// TODO: use enum from model
export enum Standards {
  ProvincialStandard134b = 'Provincial Standard 134/b',
  ProvincialStandard144c = 'Provincial Standard 14.4/c',
  BuildingCodex099GNAP = 'Building Codex 0.99 GNAP',
}

export enum ComplexityRisks {
  RemoteGeographicLocation = 'Remote Geographic Location',
  UnpredictableWeather = 'Unpredictable Weather',
  UntestedOrUnprovenTechnologies = 'Untested or Unproven Technologies',
  HighlyTechnicalOrComplexProject = 'Highly Technical or Complex Project',
  InterdependenciesBetweenPhases = 'Interdependencies Between Phases',
}

export enum ReadinessRisks {
  Projectsitehasntbeenfinalized = 'Project site hasn’t been finalized',
  Landhasntbeenacquired = 'Land hasn’t been acquired',
  Potentialissueswithpermitsorauthorizations = 'Potential issues with permits or authorizations',
  Industrysupplymaynotbeabletomeetdemand = 'Industry supply may not be able to meet demand',
  NonDRIFfundingsourcesarenotsecuredfortheentireprojectcost = 'Non-DRIF funding sources are not secured for the entire project cost',
}

export enum SensitivityRisks {
  Projecthasreceivednegativemediaattention = 'Project has received negative media attention',
  Certainstakeholdershavebeenvocalabouttheproject = 'Certain stakeholders have been vocal about the project',
}

export enum CapacityRisks {
  Limitedhumanresourcestocompletetheproject = 'Limited human resources to complete the project',
  Limitedtechnicalexpertisetocompletetheproject = 'Limited technical expertise to complete the project',
  Previouschallengeshaveoccurredwithsimilarprojects = 'Previous challenges have occurred with similar projects',
}

export enum TransferRisks {
  increased = 'Increased',
  transferred = 'Transferred',
}

export enum CostConsiderations {
  IneligibleCosts = 'Ineligible Costs',
  InKindContributions = 'In-kind Contributions',
  CostStacking = 'Cost Stacking',
  PhasedCosts = 'Phased Costs',
}

// TODO: temp before API provides the correct structure
export class FileForm {
  @prop()
  name?: string;

  @prop()
  url?: string;

  @prop()
  type?: string;

  @prop()
  id?: string;

  @prop()
  comment?: string;
}

export class ImpactedInfrastructureForm {
  @prop()
  @required()
  infrastructure?: string;

  @prop()
  @required()
  impact?: string;

  constructor(values: ImpactedInfrastructureForm) {
    Object.assign(this, values);
  }
}

export class ProponentAndProjectInformationForm {
  @prop()
  @required()
  projectTitle?: string;

  @prop()
  @required()
  scopeStatement?: string;

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

  @prop()
  @required()
  regionalProject?: boolean;

  @prop()
  regionalProjectComments?: string;

  constructor(values: ProponentInformationForm) {
    Object.assign(this, values);
  }
}

export class OwnershipAndAuthorizationForm {
  @prop()
  @required()
  ownershipDeclaration?: boolean;

  @prop()
  ownershipDescription?: string;

  @prop()
  @required()
  projectAuthority?: boolean;

  @prop()
  @required()
  projectAuthorityComments?: string;

  @prop()
  @required()
  operationAndMaintenance?: YesNoOption;

  @prop()
  @required()
  operationAndMaintenanceComments?: string;

  @prop()
  @required()
  firstNationsEndorsement?: YesNoOption;

  @prop()
  @required()
  localGovernmentEndorsement?: YesNoOption;

  @prop()
  @required()
  authorizationOrEndorsementComments?: string;

  constructor(values: OwnershipAndAuthorizationForm) {
    Object.assign(this, values);
  }
}

export class PermitsRegulationsAndStandardsForm {
  @prop()
  @required()
  approvals?: boolean;

  @prop()
  @required()
  approvalsComments?: string;

  @prop()
  @required()
  professionalGuidance?: boolean;

  @prop()
  @required()
  @minLength({ value: 1 })
  professionals?: string[] = [];

  @prop()
  professionalGuidanceComments?: string;

  @prop()
  @required()
  standardsAcceptable?: YesNoOption;

  @prop()
  @required()
  @minLength({ value: 1 })
  standards?: string[] = [];

  @prop()
  standardsComments?: string;

  @prop()
  @required()
  regulations?: boolean;

  @prop()
  @required()
  regulationsComments?: string;

  constructor(values: PermitsRegulationsAndStandardsForm) {
    Object.assign(this, values);
  }
}

export class YearOverYearFundingForm {
  @prop()
  @required()
  year?: string;

  @prop()
  @required()
  amount?: number;

  constructor(values: YearOverYearFundingForm) {
    Object.assign(this, values);
  }
}

export class BudgetForm {
  @prop()
  @required()
  totalProjectCost?: number;

  @prop()
  @required()
  estimatedTotal?: number;

  @prop()
  @required()
  fundingRequest?: number;

  @prop()
  @required()
  remainingAmount?: number;

  @propArray(YearOverYearFundingForm)
  yearOverYearFunding?: YearOverYearFundingForm[] = [{}];

  @prop()
  @required()
  totalDrifFundingRequest?: number;

  @prop()
  discrepancyComment?: string;

  @prop()
  @required()
  haveOtherFunding?: boolean;

  @propArray(FundingInformationItemForm)
  otherFunding?: FundingInformationItemForm[] = [{}];

  @prop()
  intendToSecureFunding?: string;

  @prop()
  @required()
  costEffective?: boolean;

  @prop()
  @required()
  costEffectiveComments?: string;

  @prop()
  @required()
  previousResponse?: YesNoOption;

  @prop()
  previousResponseCost?: number;

  @prop()
  previousResponseComments?: string;

  @prop()
  @required()
  activityCostEffectiveness?: string;

  @prop()
  @required()
  costConsiderationsApplied?: boolean;

  @prop()
  @required()
  @minLength({ value: 1 })
  costConsiderations?: string[];

  @prop()
  costConsiderationsComments?: string;

  constructor(values: BudgetForm) {
    Object.assign(this, values);
  }
}

export class ClimateAdaptationForm {
  @prop()
  @required()
  climateAdaptationScreener?: boolean;

  @prop()
  @required()
  climateAdaptation?: string;

  constructor(values: ClimateAdaptationForm) {
    Object.assign(this, values);
  }
}

export class ProjectRisksForm {
  @prop()
  @required()
  complexityRiskMitigated?: boolean;

  @prop()
  @required()
  @minLength({ value: 1 })
  complexityRisks?: string[];

  @prop()
  complexityRiskComments?: string;

  @prop()
  @required()
  readinessRiskMitigated?: boolean;

  @prop()
  @required()
  @minLength({ value: 1 })
  readinessRisks?: string[];

  @prop()
  readinessRiskComments?: string;

  @prop()
  @required()
  sensitivityRiskMitigated?: boolean;

  @prop()
  @required()
  @minLength({ value: 1 })
  sensitivityRisks?: string[];

  @prop()
  sensitivityRiskComments?: string;

  @prop()
  @required()
  capacityRiskMitigated?: boolean;

  @prop()
  @required()
  @minLength({ value: 1 })
  capacityRisks?: string[];

  @prop()
  capacityRiskComments?: string;

  @prop()
  @required()
  riskTransferMigigated?: boolean;

  @prop()
  @required()
  @minLength({ value: 1 })
  transferRisks?: string[];

  @prop()
  transferRisksComments?: string;

  constructor(values: ProjectRisksForm) {
    Object.assign(this, values);
  }
}

export class ProjectAreaForm {
  @prop()
  @required()
  locationDescription?: string;

  @prop()
  @required()
  area?: number;

  @prop()
  @required()
  units?: string; // TODO: use enum

  @prop()
  @required()
  areaDescription?: string;

  @prop()
  @required()
  @minLength({ value: 1 })
  relatedHazards?: Hazards[];

  @prop()
  otherHazardsDescription?: string;

  @prop()
  @required()
  communityImpact?: string;

  @prop()
  @required()
  estimatedPeopleImpacted?: EstimatedNumberOfPeople;

  @propArray(ImpactedInfrastructureForm)
  infrastructureImpacted?: ImpactedInfrastructureForm[] = [{}];

  constructor(values: ProjectAreaForm) {
    Object.assign(this, values);
  }
}

export class ProposedActivityForm {
  @prop()
  @required()
  activityName?: string;

  @prop()
  @required()
  activityStart?: string;

  @prop()
  @required()
  activityEnd?: string;

  @prop()
  relatedMilestone?: string;

  constructor(values: ProposedActivityForm) {
    Object.assign(this, values);
  }
}

export class ProjectPlanForm {
  @prop()
  @required()
  startDate?: string;

  @prop()
  @required()
  endDate?: string;

  @prop()
  @required()
  projectDescription?: string;

  @propArray(ProposedActivityForm)
  // TODO: @minLength({ value: 1}) doesn't work with object arrays?
  proposedActivities?: ProposedActivityForm[] = [{}];

  @prop()
  @required()
  @minLength({ value: 1 })
  verificationMethods?: string[];

  @prop()
  @required()
  verificationMethodsComments?: string;

  @prop()
  @required()
  projectAlternateOptions?: string;

  constructor(values: ProjectPlanForm) {
    Object.assign(this, values);
  }
}

export class ProjectEngagementForm {
  @prop()
  @required()
  engagedWithFirstNations?: boolean;

  @prop()
  @required()
  engagedWithFirstNationsComments?: string;

  @prop()
  @required()
  otherEngagement?: YesNoOption;

  @prop()
  @required()
  @minLength({ value: 1 })
  affectedParties?: string[] = [];

  @prop()
  @required()
  otherEngagementComments?: string;

  @prop()
  @required()
  collaborationComments?: string;

  constructor(values: ProjectEngagementForm) {
    Object.assign(this, values);
  }
}

export class ProjectOutcomesForm {
  @prop()
  @required()
  publicBenefit?: boolean;

  @prop()
  @required()
  publicBenefitComments?: string;

  @prop()
  @required()
  futureCostReduction?: boolean;

  @prop()
  costReductions?: string[];

  @prop()
  costReductionComments?: string;

  @prop()
  @required()
  produceCoBenefits?: boolean;

  @prop()
  coBenefits?: string[];

  @prop()
  coBenefitComments?: string;

  @prop()
  @required()
  @minLength({ value: 1 })
  increasedResiliency?: string[];

  @prop()
  @required()
  increasedResiliencyComments?: string;

  constructor(values: ProjectOutcomesForm) {
    Object.assign(this, values);
  }
}

export class AttachmentsForm {
  @propArray(FileForm)
  projectDocuments?: FileForm[] = [{}];

  constructor(values: AttachmentsForm) {
    Object.assign(this, values);
  }
}

export class DeclarationsForm {
  @required()
  @propObject(ContactDetailsForm)
  submitter?: ContactDetailsForm = new ContactDetailsForm({});

  @prop()
  @required()
  authorizedRepresentativeStatement?: boolean;

  @prop()
  @required()
  informationAccuracyStatement?: boolean;

  constructor(values: DeclarationsForm) {
    Object.assign(this, values);
  }
}

export class DrifFpForm {
  @propObject(ProponentAndProjectInformationForm)
  proponentAndProjectInformationForm?: ProponentAndProjectInformationForm =
    new ProponentAndProjectInformationForm({});

  @propObject(OwnershipAndAuthorizationForm)
  ownershipAndAuthorization?: OwnershipAndAuthorizationForm =
    new OwnershipAndAuthorizationForm({});

  @propObject(ProjectAreaForm)
  projectArea?: ProjectAreaForm = new ProjectAreaForm({});

  @propObject(ProjectPlanForm)
  projectPlan?: ProjectPlanForm = new ProjectPlanForm({});

  @propObject(ProjectEngagementForm)
  projectEngagement?: ProjectEngagementForm = new ProjectEngagementForm({});

  @propObject(ClimateAdaptationForm)
  climateAdaptation?: ClimateAdaptationForm = new ClimateAdaptationForm({});

  @propObject(PermitsRegulationsAndStandardsForm)
  permitsRegulationsAndStandards?: PermitsRegulationsAndStandardsForm =
    new PermitsRegulationsAndStandardsForm({});

  @propObject(ProjectOutcomesForm)
  projectOutcomes?: ProjectOutcomesForm = new ProjectOutcomesForm({});

  @propObject(ProjectRisksForm)
  projectRisks?: ProjectRisksForm = new ProjectRisksForm({});

  @propObject(BudgetForm)
  budget?: BudgetForm = new BudgetForm({});

  @propObject(AttachmentsForm)
  attachments?: AttachmentsForm = new AttachmentsForm({});

  @propObject(DeclarationsForm)
  declarations?: DeclarationsForm = new DeclarationsForm({});

  @prop()
  eoiId?: string;

  @prop()
  fundingStream?: FundingStream;

  @prop()
  projectType?: ProjectType;
  // TODO: have a factory method to create a new instance of this form
}
