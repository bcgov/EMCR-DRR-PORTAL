using AutoMapper;
using EMCR.DRR.API.Model;
using EMCR.DRR.Controllers;

namespace EMCR.DRR.Managers.Intake
{
    public class IntakeMapperProfile : Profile
    {
        public IntakeMapperProfile()
        {
            CreateMap<Application, DraftApplication>()
                .ForMember(dest => dest.AdditionalContacts, opt => opt.MapFrom(src => DRRAdditionalContactMapper(src.AdditionalContact1, src.AdditionalContact2)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DRRApplicationStatusMapper(src.Status)))
                .ForMember(dest => dest.PartneringProponents, opt => opt.MapFrom(src => src.PartneringProponents.Select(p => p.Name)))
                .ForMember(dest => dest.InfrastructureImpacted, opt => opt.MapFrom(src => src.InfrastructureImpacted.Select(p => p.Name)))
                ;

            CreateMap<EoiApplication, Application>(MemberList.None)
                .ForMember(dest => dest.AdditionalContact1, opt => opt.MapFrom(src => src.AdditionalContacts.FirstOrDefault()))
                .ForMember(dest => dest.AdditionalContact2, opt => opt.MapFrom(src => src.AdditionalContacts.ElementAtOrDefault(1)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeStatusMapper(src.Status)))
                .ForMember(dest => dest.ApplicationTypeName, opt => opt.MapFrom(src => "EOI"))
                .ForMember(dest => dest.ProgramName, opt => opt.MapFrom(src => "DRIF"))
                .ReverseMap()
                .ForMember(dest => dest.AdditionalContacts, opt => opt.MapFrom(src => DRRAdditionalContactMapper(src.AdditionalContact1, src.AdditionalContact2)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DRRApplicationStatusMapper(src.Status)))
                .ForMember(dest => dest.PartneringProponents, opt => opt.MapFrom(src => src.PartneringProponents.Select(p => p.Name)))
                .ForMember(dest => dest.InfrastructureImpacted, opt => opt.MapFrom(src => src.InfrastructureImpacted.Select(p => p.Name)))
                ;

            CreateMap<DraftEoiApplication, Application>(MemberList.None)
                .ForMember(dest => dest.AdditionalContact1, opt => opt.MapFrom(src => src.AdditionalContacts.FirstOrDefault()))
                .ForMember(dest => dest.AdditionalContact2, opt => opt.MapFrom(src => src.AdditionalContacts.ElementAtOrDefault(1)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeStatusMapper(src.Status)))
                .ForMember(dest => dest.ApplicationTypeName, opt => opt.MapFrom(src => "EOI"))
                .ForMember(dest => dest.ProgramName, opt => opt.MapFrom(src => "DRIF"))
                .ReverseMap()
                .ForMember(dest => dest.AdditionalContacts, opt => opt.MapFrom(src => DRRAdditionalContactMapper(src.AdditionalContact1, src.AdditionalContact2)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DRRApplicationStatusMapper(src.Status)))
                .ForMember(dest => dest.PartneringProponents, opt => opt.MapFrom(src => src.PartneringProponents.Select(p => p.Name)))
                .ForMember(dest => dest.InfrastructureImpacted, opt => opt.MapFrom(src => src.InfrastructureImpacted.Select(p => p.Name)))
                ;

            CreateMap<FpApplication, Application>(MemberList.None)
                .ForMember(dest => dest.AdditionalContact1, opt => opt.MapFrom(src => src.AdditionalContacts.FirstOrDefault()))
                .ForMember(dest => dest.AdditionalContact2, opt => opt.MapFrom(src => src.AdditionalContacts.ElementAtOrDefault(1)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeStatusMapper(src.Status)))
                .ForMember(dest => dest.ApplicationTypeName, opt => opt.MapFrom(src => "Full Proposal"))
                .ForMember(dest => dest.ProgramName, opt => opt.MapFrom(src => "DRIF"))
                .ReverseMap()
                .ForMember(dest => dest.AdditionalContacts, opt => opt.MapFrom(src => DRRAdditionalContactMapper(src.AdditionalContact1, src.AdditionalContact2)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DRRApplicationStatusMapper(src.Status)))
                .ForMember(dest => dest.PartneringProponents, opt => opt.MapFrom(src => src.PartneringProponents.Select(p => p.Name)))
                .ForMember(dest => dest.InfrastructureImpacted, opt => opt.MapFrom(src => src.InfrastructureImpacted.Select(p => p.Name)))
                .ForMember(dest => dest.Professionals, opt => opt.MapFrom(src => src.Professionals.Select(p => p.Name)))
                .ForMember(dest => dest.Standards, opt => opt.MapFrom(src => src.Standards.Select(p => p.Name)))
                .ForMember(dest => dest.VerificationMethods, opt => opt.MapFrom(src => src.VerificationMethods.Select(p => p.Name)))
                .ForMember(dest => dest.AffectedParties, opt => opt.MapFrom(src => src.AffectedParties.Select(p => p.Name)))
                .ForMember(dest => dest.CostReductions, opt => opt.MapFrom(src => src.CostReductions.Select(p => p.Name)))
                .ForMember(dest => dest.CoBenefits, opt => opt.MapFrom(src => src.CoBenefits.Select(p => p.Name)))
                .ForMember(dest => dest.ComplexityRisks, opt => opt.MapFrom(src => src.ComplexityRisks.Select(p => p.Name)))
                .ForMember(dest => dest.ReadinessRisks, opt => opt.MapFrom(src => src.ReadinessRisks.Select(p => p.Name)))
                .ForMember(dest => dest.SensitivityRisks, opt => opt.MapFrom(src => src.SensitivityRisks.Select(p => p.Name)))
                .ForMember(dest => dest.CapacityRisks, opt => opt.MapFrom(src => src.CapacityRisks.Select(p => p.Name)))
                ;

            CreateMap<DraftFpApplication, Application>(MemberList.None)
                .ForMember(dest => dest.AdditionalContact1, opt => opt.MapFrom(src => src.AdditionalContacts.FirstOrDefault()))
                .ForMember(dest => dest.AdditionalContact2, opt => opt.MapFrom(src => src.AdditionalContacts.ElementAtOrDefault(1)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeStatusMapper(src.Status)))
                .ForMember(dest => dest.ApplicationTypeName, opt => opt.MapFrom(src => "Full Proposal"))
                .ForMember(dest => dest.ProgramName, opt => opt.MapFrom(src => "DRIF"))
                .ReverseMap()
                .ForMember(dest => dest.AdditionalContacts, opt => opt.MapFrom(src => DRRAdditionalContactMapper(src.AdditionalContact1, src.AdditionalContact2)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DRRApplicationStatusMapper(src.Status)))
                .ForMember(dest => dest.PartneringProponents, opt => opt.MapFrom(src => src.PartneringProponents.Select(p => p.Name)))
                .ForMember(dest => dest.InfrastructureImpacted, opt => opt.MapFrom(src => src.InfrastructureImpacted.Select(p => p.Name)))
                .ForMember(dest => dest.Professionals, opt => opt.MapFrom(src => src.Professionals.Select(p => p.Name)))
                .ForMember(dest => dest.Standards, opt => opt.MapFrom(src => src.Standards.Select(p => p.Name)))
                .ForMember(dest => dest.VerificationMethods, opt => opt.MapFrom(src => src.VerificationMethods.Select(p => p.Name)))
                ;

            CreateMap<Controllers.FundingInformation, FundingInformation>()
                .ReverseMap()
                ;

            CreateMap<Controllers.YearOverYearFunding, YearOverYearFunding>()
                .ReverseMap()
                ;   

            CreateMap<Controllers.ContactDetails, ContactDetails>()
                .ForMember(dest => dest.BCeId, opt => opt.Ignore())
                .ReverseMap()
                ;

            CreateMap<string, PartneringProponent>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
                ;

            CreateMap<string, CriticalInfrastructure>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
                ;

            CreateMap<string, ProfessionalInfo>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
               ;

            CreateMap<string, ProvincialStandard>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
               ;

            CreateMap<string, CostReduction>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
               ;

            CreateMap<string, CoBenefit>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
               ;

            CreateMap<string, IncreasedResiliency>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
               ;

            CreateMap<string, VerificationMethod>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
               ;

            CreateMap<string, AffectedParty>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
               ;

            CreateMap<string, ComplexityRisk>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
               ;

            CreateMap<string, ReadinessRisk>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
               ;

            CreateMap<string, SensitivityRisk>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
               ;

            CreateMap<string, CapacityRisk>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
               ;

            CreateMap<string, TransferRisks>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
               ;

            CreateMap<Controllers.ProposedActivity, ProposedActivity>()
                .ReverseMap()
                ;

            CreateMap<Resources.Applications.EntitiesQueryResult, EntitiesQueryResult>()
                .ReverseMap()
                ;
        }

        private IEnumerable<ContactDetails> DRRAdditionalContactMapper(ContactDetails? contact1, ContactDetails? contact2)
        {
            var ret = new List<ContactDetails>();
            if (contact1 != null) ret.Add(contact1);
            if (contact2 != null) ret.Add(contact2);
            return ret;
        }

        private SubmissionPortalStatus DRRApplicationStatusMapper(ApplicationStatus status)
        {
            switch (status)
            {
                case ApplicationStatus.DraftStaff:
                case ApplicationStatus.DraftProponent:
                    return SubmissionPortalStatus.Draft;
                case ApplicationStatus.Submitted:
                case ApplicationStatus.InReview:
                    return SubmissionPortalStatus.UnderReview;
                case ApplicationStatus.Invited:
                    return SubmissionPortalStatus.EligibleInvited;
                case ApplicationStatus.InPool:
                    return SubmissionPortalStatus.EligiblePending;
                case ApplicationStatus.Ineligible:
                    return SubmissionPortalStatus.Ineligible;
                case ApplicationStatus.Withdrawn:
                    return SubmissionPortalStatus.Withdrawn;
                default:
                    return SubmissionPortalStatus.Draft;
            }
        }

        private ApplicationStatus IntakeStatusMapper(SubmissionPortalStatus? status)
        {
            switch (status)
            {
                case SubmissionPortalStatus.Draft:
                    return ApplicationStatus.DraftProponent;
                case SubmissionPortalStatus.UnderReview:
                    return ApplicationStatus.Submitted;
                case SubmissionPortalStatus.EligibleInvited:
                    return ApplicationStatus.Invited;
                case SubmissionPortalStatus.EligiblePending:
                    return ApplicationStatus.InPool;
                case SubmissionPortalStatus.Ineligible:
                    return ApplicationStatus.Ineligible;
                case SubmissionPortalStatus.Withdrawn:
                    return ApplicationStatus.Withdrawn;
                default:
                    return ApplicationStatus.DraftProponent;
            }
        }
    }
}
