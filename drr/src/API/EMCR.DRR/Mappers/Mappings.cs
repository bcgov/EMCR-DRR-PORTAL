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
            CreateMap<DraftEoiApplication, EoiApplication>();

            CreateMap<Managers.Intake.Application, Submission>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DRRApplicationStatusMapper(src.Status)))
                .ForMember(dest => dest.FundingRequest, opt => opt.MapFrom(src => src.FundingRequest.ToString()))
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => src.ModifiedOn))
                .ForMember(dest => dest.PartneringProponents, opt => opt.MapFrom(src => src.PartneringProponents.Select(p => p.Name)))
                ;

            CreateMap<Managers.Intake.DeclarationInfo, DeclarationInfo>()
                ;

            CreateMap<DeclarationInfo, DeclarationInfo>()
                ;

            CreateMap<AccountDetails, ProfileDetails>()
                .ReverseMap()
                ;
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
