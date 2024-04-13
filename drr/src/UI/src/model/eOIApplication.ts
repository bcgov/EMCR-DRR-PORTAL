/**
 * Generated by orval v6.27.1 🍺
 * Do not edit manually.
 * DRR API
 * OpenAPI spec version: 1.0.0
 */
import type { ApplicantType } from './applicantType';
import type { ContactDetails } from './contactDetails';
import type { ProjectType } from './projectType';

export interface EOIApplication {
  applicantName?: string;
  applicantType?: ApplicantType;
  /** @nullable */
  area?: number;
  backgroundDescription?: string;
  cfoConfirmation?: boolean;
  climateAdaptation?: string;
  /** @nullable */
  coordinates?: string;
  endDate?: string;
  engagementProposal?: string;
  foippaConfirmation?: boolean;
  fundingRequest?: number;
  identityConfirmation?: boolean;
  locationDescription?: string;
  otherFunding?: string[];
  otherInformation?: string;
  /** @nullable */
  ownership?: string;
  ownershipDeclaration?: boolean;
  projectContacts?: ContactDetails[];
  projectTitle?: string;
  projectType?: ProjectType;
  proposedSolution?: string;
  rationaleForFunding?: string;
  rationaleForSolution?: string;
  relatedHazards?: string[];
  startDate?: string;
  submitter?: ContactDetails;
  totalFunding?: number;
  unfundedAmount?: number;
  /** @nullable */
  units?: string;
}