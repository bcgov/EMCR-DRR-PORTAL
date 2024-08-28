using AutoMapper;
using EMCR.DRR.Managers.Intake;
using Microsoft.Dynamics.CRM;

namespace EMCR.DRR.Resources.Applications
{
    public class ApplicationMapperProfile : Profile
    {
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
                .ForMember(dest => dest.drr_drr_application_drr_criticalinfrastructureimpacted_Application, opt => opt.MapFrom(src => src.InfrastructureImpacted))
                .ForMember(dest => dest.drr_improveunderstandingriskinvestreduction, opt => opt.MapFrom(src => src.DisasterRiskUnderstanding))
                .ForMember(dest => dest.drr_includedtoaddressidentifiedriskhazards, opt => opt.MapFrom(src => src.AddressRisksAndHazards))
                .ForMember(dest => dest.drr_howdoesprojectalignwithdrifsprogramgoals, opt => opt.MapFrom(src => src.DRIFProgramGoalAlignment))
                .ForMember(dest => dest.drr_additionalrelevantinformation1, opt => opt.MapFrom(src => src.AdditionalBackgroundInformation))
                .ForMember(dest => dest.drr_rationalforproposedsolution, opt => opt.MapFrom(src => src.RationaleForSolution))
                .ForMember(dest => dest.drr_engagementwithfirstnationsorindigenousorg, opt => opt.MapFrom(src => src.FirstNationsEngagement))
                .ForMember(dest => dest.drr_plantoengageotherjurisdictionsandparties, opt => opt.MapFrom(src => src.NeighbourEngagement))
                .ForMember(dest => dest.drr_additionalrelevantinformation2, opt => opt.MapFrom(src => src.AdditionalSolutionInformation))
                .ForMember(dest => dest.drr_additionalrelevantinformation3, opt => opt.MapFrom(src => src.AdditionalEngagementInformation))
                .ForMember(dest => dest.drr_climateadaptation, opt => opt.MapFrom(src => src.ClimateAdaptation))
                .ForMember(dest => dest.drr_otherrelevantinformation, opt => opt.MapFrom(src => src.OtherInformation))
                .ForMember(dest => dest.drr_authorizedrepresentative, opt => opt.MapFrom(src => src.AuthorizedRepresentativeStatement.HasValue && src.AuthorizedRepresentativeStatement.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_foippaconfirmation, opt => opt.MapFrom(src => src.FOIPPAConfirmation.HasValue && src.FOIPPAConfirmation.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_accuracyofinformation, opt => opt.MapFrom(src => src.InformationAccuracyStatement.HasValue && src.InformationAccuracyStatement.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_submitteddate, opt => opt.MapFrom(src => src.SubmittedDate))
                .ForMember(dest => dest.statuscode, opt => opt.MapFrom(src => (int?)Enum.Parse<ApplicationStatusOptionSet>(src.Status.ToString())))
                //FP Only Fields
                //Proponent & Project Information - 1
                .ForMember(dest => dest.drr_isthisaregionalproject, opt => opt.MapFrom(src => src.RegionalProject.HasValue && src.RegionalProject.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_isthisaregionalprojectcomments, opt => opt.MapFrom(src => src.RegionalProjectComments))
                //Ownership & Authorization - 2
                .ForMember(dest => dest.drr_proponenthastheauthorityandownership, opt => opt.MapFrom(src => src.ProjectAuthority.HasValue && src.ProjectAuthority.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_commentsproponenthastheauthorityandowners, opt => opt.MapFrom(src => src.ProjectAuthorityComments))
                .ForMember(dest => dest.drr_proponenttomaintaininfrastructurelongterm, opt => opt.MapFrom(src => src.OperationAndMaintenance.HasValue ? (int?)Enum.Parse<DRRYesNoNotApplicable>(src.OperationAndMaintenance.Value.ToString()) : null))
                .ForMember(dest => dest.drr_proponenttomaintaininfrastructurecomments, opt => opt.MapFrom(src => src.OperationAndMaintenanceComments))
                .ForMember(dest => dest.drr_authorizedendorsedfirstnationpartners, opt => opt.MapFrom(src => src.FirstNationsEndorsement.HasValue ? (int?)Enum.Parse<DRRYesNoNotApplicable>(src.FirstNationsEndorsement.Value.ToString()) : null))
                .ForMember(dest => dest.drr_authorizedendorsedlocalgovpartners, opt => opt.MapFrom(src => src.LocalGovernmentEndorsement.HasValue ? (int?)Enum.Parse<DRRYesNoNotApplicable>(src.LocalGovernmentEndorsement.Value.ToString()) : null))
                .ForMember(dest => dest.drr_authorizationorendorsementcomments, opt => opt.MapFrom(src => src.AuthorizationOrEndorsementComments))
                //Project Area - 3 - intentionally blank

                //Project Plan - 4
                //.ForMember(dest => dest.drr_explainneedforproject, opt => opt.MapFrom(src => src.ProjectDescription))
                .ForMember(dest => dest.drr_drr_application_drr_proposedactivity_Application, opt => opt.MapFrom(src => src.ProposedActivities))
                .ForMember(dest => dest.drr_drr_application_drr_projectneedidentificationitem_Application, opt => opt.MapFrom(src => src.VerificationMethods))
                .ForMember(dest => dest.drr_explainneedforproject, opt => opt.MapFrom(src => src.VerificationMethodsComments))
                .ForMember(dest => dest.drr_extentalternateprojectoptionsconsidered, opt => opt.MapFrom(src => src.ProjectAlternateOptions))

                //Project Engagement - 5
                .ForMember(dest => dest.drr_meaningfullyengagedwithlocalfirstnations, opt => opt.MapFrom(src => src.EngagedWithFirstNations.HasValue && src.EngagedWithFirstNations.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_describeengagementfirstnations, opt => opt.MapFrom(src => src.EngagedWithFirstNationsComments))
                .ForMember(dest => dest.drr_effectivelyengagedwithothers, opt => opt.MapFrom(src => src.OtherEngagement.HasValue ? (int?)Enum.Parse<DRRYesNoNotApplicable>(src.OtherEngagement.Value.ToString()) : null))
                .ForMember(dest => dest.drr_drr_application_drr_impactedoraffectedpartyitem_Application, opt => opt.MapFrom(src => src.AffectedParties))
                .ForMember(dest => dest.drr_describeengagementimpactedaffectedparties, opt => opt.MapFrom(src => src.OtherEngagementComments))
                .ForMember(dest => dest.drr_howprojectcontributetocollaboration, opt => opt.MapFrom(src => src.CollaborationComments))

                //Climate Adaptation - 6
                .ForMember(dest => dest.drr_doesprojectconsiderclimatechange, opt => opt.MapFrom(src => src.ClimateAdaptationScreener.HasValue && src.ClimateAdaptationScreener.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))

                //Permits Regulations & Standards - 7
                .ForMember(dest => dest.drr_projecteligibleforrequiredpermitsapproval, opt => opt.MapFrom(src => src.Approvals.HasValue && src.Approvals.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_projecteligibleforpermitsapprovalcomments, opt => opt.MapFrom(src => src.ApprovalsComments))
                .ForMember(dest => dest.drr_guidanceofqualifiedprofessional, opt => opt.MapFrom(src => src.ProfessionalGuidance.HasValue && src.ProfessionalGuidance.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_drr_application_drr_qualifiedprofessional_Application, opt => opt.MapFrom(src => src.Professionals))
                .ForMember(dest => dest.drr_qualifiedprofessionalcomments, opt => opt.MapFrom(src => src.ProfessionalGuidanceComments))
                .ForMember(dest => dest.drr_acceptableprovincialstandards, opt => opt.MapFrom(src => src.StandardsAcceptable.HasValue ? (int?)Enum.Parse<DRRYesNoNotApplicable>(src.StandardsAcceptable.Value.ToString()) : null))
                .ForMember(dest => dest.drr_drr_application_drr_provincialstandarditem_Application, opt => opt.MapFrom(src => src.Standards))
                //.ForMember(dest => dest.drr_explainhowprojectwillmeetprovincialstanda, opt => opt.MapFrom(src => src.StandardsComments))
                .ForMember(dest => dest.drr_commentsacceptableprovincialstandards, opt => opt.MapFrom(src => src.StandardsComments))
                .ForMember(dest => dest.drr_requiredagencydiscussionsandapprovals, opt => opt.MapFrom(src => src.Regulations.HasValue && src.Regulations.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_commentsrequiredagencydiscussionsapproval, opt => opt.MapFrom(src => src.RegulationsComments))

                //Project Outcomes - 8
                .ForMember(dest => dest.drr_projectforbroadpublicuseorbenefit, opt => opt.MapFrom(src => src.PublicBenefit.HasValue && src.PublicBenefit.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_projectforbroadpublicuseorbenefitcomments, opt => opt.MapFrom(src => src.PublicBenefitComments))
                .ForMember(dest => dest.drr_willthisprojectreducecosts, opt => opt.MapFrom(src => src.FutureCostReduction.HasValue && src.FutureCostReduction.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_drr_application_drr_costreductionitem_Application, opt => opt.MapFrom(src => src.CostReductions))
                .ForMember(dest => dest.drr_explainhowcostswillbereduced, opt => opt.MapFrom(src => src.CostReductionComments))
                .ForMember(dest => dest.drr_willthisprojectproducecobenefits, opt => opt.MapFrom(src => src.ProduceCoBenefits.HasValue && src.ProduceCoBenefits.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_drr_application_drr_cobenefititem_Application, opt => opt.MapFrom(src => src.CoBenefits))
                .ForMember(dest => dest.drr_howwilltheprojectproducecobenefits, opt => opt.MapFrom(src => src.CoBenefitComments))
                //.ForMember(dest => dest., opt => opt.MapFrom(src => src.))
                .ForMember(dest => dest.drr_extentproposedprojectwillincreaseresilien, opt => opt.MapFrom(src => src.IncreasedResiliencyComments))

                //Project Risks - 9
                .ForMember(dest => dest.drr_projectcomplexityrisksmitigated, opt => opt.MapFrom(src => src.ComplexityRiskMitigated.HasValue && src.ComplexityRiskMitigated.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_drr_application_drr_projectcomplexityriskitem_Application, opt => opt.MapFrom(src => src.ComplexityRisks))
                .ForMember(dest => dest.drr_howprojectcomplexityrisksmitigated, opt => opt.MapFrom(src => src.ComplexityRiskComments))
                .ForMember(dest => dest.drr_projectreadinessrisksmitigated, opt => opt.MapFrom(src => src.ReadinessRiskMitigated.HasValue && src.ReadinessRiskMitigated.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_drr_application_drr_projectreadinessriskitem_Application, opt => opt.MapFrom(src => src.ReadinessRisks))
                .ForMember(dest => dest.drr_howprojectreadinessrisksmitigated, opt => opt.MapFrom(src => src.ReadinessRiskComments))
                .ForMember(dest => dest.drr_projectsensitivityrisksmitigated, opt => opt.MapFrom(src => src.SensitivityRiskMitigated.HasValue && src.SensitivityRiskMitigated.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_drr_application_drr_projectsensitivityriskitem_Application, opt => opt.MapFrom(src => src.SensitivityRisks))
                .ForMember(dest => dest.drr_howprojectsensitivityrisksmitigated, opt => opt.MapFrom(src => src.SensitivityRiskComments))
                .ForMember(dest => dest.drr_projectcapacityrisksmitigated, opt => opt.MapFrom(src => src.CapacityRiskMitigated.HasValue && src.CapacityRiskMitigated.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_drr_application_drr_projectcapacitychallengeitem_Application, opt => opt.MapFrom(src => src.CapacityRisks))
                .ForMember(dest => dest.drr_howcapacityrisksmitigated, opt => opt.MapFrom(src => src.CapacityRiskComments))
                .ForMember(dest => dest.drr_isriskbeingincreasedortransferred, opt => opt.MapFrom(src => src.RiskTransferMigigated.HasValue && src.RiskTransferMigigated.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                //.ForMember(dest => dest., opt => opt.MapFrom(src => src.TransferRisks))
                .ForMember(dest => dest.drr_describeriskincreasedortransferred, opt => opt.MapFrom(src => src.TransferRisksComments))

                //Budget - 10
                .ForMember(dest => dest.drr_drr_application_drr_driffundingrequest_Application, opt => opt.MapFrom(src => src.YearOverYearFunding))
                .ForMember(dest => dest.drr_totaldrifprogramfundingrequest, opt => opt.MapFrom(src => src.TotalDrifFundingRequest))
                .ForMember(dest => dest.drr_explaindiscrepancy, opt => opt.MapFrom(src => src.DiscrepancyComment))
                .ForMember(dest => dest.drr_pastresponsecostprojectdesignedtomitigate, opt => opt.MapFrom(src => src.PreviousResponse.HasValue ? (int?)Enum.Parse<DRRYesNoNotApplicable>(src.PreviousResponse.Value.ToString()) : null))
                .ForMember(dest => dest.drr_cost, opt => opt.MapFrom(src => src.PreviousResponseCost))
                .ForMember(dest => dest.drr_stepstakentobecosteffective, opt => opt.MapFrom(src => src.ActivityCostEffectiveness))
                .ForMember(dest => dest.drr_costconsiderationsapply, opt => opt.MapFrom(src => src.CostConsiderationsApplied.HasValue && src.CostConsiderationsApplied.Value ? DRRTwoOptions.Yes : DRRTwoOptions.No))
                .ForMember(dest => dest.drr_explaincostconsiderations, opt => opt.MapFrom(src => src.CostConsiderationsComments))

                //Attachments - 11

                //Review & Declaration - 12

                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
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
                .ForMember(dest => dest.InfrastructureImpacted, opt => opt.MapFrom(src => src.drr_drr_application_drr_criticalinfrastructureimpacted_Application))
                .ForMember(dest => dest.DisasterRiskUnderstanding, opt => opt.MapFrom(src => src.drr_improveunderstandingriskinvestreduction))
                .ForMember(dest => dest.AddressRisksAndHazards, opt => opt.MapFrom(src => src.drr_includedtoaddressidentifiedriskhazards))
                .ForMember(dest => dest.DRIFProgramGoalAlignment, opt => opt.MapFrom(src => src.drr_howdoesprojectalignwithdrifsprogramgoals))
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
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int?)Enum.Parse<ApplicationStatus>(((ApplicationStatusOptionSet)src.statuscode).ToString())))

                //FP
                //Proponent & Project Information - 1
                .ForMember(dest => dest.RegionalProject, opt => opt.MapFrom(src => src.drr_isthisaregionalproject == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.RegionalProjectComments, opt => opt.MapFrom(src => src.drr_isthisaregionalprojectcomments))
                //Ownership & Authorization - 2
                .ForMember(dest => dest.ProjectAuthority, opt => opt.MapFrom(src => src.drr_proponenthastheauthorityandownership == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.ProjectAuthorityComments, opt => opt.MapFrom(src => src.drr_commentsproponenthastheauthorityandowners))
                .ForMember(dest => dest.OperationAndMaintenance, opt => opt.MapFrom(src => src.drr_proponenttomaintaininfrastructurelongterm.HasValue ? (int?)Enum.Parse<YesNoOption>(((DRRYesNoNotApplicable)src.drr_proponenttomaintaininfrastructurelongterm).ToString()) : null))
                .ForMember(dest => dest.OperationAndMaintenanceComments, opt => opt.MapFrom(src => src.drr_proponenttomaintaininfrastructurecomments))
                .ForMember(dest => dest.FirstNationsEndorsement, opt => opt.MapFrom(src => src.drr_authorizedendorsedfirstnationpartners.HasValue ? (int?)Enum.Parse<YesNoOption>(((DRRYesNoNotApplicable)src.drr_authorizedendorsedfirstnationpartners).ToString()) : null))
                .ForMember(dest => dest.LocalGovernmentEndorsement, opt => opt.MapFrom(src => src.drr_authorizedendorsedlocalgovpartners.HasValue ? (int?)Enum.Parse<YesNoOption>(((DRRYesNoNotApplicable)src.drr_authorizedendorsedlocalgovpartners).ToString()) : null))
                .ForMember(dest => dest.AuthorizationOrEndorsementComments, opt => opt.MapFrom(src => src.drr_authorizationorendorsementcomments))
                //Project Area - 3
                //Project Plan - 4
                .ForMember(dest => dest.ProjectDescription, opt => opt.Ignore())
                .ForMember(dest => dest.ProposedActivities, opt => opt.MapFrom(src => src.drr_drr_application_drr_proposedactivity_Application))
                .ForMember(dest => dest.VerificationMethods, opt => opt.MapFrom(src => src.drr_drr_application_drr_projectneedidentificationitem_Application))
                .ForMember(dest => dest.VerificationMethodsComments, opt => opt.MapFrom(src => src.drr_explainneedforproject))
                .ForMember(dest => dest.ProjectAlternateOptions, opt => opt.MapFrom(src => src.drr_extentalternateprojectoptionsconsidered))
                //Project Engagement - 5
                .ForMember(dest => dest.EngagedWithFirstNations, opt => opt.MapFrom(src => src.drr_meaningfullyengagedwithlocalfirstnations == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.EngagedWithFirstNationsComments, opt => opt.MapFrom(src => src.drr_describeengagementfirstnations))
                .ForMember(dest => dest.OtherEngagement, opt => opt.MapFrom(src => src.drr_effectivelyengagedwithothers.HasValue ? (int?)Enum.Parse<YesNoOption>(((DRRYesNoNotApplicable)src.drr_effectivelyengagedwithothers).ToString()) : null))
                .ForMember(dest => dest.AffectedParties, opt => opt.MapFrom(src => src.drr_drr_application_drr_impactedoraffectedpartyitem_Application))
                .ForMember(dest => dest.OtherEngagementComments, opt => opt.MapFrom(src => src.drr_describeengagementimpactedaffectedparties))
                .ForMember(dest => dest.CollaborationComments, opt => opt.MapFrom(src => src.drr_howprojectcontributetocollaboration))
                //Climate Adaptation - 6
                .ForMember(dest => dest.ClimateAdaptationScreener, opt => opt.MapFrom(src => src.drr_doesprojectconsiderclimatechange == (int)DRRTwoOptions.Yes))
                //Permits Regulations & Standards - 7
                .ForMember(dest => dest.Approvals, opt => opt.MapFrom(src => src.drr_projecteligibleforrequiredpermitsapproval == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.ApprovalsComments, opt => opt.MapFrom(src => src.drr_projecteligibleforpermitsapprovalcomments))
                .ForMember(dest => dest.ProfessionalGuidance, opt => opt.MapFrom(src => src.drr_guidanceofqualifiedprofessional == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.Professionals, opt => opt.MapFrom(src => src.drr_drr_application_drr_qualifiedprofessional_Application))
                .ForMember(dest => dest.ProfessionalGuidanceComments, opt => opt.MapFrom(src => src.drr_qualifiedprofessionalcomments))
                .ForMember(dest => dest.StandardsAcceptable, opt => opt.MapFrom(src => src.drr_acceptableprovincialstandards.HasValue ? (int?)Enum.Parse<YesNoOption>(((DRRYesNoNotApplicable)src.drr_acceptableprovincialstandards).ToString()) : null))
                .ForMember(dest => dest.Standards, opt => opt.MapFrom(src => src.drr_drr_application_drr_provincialstandarditem_Application))
                .ForMember(dest => dest.StandardsComments, opt => opt.MapFrom(src => src.drr_explainhowprojectwillmeetprovincialstanda))
                .ForMember(dest => dest.StandardsComments, opt => opt.MapFrom(src => src.drr_commentsacceptableprovincialstandards))
                .ForMember(dest => dest.Regulations, opt => opt.MapFrom(src => src.drr_requiredagencydiscussionsandapprovals == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.RegulationsComments, opt => opt.MapFrom(src => src.drr_commentsrequiredagencydiscussionsapproval))
                //Project Outcomes - 8
                .ForMember(dest => dest.PublicBenefit, opt => opt.MapFrom(src => src.drr_projectforbroadpublicuseorbenefit == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.PublicBenefitComments, opt => opt.MapFrom(src => src.drr_projectforbroadpublicuseorbenefitcomments))
                .ForMember(dest => dest.FutureCostReduction, opt => opt.MapFrom(src => src.drr_willthisprojectreducecosts == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.CostReductions, opt => opt.MapFrom(src => src.drr_drr_application_drr_costreductionitem_Application))
                .ForMember(dest => dest.CostReductionComments, opt => opt.MapFrom(src => src.drr_explainhowcostswillbereduced))
                .ForMember(dest => dest.ProduceCoBenefits, opt => opt.MapFrom(src => src.drr_willthisprojectproducecobenefits == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.CoBenefits, opt => opt.MapFrom(src => src.drr_drr_application_drr_cobenefititem_Application))
                .ForMember(dest => dest.CoBenefitComments, opt => opt.MapFrom(src => src.drr_howwilltheprojectproducecobenefits))
                .ForMember(dest => dest.IncreasedResiliency, opt => opt.Ignore())
                .ForMember(dest => dest.IncreasedResiliencyComments, opt => opt.MapFrom(src => src.drr_extentproposedprojectwillincreaseresilien))
                //Project Risks - 9
                .ForMember(dest => dest.ComplexityRiskMitigated, opt => opt.MapFrom(src => src.drr_projectcomplexityrisksmitigated == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.ComplexityRisks, opt => opt.MapFrom(src => src.drr_drr_application_drr_projectcomplexityriskitem_Application))
                .ForMember(dest => dest.ComplexityRiskComments, opt => opt.MapFrom(src => src.drr_howprojectcomplexityrisksmitigated))
                .ForMember(dest => dest.ReadinessRiskMitigated, opt => opt.MapFrom(src => src.drr_projectreadinessrisksmitigated == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.ReadinessRisks, opt => opt.MapFrom(src => src.drr_drr_application_drr_projectreadinessriskitem_Application))
                .ForMember(dest => dest.ReadinessRiskComments, opt => opt.MapFrom(src => src.drr_howprojectreadinessrisksmitigated))
                .ForMember(dest => dest.SensitivityRiskMitigated, opt => opt.MapFrom(src => src.drr_projectsensitivityrisksmitigated == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.SensitivityRisks, opt => opt.MapFrom(src => src.drr_drr_application_drr_projectsensitivityriskitem_Application))
                .ForMember(dest => dest.SensitivityRiskComments, opt => opt.MapFrom(src => src.drr_howprojectsensitivityrisksmitigated))
                .ForMember(dest => dest.CapacityRiskMitigated, opt => opt.MapFrom(src => src.drr_projectcapacityrisksmitigated == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.CapacityRisks, opt => opt.MapFrom(src => src.drr_drr_application_drr_projectcapacitychallengeitem_Application))
                .ForMember(dest => dest.CapacityRiskComments, opt => opt.MapFrom(src => src.drr_howcapacityrisksmitigated))
                .ForMember(dest => dest.RiskTransferMigigated, opt => opt.MapFrom(src => src.drr_isriskbeingincreasedortransferred == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.TransferRisks, opt => opt.Ignore())
                .ForMember(dest => dest.TransferRisksComments, opt => opt.MapFrom(src => src.drr_describeriskincreasedortransferred))
                //Budget - 10
                .ForMember(dest => dest.YearOverYearFunding, opt => opt.MapFrom(src => src.drr_drr_application_drr_driffundingrequest_Application))
                .ForMember(dest => dest.TotalDrifFundingRequest, opt => opt.MapFrom(src => src.drr_totaldrifprogramfundingrequest))
                .ForMember(dest => dest.DiscrepancyComment, opt => opt.MapFrom(src => src.drr_explaindiscrepancy))
                .ForMember(dest => dest.CostEffective, opt => opt.Ignore())
                .ForMember(dest => dest.CostEffectiveComments, opt => opt.Ignore())
                .ForMember(dest => dest.PreviousResponse, opt => opt.MapFrom(src => src.drr_pastresponsecostprojectdesignedtomitigate.HasValue ? (int?)Enum.Parse<YesNoOption>(((DRRYesNoNotApplicable)src.drr_pastresponsecostprojectdesignedtomitigate).ToString()) : null))
                .ForMember(dest => dest.PreviousResponseCost, opt => opt.MapFrom(src => src.drr_cost))
                .ForMember(dest => dest.PreviousResponseComments, opt => opt.Ignore())
                .ForMember(dest => dest.ActivityCostEffectiveness, opt => opt.MapFrom(src => src.drr_stepstakentobecosteffective))
                .ForMember(dest => dest.CostConsiderationsApplied, opt => opt.MapFrom(src => src.drr_costconsiderationsapply == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.CostConsiderations, opt => opt.Ignore())
                .ForMember(dest => dest.CostConsiderationsComments, opt => opt.MapFrom(src => src.drr_explaincostconsiderations))
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

            CreateMap<CriticalInfrastructure, drr_criticalinfrastructureimpacted>(MemberList.None)
                .ForMember(dest => dest.drr_name, opt => opt.MapFrom(src => src.Name))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_name))
                ;

            CreateMap<drr_legaldeclaration, DeclarationInfo>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<DeclarationTypeOptionSet>(((DeclarationTypeOptionSet)src.drr_declarationtype).ToString())))
                .ForMember(dest => dest.ApplicationTypeName, opt => opt.MapFrom(src => src.drr_ApplicationType.drr_name))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.drr_declarationtext));

            CreateMap<ProfessionalInfo, drr_qualifiedprofessional>(MemberList.None)
                .ForMember(dest => dest.drr_name, opt => opt.MapFrom(src => src.Name))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_name))
            ;

            CreateMap<ProposedActivity, drr_proposedactivity>(MemberList.None)
                .ForMember(dest => dest.drr_name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.drr_anticipatedstartdate, opt => opt.MapFrom(src => src.StartDate.HasValue ? src.StartDate.Value.ToUniversalTime() : (DateTimeOffset?)null))
                .ForMember(dest => dest.drr_anticipatedenddate, opt => opt.MapFrom(src => src.EndDate.HasValue ? src.EndDate.Value.ToUniversalTime() : (DateTimeOffset?)null))
                .ForMember(dest => dest.drr_relatedmilestone, opt => opt.MapFrom(src => src.RelatedMilestone))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.drr_name))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.drr_anticipatedstartdate.HasValue ? src.drr_anticipatedstartdate.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.drr_anticipatedenddate.HasValue ? src.drr_anticipatedenddate.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.RelatedMilestone, opt => opt.MapFrom(src => src.drr_relatedmilestone))
            ;

            CreateMap<ProvincialStandard, drr_provincialstandarditem>(MemberList.None)
                .ForMember(dest => dest.drr_ProvincialStandard, opt => opt.MapFrom(src => new drr_provincialstandard { drr_name = src.Name }))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.drr_provincialstandarditemcomments) ? src.drr_ProvincialStandard.drr_name : src.drr_provincialstandarditemcomments))
            ;

            CreateMap<AffectedParty, drr_impactedoraffectedpartyitem>(MemberList.None)
               .ForMember(dest => dest.drr_ImpactedorAffectedParty, opt => opt.MapFrom(src => new drr_impactedoraffectedparty { drr_name = src.Name }))
               .ReverseMap()
               .ValidateMemberList(MemberList.Destination)
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.drr_impactedoraffectedpartycomments) ? src.drr_ImpactedorAffectedParty.drr_name : src.drr_impactedoraffectedpartycomments))
            ;

            CreateMap<VerificationMethod, drr_projectneedidentificationitem>(MemberList.None)
               .ForMember(dest => dest.drr_projectneedidentification, opt => opt.MapFrom(src => new drr_projectneedidentification { drr_name = src.Name }))
               .ReverseMap()
               .ValidateMemberList(MemberList.Destination)
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.drr_projectneedidentifiedcomments) ? src.drr_projectneedidentification.drr_name : src.drr_projectneedidentifiedcomments))
            ;

            CreateMap<CostReduction, drr_costreductionitem>(MemberList.None)
               .ForMember(dest => dest.drr_CostReduction, opt => opt.MapFrom(src => new drr_costreduction { drr_name = src.Name }))
               .ReverseMap()
               .ValidateMemberList(MemberList.Destination)
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.drr_costreductionitemcomments) ? src.drr_CostReduction.drr_name : src.drr_costreductionitemcomments))
            ;

            CreateMap<CoBenefit, drr_cobenefititem>(MemberList.None)
               .ForMember(dest => dest.drr_CoBenefit, opt => opt.MapFrom(src => new drr_cobenefit { drr_name = src.Name }))
               .ReverseMap()
               .ValidateMemberList(MemberList.Destination)
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.drr_cobenefitcomments) ? src.drr_CoBenefit.drr_name : src.drr_cobenefitcomments))
            ;

            CreateMap<ComplexityRisk, drr_projectcomplexityriskitem>(MemberList.None)
               .ForMember(dest => dest.drr_ProjectComplexityRisk, opt => opt.MapFrom(src => new drr_projectcomplexityrisk { drr_name = src.Name }))
               .ReverseMap()
               .ValidateMemberList(MemberList.Destination)
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.drr_projectcomplexityriskitemcomments) ? src.drr_ProjectComplexityRisk.drr_name : src.drr_projectcomplexityriskitemcomments))
            ;

            CreateMap<ReadinessRisk, drr_projectreadinessriskitem>(MemberList.None)
               .ForMember(dest => dest.drr_ProjectReadinessRisk, opt => opt.MapFrom(src => new drr_projectreadinessrisk { drr_name = src.Name }))
               .ReverseMap()
               .ValidateMemberList(MemberList.Destination)
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.drr_projectreadinessriskcomments) ? src.drr_ProjectReadinessRisk.drr_name : src.drr_projectreadinessriskcomments))
            ;

            CreateMap<SensitivityRisk, drr_projectsensitivityriskitem>(MemberList.None)
              .ForMember(dest => dest.drr_ProjectSensitivityRisk, opt => opt.MapFrom(src => new drr_projectsensitivityrisk { drr_name = src.Name }))
              .ReverseMap()
              .ValidateMemberList(MemberList.Destination)
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.drr_projectsensitivityriskcomments) ? src.drr_ProjectSensitivityRisk.drr_name : src.drr_projectsensitivityriskcomments))
            ;

            CreateMap<CapacityRisk, drr_projectcapacitychallengeitem>(MemberList.None)
              .ForMember(dest => dest.drr_ProjectCapacityChallenge, opt => opt.MapFrom(src => new drr_projectcapacitychallenge { drr_name = src.Name }))
              .ReverseMap()
              .ValidateMemberList(MemberList.Destination)
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.drr_projectcapacitychallengecomments) ? src.drr_ProjectCapacityChallenge.drr_name : src.drr_projectcapacitychallengecomments))
            ;

            CreateMap<YearOverYearFunding, drr_driffundingrequest>(MemberList.None)
                .ForMember(dest => dest.drr_FiscalYear, opt => opt.MapFrom(src => new drr_fiscalyear { drr_name = src.Year }))
                .ForMember(dest => dest.drr_drifprogramfundingrequest, opt => opt.MapFrom(src => src.Amount))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.drr_FiscalYear.drr_name))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.drr_drifprogramfundingrequest))
                ;
        }
    }
}
#pragma warning restore CS8629 // Nullable value type may be null.
