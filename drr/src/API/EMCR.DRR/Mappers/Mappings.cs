using AutoMapper;
using EMCR.DRR.API.Controllers;
using EMCR.DRR.API.Model;
using EMCR.DRR.API.Services;
using EMCR.DRR.API.Services.S3;
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
                .ForMember(dest => dest.AuthorizedRepresentativeStatement, opt => opt.Ignore())
                .ForMember(dest => dest.InformationAccuracyStatement, opt => opt.Ignore())
                ;

            CreateMap<Managers.Intake.Application, Submission>()
                .ForMember(dest => dest.ApplicationType, opt => opt.MapFrom(src => DRRApplicationTypeMapper(src.ApplicationTypeName)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DRRApplicationStatusMapper(src.Status)))
                .ForMember(dest => dest.FundingRequest, opt => opt.MapFrom(src => FundingRequestMapper(src)))
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => src.ModifiedOn))
                .ForMember(dest => dest.ExistingFpId, opt => opt.MapFrom(src => src.FpId))
                .ForMember(dest => dest.PartneringProponents, opt => opt.MapFrom(src => src.PartneringProponents.Select(p => p.Name)))
                .ForMember(dest => dest.ProgramType, opt => opt.MapFrom(src => Enum.Parse<ProgramType>(src.ProgramName)))
                .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => DRRActionsMapper(src)))
                ;

#pragma warning disable CS8604 // Possible null reference argument.
            CreateMap<Managers.Intake.DeclarationInfo, DeclarationInfo>()
                .ForMember(dest => dest.ApplicationType, opt => opt.MapFrom(src => DRRApplicationTypeMapper(src.ApplicationTypeName)))
                ;
#pragma warning restore CS8604 // Possible null reference argument.


            CreateMap<AccountDetails, ProfileDetails>()
                .ReverseMap()
                ;


            CreateMap<FileData, Managers.Intake.AttachmentInfo>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.File, opt => opt.MapFrom(src => new S3File { Content = src.Content, ContentType = src.ContentType, FileName = src.Name }))
                ;

            CreateMap<DraftDrrProject, DrrProject>()
                ;

        }

#pragma warning disable CS8603 // Possible null reference return.
        private string FundingRequestMapper(Managers.Intake.Application application)
        {
            var applicationType = DRRApplicationTypeMapper(application.ApplicationTypeName);
            switch (applicationType)
            {
                case ApplicationType.EOI: //always return drr_estimateddriffundingprogramrequest
                    return application.FundingRequest.ToString();
                case ApplicationType.FP: //always return drr_totaldrifprogramfundingrequest
                    return application.TotalDrifFundingRequest.ToString();
                default:
                    return application.EligibleFundingRequest.ToString();
            }
        }
#pragma warning restore CS8603 // Possible null reference return.

        private ApplicationType? DRRApplicationTypeMapper(string type)
        {
            switch (type)
            {
                case "EOI":
                    return ApplicationType.EOI;
                case "Full Proposal":
                    return ApplicationType.FP;
                case "Progress Report":
                    return ApplicationType.Progress;
                case "Budget Forecast Report":
                    return ApplicationType.Forecast;
                case "Claims Form":
                    return ApplicationType.Claim;
                default: return null;
            }
        }

        private IEnumerable<Actions> DRRActionsMapper(Managers.Intake.Application application)
        {
            var ret = new List<Actions>();

            if (application.ApplicationTypeName.Equals("EOI") && application.Status == Managers.Intake.ApplicationStatus.Invited && string.IsNullOrEmpty(application.FpId)) ret.Add(Actions.CreateFP);
            if (application.ApplicationTypeName.Equals("EOI") && (application.Status == Managers.Intake.ApplicationStatus.DraftStaff || application.Status == Managers.Intake.ApplicationStatus.DraftProponent)) ret.Add(Actions.Delete);
            if (application.Status == Managers.Intake.ApplicationStatus.DraftStaff || application.Status == Managers.Intake.ApplicationStatus.DraftProponent || application.Status == Managers.Intake.ApplicationStatus.Withdrawn) ret.Add(Actions.Edit);
            if (application.Status == Managers.Intake.ApplicationStatus.Submitted) ret.Add(Actions.Withdraw);

            return ret;
        }

        private SubmissionPortalStatus DRRApplicationStatusMapper(Managers.Intake.ApplicationStatus status)
        {
            switch (status)
            {
                case Managers.Intake.ApplicationStatus.Approved:
                    return SubmissionPortalStatus.Approved;
                case Managers.Intake.ApplicationStatus.ApprovedInPrinciple:
                    return SubmissionPortalStatus.ApprovedInPrinciple;
                case Managers.Intake.ApplicationStatus.Closed:
                    return SubmissionPortalStatus.Closed;
                case Managers.Intake.ApplicationStatus.DraftProponent:
                case Managers.Intake.ApplicationStatus.DraftStaff:
                    return SubmissionPortalStatus.Draft;
                case Managers.Intake.ApplicationStatus.Invited:
                    return SubmissionPortalStatus.EligibleInvited;
                case Managers.Intake.ApplicationStatus.InPool:
                    return SubmissionPortalStatus.EligiblePending;
                case Managers.Intake.ApplicationStatus.FPSubmitted:
                    return SubmissionPortalStatus.FullProposalSubmitted;
                case Managers.Intake.ApplicationStatus.Ineligible:
                    return SubmissionPortalStatus.Ineligible;
                case Managers.Intake.ApplicationStatus.Submitted:
                case Managers.Intake.ApplicationStatus.InReview:
                    return SubmissionPortalStatus.UnderReview;
                case Managers.Intake.ApplicationStatus.Withdrawn:
                    return SubmissionPortalStatus.Withdrawn;
                case Managers.Intake.ApplicationStatus.Deleted:
                    return SubmissionPortalStatus.Deleted;
                default:
                    return SubmissionPortalStatus.Draft;
            }
        }
    }
}
