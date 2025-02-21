import {
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
  ActivityType,
  Attachment,
  CostCategory,
  CostEstimate,
  CostEstimateClass,
  CostUnit,
  DocumentType,
  EstimatedNumberOfPeopleFP,
  FundingStream,
  Hazards,
  ProjectType,
  ProposedActivity,
  ResourceCategory,
  StandardInfo,
  YesNoOption,
} from '../../../model';
import {
  ContactDetailsForm,
  FundingInformationItemForm,
  ProponentInformationForm,
  StringItem,
} from '../drif-eoi/drif-eoi-form';

export class AttachmentForm implements Attachment {
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

export class StandardInfoForm implements StandardInfo {
  @prop()
  category?: string | undefined;

  @prop()
  isCategorySelected?: boolean | undefined;

  @prop()
  standards?: string[] | undefined;

  constructor(values: StandardInfoForm) {
    Object.assign(this, values);
  }
}

export class ProponentAndProjectInformationForm {
  @prop()
  @required()
  projectTitle?: string;

  @prop()
  @required()
  mainDeliverable?: string;

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
  haveAuthorityToDevelop?: boolean;

  @prop()
  @required()
  operationAndMaintenance?: YesNoOption;

  @prop()
  @required()
  operationAndMaintenanceComments?: string;

  @prop()
  @required()
  firstNationsAuthorizedByPartners?: YesNoOption;

  @prop()
  @required()
  localGovernmentAuthorizedByPartners?: YesNoOption;

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
  meetsEligibilityRequirements?: boolean;

  @prop()
  meetsEligibilityComments?: string;

  @prop()
  @required()
  professionalGuidance?: boolean;

  @prop()
  professionals?: string[] = [];

  @prop()
  professionalGuidanceComments?: string;

  @prop()
  knowledgeHolders?: string;

  @prop()
  @required()
  standardsAcceptable?: YesNoOption;

  @propArray(StandardInfoForm)
  standards?: StandardInfoForm[] = [];

  // not used by form directly, but used to determine if the form is valid
  // as there's not way to validate the array of standards selection and make form invalid
  @prop()
  standardsValid?: boolean | undefined;

  @prop()
  standardsComments?: string;

  @prop()
  @required()
  meetsRegulatoryRequirements?: boolean;

  @prop()
  permits?: string[] = [];

  @propArray(StringItem)
  permitsArray?: StringItem[] = [{ value: '' }];

  @prop()
  meetsRegulatoryComments?: string;

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
  @maxNumber({ value: 999999999 })
  amount?: number;

  constructor(values: YearOverYearFundingForm) {
    Object.assign(this, values);
  }
}

export class CostEstimateForm implements CostEstimate {
  @prop()
  id?: string;

  @prop()
  @required()
  taskName?: string;

  @prop()
  @required()
  costCategory?: CostCategory;

  @prop()
  description?: string;

  @prop()
  resources?: ResourceCategory;

  @prop()
  @required()
  units?: CostUnit;

  @prop()
  @required()
  @maxNumber({ value: 999999999 })
  @minNumber({ value: 0 })
  quantity?: number;

  @prop()
  @required()
  @minNumber({ value: 0 })
  @maxNumber({ value: 999999999 })
  unitRate?: number;

  @prop()
  @required()
  @minNumber({ value: 0 })
  @maxNumber({ value: 999999999 })
  totalCost?: number;

  constructor(values: CostEstimateForm) {
    Object.assign(this, values);
  }
}

export class BudgetForm {
  @prop()
  @required()
  totalProjectCost?: number;

  @prop()
  totalProjectCostChangeComments?: string;

  @prop()
  costEstimateClass?: CostEstimateClass;

  @prop()
  isContingencyPercentageThreasholdMet?: boolean;

  @prop()
  estimatedUnfundedAmount?: number;

  //* used to calculate remaining amount and hold original(potentially negative) value */
  @prop()
  remainingAmount?: number;

  //* used for display purposes only */
  @prop()
  remainingAmountAbs?: number;

  @propArray(YearOverYearFundingForm)
  yearOverYearFunding?: YearOverYearFundingForm[] = [{}];

  //* used to calculate total funding request from year over year forecasting */
  @prop()
  @required()
  @maxNumber({ value: 999999999 })
  @minNumber({ value: -999999999 })
  totalDrifFundingRequest?: number;

  //* represents EOI/FP funding request */
  @prop()
  eligibleFundingRequest?: number;

  @prop()
  fundingRequestDiscrepancy?: number = 0;

  @prop()
  discrepancyComment?: string;

  @prop()
  @required()
  haveOtherFunding?: boolean;

  @propArray(FundingInformationItemForm)
  otherFunding?: FundingInformationItemForm[] = [];

  @prop()
  intendToSecureFunding?: string;

  @propArray(CostEstimateForm)
  costEstimates?: CostEstimateForm[] = [];

  //* used to determine if the form is valid */
  @prop()
  @requiredTrue()
  estimatesMatchFundingRequest?: boolean;

  //* used to calculate contingency */
  @prop()
  @maxNumber({ value: 100 })
  @minNumber({ value: 0 })
  contingency?: number;

  //* used to calculate total cost estimate */
  @prop()
  @required()
  @maxNumber({ value: 999999999 })
  @minNumber({ value: 0 })
  totalEligibleCosts?: number;

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
  costConsiderationsApplied?: boolean;

  @prop()
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
  incorporateFutureClimateConditions?: boolean;

  @prop()
  // TODO: investigate further
  // conditionalExpression does not set validator, but form control errors which is not enough if field is filled
  // @required({
  //   conditionalExpression: (form: ClimateAdaptationForm) =>
  //     form.incorporateFutureClimateConditions === true,
  // })
  climateAdaptation?: string;

  @prop()
  @required()
  climateAssessment?: boolean;

  @prop()
  climateAssessmentTools?: string[] = [];

  @prop()
  climateAssessmentComments?: string;

  constructor(values: ClimateAdaptationForm) {
    Object.assign(this, values);
  }
}

export class ProjectRisksForm {
  @prop()
  @required()
  complexityRiskMitigated?: boolean;

  @prop()
  complexityRisks?: string[];

  @prop()
  complexityRiskComments?: string;

  @prop()
  @required()
  readinessRiskMitigated?: boolean;

  @prop()
  readinessRisks?: string[];

  @prop()
  readinessRiskComments?: string;

  @prop()
  @required()
  sensitivityRiskMitigated?: boolean;

  @prop()
  sensitivityRisks?: string[];

  @prop()
  sensitivityRiskComments?: string;

  @prop()
  @required()
  capacityRiskMitigated?: boolean;

  @prop()
  capacityRisks?: string[];

  @prop()
  capacityRiskComments?: string;

  @prop()
  @required()
  riskTransferMigigated?: boolean;

  @prop()
  increasedOrTransferred?: string[];

  @prop()
  increasedOrTransferredComments?: string;

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
  estimatedPeopleImpactedFP?: EstimatedNumberOfPeopleFP;

  @prop()
  @required()
  isInfrastructureImpacted?: boolean;

  @propArray(ImpactedInfrastructureForm)
  infrastructureImpacted?: ImpactedInfrastructureForm[] = [];

  constructor(values: ProjectAreaForm) {
    Object.assign(this, values);
  }
}

export class ProposedActivityForm implements ProposedActivity {
  @prop()
  id?: string;

  @prop()
  @required()
  activity?: ActivityType;

  @prop()
  startDate?: string;

  @prop()
  @required()
  endDate?: string;

  @prop()
  tasks?: string;

  @prop()
  deliverables?: string;

  @prop()
  preCreatedActivity?: boolean;

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
  proposedActivities?: ProposedActivityForm[] = [{}];

  @prop()
  @required()
  @minLength({ value: 1 })
  foundationalOrPreviousWorks?: string[];

  @prop()
  @required()
  howWasNeedIdentified?: string;

  @prop()
  @required()
  addressRisksAndHazards?: string;

  @prop()
  @required()
  disasterRiskUnderstanding?: string;

  @prop()
  @required()
  rationaleForFunding?: string;

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
  engagedWithFirstNationsOccurred?: boolean;

  @prop()
  @required()
  engagedWithFirstNationsComments?: string;

  @prop()
  @required()
  otherEngagement?: YesNoOption;

  @prop()
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
  @propArray(AttachmentForm)
  attachments?: AttachmentForm[] = [
    {
      documentType: DocumentType.DetailedCostEstimate,
    },
  ];

  @prop()
  @required()
  haveResolution?: boolean;

  constructor(values: AttachmentsForm) {
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

export class DrifFpForm {
  @propObject(ProponentAndProjectInformationForm)
  proponentAndProjectInformation?: ProponentAndProjectInformationForm =
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

  @propObject(DeclarationForm)
  declaration?: DeclarationForm = new DeclarationForm({});

  @prop()
  eoiId?: string;

  @prop()
  fundingStream?: FundingStream;

  @prop()
  projectType?: ProjectType;
  // TODO: have a factory method to create a new instance of this form
}
