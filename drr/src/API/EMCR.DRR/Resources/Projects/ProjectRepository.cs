using System.Text.RegularExpressions;
using AutoMapper;
using EMCR.DRR.API.Services;
using EMCR.DRR.Dynamics;
using EMCR.DRR.Managers.Intake;
using EMCR.Utilities.Extensions;
using Microsoft.Dynamics.CRM;

namespace EMCR.DRR.API.Resources.Projects
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly IDRRContextFactory dRRContextFactory;
        private readonly IMapper mapper;

        public ProjectRepository(IDRRContextFactory dRRContextFactory, IMapper mapper)
        {
            this.mapper = mapper;
            this.dRRContextFactory = dRRContextFactory;
        }

        public async Task<ManageProjectCommandResult> Manage(ManageProjectCommand cmd)
        {
            return cmd switch
            {
                SaveConditionRequest c => await HandleSaveConditionRequest(c),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

        public async Task<ManageProjectCommandResult> HandleSaveConditionRequest(SaveConditionRequest cmd)
        {
            var ctx = dRRContextFactory.Create();
            var existingRequest = await ctx.drr_requests.Expand(r => r.drr_ProjectConditionId).Where(p => p.drr_ProjectConditionId.drr_name == cmd.Condition.Id).SingleOrDefaultAsync();
            if (existingRequest == null) throw new NotFoundException("Condition Request not found");

            ctx.DetachAll();
            var drrRequest = mapper.Map<drr_request>(cmd.Condition);
            drrRequest.drr_requestid = existingRequest.drr_requestid;

            ctx.AttachTo(nameof(ctx.drr_requests), drrRequest);

            //ctx.UpdateObject(drrRequest);
            //await ctx.SaveChangesAsync();
            ctx.DetachAll();

            return new ManageProjectCommandResult { Id = existingRequest.drr_ProjectConditionId.drr_name };
        }

        public async Task<ProjectQueryResult> Query(ProjectQuery query)
        {
            return query switch
            {
                ProjectsQuery q => await HandleQueryProject(q),
                _ => throw new NotSupportedException($"{query.GetType().Name} is not supported")
            };
        }

        public async Task<RequestQueryResult> Query(RequestQuery query)
        {
            return query switch
            {
                RequestsQuery q => await HandleQueryRequest(q),
                _ => throw new NotSupportedException($"{query.GetType().Name} is not supported")
            };
        }

        private async Task<RequestQueryResult> HandleQueryRequest(RequestsQuery query)
        {
            var ct = new CancellationTokenSource().Token;
            var readCtx = dRRContextFactory.CreateReadOnly();

            var requestsQuery = readCtx.drr_requests.Expand(r => r.drr_ProjectConditionId)
                .Where(a => a.statuscode != (int)InvoiceStatusOptionSet.Inactive);
            if (!string.IsNullOrEmpty(query.Id)) requestsQuery = requestsQuery.Where(a => a.drr_requestid == Guid.Parse(query.Id));
            if (!string.IsNullOrEmpty(query.Name)) requestsQuery = requestsQuery.Where(a => a.drr_name == query.Name);
            if (!string.IsNullOrEmpty(query.ConditionId)) requestsQuery = requestsQuery.Where(a => a.drr_ProjectConditionId.drr_name == query.ConditionId);

            var results = (await requestsQuery.GetAllPagesAsync(ct)).ToList();
            var length = results.Count;
            
            await Parallel.ForEachAsync(results, ct, async (request, ct) => await ParallelLoadRequest(readCtx, request, ct));

            return new RequestQueryResult { Items = mapper.Map<IEnumerable<Request>>(results), Length = length };
        }

        private async Task<ProjectQueryResult> HandleQueryProject(ProjectsQuery query)
        {
            var ct = new CancellationTokenSource().Token;
            var readCtx = dRRContextFactory.CreateReadOnly();

            var projectsQuery = readCtx.drr_projects.Expand(a => a.drr_ProponentName)
                .Where(a => a.statuscode != (int)ProjectStatusOptionSet.Inactive
                && a.statuscode != (int)ProjectStatusOptionSet.NotStarted
                && a.drr_ProponentName.drr_bceidguid == query.BusinessId);
            //drr_ProponentName.drr_bceidguid.Equals(businessId);
            if (!string.IsNullOrEmpty(query.Id)) projectsQuery = projectsQuery.Where(a => a.drr_name == query.Id);

            var results = (await projectsQuery.GetAllPagesAsync(ct)).ToList();
            var length = results.Count;

            results = SortAndPageResults(results, query);

            if (length == 1)
            {
                await Parallel.ForEachAsync(results, ct, async (prj, ct) => await ParallelLoadProjectAsync(readCtx, prj, ct));
                await ParallelLoadCases(readCtx, results);
            }

            return new ProjectQueryResult { Items = mapper.Map<IEnumerable<Project>>(results), Length = length };
        }

        private static async Task ParallelLoadCases(DRRContext ctx, List<drr_project> projects)
        {
            var cases = projects.Where(prj => prj.drr_Case != null).Select(prj => prj.drr_Case).DistinctBy(c => c.incidentid).ToList();
            await cases.ForEachAsync(5, async c =>
            {
                ctx.AttachTo(nameof(DRRContext.incidents), c);
                await ctx.LoadPropertyAsync(c, nameof(incident.drr_EOIApplication));
            });
        }

        private static async Task ParallelLoadProjectAsync(DRRContext ctx, drr_project project, CancellationToken ct)
        {
            ctx.AttachTo(nameof(DRRContext.drr_projects), project);

            var loadTasks = new List<Task>
            {
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_FullProposalApplication), ct),
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_Case), ct),
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_Program), ct),
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_ReportingSchedule), ct),
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_drr_project_drr_projectreport_Project), ct),
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_firstreportdue), ct),
            };

            await Task.WhenAll(loadTasks);

            //For some reason when testing locally I get this error (though not when debugging... of course...):
            //The SSL connection could not be established, see inner exception.
            //----> System.IO.IOException : Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host..----> System.Net.Sockets.SocketException : An existing connection was forcibly closed by the remote host.
            //But if I load these in two steps, it works consistently...

            var loadTasks2 = new List<Task>
            {
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_drr_project_drr_projectprogress_Project), ct),
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_drr_project_drr_projectbudgetforecast_Project), ct),
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_drr_project_drr_projectclaim_Project), ct),
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_drr_project_drr_projectcondition_Project), ct),
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_drr_project_drr_projectevent_Project), ct),
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_drr_project_drr_driffundingrequest_Project), ct),
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_project_drr_request_ProjectId), ct),
            };

            await Task.WhenAll(loadTasks2);

            await Task.WhenAll([
                ParallelLoadReportDetails(ctx, project, ct),
                ParallelLoadProgressReports(ctx, project, ct),
                ParallelLoadForecasts(ctx, project, ct),
                ParallelLoadClaims(ctx, project, ct),
                ParallelLoadProjectConditions(ctx, project, ct),
                ParallelLoadFundingRequests(ctx, project, ct),
                ParallelLoadRequests(ctx, project, ct),
                ]);

            //sort lists
            project.drr_drr_project_drr_projectreport_Project = new System.Collections.ObjectModel.Collection<drr_projectreport>(project.drr_drr_project_drr_projectreport_Project.OrderByDescending(rep => rep.drr_reportdate).ToList());
            project.drr_drr_project_drr_projectprogress_Project = new System.Collections.ObjectModel.Collection<drr_projectprogress>(project.drr_drr_project_drr_projectprogress_Project.OrderByDescending(rep => rep.drr_datesubmitted).ToList());
            project.drr_drr_project_drr_projectbudgetforecast_Project = new System.Collections.ObjectModel.Collection<drr_projectbudgetforecast>(project.drr_drr_project_drr_projectbudgetforecast_Project.OrderByDescending(rep => rep.drr_submissiondate).ToList());
            project.drr_drr_project_drr_projectclaim_Project = new System.Collections.ObjectModel.Collection<drr_projectclaim>(project.drr_drr_project_drr_projectclaim_Project.OrderBy(rep => rep.drr_claimnumber).ToList());
            project.drr_drr_project_drr_projectcondition_Project = new System.Collections.ObjectModel.Collection<drr_projectcondition>(project.drr_drr_project_drr_projectcondition_Project.OrderBy(rep => rep.drr_conditionpercentagelimit).ToList());
        }

        private static async Task ParallelLoadReportDetails(DRRContext ctx, drr_project project, CancellationToken ct)
        {
            await project.drr_drr_project_drr_projectreport_Project.ForEachAsync(5, async report =>
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
            });
        }

        private static async Task ParallelLoadProgressReports(DRRContext ctx, drr_project project, CancellationToken ct)
        {
            await project.drr_drr_project_drr_projectprogress_Project.ForEachAsync(5, async report =>
            {
                ctx.AttachTo(nameof(DRRContext.drr_projectprogresses), report);
                await ctx.LoadPropertyAsync(report, nameof(drr_projectprogress.drr_ProjectReport), ct);
                await ctx.LoadPropertyAsync(report, nameof(drr_projectprogress.drr_drr_projectprogress_drr_projectworkplanactivity_ProjectProgressReport), ct);
                var projectReport = project.drr_drr_project_drr_projectreport_Project.Where(pr => pr._drr_progressreport_value == report.drr_projectprogressid).SingleOrDefault();
                if (projectReport != null) report.drr_ProjectReport = projectReport;
            });
        }

        private static async Task ParallelLoadProjectConditions(DRRContext ctx, drr_project project, CancellationToken ct)
        {
            await project.drr_drr_project_drr_projectcondition_Project.ForEachAsync(5, async condition =>
            {
                ctx.AttachTo(nameof(DRRContext.drr_projectconditions), condition);
                await ctx.LoadPropertyAsync(condition, nameof(drr_projectcondition.drr_Condition), ct);
            });
        }

        private static async Task ParallelLoadFundingRequests(DRRContext ctx, drr_project project, CancellationToken ct)
        {
            await project.drr_drr_project_drr_driffundingrequest_Project.ForEachAsync(5, async fund =>
            {
                ctx.AttachTo(nameof(DRRContext.drr_driffundingrequests), fund);
                await ctx.LoadPropertyAsync(fund, nameof(drr_driffundingrequest.drr_FiscalYear), ct);
            });
        }
        
        private static async Task ParallelLoadRequests(DRRContext ctx, drr_project project, CancellationToken ct)
        {
            await project.drr_project_drr_request_ProjectId.ForEachAsync(5, async request =>
            {
                ctx.AttachTo(nameof(DRRContext.drr_requests), request);

                var loadTasks = new List<Task>
                {
                    ctx.LoadPropertyAsync(request, nameof(drr_request.drr_request_bcgov_documenturl_RequestId), ct),
                    ctx.LoadPropertyAsync(request, nameof(drr_request.drr_ProjectConditionId), ct),
                    ctx.LoadPropertyAsync(request, nameof(drr_request.drr_AuthorizedRepresentativeContactId), ct),
                };

                await Task.WhenAll(loadTasks);
            });
        }

        private static async Task ParallelLoadForecasts(DRRContext ctx, drr_project project, CancellationToken ct)
        {
            await project.drr_drr_project_drr_projectbudgetforecast_Project.ForEachAsync(5, async report =>
            {
                ctx.AttachTo(nameof(DRRContext.drr_projectbudgetforecasts), report);
                await ctx.LoadPropertyAsync(report, nameof(drr_projectbudgetforecast.drr_ProjectReport), ct);
                var projectReport = project.drr_drr_project_drr_projectreport_Project.Where(pr => pr._drr_budgetforecast_value == report.drr_projectbudgetforecastid).SingleOrDefault();
                if (projectReport != null) report.drr_ProjectReport = projectReport;
            });
        }

        private static async Task ParallelLoadClaims(DRRContext ctx, drr_project project, CancellationToken ct)
        {
            await project.drr_drr_project_drr_projectclaim_Project.ForEachAsync(5, async report =>
            {
                ctx.AttachTo(nameof(DRRContext.drr_projectclaims), report);
                await ctx.LoadPropertyAsync(report, nameof(drr_projectclaim.drr_ProjectReport), ct);
                var projectReport = project.drr_drr_project_drr_projectreport_Project.Where(pr => pr._drr_claimreport_value == report.drr_projectclaimid).SingleOrDefault();
                if (projectReport != null) report.drr_ProjectReport = projectReport;
            });
        }

        private static async Task ParallelLoadRequest(DRRContext ctx, drr_request request, CancellationToken ct)
        {
            ctx.AttachTo(nameof(DRRContext.drr_requests), request);
            await ctx.LoadPropertyAsync(request, nameof(drr_request.drr_ProjectConditionId), ct);
        }

        public async Task<bool> CanAccessProject(string id, string businessId)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();
            var existingProject = await readCtx.drr_projects.Expand(a => a.drr_ProponentName).Where(a => a.drr_name == id).SingleOrDefaultAsync();
            if (existingProject == null) return true;
            if (existingProject.drr_ProponentName == null) return false;
            return (!string.IsNullOrEmpty(existingProject.drr_ProponentName.drr_bceidguid)) && existingProject.drr_ProponentName.drr_bceidguid.Equals(businessId);
        }

        public async Task<bool> CanAccessCondition(string id, string businessId)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();
            var existingCondition = await readCtx.drr_projectconditions.Expand(a => a.drr_Project).Where(a => a.drr_name == id).SingleOrDefaultAsync();
            if (existingCondition == null) return true;
            readCtx.AttachTo(nameof(readCtx.drr_projects), existingCondition.drr_Project);
            await readCtx.LoadPropertyAsync(existingCondition.drr_Project, nameof(drr_project.drr_ProponentName));
            return (!string.IsNullOrEmpty(existingCondition.drr_Project.drr_ProponentName.drr_bceidguid)) && existingCondition.drr_Project.drr_ProponentName.drr_bceidguid.Equals(businessId);
        }

        private List<drr_project> SortAndPageResults(List<drr_project> projects, ProjectsQuery query)
        {
            var descending = false;
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                if (query.OrderBy.Contains(" desc"))
                {
                    descending = true;
                    query.OrderBy = Regex.Replace(query.OrderBy, @" desc", "");
                }
                if (descending) projects = projects.OrderByDescending(a => GetPropertyValueForSort(a, query.OrderBy)).ToList();
                else projects = projects.OrderBy(a => GetPropertyValueForSort(a, query.OrderBy)).ToList();
            }

            if (query.Page > 0)
            {
                var skip = query.Count * (query.Page - 1);
                projects = projects.Skip(skip).ToList();
            }

            if (query.Count > 0)
            {
                projects = projects.Take(query.Count).ToList();
            }

            return projects;
        }

#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
        private static object GetPropertyValueForSort(object src, string propName)
        {


            if (src == null) throw new ArgumentException("Value cannot be null.", "src");
            if (propName == null) throw new ArgumentException("Value cannot be null.", "propName");

            if (propName.Contains("."))//complex type nested
            {
                var temp = propName.Split(new char[] { '.' }, 2);
                return GetPropertyValueForSort(GetPropertyValueForSort(src, temp[0]), temp[1]);
            }
            else
            {
                var prop = src.GetType().GetProperty(propName);
                if (propName == "statuscode") return GetSortedStatuses(prop.GetValue(src, null).ToString());
                return prop != null ? prop.GetValue(src, null) : null;
            }
        }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8603 // Possible null reference return.

        private static int GetSortedStatuses(string status)
        {
            var statusOption = Enum.Parse<ProjectStatusOptionSet>(status);
            switch (statusOption)
            {
                case ProjectStatusOptionSet.Complete:
                    return 0; //Complete

                case ProjectStatusOptionSet.InProgress:
                    return 1; //InProgress

                case ProjectStatusOptionSet.Inactive:
                    return 2; //Inactive

                case ProjectStatusOptionSet.NotStarted:
                    return 3; //NotStarted

                default: return 0;
            }
        }
    }
}
