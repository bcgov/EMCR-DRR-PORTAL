/**
 * Generated by orval v7.3.0 🍺
 * Do not edit manually.
 * DRR API
 * OpenAPI spec version: 1.0.0
 */
import type { Attachment } from './attachment';
import type { ProjectClaim } from './projectClaim';
import type { PaymentCondition } from './paymentCondition';
import type { ContactDetails } from './contactDetails';
import type { Forecast } from './forecast';
import type { FundingStream } from './fundingStream';
import type { InterimReport } from './interimReport';
import type { ProgramType } from './programType';
import type { ProgressReport } from './progressReport';
import type { ReportingScheduleType } from './reportingScheduleType';
import type { ProjectStatus } from './projectStatus';

export interface DrrProject {
  /** @nullable */
  attachments?: Attachment[];
  /** @nullable */
  claims?: ProjectClaim[];
  /** @nullable */
  conditions?: PaymentCondition[];
  /** @nullable */
  contacts?: ContactDetails[];
  /** @nullable */
  contractNumber?: string;
  /** @nullable */
  endDate?: string;
  /** @nullable */
  eoiId?: string;
  /** @nullable */
  forecast?: Forecast[];
  /** @nullable */
  fpId?: string;
  /** @nullable */
  fundingAmount?: number;
  /** @nullable */
  fundingStream?: FundingStream;
  /** @nullable */
  id?: string;
  /** @nullable */
  interimReports?: InterimReport[];
  /** @nullable */
  programType?: ProgramType;
  /** @nullable */
  progressReports?: ProgressReport[];
  /** @nullable */
  projectNumber?: string;
  /** @nullable */
  projectTitle?: string;
  /** @nullable */
  proponentName?: string;
  /** @nullable */
  reportingScheduleType?: ReportingScheduleType;
  /** @nullable */
  startDate?: string;
  /** @nullable */
  status?: ProjectStatus;
}