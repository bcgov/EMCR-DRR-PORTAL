using AutoMapper;
using Microsoft.Dynamics.CRM;

namespace EMCR.DRR.API.Resources.Cases
{
    public class CaseMapperProfile : Profile
    {
        public CaseMapperProfile()
        {
            CreateMap<incident, Case>(MemberList.None)
                .ForMember(dest => dest.EoiId, opt => opt.MapFrom(src => src.drr_EOIApplication.drr_name))
                .ForMember(dest => dest.FpId, opt => opt.MapFrom(src => src.drr_FullProposalApplication.drr_name))
                ;
        }
    }
}
