import {
  maxNumber,
  minNumber,
  prop,
  propArray,
  propObject,
  required,
  requiredTrue,
} from '@rxweb/reactive-form-validators';
import {
  ActivityType,
  Delay,
  DraftProgressReport,
  EventInformation,
  FundingSignage,
  InterimProjectType,
  PastEvent,
  ProjectEvent,
  ProjectProgressStatus,
  SignageType,
  Workplan,
  WorkplanActivity,
  WorkplanStatus,
} from '../../../../model';
import { ContactDetailsForm } from '../../drif-eoi/drif-eoi-form';
import { AttachmentForm } from '../../drif-fp/drif-fp-form';

export enum EventProgressType {
  NotStarted = 'Not Started',
  InProgress = 'In Progress',
  Completed = 'Completed',
}

export class WorkplanActivityForm implements WorkplanActivity {
  @prop()
  id?: string;

  @prop()
  @required()
  activity?: ActivityType;

  @prop()
  preCreatedActivity?: boolean;

  @prop()
  isMandatory?: boolean;

  @prop()
  @required()
  status?: WorkplanStatus;

  @prop()
  // @required({
  //   conditionalExpression: function (
  //     model: any,
  //     form: any,
  //     control: any
  //   ) {
  //      TODO: this is working, but control doesn't update its state, need to investigate further
  //     console.log('model: ', model);
  //     console.log('form: ', form);
  //     console.log('control: ', control);
  //     return model.status === WorkplanProgressType.NotStarted;
  //   },
  // })
  comment?: string;

  @prop()
  plannedStartDate?: string;

  @prop()
  plannedCompletionDate?: string;

  @prop()
  actualStartDate?: string;

  @prop()
  actualCompletionDate?: string;

  constructor(values: WorkplanActivityForm) {
    Object.assign(this, values);
  }
}

export class FundingSignageForm implements FundingSignage {
  @prop()
  id?: string;

  @prop()
  @required()
  type?: SignageType | undefined;

  @prop()
  @required()
  beenApproved?: boolean;

  @prop()
  dateInstalled?: string;

  @prop()
  dateRemoved?: string;

  constructor(values: FundingSignageForm) {
    Object.assign(this, values);
  }
}

export class WorkplanForm implements Workplan {
  @prop()
  @required()
  projectProgress?: ProjectProgressStatus;

  @prop()
  @required({
    conditionalExpression: function (control: any) {
      return control.projectProgress == ProjectProgressStatus.BehindSchedule;
    },
  })
  delayReason?: Delay;

  @prop()
  otherDelayReason?: string;

  // TODO: the reason why this is not working with hasValidator(Validators.required)
  // is because rxweb create a custom validator that doesn't match
  // it is possible unsolvable, need to investigate further
  @required({
    conditionalExpression: function (model: any) {
      return model.projectProgress == ProjectProgressStatus.BehindSchedule;
    },
  })
  @prop()
  behindScheduleMitigatingComments?: string;

  @prop()
  @required({
    conditionalExpression: function (control: any) {
      return control.projectProgress == ProjectProgressStatus.AheadOfSchedule;
    },
  })
  aheadOfScheduleComments?: string;

  @propArray(WorkplanActivityForm)
  workplanActivities?: WorkplanActivityForm[] = [];

  @prop()
  @required()
  @minNumber({ value: 0 })
  @maxNumber({ value: 100 })
  projectCompletionPercentage?: number;

  @prop()
  @required()
  @minNumber({ value: 0 })
  @maxNumber({ value: 100 })
  constructionCompletionPercentage?: number;

  @prop()
  @required()
  mediaAnnouncement?: boolean;

  @prop()
  mediaAnnouncementDate?: string;

  @prop()
  mediaAnnouncementComment?: string;

  @prop()
  @required()
  outstandingIssues?: boolean;

  @prop()
  outstandingIssuesComments?: string;

  @prop()
  @required()
  fundingSourcesChanged?: boolean;

  @prop()
  fundingSourcesChangedComment?: string;

  @prop()
  @required()
  signageRequired?: boolean;

  @propArray(FundingSignageForm)
  fundingSignage?: FundingSignageForm[] = [{}];

  @prop()
  signageNotRequiredComments?: string;

  constructor(values: WorkplanForm) {
    Object.assign(this, values);
  }
}

export class ProjectEventForm implements ProjectEvent {
  @prop()
  id?: string;

  @prop()
  @required()
  details?: string;

  @prop()
  @required()
  date?: string;

  @propObject(ContactDetailsForm)
  contact?: ContactDetailsForm = new ContactDetailsForm({});

  @prop()
  @required()
  provincialRepresentativeRequest?: boolean;

  constructor(values: ProjectEventForm) {
    Object.assign(this, values);
  }
}

export class PastEventForm implements PastEvent {
  @prop()
  id?: string;

  @prop()
  @required()
  details?: string;

  @prop()
  @required()
  date?: string;

  constructor(values: PastEventForm) {
    Object.assign(this, values);
  }
}

export class EventInformationForm implements EventInformation {
  @prop()
  @required()
  eventsOccurredSinceLastReport?: boolean | undefined;

  @propArray(PastEventForm)
  pastEvents?: PastEventForm[] = [];

  @prop()
  @required()
  anyUpcomingEvents?: boolean | undefined;

  @propArray(ProjectEventForm)
  upcomingEvents?: ProjectEventForm[] = [];

  constructor(values: EventInformationForm) {
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

export class ProgressReportForm implements DraftProgressReport {
  @prop()
  projectType?: InterimProjectType | undefined;

  @propObject(WorkplanForm)
  workplan?: WorkplanForm = new WorkplanForm({});

  @propObject(EventInformationForm)
  eventInformation?: EventInformationForm = new EventInformationForm({});

  @propArray(AttachmentForm)
  attachments?: AttachmentForm[] = [];

  @propObject(DeclarationForm)
  declaration?: DeclarationForm = new DeclarationForm({});
}
