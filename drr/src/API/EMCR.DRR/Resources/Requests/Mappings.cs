using AutoMapper;
using EMCR.DRR.Dynamics;
using EMCR.DRR.Managers.Intake;
using EMCR.DRR.Resources.Applications;
using Microsoft.Dynamics.CRM;

namespace EMCR.DRR.API.Resources.Requests
{
    public class RequestMapperProfile : Profile
    {
        public RequestMapperProfile()
        {
            CreateMap<Request, drr_request>(MemberList.None)
                    .ForMember(dest => dest.drr_name, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.drr_descriptionofrequest, opt => opt.MapFrom(src => src.Description))
                    .ForMember(dest => dest.drr_request_bcgov_documenturl_RequestId, opt => opt.MapFrom(src => src.Attachments))
                    .ForMember(dest => dest.drr_AuthorizedRepresentativeContactId, opt => opt.MapFrom(src => src.AuthorizedRepresentative))
                    .ForMember(dest => dest.drr_authorizedrepresentative, opt => opt.MapFrom(src => src.AuthorizedRepresentativeStatement.HasValue ? src.AuthorizedRepresentativeStatement.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                    .ForMember(dest => dest.drr_accuracyofinformation, opt => opt.MapFrom(src => src.InformationAccuracyStatement.HasValue ? src.InformationAccuracyStatement.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                    .ReverseMap()
                    .ValidateMemberList(MemberList.Destination)
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.drr_name))
                    .ForMember(dest => dest.CrmId, opt => opt.MapFrom(src => src.drr_requestid))
                    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.drr_descriptionofrequest))
                    .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.drr_requesttype.HasValue ? (int?)Enum.Parse<RequestType>(((RequestTypeOptionSet)src.drr_requesttype).ToString()) : null))
                    .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => src.drr_ProjectConditionId))
                    .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.drr_request_bcgov_documenturl_RequestId.Where(c => c.statecode == (int)EntityState.Active)))
                    .ForMember(dest => dest.AuthorizedRepresentative, opt => opt.MapFrom(src => src.drr_AuthorizedRepresentativeContactId))
                    .ForMember(dest => dest.AuthorizedRepresentativeStatement, opt => opt.MapFrom(src => src.drr_authorizedrepresentative == (int)DRRTwoOptions.Yes))
                    .ForMember(dest => dest.InformationAccuracyStatement, opt => opt.MapFrom(src => src.drr_accuracyofinformation == (int)DRRTwoOptions.Yes))
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.statuscode.HasValue ? (int?)Enum.Parse<RequestStatus>(((RequestStatusOptionSet)src.statuscode).ToString()) : null))
                ;

            CreateMap<ConditionRequest, drr_request>(MemberList.None)
                //.ForMember(dest => dest.drr_name, opt => opt.MapFrom(src => src.ConditionId))
                .ForMember(dest => dest.drr_descriptionofrequest, opt => opt.MapFrom(src => src.Explanation))
                .ForMember(dest => dest.drr_request_bcgov_documenturl_RequestId, opt => opt.MapFrom(src => src.Attachments))
                .ForMember(dest => dest.drr_AuthorizedRepresentativeContactId, opt => opt.MapFrom(src => src.AuthorizedRepresentative))
                .ForMember(dest => dest.drr_authorizedrepresentative, opt => opt.MapFrom(src => src.AuthorizedRepresentativeStatement.HasValue ? src.AuthorizedRepresentativeStatement.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ForMember(dest => dest.drr_accuracyofinformation, opt => opt.MapFrom(src => src.InformationAccuracyStatement.HasValue ? src.InformationAccuracyStatement.Value ? (int?)DRRTwoOptions.Yes : (int?)DRRTwoOptions.No : null))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.drr_name))
                .ForMember(dest => dest.ConditionId, opt => opt.MapFrom(src => src.drr_ProjectConditionId.drr_name))
                .ForMember(dest => dest.ConditionName, opt => opt.MapFrom(src => src.drr_ProjectConditionId.drr_Condition.drr_name))
                .ForMember(dest => dest.Limit, opt => opt.MapFrom(src => src.drr_ProjectConditionId.drr_conditionpercentagelimit))
                .ForMember(dest => dest.DateMet, opt => opt.MapFrom(src => src.drr_ProjectConditionId.drr_conditionmetdate.HasValue ? src.drr_ProjectConditionId.drr_conditionmetdate.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.ConditionMet, opt => opt.MapFrom(src => src.drr_ProjectConditionId.drr_conditionmet.HasValue ? src.drr_ProjectConditionId.drr_conditionmet == (int)DRRTwoOptions.Yes : (bool?)null))
                .ForMember(dest => dest.Explanation, opt => opt.MapFrom(src => src.drr_descriptionofrequest))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.drr_request_bcgov_documenturl_RequestId.Where(c => c.statecode == (int)EntityState.Active)))
                .ForMember(dest => dest.AuthorizedRepresentative, opt => opt.MapFrom(src => src.drr_AuthorizedRepresentativeContactId))
                .ForMember(dest => dest.AuthorizedRepresentativeStatement, opt => opt.MapFrom(src => src.drr_authorizedrepresentative == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.InformationAccuracyStatement, opt => opt.MapFrom(src => src.drr_accuracyofinformation == (int)DRRTwoOptions.Yes))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.statuscode.HasValue ? (int?)Enum.Parse<RequestStatus>(((RequestStatusOptionSet)src.statuscode).ToString()) : null))
            ;
        }
    }
}
