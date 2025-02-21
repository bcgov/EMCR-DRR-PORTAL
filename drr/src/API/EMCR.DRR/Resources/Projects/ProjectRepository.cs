using AutoMapper;
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

        public async Task<ProjectQueryResult> Query(ProjectQuery query)
        {
            return query switch
            {
                ProjectsQuery q => await HandleQueryProject(q),
                _ => throw new NotSupportedException($"{query.GetType().Name} is not supported")
            };
        }

        private async Task<ProjectQueryResult> HandleQueryProject(ProjectsQuery query)
        {
            var ct = new CancellationTokenSource().Token;
            var readCtx = dRRContextFactory.CreateReadOnly();

            var projectsQuery = readCtx.drr_projects.Expand(a => a.drr_ProponentName)
                .Where(a => a.statuscode != (int)ProjectStatusOptionSet.Inactive
                && a.drr_ProponentName.drr_bceidguid == query.BusinessId);
            //drr_ProponentName.drr_bceidguid.Equals(businessId);
            if (!string.IsNullOrEmpty(query.Id)) projectsQuery = projectsQuery.Where(a => a.drr_name == query.Id);

            var results = (await projectsQuery.GetAllPagesAsync(ct)).ToList();
            var length = results.Count;

            //results = SortAndPageResults(results, query);

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
            };

            await Task.WhenAll(loadTasks);

            //For some reason when testing locally I get this error (though not when debugging... of course...):
            //The SSL connection could not be established, see inner exception.
            //----> System.IO.IOException : Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host..----> System.Net.Sockets.SocketException : An existing connection was forcibly closed by the remote host.
            //But if I load these in two steps, it works consistently...
            var loadTasks2 = new List<Task>
            {
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_drr_project_drr_projectclaim_Project), ct),
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_drr_project_drr_projectprogress_Project), ct),
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_drr_project_drr_projectcondition_Project), ct),
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_drr_project_drr_projectbudgetforecast_Project), ct),
                ctx.LoadPropertyAsync(project, nameof(drr_project.drr_drr_project_drr_projectevent_Project), ct),
            };

            await Task.WhenAll(loadTasks2);

            await Task.WhenAll([
                ParallelLoadProgressReports(ctx, project, ct),
                ParallelLoadReportDetails(ctx, project, ct),
                ParallelLoadConditions(ctx, project, ct),
                ]);
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
            });
        }

        private static async Task ParallelLoadConditions(DRRContext ctx, drr_project project, CancellationToken ct)
        {
            await project.drr_drr_project_drr_projectcondition_Project.ForEachAsync(5, async condition =>
            {
                ctx.AttachTo(nameof(DRRContext.drr_projectconditions), condition);
                await ctx.LoadPropertyAsync(condition, nameof(drr_projectcondition.drr_Condition), ct);
            });
        }

        private static async Task ParallelLoadForecasts(DRRContext ctx, drr_project project, CancellationToken ct)
        {
            await project.drr_drr_project_drr_projectbudgetforecast_Project.ForEachAsync(5, async report =>
            {
                ctx.AttachTo(nameof(DRRContext.drr_projectbudgetforecasts), report);
                await ctx.LoadPropertyAsync(report, nameof(drr_projectbudgetforecast.drr_ProjectReport), ct);
            });
        }

        private static async Task ParallelLoadClaims(DRRContext ctx, drr_project project, CancellationToken ct)
        {
            await project.drr_drr_project_drr_projectclaim_Project.ForEachAsync(5, async report =>
            {
                ctx.AttachTo(nameof(DRRContext.drr_projectclaims), report);
                await ctx.LoadPropertyAsync(report, nameof(drr_projectclaim.drr_ProjectReport), ct);
            });
        }

        public async Task<bool> CanAccessProject(string id, string businessId)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();
            var existingProject = await readCtx.drr_projects.Expand(a => a.drr_ProponentName).Where(a => a.drr_name == id).SingleOrDefaultAsync();
            if (existingProject == null) return true;
            if (existingProject.drr_ProponentName == null) return false;
            return (!string.IsNullOrEmpty(existingProject.drr_ProponentName.drr_bceidguid)) && existingProject.drr_ProponentName.drr_bceidguid.Equals(businessId);
        }
    }
}
