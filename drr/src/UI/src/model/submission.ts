/**
 * Generated by orval v6.27.1 🍺
 * Do not edit manually.
 * DRR API
 * OpenAPI spec version: 1.0.0
 */
import type { SubmissionPortalStatus } from './submissionPortalStatus';

export interface Submission {
  fundingRequest?: string;
  id?: string;
  modifiedDate?: string;
  partneringProponents?: string[];
  projectTitle?: string;
  status?: SubmissionPortalStatus;
  /** @nullable */
  submittedDate?: string;
}
