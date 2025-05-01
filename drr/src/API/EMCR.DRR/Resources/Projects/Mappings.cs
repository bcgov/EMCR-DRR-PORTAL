using AutoMapper;
using EMCR.DRR.Dynamics;
using EMCR.DRR.Managers.Intake;
using EMCR.DRR.Resources.Applications;
using Microsoft.Dynamics.CRM;

namespace EMCR.DRR.API.Resources.Projects
{
    public class ProjectMapperProfile : Profile
    {
        public ProjectMapperProfile()
        {
#pragma warning disable CS8629 // Nullable value type may be null.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            CreateMap<Project, drr_project>(MemberList.None)
                //.ForMember(dest => dest.drr_name, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.drr_name))
                .ForMember(dest => dest.EoiId, opt => opt.MapFrom(src => src.drr_Case.drr_EOIApplication.drr_name))
                .ForMember(dest => dest.FpId, opt => opt.MapFrom(src => src.drr_FullProposalApplication.drr_name))
                .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => src.drr_projectname))
                .ForMember(dest => dest.ContractNumber, opt => opt.MapFrom(src => src.drr_contractnumber))
                .ForMember(dest => dest.ProponentName, opt => opt.MapFrom(src => src.drr_ProponentName.name))
                .ForMember(dest => dest.FundingStream, opt => opt.MapFrom(src => (int?)Enum.Parse<FundingStream>(((FundingStreamOptionSet)src.drr_projecttype).ToString())))
                //.ForMember(dest => dest.ProjectNumber, opt => opt.MapFrom(src => src.drr_name))
                .ForMember(dest => dest.ProjectNumber, opt => opt.Ignore())
                .ForMember(dest => dest.ProgramType, opt => opt.MapFrom(src => src.drr_Program.drr_name))
                .ForMember(dest => dest.ReportingScheduleType, opt => opt.MapFrom(src => src.drr_ReportingSchedule.drr_name))
                .ForMember(dest => dest.FirstReportPeriod, opt => opt.MapFrom(src => src.drr_firstreportdue != null ? src.drr_firstreportdue.drr_name : string.Empty))
                .ForMember(dest => dest.CostProjections, opt => opt.MapFrom(src => src.drr_drr_project_drr_driffundingrequest_Project.Where(c => c.statecode == (int)EntityState.Active)))
                .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.drr_drr_project_drr_projectcondition_Project.Where(c => c.statecode == (int)EntityState.Active)))
                .ForMember(dest => dest.FundingAmount, opt => opt.MapFrom(src => src.drr_fundingamount))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.drr_plannedstartdate.HasValue ? src.drr_plannedstartdate.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.drr_plannedenddate.HasValue ? src.drr_plannedenddate.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.statuscode.HasValue ? (int?)Enum.Parse<ProjectStatus>(((ProjectStatusOptionSet)src.statuscode).ToString()) : null))
                //.ForMember(dest => dest.Contacts, opt => opt.MapFrom(src => src.con))
                .ForMember(dest => dest.Contacts, opt => opt.Ignore())
                .ForMember(dest => dest.InterimReports, opt => opt.MapFrom(src => src.drr_drr_project_drr_projectreport_Project.Where(c => c.statecode == (int)EntityState.Active)))
                .ForMember(dest => dest.Claims, opt => opt.MapFrom(src => src.drr_drr_project_drr_projectclaim_Project.Where(c => c.statecode == (int)EntityState.Active)))
                .ForMember(dest => dest.ProgressReports, opt => opt.MapFrom(src => src.drr_drr_project_drr_projectprogress_Project.Where(c => c.statecode == (int)EntityState.Active)))
                .ForMember(dest => dest.Forecast, opt => opt.MapFrom(src => src.drr_drr_project_drr_projectbudgetforecast_Project.Where(c => c.statecode == (int)EntityState.Active)))
                .ForMember(dest => dest.Events, opt => opt.MapFrom(src => src.drr_drr_project_drr_projectevent_Project.Where(c => c.statecode == (int)EntityState.Active)))
                .ForMember(dest => dest.Requests, opt => opt.MapFrom(src => src.drr_project_drr_request_ProjectId.Where(c => c.statecode == (int)EntityState.Active)))
                //.ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.doc))
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                ;

            CreateMap<InterimReport, drr_projectreport>(MemberList.None)
                .ForMember(dest => dest.drr_name, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.drr_name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.drr_progressdescription))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.drr_reportduedate.HasValue ? src.drr_reportduedate.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.ReportDate, opt => opt.MapFrom(src => src.drr_reportdate.HasValue ? src.drr_reportdate.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.statuscode.HasValue ? (int?)Enum.Parse<InterimReportStatus>(((ProjectReportStatusOptionSet)src.statuscode).ToString()) : null))
                .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => src.drr_projecttype.HasValue ? (int?)Enum.Parse<InterimProjectType>(((FundingStreamOptionSet)src.drr_projecttype).ToString()) : null))
                .ForMember(dest => dest.PeriodType, opt => opt.MapFrom(src => src.drr_periodtype.HasValue ? (int?)Enum.Parse<PeriodType>(((PeriodTypeOptionSet)src.drr_periodtype).ToString()) : null))
                .ForMember(dest => dest.ReportPeriod, opt => opt.MapFrom(src => src.drr_ReportPeriod != null ? src.drr_ReportPeriod.drr_name : string.Empty))
                .ForMember(dest => dest.ProjectClaim, opt => opt.MapFrom(src => src.drr_ClaimReport))
                .ForMember(dest => dest.ProgressReport, opt => opt.MapFrom(src => src.drr_ProgressReport))
                .ForMember(dest => dest.Forecast, opt => opt.MapFrom(src => src.drr_BudgetForecast))
            ;

            CreateMap<ProjectClaim, drr_projectclaim>(MemberList.None)
                .ForMember(dest => dest.drr_name, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.drr_name))
                .ForMember(dest => dest.ReportPeriod, opt => opt.MapFrom(src => src.drr_ProjectReport != null ? src.drr_ProjectReport.drr_ReportPeriod != null ? src.drr_ProjectReport.drr_ReportPeriod.drr_name : string.Empty : string.Empty))
                .ForMember(dest => dest.ContractNumber, opt => opt.MapFrom(src => src.drr_Project != null ? src.drr_Project.drr_contractnumber : string.Empty))
                .ForMember(dest => dest.ReportDate, opt => opt.MapFrom(src => src.drr_ProjectReport != null ? src.drr_ProjectReport.drr_reportdate.HasValue ? src.drr_ProjectReport.drr_reportdate.Value.UtcDateTime : (DateTime?)null : (DateTime?)null))
                .ForMember(dest => dest.DateApproved, opt => opt.MapFrom(src => src.drr_dateapproved.HasValue ? src.drr_dateapproved.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.DateSubmitted, opt => opt.MapFrom(src => src.drr_datesubmitted.HasValue ? src.drr_datesubmitted.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.statuscode.HasValue ? (int?)Enum.Parse<ClaimStatus>(((ProjectClaimStatusOptionSet)src.statuscode).ToString()) : null))
            ;

            CreateMap<ProgressReport, drr_projectprogress>(MemberList.None)
                .ForMember(dest => dest.drr_name, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.drr_name))
                .ForMember(dest => dest.ReportPeriod, opt => opt.MapFrom(src => src.drr_ProjectReport != null ? src.drr_ProjectReport.drr_ReportPeriod != null ? src.drr_ProjectReport.drr_ReportPeriod.drr_name : string.Empty : string.Empty))
                .ForMember(dest => dest.CrmId, opt => opt.MapFrom(src => src.drr_projectprogressid.ToString()))
                .ForMember(dest => dest.ReportDate, opt => opt.MapFrom(src => src.drr_ProjectReport != null ? src.drr_ProjectReport.drr_reportdate.HasValue ? src.drr_ProjectReport.drr_reportdate.Value.UtcDateTime : (DateTime?)null : (DateTime?)null))
                .ForMember(dest => dest.DateApproved, opt => opt.MapFrom(src => src.drr_dateapproved.HasValue ? src.drr_dateapproved.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.DateSubmitted, opt => opt.MapFrom(src => src.drr_datesubmitted.HasValue ? src.drr_datesubmitted.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.drr_duedate.HasValue ? src.drr_duedate.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.statuscode.HasValue ? (int?)Enum.Parse<ProgressReportStatus>(((ProjectProgressReportStatusOptionSet)src.statuscode).ToString()) : null))
            ;

            CreateMap<Forecast, drr_projectbudgetforecast>(MemberList.None)
                .ForMember(dest => dest.drr_name, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.drr_name))
                .ForMember(dest => dest.CrmId, opt => opt.MapFrom(src => src.drr_projectbudgetforecastid))
                .ForMember(dest => dest.ReportPeriod, opt => opt.MapFrom(src => src.drr_ProjectReport != null ? src.drr_ProjectReport.drr_ReportPeriod != null ? src.drr_ProjectReport.drr_ReportPeriod.drr_name : string.Empty : string.Empty))
                .ForMember(dest => dest.ReportDate, opt => opt.MapFrom(src => src.drr_ProjectReport != null ? src.drr_ProjectReport.drr_reportdate.HasValue ? src.drr_ProjectReport.drr_reportdate.Value.UtcDateTime : (DateTime?)null : (DateTime?)null))
                .ForMember(dest => dest.DateSubmitted, opt => opt.MapFrom(src => src.drr_submissiondate.HasValue ? src.drr_submissiondate.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.DateApproved, opt => opt.MapFrom(src => src.drr_dateapproved.HasValue ? src.drr_dateapproved.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.drr_totalprojectremainingexpenditure))
                .ForMember(dest => dest.OriginalForecast, opt => opt.MapFrom(src => src.drr_totaloriginalforecast))
                .ForMember(dest => dest.Variance, opt => opt.MapFrom(src => src.drr_variance))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.statuscode.HasValue ? (int?)Enum.Parse<ForecastStatus>(((ForecastStatusOptionSet)src.statuscode).ToString()) : null))
            ;

            CreateMap<CostProjectionItem, drr_driffundingrequest>(MemberList.None)
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.FiscalYear, opt => opt.MapFrom(src => src.drr_FiscalYear.drr_name))
                .ForMember(dest => dest.OriginalForecast, opt => opt.MapFrom(src => src.drr_drifprogramfundingrequest))
                .ForMember(dest => dest.CurrentForecast, opt => opt.MapFrom(src => src.drr_currentforecast))
                ;

            CreateMap<ProjectEvent, drr_projectevent>(MemberList.None)
                .ForMember(dest => dest.drr_name, opt => opt.Ignore())
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.statuscode.HasValue ? (int?)Enum.Parse<EventStatus>(((EventStatusOptionSet)src.statuscode).ToString()) : null))
            ;

            CreateMap<PaymentCondition, drr_projectcondition>(MemberList.None)
                .ForMember(dest => dest.drr_name, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.drr_conditionpercentagelimit, opt => opt.MapFrom(src => src.Limit))
                .ReverseMap()
                .ValidateMemberList(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.drr_name))
                .ForMember(dest => dest.ConditionName, opt => opt.MapFrom(src => src.drr_Condition.drr_name))
                .ForMember(dest => dest.Limit, opt => opt.MapFrom(src => src.drr_conditionpercentagelimit))
                .ForMember(dest => dest.DateMet, opt => opt.MapFrom(src => src.drr_conditionmetdate.HasValue ? src.drr_conditionmetdate.Value.UtcDateTime : (DateTime?)null))
                .ForMember(dest => dest.ConditionMet, opt => opt.MapFrom(src => src.drr_conditionmet.HasValue ? src.drr_conditionmet == (int)DRRTwoOptions.Yes : (bool?)null))
            ;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8629 // Nullable value type may be null.
        }
    }
}
