using AutoMapper;
using EMBC.DRR.API.Services;
using EMCR.DRR.API.Controllers;
using EMCR.DRR.API.Model;
using EMCR.DRR.Controllers;

namespace EMCR.DRR.API.Mappers
{
    public class Mappings : Profile
    {
        public Mappings()
        {
            CreateMap<DraftEoiApplication, EoiApplication>()
                .ForMember(dest => dest.AuthorizedRepresentativeStatement, opt => opt.Ignore())
                .ForMember(dest => dest.FOIPPAConfirmation, opt => opt.Ignore())
                .ForMember(dest => dest.InformationAccuracyStatement, opt => opt.Ignore())
                ;

            CreateMap<DraftFpApplication, FpApplication>()
                ;

            CreateMap<Managers.Intake.Application, Submission>()
                .ForMember(dest => dest.ApplicationType, opt => opt.MapFrom(src => DRRApplicationTypeMapper(src.ApplicationTypeName)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DRRApplicationStatusMapper(src.Status)))
                .ForMember(dest => dest.FundingRequest, opt => opt.MapFrom(src => src.FundingRequest.ToString()))
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => src.ModifiedOn))
                .ForMember(dest => dest.ExistingFpId, opt => opt.MapFrom(src => src.FpId))
                .ForMember(dest => dest.PartneringProponents, opt => opt.MapFrom(src => src.PartneringProponents.Select(p => p.Name)))
                ;

            CreateMap<Managers.Intake.DeclarationInfo, DeclarationInfo>()
                .ForMember(dest => dest.ApplicationType, opt => opt.MapFrom(src => DRRApplicationTypeMapper(src.ApplicationTypeName)))
                ;


            CreateMap<AccountDetails, ProfileDetails>()
                .ReverseMap()
                ;
        }

        private ApplicationType DRRApplicationTypeMapper(string type)
        {
            switch (type)
            {
                case "EOI":
                    return ApplicationType.EOI;
                case "Full Proposal":
                    return ApplicationType.FP;
                default: return ApplicationType.EOI;
            }
        }

        private SubmissionPortalStatus DRRApplicationStatusMapper(Managers.Intake.ApplicationStatus status)
        {
            switch (status)
            {
                case Managers.Intake.ApplicationStatus.DraftStaff:
                case Managers.Intake.ApplicationStatus.DraftProponent:
                    return SubmissionPortalStatus.Draft;
                case Managers.Intake.ApplicationStatus.Submitted:
                case Managers.Intake.ApplicationStatus.InReview:
                    return SubmissionPortalStatus.UnderReview;
                case Managers.Intake.ApplicationStatus.Invited:
                    return SubmissionPortalStatus.EligibleInvited;
                case Managers.Intake.ApplicationStatus.InPool:
                    return SubmissionPortalStatus.EligiblePending;
                case Managers.Intake.ApplicationStatus.Ineligible:
                    return SubmissionPortalStatus.Ineligible;
                case Managers.Intake.ApplicationStatus.Withdrawn:
                    return SubmissionPortalStatus.Withdrawn;
                default:
                    return SubmissionPortalStatus.Draft;
            }
        }
    }
}
