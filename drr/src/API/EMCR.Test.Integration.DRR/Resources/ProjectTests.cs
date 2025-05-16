using EMCR.DRR.API.Resources.Projects;
using EMCR.DRR.API.Resources.Reports;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EMCR.Tests.Integration.DRR.Resources
{
    public class ProjectTests
    {

        private string TestPrefix = "autotest-dev";
        private string TestBusinessId = "autotest-dev-business-bceid";
        //private string TestUserId = "autotest-dev-user-bceid";
        private readonly IProjectRepository projectRepository;
        private readonly IReportRepository reportRepository;

        public ProjectTests()
        {
            var host = Application.Host;
            projectRepository = host.Services.GetRequiredService<IProjectRepository>();
            reportRepository = host.Services.GetRequiredService<IReportRepository>();
        }

        [Test]
        public async Task CanQueryProjects()
        {
            var projects = (await projectRepository.Query(new ProjectsQuery { BusinessId = TestBusinessId })).Items;
            projects.ShouldNotBeEmpty();
        }

        [Test]
        public async Task CanQueryReports()
        {
            var reports = (await reportRepository.Query(new ReportsQuery { BusinessId = TestBusinessId })).Items;
            reports.ShouldNotBeEmpty();
        }
    }
}
