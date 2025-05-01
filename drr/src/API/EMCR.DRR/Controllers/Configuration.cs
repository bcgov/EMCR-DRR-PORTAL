using EMCR.DRR.API.Resources.Accounts;
using EMCR.DRR.API.Resources.Cases;
using EMCR.DRR.API.Resources.Documents;
using EMCR.DRR.API.Resources.Projects;
using EMCR.DRR.API.Resources.Reports;
using EMCR.DRR.API.Resources.Requests;
using EMCR.DRR.Resources.Applications;

namespace EMCR.DRR.Controllers
{
    public static class Configuration
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<IApplicationRepository, ApplicationRepository>();
            services.AddTransient<IAccountRepository, AccountRepository>();
            services.AddTransient<ICaseRepository, CaseRepository>();
            services.AddTransient<IDocumentRepository, DocumentRepository>();
            services.AddTransient<IProjectRepository, ProjectRepository>();
            services.AddTransient<IReportRepository, ReportRepository>();
            services.AddTransient<IRequestRepository, RequestRepository>();
            return services;
        }
    }
}
