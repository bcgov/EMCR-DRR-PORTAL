using AutoMapper;
using EMCR.DRR.API.Resources.Documents;
using Microsoft.Dynamics.CRM;

namespace EMCR.DRR.API.Resources.Cases
{
    public class DocumentMapperProfile : Profile
    {
        public DocumentMapperProfile()
        {
            CreateMap<Document, bcgov_documenturl>(MemberList.None)
                .ForMember(dest => dest.bcgov_filename, opt => opt.MapFrom(src => src.Name))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.bcgov_filename))
                .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.bcgov_filesize))
                .ForMember(dest => dest.DocumentType, opt => opt.Ignore())
                .ForMember(dest => dest.RecordType, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    if (src._bcgov_application_value != null) { dest.RecordType = Managers.Intake.RecordType.FullProposal; }
                    else if (src._bcgov_progressreport_value != null) { dest.RecordType = Managers.Intake.RecordType.ProgressReport; }
                    else if (src._bcgov_projectexpenditure_value != null) { dest.RecordType = Managers.Intake.RecordType.Invoice; }
                    else if (src._bcgov_projectbudgetforecastid_value != null) { dest.RecordType = Managers.Intake.RecordType.ForecastReport; }
                    else if (src._drr_requestid_value != null) { dest.RecordType = Managers.Intake.RecordType.ConditionRequest; }
                    else if (src._bcgov_projectid_value != null) { dest.RecordType = Managers.Intake.RecordType.Project; }
                    else { dest.RecordType = Managers.Intake.RecordType.None; }
                })
                ;
        }
    }
}
