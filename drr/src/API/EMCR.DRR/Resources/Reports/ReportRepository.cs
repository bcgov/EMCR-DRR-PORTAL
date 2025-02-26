using AutoMapper;
using EMCR.DRR.API.Resources.Projects;
using EMCR.DRR.API.Services;
using EMCR.DRR.Dynamics;
using EMCR.DRR.Managers.Intake;
using EMCR.Utilities.Extensions;
using Microsoft.Dynamics.CRM;

namespace EMCR.DRR.API.Resources.Reports
{
    public class ReportRepository : IReportRepository
    {
        private readonly IDRRContextFactory dRRContextFactory;
        private readonly IMapper mapper;

        public ReportRepository(IDRRContextFactory dRRContextFactory, IMapper mapper)
        {
            this.mapper = mapper;
            this.dRRContextFactory = dRRContextFactory;
        }

        public async Task<bool> CanAccessProgressReport(string id, string businessId)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();
            var existingProgressReport = await readCtx.drr_projectprogresses.Expand(a => a.drr_Project).Where(a => a.drr_name == id).SingleOrDefaultAsync();
            if (existingProgressReport == null) return true;
            readCtx.AttachTo(nameof(readCtx.drr_projects), existingProgressReport.drr_Project);
            await readCtx.LoadPropertyAsync(existingProgressReport.drr_Project, nameof(drr_project.drr_ProponentName));
            return (!string.IsNullOrEmpty(existingProgressReport.drr_Project.drr_ProponentName.drr_bceidguid)) && existingProgressReport.drr_Project.drr_ProponentName.drr_bceidguid.Equals(businessId);
        }

        public async Task<bool> CanAccessProgressReportFromDocumentId(string id, string businessId)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();
            var document = await readCtx.bcgov_documenturls.Expand(d => d.bcgov_ProgressReport).Where(a => a.bcgov_documenturlid == Guid.Parse(id)).SingleOrDefaultAsync();
            var existingProgressReport = await readCtx.drr_projectprogresses.Expand(a => a.drr_Project).Where(a => a.drr_projectprogressid == document.bcgov_ProgressReport.drr_projectprogressid).SingleOrDefaultAsync();
            if (existingProgressReport == null) return true;
            readCtx.AttachTo(nameof(readCtx.drr_projects), existingProgressReport.drr_Project);
            await readCtx.LoadPropertyAsync(existingProgressReport.drr_Project, nameof(drr_project.drr_ProponentName));
            return (!string.IsNullOrEmpty(existingProgressReport.drr_Project.drr_ProponentName.drr_bceidguid)) && existingProgressReport.drr_Project.drr_ProponentName.drr_bceidguid.Equals(businessId);
        }

        public async Task<ManageReportCommandResult> Manage(ManageReportCommand cmd)
        {
            return cmd switch
            {
                SaveProgressReport c => await HandleSaveProgressReport(c),
                SubmitProgressReport c => await HandleSubmitProgressReport(c),
                CreateProjectReport c => await HandleCreateProjectReport(c),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

#pragma warning disable CS8604 // Possible null reference argument.
        public async Task<ManageReportCommandResult> HandleSaveProgressReport(SaveProgressReport cmd)
        {
            var ctx = dRRContextFactory.Create();
            var existingProgressReport = await ctx.drr_projectprogresses.Where(p => p.drr_name == cmd.ProgressReport.Id).SingleOrDefaultAsync();
            if (existingProgressReport == null) throw new NotFoundException("Progress Report not found");

            var loadTasks = new List<Task>
            {
                ctx.LoadPropertyAsync(existingProgressReport, nameof(drr_projectprogress.drr_drr_projectprogress_drr_projectworkplanactivity_ProjectProgressReport)),
                ctx.LoadPropertyAsync(existingProgressReport, nameof(drr_projectprogress.drr_drr_projectprogress_drr_projectpastevent_ProjectProgress)),
                ctx.LoadPropertyAsync(existingProgressReport, nameof(drr_projectprogress.drr_drr_projectprogress_drr_projectevent_ProjectProgress)),
                ctx.LoadPropertyAsync(existingProgressReport, nameof(drr_projectprogress.drr_drr_projectprogress_drr_temporaryprovincialfundingsignage_ProjectProgress)),
                ctx.LoadPropertyAsync(existingProgressReport, nameof(drr_projectprogress.bcgov_drr_projectprogress_bcgov_documenturl_ProgressReport)),
            };

            await Task.WhenAll(loadTasks);

            ctx.DetachAll();
            var drrProgressReport = mapper.Map<drr_projectprogress>(cmd.ProgressReport);
            drrProgressReport.drr_projectprogressid = existingProgressReport.drr_projectprogressid;
            foreach (var doc in drrProgressReport.bcgov_drr_projectprogress_bcgov_documenturl_ProgressReport)
            {
                var curr = existingProgressReport.bcgov_drr_projectprogress_bcgov_documenturl_ProgressReport.SingleOrDefault(d => d.bcgov_documenturlid == doc.bcgov_documenturlid);
                if (curr != null) curr.bcgov_documentcomments = doc.bcgov_documentcomments;
            }

            drrProgressReport.bcgov_drr_projectprogress_bcgov_documenturl_ProgressReport = existingProgressReport.bcgov_drr_projectprogress_bcgov_documenturl_ProgressReport;

            RemoveOldData(ctx, existingProgressReport, drrProgressReport);
            ctx.AttachTo(nameof(ctx.drr_projectprogresses), drrProgressReport);

            var projectActivityMasterListTask = LoadProjectActivityList(ctx, drrProgressReport);
            await Task.WhenAll([
                projectActivityMasterListTask,
            ]);
            var projectActivityMasterList = projectActivityMasterListTask.Result;

            AddWorkplanActivities(ctx, drrProgressReport, projectActivityMasterList, existingProgressReport);
            AddPastEvents(ctx, drrProgressReport, existingProgressReport);
            await AddUpcomingEvents(ctx, drrProgressReport, existingProgressReport);
            AddFundingSignage(ctx, drrProgressReport, existingProgressReport);
            UpdateDocuments(ctx, drrProgressReport);

            ctx.UpdateObject(drrProgressReport);
            await ctx.SaveChangesAsync();
            ctx.DetachAll();

            return new ManageReportCommandResult { Id = existingProgressReport.drr_name };

        }

        public async Task<ManageReportCommandResult> HandleSubmitProgressReport(SubmitProgressReport cmd)
        {
            var ctx = dRRContextFactory.Create();
            var progressReport = await ctx.drr_projectprogresses.Where(a => a.drr_name == cmd.Id).SingleOrDefaultAsync();
            progressReport.statuscode = (int)ProjectProgressReportStatusOptionSet.Submitted;
            progressReport.drr_datesubmitted = DateTime.UtcNow;
            ctx.UpdateObject(progressReport);
            await ctx.SaveChangesAsync();
            ctx.DetachAll();
            return new ManageReportCommandResult { Id = cmd.Id };
        }
#pragma warning restore CS8604 // Possible null reference argument.
        public async Task<ManageReportCommandResult> HandleCreateProjectReport(CreateProjectReport cmd)
        {
            var ctx = dRRContextFactory.Create();
            var project = await ctx.drr_projects.Expand(p => p.drr_ProponentName).Where(a => a.drr_name == cmd.ProjectId).SingleOrDefaultAsync();
            if (project == null) throw new NotFoundException("Project not found");
            var reportPeriod = await ctx.drr_reportperiods.Where(p => p.drr_name == cmd.ReportPeriodName).SingleOrDefaultAsync();
            if (reportPeriod == null) throw new NotFoundException("Report Period not found");

            var report = new drr_projectreport
            {
                drr_projectreportid = Guid.NewGuid(),
                drr_Project = project,
                drr_Proponent = project.drr_ProponentName,
                drr_periodtype = (int?)Enum.Parse<PeriodTypeOptionSet>(cmd.ReportType.ToString()),
                drr_projecttype = project.drr_projecttype,
                drr_ReportPeriod = reportPeriod,
                drr_reportdate = DateTime.UtcNow,
            };

            ctx.AddTodrr_projectreports(report);
            ctx.SetLink(report, nameof(report.drr_Project), project);
            ctx.SetLink(report, nameof(report.drr_ReportPeriod), reportPeriod);
            await ctx.SaveChangesAsync();

            ctx.DetachAll();

            var createdReport = await ctx.drr_projectreports.Where(r => r.drr_projectreportid == report.drr_projectreportid).SingleOrDefaultAsync();
            //return new ManageReportCommandResult { Id = $"DRIF-{createdReport.drr_autonumber}" };
            return new ManageReportCommandResult { Id = createdReport.drr_name };

        }

        private void RemoveOldData(DRRContext ctx, drr_projectprogress existingProgressReport, drr_projectprogress drrProgressReport)
        {
            var activitiesToRemove = existingProgressReport.drr_drr_projectprogress_drr_projectworkplanactivity_ProjectProgressReport.Where(curr =>
            !drrProgressReport.drr_drr_projectprogress_drr_projectworkplanactivity_ProjectProgressReport.Any(updated => updated.drr_projectworkplanactivityid == curr.drr_projectworkplanactivityid)).ToList();

            foreach (var activity in activitiesToRemove)
            {
                ctx.AttachTo(nameof(ctx.drr_projectworkplanactivities), activity);
                ctx.DeleteObject(activity);
            }

            var upcomingEventsToRemove = existingProgressReport.drr_drr_projectprogress_drr_projectevent_ProjectProgress.Where(curr =>
            !drrProgressReport.drr_drr_projectprogress_drr_projectevent_ProjectProgress.Any(updated => updated.drr_projecteventid == curr.drr_projecteventid)).ToList();

            foreach (var projectEvent in upcomingEventsToRemove)
            {
                ctx.AttachTo(nameof(ctx.drr_projectevents), projectEvent);
                ctx.DeleteObject(projectEvent);
            }

            var pastEventsToRemove = existingProgressReport.drr_drr_projectprogress_drr_projectpastevent_ProjectProgress.Where(curr =>
            !drrProgressReport.drr_drr_projectprogress_drr_projectpastevent_ProjectProgress.Any(updated => updated.drr_projectpasteventid == curr.drr_projectpasteventid)).ToList();

            foreach (var pastEvent in pastEventsToRemove)
            {
                ctx.AttachTo(nameof(ctx.drr_projectpastevents), pastEvent);
                ctx.DeleteObject(pastEvent);
            }

            var signageToRemove = existingProgressReport.drr_drr_projectprogress_drr_temporaryprovincialfundingsignage_ProjectProgress.Where(curr =>
            !drrProgressReport.drr_drr_projectprogress_drr_temporaryprovincialfundingsignage_ProjectProgress.Any(updated => updated.drr_temporaryprovincialfundingsignageid == curr.drr_temporaryprovincialfundingsignageid)).ToList();

            foreach (var signage in signageToRemove)
            {
                ctx.AttachTo(nameof(ctx.drr_temporaryprovincialfundingsignages), signage);
                ctx.DeleteObject(signage);
            }
        }

        private async Task<List<drr_projectactivity>> LoadProjectActivityList(DRRContext ctx, drr_projectprogress drrProgressReport)
        {
            return drrProgressReport.drr_drr_projectprogress_drr_projectworkplanactivity_ProjectProgressReport.Count > 0 ?
                (await ctx.drr_projectactivities.GetAllPagesAsync()).ToList() :
                new List<drr_projectactivity>();
        }

        private static void AddWorkplanActivities(DRRContext drrContext, drr_projectprogress progressReport, List<drr_projectactivity> projectActivityMasterList, drr_projectprogress? oldReport = null)
        {
            foreach (var activity in progressReport.drr_drr_projectprogress_drr_projectworkplanactivity_ProjectProgressReport)
            {
                if (activity != null && !string.IsNullOrEmpty(activity.drr_name))
                {
                    var masterVal = projectActivityMasterList.FirstOrDefault(s => s.drr_name == activity.drr_ActivityType?.drr_name);
                    if (masterVal == null)
                    {
                        masterVal = projectActivityMasterList.FirstOrDefault(s => s.drr_name == "Other");
                    }
                    activity.drr_ActivityType = masterVal;

                    if (activity.drr_projectworkplanactivityid == null ||
                        (oldReport != null && !oldReport.drr_drr_projectprogress_drr_projectworkplanactivity_ProjectProgressReport.Any(a => a.drr_projectworkplanactivityid == activity.drr_projectworkplanactivityid)))
                    {
                        drrContext.AddTodrr_projectworkplanactivities(activity);
                        drrContext.AddLink(progressReport, nameof(progressReport.drr_drr_projectprogress_drr_projectworkplanactivity_ProjectProgressReport), activity);
                        drrContext.SetLink(activity, nameof(activity.drr_ProjectProgressReport), progressReport);
                        drrContext.SetLink(activity, nameof(activity.drr_ActivityType), masterVal);
                    }
                    else
                    {
                        drrContext.AttachTo(nameof(drrContext.drr_projectworkplanactivities), activity);
                        drrContext.UpdateObject(activity);
                        drrContext.SetLink(activity, nameof(activity.drr_ActivityType), masterVal);
                    }
                }
            }
        }

        private static void AddPastEvents(DRRContext drrContext, drr_projectprogress progressReport, drr_projectprogress oldReport)
        {
            foreach (var pastEvent in progressReport.drr_drr_projectprogress_drr_projectpastevent_ProjectProgress)
            {
                if (pastEvent != null)
                {
                    if (pastEvent.drr_projectpasteventid == null ||
                        (oldReport != null && !oldReport.drr_drr_projectprogress_drr_projectpastevent_ProjectProgress.Any(a => a.drr_projectpasteventid == pastEvent.drr_projectpasteventid)))
                    {
                        drrContext.AddTodrr_projectpastevents(pastEvent);
                        drrContext.AddLink(progressReport, nameof(progressReport.drr_drr_projectprogress_drr_projectpastevent_ProjectProgress), pastEvent);
                        drrContext.SetLink(pastEvent, nameof(pastEvent.drr_ProjectProgress), progressReport);
                    }
                    else
                    {
                        drrContext.AttachTo(nameof(drrContext.drr_projectpastevents), pastEvent);
                        drrContext.UpdateObject(pastEvent);
                    }
                }
            }
        }

        private async static Task AddUpcomingEvents(DRRContext drrContext, drr_projectprogress progressReport, drr_projectprogress oldReport)
        {
            foreach (var upcomingEvent in progressReport.drr_drr_projectprogress_drr_projectevent_ProjectProgress)
            {
                if (upcomingEvent != null)
                {
                    if (upcomingEvent.drr_projecteventid == null ||
                        (oldReport != null && !oldReport.drr_drr_projectprogress_drr_projectevent_ProjectProgress.Any(a => a.drr_projecteventid == upcomingEvent.drr_projecteventid)))
                    {
                        drrContext.AddTodrr_projectevents(upcomingEvent);
                        drrContext.AddLink(progressReport, nameof(progressReport.drr_drr_projectprogress_drr_projectevent_ProjectProgress), upcomingEvent);
                        drrContext.SetLink(upcomingEvent, nameof(upcomingEvent.drr_ProjectProgress), progressReport);
                    }
                    else
                    {
                        drrContext.AttachTo(nameof(drrContext.drr_projectevents), upcomingEvent);
                        drrContext.UpdateObject(upcomingEvent);
                    }

                    var eventContact = upcomingEvent.drr_EventContact;

                    if (eventContact != null)
                    {
                        var existingContact = await drrContext.contacts.Where(c => c.contactid == eventContact.contactid).SingleOrDefaultAsync();
                        if (existingContact == null)
                        {
                            drrContext.AddTocontacts(eventContact);
                            drrContext.SetLink(upcomingEvent, nameof(upcomingEvent.drr_EventContact), eventContact);
                        }
                        else
                        {
                            eventContact.contactid = existingContact.contactid;
                            drrContext.Detach(existingContact);
                            drrContext.AttachTo(nameof(drrContext.contacts), eventContact);
                            drrContext.UpdateObject(eventContact);
                        }
                    }
                }
            }
        }

        private static void AddFundingSignage(DRRContext drrContext, drr_projectprogress progressReport, drr_projectprogress oldReport)
        {
            foreach (var signage in progressReport.drr_drr_projectprogress_drr_temporaryprovincialfundingsignage_ProjectProgress)
            {
                if (signage != null)
                {
                    if (signage.drr_temporaryprovincialfundingsignageid == null ||
                        (oldReport != null && !oldReport.drr_drr_projectprogress_drr_temporaryprovincialfundingsignage_ProjectProgress.Any(a => a.drr_temporaryprovincialfundingsignageid == signage.drr_temporaryprovincialfundingsignageid)))
                    {
                        drrContext.AddTodrr_temporaryprovincialfundingsignages(signage);
                        drrContext.AddLink(progressReport, nameof(progressReport.drr_drr_projectprogress_drr_temporaryprovincialfundingsignage_ProjectProgress), signage);
                        drrContext.SetLink(signage, nameof(signage.drr_ProjectProgress), progressReport);
                    }
                    else
                    {
                        drrContext.AttachTo(nameof(drrContext.drr_temporaryprovincialfundingsignages), signage);
                        drrContext.UpdateObject(signage);
                    }
                }
            }
        }

        private static void UpdateDocuments(DRRContext drrContext, drr_projectprogress progressReport)
        {
            foreach (var doc in progressReport.bcgov_drr_projectprogress_bcgov_documenturl_ProgressReport)
            {
                if (doc != null)
                {
                    drrContext.AttachTo(nameof(drrContext.bcgov_documenturls), doc);
                    drrContext.UpdateObject(doc);
                }
            }
        }

        public async Task<ReportQueryResult> Query(ReportQuery query)
        {
            return query switch
            {
                ReportsQuery q => await HandleQueryReport(q),
                _ => throw new NotSupportedException($"{query.GetType().Name} is not supported")
            };
        }

        public async Task<ClaimQueryResult> Query(ClaimQuery query)
        {
            return query switch
            {
                ClaimsQuery q => await HandleQueryClaim(q),
                _ => throw new NotSupportedException($"{query.GetType().Name} is not supported")
            };
        }

        public async Task<ProgressReportQueryResult> Query(ProgressReportQuery query)
        {
            return query switch
            {
                ProgressReportsQuery q => await HandleQueryProgressReport(q),
                _ => throw new NotSupportedException($"{query.GetType().Name} is not supported")
            };
        }

        public async Task<ForecastQueryResult> Query(ForecastQuery query)
        {
            return query switch
            {
                ForecastsQuery q => await HandleQueryForecast(q),
                _ => throw new NotSupportedException($"{query.GetType().Name} is not supported")
            };
        }

        private async Task<ReportQueryResult> HandleQueryReport(ReportsQuery query)
        {
            var ct = new CancellationTokenSource().Token;
            var readCtx = dRRContextFactory.CreateReadOnly();

            var claimsQuery = readCtx.drr_projectreports
                .Where(a => a.statuscode != (int)ProjectReportStatusOptionSet.Inactive);
            if (!string.IsNullOrEmpty(query.Id)) claimsQuery = claimsQuery.Where(a => a.drr_name == query.Id);

            var results = (await claimsQuery.GetAllPagesAsync(ct)).ToList();
            var length = results.Count;

            await Parallel.ForEachAsync(results, ct, async (rep, ct) => await ParallelLoadReportDetails(readCtx, rep, ct));

            return new ReportQueryResult { Items = mapper.Map<IEnumerable<InterimReportDetails>>(results), Length = length };
        }

        private async Task<ClaimQueryResult> HandleQueryClaim(ClaimsQuery query)
        {
            var ct = new CancellationTokenSource().Token;
            var readCtx = dRRContextFactory.CreateReadOnly();

            var claimsQuery = readCtx.drr_projectclaims
                .Where(a => a.statuscode != (int)ProjectClaimStatusOptionSet.Inactive);
            if (!string.IsNullOrEmpty(query.Id)) claimsQuery = claimsQuery.Where(a => a.drr_name == query.Id);

            var results = (await claimsQuery.GetAllPagesAsync(ct)).ToList();
            var length = results.Count;

            return new ClaimQueryResult { Items = mapper.Map<IEnumerable<ClaimDetails>>(results), Length = length };
        }

        private async Task<ProgressReportQueryResult> HandleQueryProgressReport(ProgressReportsQuery query)
        {
            var ct = new CancellationTokenSource().Token;
            var readCtx = dRRContextFactory.CreateReadOnly();

            var progressReportsQuery = readCtx.drr_projectprogresses
                .Where(a => a.statuscode != (int)ProjectProgressReportStatusOptionSet.Inactive);
            if (!string.IsNullOrEmpty(query.Id)) progressReportsQuery = progressReportsQuery.Where(a => a.drr_name == query.Id);

            var results = (await progressReportsQuery.GetAllPagesAsync(ct)).ToList();
            var length = results.Count;

            await Parallel.ForEachAsync(results, ct, async (pr, ct) => await ParallelLoadProgressReport(readCtx, pr, ct));
            return new ProgressReportQueryResult { Items = mapper.Map<IEnumerable<ProgressReportDetails>>(results), Length = length };
        }

        private async Task<ForecastQueryResult> HandleQueryForecast(ForecastsQuery query)
        {
            var ct = new CancellationTokenSource().Token;
            var readCtx = dRRContextFactory.CreateReadOnly();

            var forecastsQuery = readCtx.drr_projectbudgetforecasts
                .Where(a => a.statuscode != (int)ProjectStatusOptionSet.Inactive);
            if (!string.IsNullOrEmpty(query.Id)) forecastsQuery = forecastsQuery.Where(a => a.drr_name == query.Id);

            var results = (await forecastsQuery.GetAllPagesAsync(ct)).ToList();
            var length = results.Count;

            return new ForecastQueryResult { Items = mapper.Map<IEnumerable<ForecastDetails>>(results), Length = length };
        }

        private static async Task ParallelLoadReportDetails(DRRContext ctx, drr_projectreport report, CancellationToken ct)
        {
            ctx.AttachTo(nameof(DRRContext.drr_projectreports), report);
            var loadTasks = new List<Task>
            {
                ctx.LoadPropertyAsync(report, nameof(drr_projectreport.drr_ClaimReport), ct),
                ctx.LoadPropertyAsync(report, nameof(drr_projectreport.drr_ProgressReport), ct),
                ctx.LoadPropertyAsync(report, nameof(drr_projectreport.drr_BudgetForecast), ct),
                ctx.LoadPropertyAsync(report, nameof(drr_projectreport.drr_ReportPeriod), ct),
            };

            await Task.WhenAll(loadTasks);
        }

        private static async Task ParallelLoadProgressReport(DRRContext ctx, drr_projectprogress pr, CancellationToken ct)
        {
            ctx.AttachTo(nameof(DRRContext.drr_projectprogresses), pr);
            var loadTasks = new List<Task>
            {
                ctx.LoadPropertyAsync(pr, nameof(drr_projectprogress.drr_Project), ct),
                ctx.LoadPropertyAsync(pr, nameof(drr_projectprogress.drr_ProjectReport), ct),
                ctx.LoadPropertyAsync(pr, nameof(drr_projectprogress.drr_drr_projectprogress_drr_projectworkplanactivity_ProjectProgressReport), ct),
                ctx.LoadPropertyAsync(pr, nameof(drr_projectprogress.drr_drr_projectprogress_drr_projectevent_ProjectProgress), ct),
                ctx.LoadPropertyAsync(pr, nameof(drr_projectprogress.drr_drr_projectprogress_drr_projectpastevent_ProjectProgress), ct),
                ctx.LoadPropertyAsync(pr, nameof(drr_projectprogress.drr_drr_projectprogress_drr_temporaryprovincialfundingsignage_ProjectProgress), ct),
                ctx.LoadPropertyAsync(pr, nameof(drr_projectprogress.bcgov_drr_projectprogress_bcgov_documenturl_ProgressReport), ct),
            };

            await Task.WhenAll(loadTasks);

            await Task.WhenAll([
                ParallelLoadActivityTypes(ctx, pr, ct),
                ParallelLoadEventContacts(ctx, pr, ct),
                ParallelLoadDocumentTypes(ctx, pr, ct),
                ]);
        }

        private static async Task ParallelLoadActivityTypes(DRRContext ctx, drr_projectprogress pr, CancellationToken ct)
        {
            await pr.drr_drr_projectprogress_drr_projectworkplanactivity_ProjectProgressReport.ForEachAsync(5, async wa =>
            {
                ctx.AttachTo(nameof(DRRContext.drr_projectworkplanactivities), wa);
                await ctx.LoadPropertyAsync(wa, nameof(drr_projectworkplanactivity.drr_ActivityType), ct);
                await ctx.LoadPropertyAsync(wa, nameof(drr_projectworkplanactivity.drr_CopiedfromReport), ct);
            });
        }

        private static async Task ParallelLoadEventContacts(DRRContext ctx, drr_projectprogress pr, CancellationToken ct)
        {
            await pr.drr_drr_projectprogress_drr_projectevent_ProjectProgress.ForEachAsync(5, async e =>
            {
                ctx.AttachTo(nameof(DRRContext.drr_projectevents), e);
                await ctx.LoadPropertyAsync(e, nameof(drr_projectevent.drr_EventContact), ct);
            });
        }

        private static async Task ParallelLoadDocumentTypes(DRRContext ctx, drr_projectprogress pr, CancellationToken ct)
        {
            await pr.bcgov_drr_projectprogress_bcgov_documenturl_ProgressReport.ForEachAsync(5, async doc =>
            {
                ctx.AttachTo(nameof(DRRContext.bcgov_documenturls), doc);
                await ctx.LoadPropertyAsync(doc, nameof(bcgov_documenturl.bcgov_DocumentType), ct);
            });
        }

        public async Task<bool> CanAccessProject(string id, string businessId)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();
            var existingProject = await readCtx.drr_projects.Expand(a => a.drr_ProponentName).Where(a => a.drr_name == id).SingleOrDefaultAsync();
            if (existingProject == null) return true;
            return (!string.IsNullOrEmpty(existingProject.drr_ProponentName.drr_bceidguid)) && existingProject.drr_ProponentName.drr_bceidguid.Equals(businessId);
        }
    }
}
