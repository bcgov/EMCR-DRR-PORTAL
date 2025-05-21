using AutoMapper;
using EMCR.DRR.API.Model;
using EMCR.DRR.Controllers;
using EMCR.Utilities.Extensions;

namespace EMCR.DRR.Managers.Intake
{
    public class IntakeMapperProfile : Profile
    {
        public IntakeMapperProfile()
        {
            CreateMap<EoiApplication, Application>(MemberList.None)
                .ForMember(dest => dest.AdditionalContact1, opt => opt.MapFrom(src => src.AdditionalContacts.FirstOrDefault()))
                .ForMember(dest => dest.AdditionalContact2, opt => opt.MapFrom(src => src.AdditionalContacts.ElementAtOrDefault(1)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeStatusMapper(src.Status)))
                .ForMember(dest => dest.ApplicationTypeName, opt => opt.MapFrom(src => "EOI"))
                .ForMember(dest => dest.ProgramName, opt => opt.MapFrom(src => "DRIF"))
                .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => src.Stream))
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.AdditionalContacts, opt => opt.MapFrom(src => DRRAdditionalContactMapper(src.AdditionalContact1, src.AdditionalContact2)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DRRApplicationStatusMapper(src.Status)))
                .ForMember(dest => dest.PartneringProponents, opt => opt.MapFrom(src => src.PartneringProponents.Select(p => p.Name)))
                .ForMember(dest => dest.Stream, opt => opt.MapFrom(src => src.ProjectType))
                ;

            CreateMap<DraftEoiApplication, Application>(MemberList.None)
                .ForMember(dest => dest.AdditionalContact1, opt => opt.MapFrom(src => src.AdditionalContacts.FirstOrDefault()))
                .ForMember(dest => dest.AdditionalContact2, opt => opt.MapFrom(src => src.AdditionalContacts.ElementAtOrDefault(1)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeStatusMapper(src.Status)))
                .ForMember(dest => dest.ApplicationTypeName, opt => opt.MapFrom(src => "EOI"))
                .ForMember(dest => dest.ProgramName, opt => opt.MapFrom(src => "DRIF"))
                .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => src.Stream))
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.AdditionalContacts, opt => opt.MapFrom(src => DRRAdditionalContactMapper(src.AdditionalContact1, src.AdditionalContact2)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DRRApplicationStatusMapper(src.Status)))
                .ForMember(dest => dest.PartneringProponents, opt => opt.MapFrom(src => src.PartneringProponents.Select(p => p.Name)))
                .ForMember(dest => dest.Stream, opt => opt.MapFrom(src => src.ProjectType))
                ;

            CreateMap<FpApplication, Application>(MemberList.None)
                .ForMember(dest => dest.AdditionalContact1, opt => opt.MapFrom(src => src.AdditionalContacts.FirstOrDefault()))
                .ForMember(dest => dest.AdditionalContact2, opt => opt.MapFrom(src => src.AdditionalContacts.ElementAtOrDefault(1)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeStatusMapper(src.Status)))
                .ForMember(dest => dest.ApplicationTypeName, opt => opt.MapFrom(src => "Full Proposal"))
                .ForMember(dest => dest.ProgramName, opt => opt.MapFrom(src => "DRIF"))
                .ForMember(dest => dest.EstimatedTotal, opt => opt.MapFrom(src => src.TotalProjectCost))
                .AfterMap((_, dest) =>
                {
                    int i = 1;
                    if (dest.ProposedActivities != null)
                    {
                        foreach (var activity in dest.ProposedActivities)
                        {
                            activity.ActivityNumber = i++;
                        }
                    }
                    i = 1;
                    if (dest.CostEstimates != null)
                    {
                        foreach (var cost in dest.CostEstimates)
                        {
                            cost.TaskNumber = i++;
                        }
                    }

                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.AdditionalContacts, opt => opt.MapFrom(src => DRRAdditionalContactMapper(src.AdditionalContact1, src.AdditionalContact2)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DRRApplicationStatusMapper(src.Status)))
                .ForMember(dest => dest.PartneringProponents, opt => opt.MapFrom(src => src.PartneringProponents.Select(p => p.Name)))
                .ForMember(dest => dest.Professionals, opt => opt.MapFrom(src => src.Professionals.Select(p => p.Name)))
                .ForMember(dest => dest.FoundationalOrPreviousWorks, opt => opt.MapFrom(src => src.FoundationalOrPreviousWorks.Select(p => p.Name)))
                .ForMember(dest => dest.AffectedParties, opt => opt.MapFrom(src => src.AffectedParties.Select(p => p.Name)))
                .ForMember(dest => dest.CostReductions, opt => opt.MapFrom(src => src.CostReductions.Select(p => p.Name)))
                .ForMember(dest => dest.CoBenefits, opt => opt.MapFrom(src => src.CoBenefits.Select(p => p.Name)))
                .ForMember(dest => dest.IncreasedResiliency, opt => opt.MapFrom(src => src.IncreasedResiliency.Select(p => p.Name)))
                .ForMember(dest => dest.ComplexityRisks, opt => opt.MapFrom(src => src.ComplexityRisks.Select(p => p.Name)))
                .ForMember(dest => dest.ReadinessRisks, opt => opt.MapFrom(src => src.ReadinessRisks.Select(p => p.Name)))
                .ForMember(dest => dest.SensitivityRisks, opt => opt.MapFrom(src => src.SensitivityRisks.Select(p => p.Name)))
                .ForMember(dest => dest.CapacityRisks, opt => opt.MapFrom(src => src.CapacityRisks.Select(p => p.Name)))
                .ForMember(dest => dest.ClimateAssessmentTools, opt => opt.MapFrom(src => src.ClimateAssessmentTools.Select(p => p.Name)))
                .ForMember(dest => dest.CostConsiderations, opt => opt.MapFrom(src => src.CostConsiderations.Select(p => p.Name)))
                .ForMember(dest => dest.TotalProjectCost, opt => opt.MapFrom(src => src.EstimatedTotal))
                .ForMember(dest => dest.OriginalTotalProjectCost, opt => opt.MapFrom(src => src.EstimatedTotalFromEOI))
                //.ForMember(dest => dest.EligibleFundingRequest, opt => opt.MapFrom(src => src.EligibleAmountForFP))
                .ForMember(dest => dest.Permits, opt => opt.MapFrom(src => src.Permits.Select(p => p.Name)))
                ;

            CreateMap<DraftFpApplication, Application>(MemberList.None)
                .ForMember(dest => dest.AdditionalContact1, opt => opt.MapFrom(src => src.AdditionalContacts.FirstOrDefault()))
                .ForMember(dest => dest.AdditionalContact2, opt => opt.MapFrom(src => src.AdditionalContacts.ElementAtOrDefault(1)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeStatusMapper(src.Status)))
                .ForMember(dest => dest.ApplicationTypeName, opt => opt.MapFrom(src => "Full Proposal"))
                .ForMember(dest => dest.ProgramName, opt => opt.MapFrom(src => "DRIF"))
                .ForMember(dest => dest.EstimatedTotal, opt => opt.MapFrom(src => src.TotalProjectCost))
                .AfterMap((_, dest) =>
                {
                    int i = 1;
                    if (dest.ProposedActivities != null)
                    {
                        foreach (var activity in dest.ProposedActivities)
                        {
                            activity.ActivityNumber = i++;
                        }
                    }

                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.AdditionalContacts, opt => opt.MapFrom(src => DRRAdditionalContactMapper(src.AdditionalContact1, src.AdditionalContact2)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DRRApplicationStatusMapper(src.Status)))
                .ForMember(dest => dest.PartneringProponents, opt => opt.MapFrom(src => src.PartneringProponents.Select(p => p.Name)))
                .ForMember(dest => dest.Professionals, opt => opt.MapFrom(src => src.Professionals.Select(p => p.Name)))
                .ForMember(dest => dest.FoundationalOrPreviousWorks, opt => opt.MapFrom(src => src.FoundationalOrPreviousWorks.Select(p => p.Name)))
                .ForMember(dest => dest.AffectedParties, opt => opt.MapFrom(src => src.AffectedParties.Select(p => p.Name)))
                .ForMember(dest => dest.CostReductions, opt => opt.MapFrom(src => src.CostReductions.Select(p => p.Name)))
                .ForMember(dest => dest.CoBenefits, opt => opt.MapFrom(src => src.CoBenefits.Select(p => p.Name)))
                .ForMember(dest => dest.IncreasedResiliency, opt => opt.MapFrom(src => src.IncreasedResiliency.Select(p => p.Name)))
                .ForMember(dest => dest.ComplexityRisks, opt => opt.MapFrom(src => src.ComplexityRisks.Select(p => p.Name)))
                .ForMember(dest => dest.ReadinessRisks, opt => opt.MapFrom(src => src.ReadinessRisks.Select(p => p.Name)))
                .ForMember(dest => dest.SensitivityRisks, opt => opt.MapFrom(src => src.SensitivityRisks.Select(p => p.Name)))
                .ForMember(dest => dest.CapacityRisks, opt => opt.MapFrom(src => src.CapacityRisks.Select(p => p.Name)))
                .ForMember(dest => dest.ClimateAssessmentTools, opt => opt.MapFrom(src => src.ClimateAssessmentTools.Select(p => p.Name)))
                .ForMember(dest => dest.CostConsiderations, opt => opt.MapFrom(src => src.CostConsiderations.Select(p => p.Name)))
                .ForMember(dest => dest.TotalProjectCost, opt => opt.MapFrom(src => src.EstimatedTotal))
                .ForMember(dest => dest.OriginalTotalProjectCost, opt => opt.MapFrom(src => src.EstimatedTotalFromEOI))
                //.ForMember(dest => dest.EligibleFundingRequest, opt => opt.MapFrom(src => src.EligibleAmountForFP))
                .ForMember(dest => dest.Permits, opt => opt.MapFrom(src => src.Permits.Select(p => p.Name)))
                ;

            CreateMap<Project, ProjectListItem>(MemberList.None)
                ;

            CreateMap<DraftDrrProject, Project>(MemberList.None)
                .ForMember(dest => dest.FirstReportPeriod, opt => opt.Ignore())
                .ForMember(dest => dest.Requests, opt => opt.MapFrom(src => src.ConditionRequests))
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.PartneringProponents, opt => opt.MapFrom(src => src.PartneringProponents != null ? src.PartneringProponents.Select(p => p.Name) : Array.Empty<string>()))
                .ForMember(dest => dest.ConditionRequests, opt => opt.MapFrom(src => src.Requests))
                .AfterMap((src, dest) =>
                {
                    //add any payment conditions that don't have a request to the condition requests - with action "request to clear" 
                    if (dest.Conditions != null && dest.ConditionRequests != null)
                    {
                        var uncreatedConditions = dest.Conditions.Where(c => !dest.ConditionRequests.Any(r => r.ConditionId == c.Id));
                        foreach (var c in uncreatedConditions)
                        {
                            var actionsToAppend = !string.IsNullOrEmpty(c.ConditionName) && !c.ConditionName.Equals("Final Report") ? [RequestActions.Create] : Array.Empty<RequestActions>();
                            dest.ConditionRequests = dest.ConditionRequests.Append(new ConditionRequestListItem
                            {
                                Id = null,
                                ConditionId = c.Id,
                                ConditionMet = c.ConditionMet,
                                ConditionName = c.ConditionName,
                                DateMet = c.DateMet,
                                Limit = c.Limit,
                                Status = null,
                                Actions = actionsToAppend
                            }).ToList();
                        }

                        dest.ConditionRequests = dest.ConditionRequests.OrderBy(r => r.Limit).ToList();
                    }
                })
                ;

            CreateMap<Controllers.PaymentCondition, PaymentCondition>()
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;

            CreateMap<Controllers.CostProjectionItem, CostProjectionItem>()
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;

            CreateMap<DraftProjectClaim, ProjectClaim>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeClaimStatusMapper(src.Status)))
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DrrClaimStatusMapper(src.Status)))
                .ForMember(dest => dest.ActiveCondition, opt => opt.Ignore())
                ;

            CreateMap<DraftProjectClaim, Controllers.ProjectClaim>()
                .ForMember(dest => dest.AuthorizedRepresentativeStatement, opt => opt.Ignore())
                .ForMember(dest => dest.InformationAccuracyStatement, opt => opt.Ignore())
                .ReverseMap()
                ;

            CreateMap<Controllers.ProjectClaim, ProjectClaim>()
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.ActiveCondition, opt => opt.Ignore())
                ;

            CreateMap<DraftProgressReport, ProgressReport>()
                .ForMember(dest => dest.CrmId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeProgressReportStatusMapper(src.Status)))
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DrrProgressReportStatusMapper(src.Status)))
                ;

            CreateMap<DraftProgressReport, Controllers.ProgressReport>()
                .ForMember(dest => dest.AuthorizedRepresentativeStatement, opt => opt.Ignore())
                .ForMember(dest => dest.InformationAccuracyStatement, opt => opt.Ignore())
                .ReverseMap()
                ;

            CreateMap<DraftProgressReport, ProgressReportDetails>()
                .ForMember(dest => dest.CrmId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeProgressReportStatusMapper(src.Status)))
                .ForMember(dest => dest.AuthorizedRepresentativeStatement, opt => opt.Ignore())
                .ForMember(dest => dest.InformationAccuracyStatement, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DrrProgressReportStatusMapper(src.Status)))
                ;

            CreateMap<Controllers.ProgressReport, ProgressReport>()
                .ForMember(dest => dest.CrmId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeProgressReportStatusMapper(src.Status)))
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DrrProgressReportStatusMapper(src.Status)))
                .ForMember(dest => dest.AuthorizedRepresentativeStatement, opt => opt.Ignore())
                .ForMember(dest => dest.InformationAccuracyStatement, opt => opt.Ignore())
                ;

            CreateMap<DraftForecast, Controllers.Forecast>()
                .ForMember(dest => dest.AuthorizedRepresentativeStatement, opt => opt.Ignore())
                .ForMember(dest => dest.InformationAccuracyStatement, opt => opt.Ignore())
                .ReverseMap()
                ;

            CreateMap<DraftForecast, Forecast>()
                .ForMember(dest => dest.CrmId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeForecastStatusMapper(src.Status)))
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DrrForecastStatusMapper(src.Status)))
                ;

            CreateMap<Controllers.Forecast, Forecast>()
                .ForMember(dest => dest.CrmId, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;

            CreateMap<Controllers.InterimReport, InterimReport>()
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;

            CreateMap<Controllers.ProjectClaim, ClaimDetails>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeClaimStatusMapper(src.Status)))
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DrrClaimStatusMapper(src.Status)))
                .ForMember(dest => dest.TotalProjectAmount, opt => opt.MapFrom(src => src.Project != null ? src.Project.TotalDRIFFundingRequest : null))
                .AfterMap((src, dest) =>
                {
                    dest.PreviousClaims = CalculateClaimTotals(src);
                })
                ;

            CreateMap<DraftProjectClaim, ClaimDetails>()
                .ForMember(dest => dest.AuthorizedRepresentativeStatement, opt => opt.Ignore())
                .ForMember(dest => dest.InformationAccuracyStatement, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeClaimStatusMapper(src.Status)))
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DrrClaimStatusMapper(src.Status)))
                .ForMember(dest => dest.TotalProjectAmount, opt => opt.MapFrom(src => src.Project != null ? src.Project.TotalDRIFFundingRequest : null))
                .AfterMap((src, dest) =>
                {
                    dest.PreviousClaims = CalculateClaimTotals(src);
                })
                ;

            CreateMap<Controllers.ProgressReport, ProgressReportDetails>()
                .ForMember(dest => dest.CrmId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeProgressReportStatusMapper(src.Status)))
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DrrProgressReportStatusMapper(src.Status)))
                ;

            CreateMap<Controllers.DraftForecast, ForecastDetails>()
                .ForMember(dest => dest.CrmId, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorizedRepresentativeStatement, opt => opt.Ignore())
                .ForMember(dest => dest.InformationAccuracyStatement, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeForecastStatusMapper(src.Status)))
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DrrForecastStatusMapper(src.Status)))
                ;

            CreateMap<Controllers.Forecast, ForecastDetails>()
                .ForMember(dest => dest.CrmId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeForecastStatusMapper(src.Status)))
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DrrForecastStatusMapper(src.Status)))
                ;

            CreateMap<Controllers.ForecastItem, ForecastItem>()
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;

            CreateMap<Controllers.InterimReport, InterimReportDetails>()
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;

            CreateMap<Workplan, WorkplanDetails>()
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;

            CreateMap<EventInformation, EventInformationDetails>()
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;

            CreateMap<Controllers.ProjectEvent, ProjectEventDetails>()
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;

            CreateMap<PastEvent, PastEventDetails>()
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;

            CreateMap<Controllers.Invoice, Invoice>()
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ReverseMap()
                ;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8629 // Nullable value type may be null.
            CreateMap<Controllers.WorkplanActivity, WorkplanActivityDetails>()
                .ForMember(dest => dest.ActivityType, opt => opt.MapFrom(src => new ActivityType { Name = src.Activity.Value.ToDescriptionString(), PreCreatedActivity = src.PreCreatedActivity }))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.OriginalReportId, opt => opt.Ignore())
                .ForMember(dest => dest.CopiedFromActivity, opt => opt.Ignore())
                .ForMember(dest => dest.ConstructionContractStatus, opt => opt.Ignore())
                .ForMember(dest => dest.PermitToConstructStatus, opt => opt.Ignore())
                .ForMember(dest => dest.ProgressStatus, opt => opt.Ignore())
                .ForMember(dest => dest.ContractAwardDate, opt => opt.Ignore())
                .ForMember(dest => dest.PlannedContractAwardDate, opt => opt.Ignore())
                .ForMember(dest => dest.AwardPermitToConstructDate, opt => opt.Ignore())
                .ForMember(dest => dest.PlannedAwardPermitToConstructDate, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    WorkplanStatusMapper(dest, src);
                    WorkplanAwardDateMapper(dest, src);
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Activity, opt => opt.MapFrom(src => IEnumEx.GetValueFromDescription<Controllers.ActivityType>(src.ActivityType.Name)))
                .ForMember(dest => dest.PreCreatedActivity, opt => opt.MapFrom(src => src.ActivityType != null ? src.ActivityType.PreCreatedActivity : false))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.IsMandatory, opt => opt.MapFrom(src => src.CopiedFromActivity == true))
                .AfterMap((src, dest) =>
                {
                    dest.Status = WorkplanFlatStatusMapper(src, dest.Activity);
                    if (dest.Activity == Controllers.ActivityType.ConstructionContractAward)
                    {
                        dest.PlannedStartDate = src.PlannedContractAwardDate;
                        dest.ActualStartDate = src.ContractAwardDate;
                    }
                    else if (dest.Activity == Controllers.ActivityType.PermitToConstruct)
                    {
                        dest.PlannedStartDate = src.PlannedAwardPermitToConstructDate;
                        dest.ActualStartDate = src.AwardPermitToConstructDate;
                    }
                })
                    ;
#pragma warning restore CS8629 // Nullable value type may be null.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            CreateMap<Controllers.FundingSignage, FundingSignage>()
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;

            CreateMap<Controllers.ProjectEvent, ProjectEvent>()
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;

            CreateMap<Controllers.FundingInformation, FundingInformation>()
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;

            CreateMap<Controllers.YearOverYearFunding, YearOverYearFunding>()
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;

            CreateMap<Controllers.ActiveCondition, ActiveCondition>()
                .ReverseMap()
                ;

            CreateMap<Controllers.ScreenerQuestions, ScreenerQuestions>()
                .ReverseMap()
                ;

            CreateMap<Controllers.ContactDetails, ContactDetails>()
                .ForMember(dest => dest.BCeId, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;

            CreateMap<Controllers.StandardInfo, StandardInfo>()
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Standards, opt => opt.MapFrom(src => src.Standards.Select(p => p.Name)))
                ;

            CreateMap<DraftConditionRequest, Controllers.ConditionRequest>()
                .ForMember(dest => dest.AuthorizedRepresentativeStatement, opt => opt.Ignore())
                .ForMember(dest => dest.InformationAccuracyStatement, opt => opt.Ignore())
                .ReverseMap()
                ;

            CreateMap<Controllers.ConditionRequest, ConditionRequest>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeRequestStatusMapper(src.Status)))
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DrrRequestStatusMapper(src.Status)))
                ;

            CreateMap<DraftConditionRequest, ConditionRequest>()
                .ForMember(dest => dest.AuthorizedRepresentativeStatement, opt => opt.Ignore())
                .ForMember(dest => dest.InformationAccuracyStatement, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeRequestStatusMapper(src.Status)))
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DrrRequestStatusMapper(src.Status)))
                ;

            CreateMap<ConditionRequest, Request>()
                .ForMember(dest => dest.CrmId, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => RequestType.Condition))
                .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => new PaymentCondition
                {
                    Id = src.ConditionId,
                    ConditionName = src.ConditionName,
                    Limit = src.Limit,
                    ConditionMet = src.ConditionMet,
                    DateMet = src.DateMet,
                }))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Explanation))
                .ReverseMap()
                .ForMember(dest => dest.Explanation, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ConditionId, opt => opt.MapFrom(src => src.Condition != null ? src.Condition.Id : null))
                .ForMember(dest => dest.ConditionName, opt => opt.MapFrom(src => src.Condition != null ? src.Condition.ConditionName : null))
                .ForMember(dest => dest.Limit, opt => opt.MapFrom(src => src.Condition != null ? src.Condition.Limit : null))
                .ForMember(dest => dest.ConditionMet, opt => opt.MapFrom(src => src.Condition != null ? src.Condition.ConditionMet : null))
                .ForMember(dest => dest.DateMet, opt => opt.MapFrom(src => src.Condition != null ? src.Condition.DateMet : null))
                ;

            CreateMap<ConditionRequestListItem, Request>()
                .ForMember(dest => dest.CrmId, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => RequestType.Condition))
                .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => new PaymentCondition
                {
                    Id = src.ConditionId,
                    ConditionName = src.ConditionName,
                    Limit = src.Limit,
                    ConditionMet = src.ConditionMet,
                    DateMet = src.DateMet,
                }))
                .ForMember(dest => dest.Description, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorizedRepresentative, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorizedRepresentativeStatement, opt => opt.Ignore())
                .ForMember(dest => dest.InformationAccuracyStatement, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => IntakeRequestStatusMapper(src.Status)))
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.ConditionId, opt => opt.MapFrom(src => src.Condition != null ? src.Condition.Id : null))
                .ForMember(dest => dest.ConditionName, opt => opt.MapFrom(src => src.Condition != null ? src.Condition.ConditionName : null))
                .ForMember(dest => dest.Limit, opt => opt.MapFrom(src => src.Condition != null ? src.Condition.Limit : null))
                .ForMember(dest => dest.ConditionMet, opt => opt.MapFrom(src => src.Condition != null ? src.Condition.ConditionMet : null))
                .ForMember(dest => dest.DateMet, opt => opt.MapFrom(src => src.Condition != null ? src.Condition.DateMet : null))
                .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => Array.Empty<RequestActions>()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DrrRequestStatusMapper(src.Status)))
                .AfterMap((src, dest) =>
                {
                    if (!string.IsNullOrEmpty(dest.ConditionName) && dest.ConditionName.Equals("Final Report")) return;
                    switch (dest.Status)
                    {
                        case Controllers.RequestStatus.Draft:
                        case Controllers.RequestStatus.UpdateNeeded:
                            dest.Actions = dest.Actions.Append(RequestActions.Edit);
                            break;
                        default:
                            dest.Actions = dest.Actions.Append(RequestActions.View);
                            break;
                    }
                })
                ;

            CreateMap<string, PartneringProponent>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
                ;

            CreateMap<Controllers.InfrastructureImpacted, CriticalInfrastructure>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Infrastructure))
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Infrastructure, opt => opt.MapFrom(src => src.Name))
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

            CreateMap<string, FoundationalOrPreviousWork>()
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

            CreateMap<string, ClimateAssessmentToolsInfo>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
               ;

            CreateMap<string, CostConsideration>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
               ;

            CreateMap<string, Permit>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
               ;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            CreateMap<Controllers.ProposedActivity, ProposedActivity>()
                .ForMember(dest => dest.ActivityNumber, opt => opt.Ignore())
                .ForMember(dest => dest.ActivityType, opt => opt.MapFrom(src => new ActivityType { Name = src.Activity != null ? src.Activity.Value.ToDescriptionString() : string.Empty, PreCreatedActivity = src.PreCreatedActivity }))
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.PreCreatedActivity, opt => opt.MapFrom(src => src.ActivityType != null ? src.ActivityType.PreCreatedActivity : false))
                .ForMember(dest => dest.Activity, opt => opt.MapFrom(src => IEnumEx.GetValueFromDescription<Controllers.ActivityType>(src.ActivityType.Name)))
                ;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.

            CreateMap<Controllers.CostEstimate, CostEstimate>()
                .ForMember(dest => dest.TaskNumber, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;

            CreateMap<Resources.Applications.EntitiesQueryResult, EntitiesQueryResult>()
                .ReverseMap()
                ;

            CreateMap<Attachment, BcGovDocument>()
                .AfterMap((src, dest) =>
                {
                    foreach (var prop in dest.GetType().GetProperties())
                    {
                        if (prop.PropertyType == typeof(string) && prop.GetValue(dest) == null)
                        {
                            prop.SetValue(dest, "");
                        }
                    }
                })
                .ReverseMap()
                ;
        }

        private IEnumerable<PreviousClaim> CalculateClaimTotals(ClaimDetails claim)
        {
            var ret = new List<PreviousClaim>();
            if (claim.Project != null && claim.Project.Claims != null)
            {
                var previousClaims = claim.Project.Claims.Where(c => c.ReportDate < claim.ReportDate).ToList();
                var allInvoices = previousClaims.Where(c => c.Id != claim.Id).SelectMany(c => c.Invoices ?? Enumerable.Empty<Invoice>()).ToList();
                foreach (CostCategory category in Enum.GetValues(typeof(CostCategory)))
                {

                    var categoryInvoices = allInvoices.Where(i => i.CostCategory == category).ToList();
                    var categoryTotal = categoryInvoices.Select(i => i.ClaimAmount).Sum();

                    var categoryEstimates = claim.Project.FullProposal?.CostEstimates?.Where(est => est.CostCategory == category).ToList() ?? [];
                    var estimateTotal = categoryEstimates.Select(est => est.TotalCost).Sum();

                    if (estimateTotal > 0 || categoryTotal > 0)
                    {
                        ret.Add(new PreviousClaim
                        {
                            CostCategory = (Controllers.CostCategory?)category,
                            TotalForProject = categoryTotal,
                            OriginalEstimate = estimateTotal
                        });
                    }
                }
            }

            return ret;
        }

        private IEnumerable<ContactDetails> DRRAdditionalContactMapper(ContactDetails? contact1, ContactDetails? contact2)
        {
            var ret = new List<ContactDetails>();
            if (contact1 != null) ret.Add(contact1);
            if (contact2 != null) ret.Add(contact2);
            return ret;
        }

#pragma warning disable CS8629 // Nullable value type may be null.
#pragma warning disable CS8604 // Possible null reference argument.
        private Controllers.WorkplanStatus? WorkplanFlatStatusMapper(WorkplanActivityDetails workplanDetails, Controllers.ActivityType? activityType)
        {
            if (workplanDetails.Status == WorkplanStatus.NoLongerNeeded)
            {
                return Controllers.WorkplanStatus.NoLongerNeeded;
            }

            if (activityType == null) return null;

            switch (activityType)
            {
                case Controllers.ActivityType.ConstructionContractAward:
                    {
                        return workplanDetails.ConstructionContractStatus != null ? Enum.Parse<Controllers.WorkplanStatus>(workplanDetails.ConstructionContractStatus.ToString()) : null;
                    }
                case Controllers.ActivityType.PermitToConstruct:
                    {
                        return workplanDetails.PermitToConstructStatus != null ? Enum.Parse<Controllers.WorkplanStatus>(workplanDetails.PermitToConstructStatus.ToString()) : null;
                    }
                default:
                    {
                        return workplanDetails.ProgressStatus != null ? Enum.Parse<Controllers.WorkplanStatus>(workplanDetails.ProgressStatus.ToString()) : null;
                    }
            }
        }

        private void WorkplanAwardDateMapper(WorkplanActivityDetails dest, WorkplanActivity src)
        {
            switch (src.Activity)
            {
                case Controllers.ActivityType.PermitToConstruct:
                    dest.AwardPermitToConstructDate = src.ActualStartDate;
                    dest.PlannedAwardPermitToConstructDate = src.PlannedStartDate;
                    dest.ActualStartDate = null;
                    dest.PlannedStartDate = null;
                    break;
                case Controllers.ActivityType.ConstructionContractAward:
                    dest.ContractAwardDate = src.ActualStartDate;
                    dest.PlannedContractAwardDate = src.PlannedStartDate;
                    dest.ActualStartDate = null;
                    dest.PlannedStartDate = null;
                    break;
                default:
                    break;
            }
        }

        private void WorkplanStatusMapper(WorkplanActivityDetails dest, WorkplanActivity src)
        {
            if (src.Status == Controllers.WorkplanStatus.NoLongerNeeded)
            {
                dest.Status = WorkplanStatus.NoLongerNeeded;
                return;
            }
            dest.Status = WorkplanStatus.Active;
            if (src.Activity == null) return;
            switch (src.Activity)
            {
                case Controllers.ActivityType.ConstructionContractAward:
                    {
                        dest.ProgressStatus = null;
                        dest.PermitToConstructStatus = null;
                        dest.ConstructionContractStatus = src.Status != null ? Enum.Parse<ConstructionContractStatus>(src.Status.ToString()) : null;
                        break;
                    }
                case Controllers.ActivityType.PermitToConstruct:
                    {
                        dest.ProgressStatus = null;
                        dest.ConstructionContractStatus = null;
                        dest.PermitToConstructStatus = src.Status != null ? Enum.Parse<PermitToConstructStatus>(src.Status.ToString()) : null;
                        break;
                    }
                default:
                    {
                        dest.ConstructionContractStatus = null;
                        dest.PermitToConstructStatus = null;
                        dest.ProgressStatus = src.Status != null && src.Status != Controllers.WorkplanStatus.NoLongerNeeded ? Enum.Parse<WorkplanProgress>(src.Status.ToString()) : null;
                        break;
                    }
            }
        }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8629 // Nullable value type may be null.

        private ProgressReportStatus IntakeProgressReportStatusMapper(Controllers.ProgressReportStatus? status)
        {
            switch (status)
            {
                case Controllers.ProgressReportStatus.NotStarted:
                    return ProgressReportStatus.NotStarted;
                case Controllers.ProgressReportStatus.Draft:
                    return ProgressReportStatus.DraftProponent;
                case Controllers.ProgressReportStatus.Submitted:
                    return ProgressReportStatus.Submitted;
                case Controllers.ProgressReportStatus.UpdateNeeded:
                    return ProgressReportStatus.UpdateNeeded;
                case Controllers.ProgressReportStatus.Approved:
                    return ProgressReportStatus.Approved;
                case Controllers.ProgressReportStatus.Skipped:
                    return ProgressReportStatus.Skipped;
                default: return ProgressReportStatus.DraftProponent;
            }
        }

        private Controllers.ProgressReportStatus DrrProgressReportStatusMapper(ProgressReportStatus? status)
        {
            switch (status)
            {
                case ProgressReportStatus.NotStarted:
                    return Controllers.ProgressReportStatus.NotStarted;
                case ProgressReportStatus.DraftProponent:
                case ProgressReportStatus.DraftStaff:
                    return Controllers.ProgressReportStatus.Draft;
                case ProgressReportStatus.Submitted:
                    return Controllers.ProgressReportStatus.Submitted;
                case ProgressReportStatus.UpdateNeeded:
                    return Controllers.ProgressReportStatus.UpdateNeeded;
                case ProgressReportStatus.Approved:
                    return Controllers.ProgressReportStatus.Approved;
                case ProgressReportStatus.Skipped:
                    return Controllers.ProgressReportStatus.Skipped;
                default: return Controllers.ProgressReportStatus.Draft;
            }
        }

        private ClaimStatus IntakeClaimStatusMapper(Controllers.ClaimStatus? status)
        {
            switch (status)
            {
                case Controllers.ClaimStatus.NotStarted:
                    return ClaimStatus.NotStarted;
                case Controllers.ClaimStatus.Draft:
                    return ClaimStatus.DraftProponent;
                case Controllers.ClaimStatus.Submitted:
                    return ClaimStatus.Submitted;
                case Controllers.ClaimStatus.UpdateNeeded:
                    return ClaimStatus.UpdateNeeded;
                case Controllers.ClaimStatus.Approved:
                    return ClaimStatus.Approved;
                case Controllers.ClaimStatus.Skipped:
                    return ClaimStatus.Skipped;
                default: return ClaimStatus.DraftProponent;
            }
        }

        private Controllers.ClaimStatus DrrClaimStatusMapper(ClaimStatus? status)
        {
            switch (status)
            {
                case ClaimStatus.NotStarted:
                    return Controllers.ClaimStatus.NotStarted;
                case ClaimStatus.DraftProponent:
                case ClaimStatus.DraftStaff:
                    return Controllers.ClaimStatus.Draft;
                case ClaimStatus.Submitted:
                    return Controllers.ClaimStatus.Submitted;
                case ClaimStatus.UpdateNeeded:
                    return Controllers.ClaimStatus.UpdateNeeded;
                case ClaimStatus.Approved:
                    return Controllers.ClaimStatus.Approved;
                case ClaimStatus.Skipped:
                    return Controllers.ClaimStatus.Skipped;
                default: return Controllers.ClaimStatus.Draft;
            }
        }

        private ForecastStatus IntakeForecastStatusMapper(Controllers.ForecastStatus? status)
        {
            switch (status)
            {
                case Controllers.ForecastStatus.NotStarted:
                    return ForecastStatus.NotStarted;
                case Controllers.ForecastStatus.Draft:
                    return ForecastStatus.DraftProponent;
                case Controllers.ForecastStatus.Submitted:
                    return ForecastStatus.Submitted;
                case Controllers.ForecastStatus.UpdateNeeded:
                    return ForecastStatus.UpdateNeeded;
                case Controllers.ForecastStatus.Approved:
                    return ForecastStatus.Approved;
                case Controllers.ForecastStatus.Skipped:
                    return ForecastStatus.Skipped;
                default: return ForecastStatus.DraftProponent;
            }
        }

        private Controllers.ForecastStatus DrrForecastStatusMapper(ForecastStatus? status)
        {
            switch (status)
            {
                case ForecastStatus.NotStarted:
                    return Controllers.ForecastStatus.NotStarted;
                case ForecastStatus.DraftProponent:
                case ForecastStatus.DraftStaff:
                    return Controllers.ForecastStatus.Draft;
                case ForecastStatus.Submitted:
                    return Controllers.ForecastStatus.Submitted;
                case ForecastStatus.UpdateNeeded:
                    return Controllers.ForecastStatus.UpdateNeeded;
                case ForecastStatus.Approved:
                    return Controllers.ForecastStatus.Approved;
                case ForecastStatus.Skipped:
                    return Controllers.ForecastStatus.Skipped;
                default: return Controllers.ForecastStatus.Draft;
            }
        }

        private SubmissionPortalStatus DRRApplicationStatusMapper(ApplicationStatus status)
        {
            switch (status)
            {
                case ApplicationStatus.Approved:
                    return SubmissionPortalStatus.Approved;
                case ApplicationStatus.ApprovedInPrinciple:
                    return SubmissionPortalStatus.ApprovedInPrinciple;
                case ApplicationStatus.Closed:
                    return SubmissionPortalStatus.Closed;
                case ApplicationStatus.DraftProponent:
                case ApplicationStatus.DraftStaff:
                    return SubmissionPortalStatus.Draft;
                case ApplicationStatus.Invited:
                    return SubmissionPortalStatus.EligibleInvited;
                case ApplicationStatus.InPool:
                    return SubmissionPortalStatus.EligiblePending;
                case ApplicationStatus.FPSubmitted:
                    return SubmissionPortalStatus.FullProposalSubmitted;
                case ApplicationStatus.Ineligible:
                    return SubmissionPortalStatus.Ineligible;
                case ApplicationStatus.Submitted:
                case ApplicationStatus.InReview:
                    return SubmissionPortalStatus.UnderReview;
                case ApplicationStatus.Withdrawn:
                    return SubmissionPortalStatus.Withdrawn;
                case ApplicationStatus.Deleted:
                    return SubmissionPortalStatus.Deleted;
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

        private RequestStatus IntakeRequestStatusMapper(Controllers.RequestStatus? status)
        {
            switch (status)
            {
                case Controllers.RequestStatus.Draft:
                    return RequestStatus.DraftProponent;
                case Controllers.RequestStatus.Submitted:
                    return RequestStatus.Submitted;
                case Controllers.RequestStatus.UpdateNeeded:
                    return RequestStatus.UpdateNeeded;
                case Controllers.RequestStatus.Approved:
                    return RequestStatus.Approved;
                default: return RequestStatus.DraftProponent;
            }
        }

        private Controllers.RequestStatus DrrRequestStatusMapper(RequestStatus? status)
        {
            switch (status)
            {
                case RequestStatus.DraftProponent:
                case RequestStatus.DraftStaff:
                    return Controllers.RequestStatus.Draft;

                case RequestStatus.Submitted:
                case RequestStatus.TechnicalReview:
                case RequestStatus.ApprovalReview:
                    return Controllers.RequestStatus.Submitted;

                case RequestStatus.UpdateNeeded:
                    return Controllers.RequestStatus.UpdateNeeded;

                case RequestStatus.Approved:
                    return Controllers.RequestStatus.Approved;

                default: return Controllers.RequestStatus.Draft;
            }

            //DraftProponent,
            //DraftStaff,
            //Submitted,
            //TechnicalReview,
            //ApprovalReview,
            //ReadyForApproval,
            //UpdateNeeded,
            //Approved,
            //Inactive,
        }
    }
}
