using System;
using AutoMapper;
using EMCR.DRR.Dynamics;
using EMCR.DRR.Managers.Intake;
using EMCR.Utilities.Extensions;
using Microsoft.Dynamics.CRM;

namespace EMCR.DRR.Resources.Applications
{
    public class ApplicationMapperProfile : Profile
    {
        private string ArchaeologyCategoryName = "Archaeology";
        private string EnvironmentMappingCategoryName = "Environment - Mapping and Landscape";
        private string EnvironmentSeismicCategoryName = "Environment - Seismic";
        private string EnvitonmentWaterCategoryName = "Environment - Water (includes Rivers, Flooding, etc.)";
        private string FinancialCategoryName = "Financial";
        private string OtherCategoryName = "Other";

        public ApplicationMapperProfile()
        {
#pragma warning disable CS8629 // Nullable value type may be null.
            CreateMap<Application, drr_application>(MemberList.None)
                .ForMember(dest => dest.drr_primaryproponent, opt => opt.MapFrom(src => src.ProponentType.HasValue ? (int?)Enum.Parse<ApplicantTypeOptionSet>(src.ProponentType.Value.ToString()) : null))
                .ForMember(dest => dest.drr_Primary_Proponent_Name, opt => opt.MapFrom(src => new account { name = src.ProponentName, drr_bceidguid = src.BCeIDBusinessId }))
                .ForMember(dest => dest.drr_SubmitterContact, opt => opt.MapFrom(src => src.Submitter))
                .ForMember(dest => dest.drr_PrimaryProjectContact, opt => opt.MapFrom(src => src.ProjectContact))
                .ForMember(dest => dest.drr_AdditionalContact1, opt => opt.MapFrom(src => src.AdditionalContact1))
                .ForMember(dest => dest.drr_AdditionalContact2, opt => opt.MapFrom(src => src.AdditionalContact2))
                .ForMember(dest => dest.drr_fundingstream, opt => opt.MapFrom(src => src.FundingStream.HasValue ? (int?)Enum.Parse<FundingStreamOptionSet>(src.FundingStream.Value.ToString()) : null))
                .ForMember(dest => dest.drr_projecttitle, opt => opt.MapFrom(src => src.ProjectTitle))
                .ForMember(dest => dest.drr_projecttype, opt => opt.MapFrom(src => src.ProjectType.HasValue ? (int?)Enum.Parse<ProjectTypeOptionSet>(src.ProjectType.Value.ToString()) : null))
                .ForMember(dest => dest.drr_summarizedscopestatement, opt => opt.MapFrom(src => src.ScopeStatement))
                .ForMember(dest => dest.drr_hazards, opt => opt.MapFrom(src => src.RelatedHazards.Count() > 0 ? string.Join(",", src.RelatedHazards.Select(h => (int?)Enum.Parse<HazardsOptionSet>(h.ToString()))) : null))
                .ForMember(dest => dest.drr_reasonswhyotherselectedforhazards, opt => opt.MapFrom(src => src.OtherHazardsDescription))
                .ForMember(dest => dest.drr_anticipatedprojectstartdate, opt => opt.MapFrom(src => src.StartDate.HasValue ? src.StartDate.Value.ToUniversalTime() : (DateTimeOffset?)null))
                .ForMember(dest => dest.drr_anticipatedprojectenddate, opt => opt.MapFrom(src => src.EndDate.HasValue ? src.EndDate.Value.ToUniversalTime() : (DateTimeOffset?)null))
                .ForMember(dest => dest.drr_estimated_total_project_cost, opt => opt.MapFrom(src => src.EstimatedTotal))
                .ForMember(dest => dest.drr_estimateddriffundingprogramrequest, opt => opt.MapFrom(src => src.FundingRequest))
                .ForMember(dest => dest.drr_otherfundingsources, opt => opt.MapFrom(src => src.HaveOtherFunding.HasValue ? src.HaveOtherFunding.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_application_fundingsource_Application, opt => opt.MapFrom(src => src.OtherFunding))
                .ForMember(dest => dest.drr_remaining_amount, opt => opt.MapFrom(src => src.RemainingAmount))
                .ForMember(dest => dest.drr_reasonstosecurefunding, opt => opt.MapFrom(src => src.IntendToSecureFunding))
                .ForMember(dest => dest.drr_ownershipdeclaration, opt => opt.MapFrom(src => src.OwnershipDeclaration.HasValue ? src.OwnershipDeclaration.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_ownershipdeclarationcontext, opt => opt.MapFrom(src => src.OwnershipDescription))
                .ForMember(dest => dest.drr_locationdescription, opt => opt.MapFrom(src => src.LocationDescription))
                .ForMember(dest => dest.drr_rationaleforfundingrequest, opt => opt.MapFrom(src => src.RationaleForFunding))
                .ForMember(dest => dest.drr_estimatednumberpeopleimpacted, opt => opt.MapFrom(src => src.EstimatedPeopleImpacted.HasValue ? (int?)Enum.Parse<EstimatedNumberOfPeopleOptionSet>(src.EstimatedPeopleImpacted.Value.ToString()) : null))
                .ForMember(dest => dest.drr_impacttocommunity, opt => opt.MapFrom(src => src.CommunityImpact))
                .ForMember(dest => dest.drr_criticalinfrastructurewillormaybeimpacted, opt => opt.MapFrom(src => src.IsInfrastructureImpacted.HasValue ? src.IsInfrastructureImpacted.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_drr_application_drr_criticalinfrastructureimpacted_Application, opt => opt.MapFrom(src => src.InfrastructureImpacted))
                .ForMember(dest => dest.drr_improveunderstandingriskinvestreduction, opt => opt.MapFrom(src => src.DisasterRiskUnderstanding))
                .ForMember(dest => dest.drr_includedtoaddressidentifiedriskhazards, opt => opt.MapFrom(src => src.AddressRisksAndHazards))
                .ForMember(dest => dest.drr_howdoesprojectalignwithdrifsprogramgoals, opt => opt.MapFrom(src => src.ProjectDescription))
                .ForMember(dest => dest.drr_additionalrelevantinformation1, opt => opt.MapFrom(src => src.AdditionalBackgroundInformation))
                .ForMember(dest => dest.drr_rationalforproposedsolution, opt => opt.MapFrom(src => src.RationaleForSolution))
                .ForMember(dest => dest.drr_engagementwithfirstnationsorindigenousorg, opt => opt.MapFrom(src => src.FirstNationsEngagement))
                .ForMember(dest => dest.drr_plantoengageotherjurisdictionsandparties, opt => opt.MapFrom(src => src.NeighbourEngagement))
                .ForMember(dest => dest.drr_additionalrelevantinformation2, opt => opt.MapFrom(src => src.AdditionalSolutionInformation))
                .ForMember(dest => dest.drr_additionalrelevantinformation3, opt => opt.MapFrom(src => src.AdditionalEngagementInformation))
                .ForMember(dest => dest.drr_climateadaptation, opt => opt.MapFrom(src => src.ClimateAdaptation))
                .ForMember(dest => dest.drr_otherrelevantinformation, opt => opt.MapFrom(src => src.OtherInformation))
                .ForMember(dest => dest.drr_authorizedrepresentative, opt => opt.MapFrom(src => src.AuthorizedRepresentativeStatement.HasValue ? src.AuthorizedRepresentativeStatement.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_foippaconfirmation, opt => opt.MapFrom(src => src.FOIPPAConfirmation.HasValue ? src.FOIPPAConfirmation.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_accuracyofinformation, opt => opt.MapFrom(src => src.InformationAccuracyStatement.HasValue ? src.InformationAccuracyStatement.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_submitteddate, opt => opt.MapFrom(src => src.SubmittedDate))
                .ForMember(dest => dest.statuscode, opt => opt.MapFrom(src => (int?)Enum.Parse<ApplicationStatusOptionSet>(src.Status.ToString())))
                //FP Only Fields
                //Proponent & Project Information - 1
                .ForMember(dest => dest.drr_isthisaregionalproject, opt => opt.MapFrom(src => src.RegionalProject.HasValue ? src.RegionalProject.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_isthisaregionalprojectcomments, opt => opt.MapFrom(src => src.RegionalProjectComments))
                .ForMember(dest => dest.drr_projecttypefullproposal, opt => opt.MapFrom(src => src.MainDeliverable))
                //Ownership & Authorization - 2
                .ForMember(dest => dest.drr_proponenthastheauthorityandownership, opt => opt.MapFrom(src => src.HaveAuthorityToDevelop.HasValue ? src.HaveAuthorityToDevelop.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_proponenttomaintaininfrastructurelongterm, opt => opt.MapFrom(src => src.OperationAndMaintenance.HasValue ? (int?)Enum.Parse<DRRYesNoNotApplicable>(src.OperationAndMaintenance.Value.ToString()) : null))
                .ForMember(dest => dest.drr_commentsprojectauthority, opt => opt.MapFrom(src => src.OperationAndMaintenanceComments))
                .ForMember(dest => dest.drr_authorizedendorsedfirstnationpartners, opt => opt.MapFrom(src => src.FirstNationsAuthorizedByPartners.HasValue ? (int?)Enum.Parse<DRRYesNoNotApplicable>(src.FirstNationsAuthorizedByPartners.Value.ToString()) : null))
                .ForMember(dest => dest.drr_authorizedendorsedlocalgovpartners, opt => opt.MapFrom(src => src.LocalGovernmentAuthorizedByPartners.HasValue ? (int?)Enum.Parse<DRRYesNoNotApplicable>(src.LocalGovernmentAuthorizedByPartners.Value.ToString()) : null))
                .ForMember(dest => dest.drr_authorizationorendorsementcomments, opt => opt.MapFrom(src => src.AuthorizationOrEndorsementComments))
                //Project Area - 3 - intentionally blank
                .ForMember(dest => dest.drr_estimatedsizeofprojectarea, opt => opt.MapFrom(src => src.Area))
                .ForMember(dest => dest.drr_unitofmeasure, opt => opt.MapFrom(src => src.Units.HasValue ? (int?)Enum.Parse<AreaUnitsOptionSet>(src.Units.Value.ToString()) : null))
                .ForMember(dest => dest.drr_projectarea, opt => opt.MapFrom(src => src.AreaDescription))
                .ForMember(dest => dest.drr_criticalinfrastructurewillormaybeimpacted, opt => opt.MapFrom(src => src.IsInfrastructureImpacted.HasValue ? src.IsInfrastructureImpacted.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_estimatednumberofpeopleimpactedfp, opt => opt.MapFrom(src => src.EstimatedPeopleImpactedFP.HasValue ? (int?)Enum.Parse<EstimatedNumberOfPeopleFPOptionSet>(src.EstimatedPeopleImpactedFP.Value.ToString()) : null))

                //Project Plan - 4
                .ForMember(dest => dest.drr_drr_application_drr_proposedactivity_Application, opt => opt.MapFrom(src => src.ProposedActivities))
                .ForMember(dest => dest.drr_drr_application_drr_foundationalorpreviousworkitem_Application, opt => opt.MapFrom(src => src.FoundationalOrPreviousWorks))
                .ForMember(dest => dest.drr_howwastheneedfortheprojectidentified, opt => opt.MapFrom(src => src.HowWasNeedIdentified))
                .ForMember(dest => dest.drr_extentalternateprojectoptionsconsidered, opt => opt.MapFrom(src => src.ProjectAlternateOptions))

                //Project Engagement - 5
                .ForMember(dest => dest.drr_meaningfullyengagedwithlocalfirstnations, opt => opt.MapFrom(src => src.EngagedWithFirstNationsOccurred.HasValue ? src.EngagedWithFirstNationsOccurred.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_describeengagementfirstnations, opt => opt.MapFrom(src => src.EngagedWithFirstNationsComments))
                .ForMember(dest => dest.drr_effectivelyengagedwithothers, opt => opt.MapFrom(src => src.OtherEngagement.HasValue ? (int?)Enum.Parse<DRRYesNoNotApplicable>(src.OtherEngagement.Value.ToString()) : null))
                .ForMember(dest => dest.drr_drr_application_drr_impactedoraffectedpartyitem_Application, opt => opt.MapFrom(src => src.AffectedParties))
                .ForMember(dest => dest.drr_describeengagementimpactedaffectedparties, opt => opt.MapFrom(src => src.OtherEngagementComments))
                .ForMember(dest => dest.drr_howprojectcontributetocollaboration, opt => opt.MapFrom(src => src.CollaborationComments))

                //Climate Adaptation - 6
                .ForMember(dest => dest.drr_doesprojectconsiderclimatechange, opt => opt.MapFrom(src => src.IncorporateFutureClimateConditions.HasValue ? src.IncorporateFutureClimateConditions.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_hasprojectusedclimatechangeassessmenttool, opt => opt.MapFrom(src => src.ClimateAssessment.HasValue ? src.ClimateAssessment.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_drr_application_drr_climateassessmenttoolitem_Application, opt => opt.MapFrom(src => src.ClimateAssessmentTools))
                .ForMember(dest => dest.drr_howtoolsusedtobenefitproject, opt => opt.MapFrom(src => src.ClimateAssessmentComments))

                //Permits Regulations & Standards - 7
                .ForMember(dest => dest.drr_drr_application_drr_permitslicensesandauthorizations_Application, opt => opt.MapFrom(src => src.Permits))
                .ForMember(dest => dest.drr_archaeology, opt => opt.MapFrom(src => DRRCategorySelectedMapper(src.Standards.SingleOrDefault(s => s.Category == ArchaeologyCategoryName))))
                .ForMember(dest => dest.drr_environmentmappingandlandscape, opt => opt.MapFrom(src => DRRCategorySelectedMapper(src.Standards.SingleOrDefault(s => s.Category == EnvironmentMappingCategoryName))))
                .ForMember(dest => dest.drr_environmentseismic, opt => opt.MapFrom(src => DRRCategorySelectedMapper(src.Standards.SingleOrDefault(s => s.Category == EnvironmentSeismicCategoryName))))
                .ForMember(dest => dest.drr_environmentwater, opt => opt.MapFrom(src => DRRCategorySelectedMapper(src.Standards.SingleOrDefault(s => s.Category == EnvitonmentWaterCategoryName))))
                .ForMember(dest => dest.drr_financial, opt => opt.MapFrom(src => DRRCategorySelectedMapper(src.Standards.SingleOrDefault(s => s.Category == FinancialCategoryName))))
                .ForMember(dest => dest.drr_othercategory, opt => opt.MapFrom(src => DRRCategorySelectedMapper(src.Standards.SingleOrDefault(s => s.Category == OtherCategoryName))))
                .ForMember(dest => dest.drr_drr_application_drr_provincialstandarditem_Application, opt => opt.MapFrom(src => DRRProvincialStandardItemMapper(src.Standards)))
                .ForMember(dest => dest.drr_commentsacceptableprovincialstandards, opt => opt.MapFrom(src => src.StandardsComments))
                .ForMember(dest => dest.drr_guidanceofqualifiedprofessional, opt => opt.MapFrom(src => src.ProfessionalGuidance.HasValue ? src.ProfessionalGuidance.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_drr_application_drr_qualifiedprofessionalitem_Application, opt => opt.MapFrom(src => src.Professionals))
                .ForMember(dest => dest.drr_qualifiedprofessionalcomments, opt => opt.MapFrom(src => src.ProfessionalGuidanceComments))
                .ForMember(dest => dest.drr_additionaloversightadviceandorexpertise, opt => opt.MapFrom(src => src.KnowledgeHolders))
                .ForMember(dest => dest.drr_acceptableprovincialstandards, opt => opt.MapFrom(src => src.StandardsAcceptable.HasValue ? (int?)Enum.Parse<DRRYesNoNotApplicable>(src.StandardsAcceptable.Value.ToString()) : null))
                .ForMember(dest => dest.drr_requiredagencydiscussionsandapprovals, opt => opt.MapFrom(src => src.MeetsRegulatoryRequirements.HasValue ? src.MeetsRegulatoryRequirements.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_commentsrequiredagencydiscussionsapproval, opt => opt.MapFrom(src => src.MeetsRegulatoryComments))
                .ForMember(dest => dest.drr_projecteligibleforrequiredpermitsapproval, opt => opt.MapFrom(src => src.MeetsEligibilityRequirements.HasValue ? src.MeetsEligibilityRequirements.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_projecteligibleforpermitsapprovalcomments, opt => opt.MapFrom(src => src.MeetsEligibilityComments))

                //Project Outcomes - 8
                .ForMember(dest => dest.drr_projectforbroadpublicuseorbenefit, opt => opt.MapFrom(src => src.PublicBenefit.HasValue ? src.PublicBenefit.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_projectforbroadpublicuseorbenefitcomments, opt => opt.MapFrom(src => src.PublicBenefitComments))
                .ForMember(dest => dest.drr_willthisprojectreducecosts, opt => opt.MapFrom(src => src.FutureCostReduction.HasValue ? src.FutureCostReduction.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_drr_application_drr_costreductionitem_Application, opt => opt.MapFrom(src => src.CostReductions))
                .ForMember(dest => dest.drr_explainhowcostswillbereduced, opt => opt.MapFrom(src => src.CostReductionComments))
                .ForMember(dest => dest.drr_willthisprojectproducecobenefits, opt => opt.MapFrom(src => src.ProduceCoBenefits.HasValue ? src.ProduceCoBenefits.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_drr_application_drr_cobenefititem_Application, opt => opt.MapFrom(src => src.CoBenefits))
                .ForMember(dest => dest.drr_howwilltheprojectproducecobenefits, opt => opt.MapFrom(src => src.CoBenefitComments))
                .ForMember(dest => dest.drr_projectincreasesresiliency, opt => opt.MapFrom(src => src.IncreasedResiliency.Count() > 0 ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No))
                .ForMember(dest => dest.drr_drr_application_drr_resiliencyitem_Application, opt => opt.MapFrom(src => src.IncreasedResiliency))
                .ForMember(dest => dest.drr_resiliencycomments, opt => opt.MapFrom(src => src.IncreasedResiliencyComments))

                //Project Risks - 9
                .ForMember(dest => dest.drr_projectcomplexityrisksmitigated, opt => opt.MapFrom(src => src.ComplexityRiskMitigated.HasValue ? src.ComplexityRiskMitigated.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_drr_application_drr_projectcomplexityriskitem_Application, opt => opt.MapFrom(src => src.ComplexityRisks))
                .ForMember(dest => dest.drr_howprojectcomplexityrisksmitigated, opt => opt.MapFrom(src => src.ComplexityRiskComments))
                .ForMember(dest => dest.drr_projectreadinessrisksmitigated, opt => opt.MapFrom(src => src.ReadinessRiskMitigated.HasValue ? src.ReadinessRiskMitigated.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_drr_application_drr_projectreadinessriskitem_Application, opt => opt.MapFrom(src => src.ReadinessRisks))
                .ForMember(dest => dest.drr_howprojectreadinessrisksmitigated, opt => opt.MapFrom(src => src.ReadinessRiskComments))
                .ForMember(dest => dest.drr_projectsensitivityrisksmitigated, opt => opt.MapFrom(src => src.SensitivityRiskMitigated.HasValue ? src.SensitivityRiskMitigated.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_drr_application_drr_projectsensitivityriskitem_Application, opt => opt.MapFrom(src => src.SensitivityRisks))
                .ForMember(dest => dest.drr_howprojectsensitivityrisksmitigated, opt => opt.MapFrom(src => src.SensitivityRiskComments))
                .ForMember(dest => dest.drr_projectcapacityrisksmitigated, opt => opt.MapFrom(src => src.CapacityRiskMitigated.HasValue ? src.CapacityRiskMitigated.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_drr_application_drr_projectcapacitychallengeitem_Application, opt => opt.MapFrom(src => src.CapacityRisks))
                .ForMember(dest => dest.drr_howcapacityrisksmitigated, opt => opt.MapFrom(src => src.CapacityRiskComments))
                .ForMember(dest => dest.drr_isriskbeingincreasedortransferred, opt => opt.MapFrom(src => src.RiskTransferMigigated.HasValue ? src.RiskTransferMigigated.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_increasedortransferred, opt => opt.MapFrom(src => src.IncreasedOrTransferred.Count() > 0 ? string.Join(",", src.IncreasedOrTransferred.Select(t => (int?)Enum.Parse<IncreasedOrTransferredOptionSet>(t.ToString()))) : null))
                .ForMember(dest => dest.drr_describeriskincreasedortransferred, opt => opt.MapFrom(src => src.IncreasedOrTransferredComments))

                //Budget - 10
                .ForMember(dest => dest.drr_eligibleamount, opt => opt.MapFrom(src => src.EligibleFundingRequest))
                .ForMember(dest => dest.drr_drr_application_drr_driffundingrequest_Application, opt => opt.MapFrom(src => src.YearOverYearFunding))
                .ForMember(dest => dest.drr_totaldrifprogramfundingrequest, opt => opt.MapFrom(src => src.TotalDrifFundingRequest))
                .ForMember(dest => dest.drr_explaindiscrepancy, opt => opt.MapFrom(src => src.DiscrepancyComment))
                .ForMember(dest => dest.drr_costeffectiveness, opt => opt.MapFrom(src => src.CostEffectiveComments))
                .ForMember(dest => dest.drr_pastresponsecostprojectdesignedtomitigate, opt => opt.MapFrom(src => src.PreviousResponse.HasValue ? src.PreviousResponse == YesNoOption.No ? (int?)DRRYesNoNotApplicable.No : (int?)DRRYesNoNotApplicable.Yes : null))
                .ForMember(dest => dest.drr_costunknown, opt => opt.MapFrom(src => src.PreviousResponse.HasValue && src.PreviousResponse == YesNoOption.NotApplicable ? (int?)DRRTwoOptions.Yes : null))
                .ForMember(dest => dest.drr_cost, opt => opt.MapFrom(src => src.PreviousResponseCost))
                .ForMember(dest => dest.drr_stepstakentobecosteffective, opt => opt.MapFrom(src => src.PreviousResponseComments))
                .ForMember(dest => dest.drr_costconsiderationsapply, opt => opt.MapFrom(src => src.CostConsiderationsApplied.HasValue ? src.CostConsiderationsApplied.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_drr_application_drr_costconsiderationitem_Application, opt => opt.MapFrom(src => src.CostConsiderations))
                .ForMember(dest => dest.drr_explaincostconsiderations, opt => opt.MapFrom(src => src.CostConsiderationsComments))

                //Attachments - 11
                .ForMember(dest => dest.drr_hasresolution, opt => opt.MapFrom(src => src.HaveResolution.HasValue ? src.HaveResolution.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.bcgov_drr_application_bcgov_documenturl_Application, opt => opt.MapFrom(src => src.Attachments))

                //Review & Declaration - 12

                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.CrmId, opt => opt.MapFrom(src => src.drr_applicationid.ToString()))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.drr_name))
                .ForMember(dest => dest.FpId, opt => opt.MapFrom(src => src.drr_FullProposalApplication != null ? src.drr_FullProposalApplication.drr_name : string.Empty))
                .ForMember(dest => dest.EoiId, opt => opt.MapFrom(src => src.drr_EOIApplication != null ? src.drr_EOIApplication.drr_name : string.Empty))
                .ForMember(dest => dest.ApplicationTypeName, opt => opt.MapFrom(src => src.drr_ApplicationType.drr_name))
                .ForMember(dest => dest.ProgramName, opt => opt.MapFrom(src => src.drr_Program.drr_name))
                .ForMember(dest => dest.ProponentType, opt => opt.MapFrom(src => src.drr_primaryproponent.HasValue ? (int?)Enum.Parse<ProponentType>(((ApplicantTypeOptionSet)src.drr_primaryproponent).ToString()) : null))
                .ForMember(dest => dest.ProponentName, opt => opt.MapFrom(src => src.drr_Primary_Proponent_Name.name))
                .ForMember(dest => dest.BCeIDBusinessId, opt => opt.MapFrom(src => src.drr_Primary_Proponent_Name.drr_bceidguid))
                .ForMember(dest => dest.Submitter, opt => opt.MapFrom(src => src.drr_SubmitterContact))
                .ForMember(dest => dest.ProjectContact, opt => opt.MapFrom(src => src.drr_PrimaryProjectContact))
                .ForMember(dest => dest.AdditionalContact1, opt => opt.MapFrom(src => src.drr_AdditionalContact1))
                .ForMember(dest => dest.AdditionalContact2, opt => opt.MapFrom(src => src.drr_AdditionalContact2))
                .ForMember(dest => dest.PartneringProponents, opt => opt.MapFrom(src => src.drr_application_connections1))
                .ForMember(dest => dest.FundingStream, opt => opt.MapFrom(src => src.drr_fundingstream.HasValue ? (int?)Enum.Parse<FundingStream>(((FundingStreamOptionSet)src.drr_fundingstream).ToString()) : null))
                .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => src.drr_projecttitle))
                .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => src.drr_projecttype.HasValue ? (int?)Enum.Parse<ProjectType>(((ProjectTypeOptionSet)src.drr_projecttype).ToString()) : null))
                .ForMember(dest => dest.ScopeStatement, opt => opt.MapFrom(src => src.drr_summarizedscopestatement))
                .ForMember(dest => dest.RelatedHazards, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.drr_hazards) ? src.drr_hazards.Split(',', StringSplitOptions.None).Select(h => Enum.Parse<Hazards>(((HazardsOptionSet)int.Parse(h)).ToString()).ToString()) : null))
                .ForMember(dest => dest.OtherHazardsDescription, opt => opt.MapFrom(src => src.drr_reasonswhyotherselectedforhazards))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.drr_anticipatedprojectstartdate.HasValue ? src.drr_anticipatedprojectstartdate.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.drr_anticipatedprojectenddate.HasValue ? src.drr_anticipatedprojectenddate.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.EstimatedTotal, opt => opt.MapFrom(src => src.drr_estimated_total_project_cost))
                .ForMember(dest => dest.FundingRequest, opt => opt.MapFrom(src => src.drr_estimateddriffundingprogramrequest))
                .ForMember(dest => dest.HaveOtherFunding, opt => opt.MapFrom(src => src.drr_otherfundingsources.HasValue ? src.drr_otherfundingsources == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.OtherFunding, opt => opt.MapFrom(src => src.drr_application_fundingsource_Application))
                .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => src.drr_remaining_amount))
                .ForMember(dest => dest.IntendToSecureFunding, opt => opt.MapFrom(src => src.drr_reasonstosecurefunding))
                .ForMember(dest => dest.OwnershipDeclaration, opt => opt.MapFrom(src => src.drr_ownershipdeclaration.HasValue ? src.drr_ownershipdeclaration == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.OwnershipDescription, opt => opt.MapFrom(src => src.drr_ownershipdeclarationcontext))
                .ForMember(dest => dest.LocationDescription, opt => opt.MapFrom(src => src.drr_locationdescription))
                .ForMember(dest => dest.RationaleForFunding, opt => opt.MapFrom(src => src.drr_rationaleforfundingrequest))
                .ForMember(dest => dest.EstimatedPeopleImpacted, opt => opt.MapFrom(src => src.drr_estimatednumberpeopleimpacted.HasValue ? (int?)Enum.Parse<EstimatedNumberOfPeople>(((EstimatedNumberOfPeopleOptionSet)src.drr_estimatednumberpeopleimpacted).ToString()) : null))
                .ForMember(dest => dest.CommunityImpact, opt => opt.MapFrom(src => src.drr_impacttocommunity))
                .ForMember(dest => dest.InfrastructureImpacted, opt => opt.MapFrom(src => src.drr_drr_application_drr_criticalinfrastructureimpacted_Application.Where(c => c.statecode == (int)EntityState.Active)))
                .ForMember(dest => dest.DisasterRiskUnderstanding, opt => opt.MapFrom(src => src.drr_improveunderstandingriskinvestreduction))
                .ForMember(dest => dest.AddressRisksAndHazards, opt => opt.MapFrom(src => src.drr_includedtoaddressidentifiedriskhazards))
                .ForMember(dest => dest.ProjectDescription, opt => opt.MapFrom(src => src.drr_howdoesprojectalignwithdrifsprogramgoals))
                .ForMember(dest => dest.AdditionalBackgroundInformation, opt => opt.MapFrom(src => src.drr_additionalrelevantinformation1))
                .ForMember(dest => dest.RationaleForSolution, opt => opt.MapFrom(src => src.drr_rationalforproposedsolution))
                .ForMember(dest => dest.FirstNationsEngagement, opt => opt.MapFrom(src => src.drr_engagementwithfirstnationsorindigenousorg))
                .ForMember(dest => dest.NeighbourEngagement, opt => opt.MapFrom(src => src.drr_plantoengageotherjurisdictionsandparties))
                .ForMember(dest => dest.AdditionalSolutionInformation, opt => opt.MapFrom(src => src.drr_additionalrelevantinformation2))
                .ForMember(dest => dest.AdditionalEngagementInformation, opt => opt.MapFrom(src => src.drr_additionalrelevantinformation3))
                .ForMember(dest => dest.ClimateAdaptation, opt => opt.MapFrom(src => src.drr_climateadaptation))
                .ForMember(dest => dest.OtherInformation, opt => opt.MapFrom(src => src.drr_otherrelevantinformation))
                .ForMember(dest => dest.AuthorizedRepresentativeStatement, opt => opt.MapFrom(src => src.drr_authorizedrepresentative == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.FOIPPAConfirmation, opt => opt.MapFrom(src => src.drr_foippaconfirmation == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.InformationAccuracyStatement, opt => opt.MapFrom(src => src.drr_accuracyofinformation == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.SubmittedDate, opt => opt.MapFrom(src => src.drr_submitteddate.HasValue ? src.drr_submitteddate.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.ModifiedOn, opt => opt.MapFrom(src => src.modifiedon.HasValue ? src.modifiedon.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<ApplicationStatus>(((ApplicationStatusOptionSet)src.statuscode).ToString())))

                //FP
                //Proponent & Project Information - 1
                .ForMember(dest => dest.RegionalProject, opt => opt.MapFrom(src => src.drr_isthisaregionalproject.HasValue ? src.drr_isthisaregionalproject.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.RegionalProjectComments, opt => opt.MapFrom(src => src.drr_isthisaregionalprojectcomments))
                .ForMember(dest => dest.MainDeliverable, opt => opt.MapFrom(src => src.drr_projecttypefullproposal))
                //Ownership & Authorization - 2
                .ForMember(dest => dest.HaveAuthorityToDevelop, opt => opt.MapFrom(src => src.drr_proponenthastheauthorityandownership.HasValue ? src.drr_proponenthastheauthorityandownership.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.OperationAndMaintenance, opt => opt.MapFrom(src => src.drr_proponenttomaintaininfrastructurelongterm.HasValue ? (int?)Enum.Parse<YesNoOption>(((DRRYesNoNotApplicable)src.drr_proponenttomaintaininfrastructurelongterm).ToString()) : null))
                .ForMember(dest => dest.OperationAndMaintenanceComments, opt => opt.MapFrom(src => src.drr_commentsprojectauthority))
                .ForMember(dest => dest.FirstNationsAuthorizedByPartners, opt => opt.MapFrom(src => src.drr_authorizedendorsedfirstnationpartners.HasValue ? (int?)Enum.Parse<YesNoOption>(((DRRYesNoNotApplicable)src.drr_authorizedendorsedfirstnationpartners).ToString()) : null))
                .ForMember(dest => dest.LocalGovernmentAuthorizedByPartners, opt => opt.MapFrom(src => src.drr_authorizedendorsedlocalgovpartners.HasValue ? (int?)Enum.Parse<YesNoOption>(((DRRYesNoNotApplicable)src.drr_authorizedendorsedlocalgovpartners).ToString()) : null))
                .ForMember(dest => dest.AuthorizationOrEndorsementComments, opt => opt.MapFrom(src => src.drr_authorizationorendorsementcomments))
                //Project Area - 3
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.drr_estimatedsizeofprojectarea))
                .ForMember(dest => dest.Units, opt => opt.MapFrom(src => src.drr_unitofmeasure.HasValue ? (int?)Enum.Parse<AreaUnits>(((AreaUnitsOptionSet)src.drr_unitofmeasure).ToString()) : null))
                .ForMember(dest => dest.AreaDescription, opt => opt.MapFrom(src => src.drr_projectarea))
                .ForMember(dest => dest.IsInfrastructureImpacted, opt => opt.MapFrom(src => src.drr_criticalinfrastructurewillormaybeimpacted.HasValue ? src.drr_criticalinfrastructurewillormaybeimpacted.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.EstimatedPeopleImpactedFP, opt => opt.MapFrom(src => src.drr_estimatednumberofpeopleimpactedfp.HasValue ? (int?)Enum.Parse<EstimatedNumberOfPeopleFP>(((EstimatedNumberOfPeopleFPOptionSet)src.drr_estimatednumberofpeopleimpactedfp).ToString()) : null))
                //Project Plan - 4
                .ForMember(dest => dest.ProposedActivities, opt => opt.MapFrom(src => src.drr_drr_application_drr_proposedactivity_Application))
                .ForMember(dest => dest.FoundationalOrPreviousWorks, opt => opt.MapFrom(src => src.drr_drr_application_drr_foundationalorpreviousworkitem_Application))
                .ForMember(dest => dest.HowWasNeedIdentified, opt => opt.MapFrom(src => src.drr_howwastheneedfortheprojectidentified))
                .ForMember(dest => dest.ProjectAlternateOptions, opt => opt.MapFrom(src => src.drr_extentalternateprojectoptionsconsidered))
                //Project Engagement - 5
                .ForMember(dest => dest.EngagedWithFirstNationsOccurred, opt => opt.MapFrom(src => src.drr_meaningfullyengagedwithlocalfirstnations.HasValue ? src.drr_meaningfullyengagedwithlocalfirstnations.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.EngagedWithFirstNationsComments, opt => opt.MapFrom(src => src.drr_describeengagementfirstnations))
                .ForMember(dest => dest.OtherEngagement, opt => opt.MapFrom(src => src.drr_effectivelyengagedwithothers.HasValue ? (int?)Enum.Parse<YesNoOption>(((DRRYesNoNotApplicable)src.drr_effectivelyengagedwithothers).ToString()) : null))
                .ForMember(dest => dest.AffectedParties, opt => opt.MapFrom(src => src.drr_drr_application_drr_impactedoraffectedpartyitem_Application))
                .ForMember(dest => dest.OtherEngagementComments, opt => opt.MapFrom(src => src.drr_describeengagementimpactedaffectedparties))
                .ForMember(dest => dest.CollaborationComments, opt => opt.MapFrom(src => src.drr_howprojectcontributetocollaboration))
                //Climate Adaptation - 6
                .ForMember(dest => dest.IncorporateFutureClimateConditions, opt => opt.MapFrom(src => src.drr_doesprojectconsiderclimatechange.HasValue ? src.drr_doesprojectconsiderclimatechange.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.ClimateAssessment, opt => opt.MapFrom(src => src.drr_hasprojectusedclimatechangeassessmenttool.HasValue ? src.drr_hasprojectusedclimatechangeassessmenttool.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.ClimateAssessmentTools, opt => opt.MapFrom(src => src.drr_drr_application_drr_climateassessmenttoolitem_Application))
                .ForMember(dest => dest.ClimateAssessmentComments, opt => opt.MapFrom(src => src.drr_howtoolsusedtobenefitproject))
                //Permits Regulations & Standards - 7
                .ForMember(dest => dest.Permits, opt => opt.MapFrom(src => src.drr_drr_application_drr_permitslicensesandauthorizations_Application))
                .ForMember(dest => dest.StandardsAcceptable, opt => opt.MapFrom(src => src.drr_acceptableprovincialstandards.HasValue ? (int?)Enum.Parse<YesNoOption>(((DRRYesNoNotApplicable)src.drr_acceptableprovincialstandards).ToString()) : null))
                .ForMember(dest => dest.Standards, opt => opt.MapFrom(src => DRRStandardInfoMapper(src, src.drr_drr_application_drr_provincialstandarditem_Application)))
                .ForMember(dest => dest.StandardsComments, opt => opt.MapFrom(src => src.drr_commentsacceptableprovincialstandards))
                .ForMember(dest => dest.ProfessionalGuidance, opt => opt.MapFrom(src => src.drr_guidanceofqualifiedprofessional.HasValue ? src.drr_guidanceofqualifiedprofessional.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.Professionals, opt => opt.MapFrom(src => src.drr_drr_application_drr_qualifiedprofessionalitem_Application))
                .ForMember(dest => dest.ProfessionalGuidanceComments, opt => opt.MapFrom(src => src.drr_qualifiedprofessionalcomments))
                .ForMember(dest => dest.KnowledgeHolders, opt => opt.MapFrom(src => src.drr_additionaloversightadviceandorexpertise))
                .ForMember(dest => dest.MeetsRegulatoryRequirements, opt => opt.MapFrom(src => src.drr_requiredagencydiscussionsandapprovals.HasValue ? src.drr_requiredagencydiscussionsandapprovals.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.MeetsRegulatoryComments, opt => opt.MapFrom(src => src.drr_commentsrequiredagencydiscussionsapproval))
                .ForMember(dest => dest.MeetsEligibilityRequirements, opt => opt.MapFrom(src => src.drr_projecteligibleforrequiredpermitsapproval.HasValue ? src.drr_projecteligibleforrequiredpermitsapproval.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.MeetsEligibilityComments, opt => opt.MapFrom(src => src.drr_projecteligibleforpermitsapprovalcomments))
                //Project Outcomes - 8
                .ForMember(dest => dest.PublicBenefit, opt => opt.MapFrom(src => src.drr_projectforbroadpublicuseorbenefit.HasValue ? src.drr_projectforbroadpublicuseorbenefit.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.PublicBenefitComments, opt => opt.MapFrom(src => src.drr_projectforbroadpublicuseorbenefitcomments))
                .ForMember(dest => dest.FutureCostReduction, opt => opt.MapFrom(src => src.drr_willthisprojectreducecosts.HasValue ? src.drr_willthisprojectreducecosts.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.CostReductions, opt => opt.MapFrom(src => src.drr_drr_application_drr_costreductionitem_Application))
                .ForMember(dest => dest.CostReductionComments, opt => opt.MapFrom(src => src.drr_explainhowcostswillbereduced))
                .ForMember(dest => dest.ProduceCoBenefits, opt => opt.MapFrom(src => src.drr_willthisprojectproducecobenefits.HasValue ? src.drr_willthisprojectproducecobenefits.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.CoBenefits, opt => opt.MapFrom(src => src.drr_drr_application_drr_cobenefititem_Application))
                .ForMember(dest => dest.CoBenefitComments, opt => opt.MapFrom(src => src.drr_howwilltheprojectproducecobenefits))
                .ForMember(dest => dest.IncreasedResiliency, opt => opt.MapFrom(src => src.drr_drr_application_drr_resiliencyitem_Application))
                .ForMember(dest => dest.IncreasedResiliencyComments, opt => opt.MapFrom(src => src.drr_resiliencycomments))
                //Project Risks - 9
                .ForMember(dest => dest.ComplexityRiskMitigated, opt => opt.MapFrom(src => src.drr_projectcomplexityrisksmitigated.HasValue ? src.drr_projectcomplexityrisksmitigated.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.ComplexityRisks, opt => opt.MapFrom(src => src.drr_drr_application_drr_projectcomplexityriskitem_Application))
                .ForMember(dest => dest.ComplexityRiskComments, opt => opt.MapFrom(src => src.drr_howprojectcomplexityrisksmitigated))
                .ForMember(dest => dest.ReadinessRiskMitigated, opt => opt.MapFrom(src => src.drr_projectreadinessrisksmitigated.HasValue ? src.drr_projectreadinessrisksmitigated.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.ReadinessRisks, opt => opt.MapFrom(src => src.drr_drr_application_drr_projectreadinessriskitem_Application))
                .ForMember(dest => dest.ReadinessRiskComments, opt => opt.MapFrom(src => src.drr_howprojectreadinessrisksmitigated))
                .ForMember(dest => dest.SensitivityRiskMitigated, opt => opt.MapFrom(src => src.drr_projectsensitivityrisksmitigated.HasValue ? src.drr_projectsensitivityrisksmitigated.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.SensitivityRisks, opt => opt.MapFrom(src => src.drr_drr_application_drr_projectsensitivityriskitem_Application))
                .ForMember(dest => dest.SensitivityRiskComments, opt => opt.MapFrom(src => src.drr_howprojectsensitivityrisksmitigated))
                .ForMember(dest => dest.CapacityRiskMitigated, opt => opt.MapFrom(src => src.drr_projectcapacityrisksmitigated.HasValue ? src.drr_projectcapacityrisksmitigated.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.CapacityRisks, opt => opt.MapFrom(src => src.drr_drr_application_drr_projectcapacitychallengeitem_Application))
                .ForMember(dest => dest.CapacityRiskComments, opt => opt.MapFrom(src => src.drr_howcapacityrisksmitigated))
                .ForMember(dest => dest.RiskTransferMigigated, opt => opt.MapFrom(src => src.drr_isriskbeingincreasedortransferred.HasValue ? src.drr_isriskbeingincreasedortransferred.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.IncreasedOrTransferred, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.drr_increasedortransferred) ? src.drr_increasedortransferred.Split(',', StringSplitOptions.None).Select(h => Enum.Parse<IncreasedOrTransferred>(((IncreasedOrTransferredOptionSet)int.Parse(h)).ToString()).ToString()) : null))
                .ForMember(dest => dest.IncreasedOrTransferredComments, opt => opt.MapFrom(src => src.drr_describeriskincreasedortransferred))
                //Budget - 10
                .ForMember(dest => dest.EligibleFundingRequest, opt => opt.MapFrom(src => src.drr_eligibleamount))
                .ForMember(dest => dest.EligibleAmountForFP, opt => opt.MapFrom(src => src.drr_eligibleamountfullproposal))
                .ForMember(dest => dest.YearOverYearFunding, opt => opt.MapFrom(src => src.drr_drr_application_drr_driffundingrequest_Application))
                .ForMember(dest => dest.TotalDrifFundingRequest, opt => opt.MapFrom(src => src.drr_totaldrifprogramfundingrequest))
                .ForMember(dest => dest.DiscrepancyComment, opt => opt.MapFrom(src => src.drr_explaindiscrepancy))
                .ForMember(dest => dest.CostEffectiveComments, opt => opt.MapFrom(src => src.drr_costeffectiveness))
                .ForMember(dest => dest.PreviousResponse, opt => opt.MapFrom(src => src.drr_pastresponsecostprojectdesignedtomitigate.HasValue ? PreviousResponseCostMapper(src) : null))
                .ForMember(dest => dest.PreviousResponseCost, opt => opt.MapFrom(src => src.drr_cost))
                .ForMember(dest => dest.PreviousResponseComments, opt => opt.MapFrom(src => src.drr_stepstakentobecosteffective))
                .ForMember(dest => dest.CostConsiderationsApplied, opt => opt.MapFrom(src => src.drr_costconsiderationsapply.HasValue ? src.drr_costconsiderationsapply.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.CostConsiderations, opt => opt.MapFrom(src => src.drr_drr_application_drr_costconsiderationitem_Application))
                .ForMember(dest => dest.CostConsiderationsComments, opt => opt.MapFrom(src => src.drr_explaincostconsiderations))

                //Attachments
                .ForMember(dest => dest.HaveResolution, opt => opt.MapFrom(src => src.drr_hasresolution.HasValue ? src.drr_hasresolution.Value == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.bcgov_drr_application_bcgov_documenturl_Application))
            ;


            CreateMap<FundingInformation, drr_fundingsource>(MemberList.None)
                .ForMember(dest => dest.drr_name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.drr_typeoffunding, opt => opt.MapFrom(src => src.Type.HasValue ? (int?)Enum.Parse<FundingTypeOptionSet>(src.Type.Value.ToString()) : null))
                .ForMember(dest => dest.drr_estimated_amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.drr_describethefundingsource, opt => opt.MapFrom(src => src.OtherDescription))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_name))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.drr_typeoffunding.HasValue ? (int?)Enum.Parse<FundingType>(((FundingTypeOptionSet)src.drr_typeoffunding).ToString()) : null))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.drr_estimated_amount))
                .ForMember(dest => dest.OtherDescription, opt => opt.MapFrom(src => src.drr_describethefundingsource))
            ;

            CreateMap<ContactDetails, contact>(MemberList.None)
                .ForMember(dest => dest.drr_userid, opt => opt.MapFrom(src => src.BCeId))
                .ForMember(dest => dest.firstname, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.lastname, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.jobtitle, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.department, opt => opt.MapFrom(src => src.Department))
                .ForMember(dest => dest.drr_phonenumber, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.emailaddress1, opt => opt.MapFrom(src => src.Email))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.BCeId, opt => opt.MapFrom(src => src.drr_userid))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.firstname))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.lastname))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.jobtitle))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.department))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.drr_phonenumber))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.emailaddress1))
            ;

            CreateMap<PartneringProponent, account>(MemberList.None)
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.Name))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
            ;

            CreateMap<connection, PartneringProponent>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
            ;

            CreateMap<Permit, drr_permitslicensesandauthorizations>(MemberList.None)
                .ForMember(dest => dest.drr_name, opt => opt.MapFrom(src => src.Name))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_name))
            ;

            CreateMap<CriticalInfrastructure, drr_criticalinfrastructureimpacted>(MemberList.None)
                .ForMember(dest => dest.drr_name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.drr_impacttothatinfrastructure, opt => opt.MapFrom(src => src.Impact))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_name))
                .ForMember(dest => dest.Impact, opt => opt.MapFrom(src => src.drr_impacttothatinfrastructure))
                ;

            CreateMap<drr_legaldeclaration, DeclarationInfo>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<DeclarationTypeOptionSet>(((DeclarationTypeOptionSet)src.drr_declarationtype).ToString())))
                .ForMember(dest => dest.ApplicationTypeName, opt => opt.MapFrom(src => src.drr_ApplicationType.drr_name))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.drr_declarationtext));

            CreateMap<ProfessionalInfo, drr_qualifiedprofessionalitem>(MemberList.None)
                .ForMember(dest => dest.drr_QualifiedProfessional, opt => opt.MapFrom(src => new drr_qualifiedprofessional { drr_name = src.Name }))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_QualifiedProfessional.drr_name != "Other" || string.IsNullOrEmpty(src.drr_qualifiedprofessionalcomments) ? src.drr_QualifiedProfessional.drr_name : src.drr_qualifiedprofessionalcomments))
            ;

            CreateMap<ProposedActivity, drr_proposedactivity>(MemberList.None)
                .ForMember(dest => dest.drr_name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.drr_Activity, opt => opt.MapFrom(src => new drr_projectactivity { drr_name = src.Name }))
                .ForMember(dest => dest.drr_anticipatedstartdate, opt => opt.MapFrom(src => src.StartDate.HasValue ? src.StartDate.Value.ToUniversalTime() : (DateTimeOffset?)null))
                .ForMember(dest => dest.drr_anticipatedenddate, opt => opt.MapFrom(src => src.EndDate.HasValue ? src.EndDate.Value.ToUniversalTime() : (DateTimeOffset?)null))
                .ForMember(dest => dest.drr_relatedtasks, opt => opt.MapFrom(src => src.Tasks))
                .ForMember(dest => dest.drr_deliverablesproducts, opt => opt.MapFrom(src => src.Deliverables))
                .ForMember(dest => dest.drr_activitynumber, opt => opt.MapFrom(src => src.ActivityNumber))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_name))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.drr_anticipatedstartdate.HasValue ? src.drr_anticipatedstartdate.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.drr_anticipatedenddate.HasValue ? src.drr_anticipatedenddate.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.Deliverables, opt => opt.MapFrom(src => src.drr_deliverablesproducts))
                .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.drr_relatedtasks))
            ;

            CreateMap<ProvincialStandard, drr_provincialstandarditem>(MemberList.None)
                .ForMember(dest => dest.drr_ProvincialStandard, opt => opt.MapFrom(src => new drr_provincialstandard { drr_name = src.Name }))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_ProvincialStandard.drr_name != "Other" || string.IsNullOrEmpty(src.drr_provincialstandarditemcomments) ? src.drr_ProvincialStandard.drr_name : src.drr_provincialstandarditemcomments))
            ;

            CreateMap<AffectedParty, drr_impactedoraffectedpartyitem>(MemberList.None)
               .ForMember(dest => dest.drr_ImpactedorAffectedParty, opt => opt.MapFrom(src => new drr_impactedoraffectedparty { drr_name = src.Name }))
               .ReverseMap()
               .ValidateMemberList(MemberList.Destination)
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_ImpactedorAffectedParty.drr_name != "Other" || string.IsNullOrEmpty(src.drr_impactedoraffectedpartycomments) ? src.drr_ImpactedorAffectedParty.drr_name : src.drr_impactedoraffectedpartycomments))
            ;

            CreateMap<FoundationalOrPreviousWork, drr_foundationalorpreviousworkitem>(MemberList.None)
               .ForMember(dest => dest.drr_FoundationalorPreviousWork, opt => opt.MapFrom(src => new drr_foundationalorpreviouswork { drr_name = src.Name }))
               .ReverseMap()
               .ValidateMemberList(MemberList.Destination)
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_FoundationalorPreviousWork.drr_name != "Other" || string.IsNullOrEmpty(src.drr_foundationalorpreviousworkcomments) ? src.drr_FoundationalorPreviousWork.drr_name : src.drr_foundationalorpreviousworkcomments))
            ;

            CreateMap<CostReduction, drr_costreductionitem>(MemberList.None)
               .ForMember(dest => dest.drr_CostReduction, opt => opt.MapFrom(src => new drr_costreduction { drr_name = src.Name }))
               .ReverseMap()
               .ValidateMemberList(MemberList.Destination)
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_CostReduction.drr_name != "Other" || string.IsNullOrEmpty(src.drr_costreductionitemcomments) ? src.drr_CostReduction.drr_name : src.drr_costreductionitemcomments))
            ;

            CreateMap<CoBenefit, drr_cobenefititem>(MemberList.None)
               .ForMember(dest => dest.drr_CoBenefit, opt => opt.MapFrom(src => new drr_cobenefit { drr_name = src.Name }))
               .ReverseMap()
               .ValidateMemberList(MemberList.Destination)
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_CoBenefit.drr_name != "Other" || string.IsNullOrEmpty(src.drr_cobenefitcomments) ? src.drr_CoBenefit.drr_name : src.drr_cobenefitcomments))
            ;

            CreateMap<ComplexityRisk, drr_projectcomplexityriskitem>(MemberList.None)
               .ForMember(dest => dest.drr_ProjectComplexityRisk, opt => opt.MapFrom(src => new drr_projectcomplexityrisk { drr_name = src.Name }))
               .ReverseMap()
               .ValidateMemberList(MemberList.Destination)
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_ProjectComplexityRisk.drr_name != "Other" || string.IsNullOrEmpty(src.drr_projectcomplexityriskitemcomments) ? src.drr_ProjectComplexityRisk.drr_name : src.drr_projectcomplexityriskitemcomments))
            ;

            CreateMap<ReadinessRisk, drr_projectreadinessriskitem>(MemberList.None)
               .ForMember(dest => dest.drr_ProjectReadinessRisk, opt => opt.MapFrom(src => new drr_projectreadinessrisk { drr_name = src.Name }))
               .ReverseMap()
               .ValidateMemberList(MemberList.Destination)
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_ProjectReadinessRisk.drr_name != "Other" || string.IsNullOrEmpty(src.drr_projectreadinessriskcomments) ? src.drr_ProjectReadinessRisk.drr_name : src.drr_projectreadinessriskcomments))
            ;

            CreateMap<SensitivityRisk, drr_projectsensitivityriskitem>(MemberList.None)
              .ForMember(dest => dest.drr_ProjectSensitivityRisk, opt => opt.MapFrom(src => new drr_projectsensitivityrisk { drr_name = src.Name }))
              .ReverseMap()
              .ValidateMemberList(MemberList.Destination)
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_ProjectSensitivityRisk.drr_name != "Other" || string.IsNullOrEmpty(src.drr_projectsensitivityriskcomments) ? src.drr_ProjectSensitivityRisk.drr_name : src.drr_projectsensitivityriskcomments))
            ;

            CreateMap<CapacityRisk, drr_projectcapacitychallengeitem>(MemberList.None)
              .ForMember(dest => dest.drr_ProjectCapacityChallenge, opt => opt.MapFrom(src => new drr_projectcapacitychallenge { drr_name = src.Name }))
              .ReverseMap()
              .ValidateMemberList(MemberList.Destination)
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_ProjectCapacityChallenge.drr_name != "Other" || string.IsNullOrEmpty(src.drr_projectcapacitychallengecomments) ? src.drr_ProjectCapacityChallenge.drr_name : src.drr_projectcapacitychallengecomments))
            ;

            CreateMap<ClimateAssessmentToolsInfo, drr_climateassessmenttoolitem>(MemberList.None)
              .ForMember(dest => dest.drr_ClimateAssessmentTool, opt => opt.MapFrom(src => new drr_climateassessmenttool { drr_name = src.Name }))
              .ReverseMap()
              .ValidateMemberList(MemberList.Destination)
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_ClimateAssessmentTool.drr_name != "Other" || string.IsNullOrEmpty(src.drr_climateassessmmenttoolcomments) ? src.drr_ClimateAssessmentTool.drr_name : src.drr_climateassessmmenttoolcomments))
            ;

            CreateMap<IncreasedResiliency, drr_resiliencyitem>(MemberList.None)
              .ForMember(dest => dest.drr_Resiliency, opt => opt.MapFrom(src => new drr_resiliency { drr_name = src.Name }))
              .ReverseMap()
              .ValidateMemberList(MemberList.Destination)
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_Resiliency.drr_name != "Other" || string.IsNullOrEmpty(src.drr_resiliencycomments) ? src.drr_Resiliency.drr_name : src.drr_resiliencycomments))
            ;

            CreateMap<CostConsideration, drr_costconsiderationitem>(MemberList.None)
              .ForMember(dest => dest.drr_CostConsideration, opt => opt.MapFrom(src => new drr_costconsideration { drr_name = src.Name }))
              .ReverseMap()
              .ValidateMemberList(MemberList.Destination)
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_CostConsideration.drr_name != "Other" || string.IsNullOrEmpty(src.drr_costconsiderationcomments) ? src.drr_CostConsideration.drr_name : src.drr_costconsiderationcomments))
            ;

            CreateMap<YearOverYearFunding, drr_driffundingrequest>(MemberList.None)
                .ForMember(dest => dest.drr_FiscalYear, opt => opt.MapFrom(src => new drr_fiscalyear { drr_name = src.Year }))
                .ForMember(dest => dest.drr_drifprogramfundingrequest, opt => opt.MapFrom(src => src.Amount))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.drr_FiscalYear.drr_name))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.drr_drifprogramfundingrequest))
                ;

            CreateMap<BcGovDocument, bcgov_documenturl>(MemberList.None)
                .ForMember(dest => dest.bcgov_documenturlid, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
                .ForMember(dest => dest.bcgov_filename, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.bcgov_DocumentType, opt => opt.MapFrom(src => new bcgov_documenttype { bcgov_name = src.DocumentType.ToDescriptionString() }))
                .ForMember(dest => dest.bcgov_documentcomments, opt => opt.MapFrom(src => src.Comments))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.bcgov_documenturlid.ToString()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.bcgov_filename))
                .ForMember(dest => dest.DocumentType, opt => opt.MapFrom(src => IEnumEx.GetValueFromDescription<DocumentType>(src.bcgov_DocumentType != null ? src.bcgov_DocumentType.bcgov_name : DocumentType.OtherSupportingDocument.ToDescriptionString())))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.bcgov_documentcomments))
            ;
        }

        private YesNoOption? PreviousResponseCostMapper(drr_application application)
        {
            if (application.drr_pastresponsecostprojectdesignedtomitigate == (int)DRRYesNoNotApplicable.Yes)
            {
                if (application.drr_costunknown == (int)DRRTwoOptions.Yes) return YesNoOption.NotApplicable;
                else return YesNoOption.Yes;
            }
            return YesNoOption.No;
        }

        private int? DRRCategorySelectedMapper(StandardInfo? standardInfo)
        {
            if (standardInfo == null) return (int?)DRRTwoOptions.No;
            return standardInfo.IsCategorySelected.HasValue && standardInfo.IsCategorySelected.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No;
        }

        private IEnumerable<drr_provincialstandarditem> DRRProvincialStandardItemMapper(IEnumerable<StandardInfo> standardInfo)
        {
            var ret = new List<drr_provincialstandarditem>();
            if (standardInfo == null) return ret;
            foreach (var info in standardInfo)
            {
                foreach (var standard in info.Standards)
                {
                    ret.Add(new drr_provincialstandarditem
                    {
                        drr_ProvincialStandard = new drr_provincialstandard { drr_name = standard.Name },
                        drr_ProvincialStandardCategory = new drr_provincialstandardcategory { drr_name = info.Category }
                    });
                }
            }
            return ret;
        }

        private IEnumerable<StandardInfo> DRRStandardInfoMapper(drr_application application, IEnumerable<drr_provincialstandarditem> standardItems)
        {
            var ret = new List<StandardInfo>();
            var categories = standardItems.Select(s => s.drr_ProvincialStandardCategory.drr_name).ToList().Distinct();

            foreach (var category in categories)
            {
                var standards = standardItems.Where(i => i.drr_ProvincialStandardCategory.drr_name == category).Select(i => new ProvincialStandard { Name = i.drr_ProvincialStandard.drr_name != "Other" || string.IsNullOrEmpty(i.drr_provincialstandarditemcomments) ? i.drr_ProvincialStandard.drr_name : i.drr_provincialstandarditemcomments }).ToList();
                ret.Add(new StandardInfo
                {
                    IsCategorySelected = GetBoolFromCategory(application, category),
                    Category = category,
                    Standards = standards
                });
            }

            foreach (var categoryName in CategoryNames)
            {
                var curr = ret.SingleOrDefault(r => r.Category == categoryName);
                if (curr == null)
                {
                    ret.Add(new StandardInfo { Category = categoryName, IsCategorySelected = false, Standards = Array.Empty<ProvincialStandard>() });
                }
            }
            return ret;
        }

        private IEnumerable<string> CategoryNames = new[]
        {
            "Archaeology",
            "Environment - Mapping and Landscape",
            "Environment - Seismic",
            "Environment - Water (includes Rivers, Flooding, etc.)",
            "Financial",
            "Other"
        };

        private bool? GetBoolFromCategory(drr_application application, string categoryName)
        {
            switch (categoryName)
            {
                case "Archaeology": return DRRTwoOptionMapper(application.drr_archaeology);
                case "Environment - Mapping and Landscape": return DRRTwoOptionMapper(application.drr_environmentmappingandlandscape);
                case "Environment - Seismic": return DRRTwoOptionMapper(application.drr_environmentseismic);
                case "Environment - Water (includes Rivers, Flooding, etc.)": return DRRTwoOptionMapper(application.drr_environmentwater);
                case "Financial": return DRRTwoOptionMapper(application.drr_financial);
                case "Other": return DRRTwoOptionMapper(application.drr_othercategory);
            }
            return false;
        }

        private bool? DRRTwoOptionMapper(int? val)
        {
            return val.HasValue ? val.Value == (int)DRRTwoOptions.Yes : (bool?)null;
        }
    }
}
#pragma warning restore CS8629 // Nullable value type may be null.
