/**
 * Generated by orval v7.3.0 🍺
 * Do not edit manually.
 * DRR API
 * OpenAPI spec version: 1.0.0
 */
import type { ProjectClaim } from './projectClaim';
import type { Forecast } from './forecast';
import type { ProgressReport } from './progressReport';
import type { InterimReportStatus } from './interimReportStatus';

export interface InterimReport {
  /** @nullable */
  claim?: ProjectClaim;
  /** @nullable */
  description?: string;
  /** @nullable */
  dueDate?: string;
  /** @nullable */
  forecast?: Forecast;
  /** @nullable */
  id?: string;
  /** @nullable */
  report?: ProgressReport;
  /** @nullable */
  status?: InterimReportStatus;
}