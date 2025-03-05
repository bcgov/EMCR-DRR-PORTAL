using System.Text.RegularExpressions;
using AutoMapper;
using EMCR.DRR.API.Model;
using EMCR.DRR.API.Resources.Cases;
using EMCR.DRR.API.Resources.Documents;
using EMCR.DRR.API.Resources.Projects;
using EMCR.DRR.API.Resources.Reports;
using EMCR.DRR.API.Services;
using EMCR.DRR.API.Services.S3;
using EMCR.DRR.Resources.Applications;
using EMCR.Utilities.Extensions;

namespace EMCR.DRR.Managers.Intake
{
    public class IntakeManager : IIntakeManager
    {
        private readonly ILogger<IntakeManager> logger;
        private readonly IMapper mapper;
        private readonly IApplicationRepository applicationRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IReportRepository reportRepository;
        private readonly IDocumentRepository documentRepository;
        private readonly ICaseRepository caseRepository;
        private readonly IS3Provider s3Provider;

        private FileTag GetDeletedFileTag() => new FileTag { Tags = new[] { new Tag { Key = "Deleted", Value = "true" } } };

        public IntakeManager(ILogger<IntakeManager> logger, IMapper mapper, IApplicationRepository applicationRepository, IDocumentRepository documentRepository, ICaseRepository caseRepository, IProjectRepository projectRepository, IReportRepository reportRepository, IS3Provider s3Provider)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.applicationRepository = applicationRepository;
            this.documentRepository = documentRepository;
            this.caseRepository = caseRepository;
            this.projectRepository = projectRepository;
            this.reportRepository = reportRepository;
            this.s3Provider = s3Provider;
        }

        public async Task<ApplicationQueryResponse> Handle(ApplicationQuery cmd)
        {
            return cmd switch
            {
                DrrApplicationsQuery c => await Handle(c),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

        public async Task<ProjectsQueryResponse> Handle(ProjectQuery cmd)
        {
            return cmd switch
            {
                DrrProjectsQuery c => await Handle(c),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

        public async Task<ReportsQueryResponse> Handle(ReportQuery cmd)
        {
            return cmd switch
            {
                DrrReportsQuery c => await Handle(c),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

        public async Task<ClaimsQueryResponse> Handle(ClaimQuery cmd)
        {
            return cmd switch
            {
                DrrClaimsQuery c => await Handle(c),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

        public async Task<ProgressReportsQueryResponse> Handle(ProgressReportQuery cmd)
        {
            return cmd switch
            {
                DrrProgressReportsQuery c => await Handle(c),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

        public async Task<ForecastsQueryResponse> Handle(ForecastQuery cmd)
        {
            return cmd switch
            {
                DrrForecastsQuery c => await Handle(c),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

        public async Task<StorageQueryResults> Handle(AttachmentQuery cmd)
        {
            return cmd switch
            {
                DownloadAttachment c => await Handle(c),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

        public async Task<string> Handle(IntakeCommand cmd)
        {
            return cmd switch
            {
                EoiSaveApplicationCommand c => await Handle(c),
                EoiSubmitApplicationCommand c => await Handle(c),
                CreateFpFromEoiCommand c => await Handle(c),
                FpSaveApplicationCommand c => await Handle(c),
                FpSubmitApplicationCommand c => await Handle(c),
                WithdrawApplicationCommand c => await Handle(c),
                DeleteApplicationCommand c => await Handle(c),
                UploadAttachmentCommand c => await Handle(c),
                DeleteAttachmentCommand c => await Handle(c),
                SaveProjectCommand c => await Handle(c),
                SubmitProjectCommand c => await Handle(c),
                SaveProgressReportCommand c => await Handle(c),
                SubmitProgressReportCommand c => await Handle(c),
                CreateInterimReportCommand c => await Handle(c),
                SaveClaimCommand c => await Handle(c),
                SubmitClaimCommand c => await Handle(c),
                CreateInvoiceCommand c => await Handle(c),
                DeleteInvoiceCommand c => await Handle(c),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

        public async Task<ApplicationQueryResponse> Handle(DrrApplicationsQuery q)
        {
            if (!string.IsNullOrEmpty(q.Id))
            {
                var canAccess = await CanAccessApplication(q.Id, q.BusinessId);
                if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");
            }
            var page = 0;
            var count = 0;
            if (q.QueryOptions != null)
            {
                page = q.QueryOptions.Page + 1;
                count = q.QueryOptions.PageSize;
            }

            var orderBy = GetOrderBy(q.QueryOptions?.OrderBy);
            var filterOptions = ParseFilter(q.QueryOptions?.Filter);

            var res = string.IsNullOrEmpty(q.Id) ? await applicationRepository.QueryList(new ApplicationsQuery { Id = q.Id, BusinessId = q.BusinessId, Page = page, Count = count, OrderBy = orderBy, FilterOptions = filterOptions }) :
            await applicationRepository.Query(new ApplicationsQuery { Id = q.Id, BusinessId = q.BusinessId, Page = page, Count = count, OrderBy = orderBy, FilterOptions = filterOptions });

            return new ApplicationQueryResponse { Items = res.Items, Length = res.Length };
        }

        public async Task<ProjectsQueryResponse> Handle(DrrProjectsQuery q)
        {
            if (!string.IsNullOrEmpty(q.Id))
            {
                var canAccess = await CanAccessProject(q.Id, q.BusinessId);
                if (!canAccess) throw new ForbiddenException("Not allowed to access this project.");
            }
            var page = 0;
            var count = 0;
            if (q.QueryOptions != null)
            {
                page = q.QueryOptions.Page + 1;
                count = q.QueryOptions.PageSize;
            }

            var orderBy = GetOrderBy(q.QueryOptions?.OrderBy);
            var filterOptions = ParseFilter(q.QueryOptions?.Filter);

            var res = await projectRepository.Query(new ProjectsQuery { Id = q.Id, BusinessId = q.BusinessId, Page = page, Count = count, OrderBy = orderBy, FilterOptions = filterOptions });

            return new ProjectsQueryResponse { Items = res.Items, Length = res.Length };
        }

        public async Task<ReportsQueryResponse> Handle(DrrReportsQuery q)
        {
            if (!string.IsNullOrEmpty(q.Id))
            {
                var canAccess = await CanAccessClaim(q.Id, q.BusinessId);
                if (!canAccess) throw new ForbiddenException("Not allowed to access this claim.");
            }
            var res = await reportRepository.Query(new ReportsQuery { Id = q.Id, BusinessId = q.BusinessId });

            return new ReportsQueryResponse { Items = res.Items, Length = res.Length };
        }

        public async Task<ClaimsQueryResponse> Handle(DrrClaimsQuery q)
        {
            if (!string.IsNullOrEmpty(q.Id))
            {
                var canAccess = await CanAccessClaim(q.Id, q.BusinessId);
                if (!canAccess) throw new ForbiddenException("Not allowed to access this claim.");
            }
            var res = await reportRepository.Query(new ClaimsQuery { Id = q.Id, BusinessId = q.BusinessId });

            return new ClaimsQueryResponse { Items = res.Items, Length = res.Length };
        }

        public async Task<ProgressReportsQueryResponse> Handle(DrrProgressReportsQuery q)
        {
            if (!string.IsNullOrEmpty(q.Id))
            {
                var canAccess = await CanAccessProgressReport(q.Id, q.BusinessId);
                if (!canAccess) throw new ForbiddenException("Not allowed to access this progress report.");
            }
            var res = await reportRepository.Query(new ProgressReportsQuery { Id = q.Id, BusinessId = q.BusinessId });

            return new ProgressReportsQueryResponse { Items = res.Items, Length = res.Length };
        }

        public async Task<ForecastsQueryResponse> Handle(DrrForecastsQuery q)
        {
            if (!string.IsNullOrEmpty(q.Id))
            {
                var canAccess = await CanAccessClaim(q.Id, q.BusinessId);
                if (!canAccess) throw new ForbiddenException("Not allowed to access this forecast.");
            }
            var res = await reportRepository.Query(new ForecastsQuery { Id = q.Id, BusinessId = q.BusinessId });

            return new ForecastsQueryResponse { Items = res.Items, Length = res.Length };
        }

        public async Task<string> Handle(EoiSaveApplicationCommand cmd)
        {
            var canAccess = await CanAccessApplication(cmd.Application.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");
            var application = mapper.Map<Application>(cmd.Application);
            application.BCeIDBusinessId = cmd.UserInfo.BusinessId;
            application.ProponentName = cmd.UserInfo.BusinessName;
            if (application.Submitter != null) application.Submitter.BCeId = cmd.UserInfo.UserId;
            var id = (await applicationRepository.Manage(new SaveApplication { Application = application })).Id;
            return id;
        }

        public async Task<string> Handle(EoiSubmitApplicationCommand cmd)
        {
            var canAccess = await CanAccessApplication(cmd.Application.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");
            var application = mapper.Map<Application>(cmd.Application);
            application.BCeIDBusinessId = cmd.UserInfo.BusinessId;
            application.ProponentName = cmd.UserInfo.BusinessName;
            application.SubmittedDate = DateTime.UtcNow;
            if (application.Submitter != null) application.Submitter.BCeId = cmd.UserInfo.UserId;
            //TODO - add field validations

            var id = (await applicationRepository.Manage(new SaveApplication { Application = application })).Id;
            return id;
        }

        public async Task<string> Handle(CreateFpFromEoiCommand cmd)
        {
            var canAccess = await CanAccessApplication(cmd.EoiId, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");

            var application = (await applicationRepository.Query(new ApplicationsQuery { Id = cmd.EoiId })).Items.SingleOrDefault();
            if (application == null) throw new NotFoundException("Application not found");
            if (!application.ApplicationTypeName.Equals("EOI")) throw new BusinessValidationException("Can only create FP from an EOI");
            if (application.Status != ApplicationStatus.Invited) throw new BusinessValidationException("Can only create FP if EOI is Invited");
            if (!string.IsNullOrEmpty(application.FpId)) throw new BusinessValidationException("This EOI already has an associated FP");

            var res = (await caseRepository.Manage(new GenerateFpFromEoi { EoiId = cmd.EoiId, ScreenerQuestions = cmd.ScreenerQuestions })).Id;
            return res;
        }

        public async Task<string> Handle(FpSaveApplicationCommand cmd)
        {
            var canAccess = await CanAccessApplication(cmd.Application.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");
            var application = mapper.Map<Application>(cmd.Application);
            application.BCeIDBusinessId = cmd.UserInfo.BusinessId;
            application.ProponentName = cmd.UserInfo.BusinessName;
            if (application.Submitter != null) application.Submitter.BCeId = cmd.UserInfo.UserId;

            //cost category cannot be Contingency if stream 1 - only available in stream 2
            if (application.FundingStream == FundingStream.Stream1 && application.CostEstimates != null && application.CostEstimates.Any(c => c.CostCategory == CostCategory.Contingency))
                throw new BusinessValidationException("Contingency Cost Category is only available for Stream 2");



            var id = (await applicationRepository.Manage(new SaveApplication { Application = application })).Id;
            return id;
        }

        public async Task<string> Handle(FpSubmitApplicationCommand cmd)
        {
            var canAccess = await CanAccessApplication(cmd.Application.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");
            var application = mapper.Map<Application>(cmd.Application);
            application.BCeIDBusinessId = cmd.UserInfo.BusinessId;
            application.ProponentName = cmd.UserInfo.BusinessName;
            if (application.Submitter != null) application.Submitter.BCeId = cmd.UserInfo.UserId;
            var id = (await applicationRepository.Manage(new SaveApplication { Application = application })).Id;
            await applicationRepository.Manage(new SubmitApplication { Id = id });
            return id;
        }

        public async Task<string> Handle(WithdrawApplicationCommand cmd)
        {
            var canAccess = await CanAccessApplication(cmd.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");
            var application = (await applicationRepository.Query(new ApplicationsQuery { Id = cmd.Id })).Items.SingleOrDefault();
            if (application == null) throw new NotFoundException("Application not found");
            if (application.Status != ApplicationStatus.Submitted) throw new BusinessValidationException("Application can only be withdrawn while it is in Submitted Status");
            application.Status = ApplicationStatus.Withdrawn;
            var id = (await applicationRepository.Manage(new SaveApplication { Application = application })).Id;
            return id;
        }

        public async Task<string> Handle(DeleteApplicationCommand cmd)
        {
            var canAccess = await CanAccessApplication(cmd.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");
            var application = (await applicationRepository.Query(new ApplicationsQuery { Id = cmd.Id })).Items.SingleOrDefault();
            if (application == null) throw new NotFoundException("Application not found");
            if (!ApplicationInEditableStatus(application)) throw new BusinessValidationException("Application can only be deleted if it is in Draft");
            if (!application.ApplicationTypeName.Equals("EOI")) throw new BusinessValidationException("Only EOI applications can be deleted");
            var id = (await applicationRepository.Manage(new DeleteApplication { Id = cmd.Id })).Id;
            return id;
        }

        public async Task<string> Handle(UploadAttachmentCommand cmd)
        {
            switch (cmd.AttachmentInfo.RecordType)
            {
                case RecordType.FullProposal: return await UploadApplicationDocument(cmd);
                case RecordType.ProgressReport: return await UploadProgressReportDocument(cmd);
                case RecordType.Invoice: return await UploadInvoiceDocument(cmd);
                default: throw new BusinessValidationException("Unsupported Record Type");
            }
        }

        public async Task<string> Handle(DeleteAttachmentCommand cmd)
        {
            var recordType = (await documentRepository.Query(new DocumentQuery { Id = cmd.Id })).Document.RecordType;

            switch (recordType)
            {
                case RecordType.FullProposal: return await DeleteApplicationDocument(cmd);
                case RecordType.ProgressReport: return await DeleteProgressReportDocument(cmd);
                case RecordType.Invoice: return await DeleteInvoiceDocument(cmd);
                default: throw new BusinessValidationException("Unsupported Record Type");
            }
        }

        public async Task<string> Handle(SaveProjectCommand cmd)
        {
            var canAccess = await CanAccessProject(cmd.Project.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this project.");
            var project = mapper.Map<Project>(cmd.Project);
            project.ProponentName = cmd.UserInfo.BusinessName;
            //var id = (await projectRepository.Manage(new SaveProject { Project = project })).Id;
            var id = Guid.NewGuid().ToString();
            return id;
        }

        public async Task<string> Handle(SubmitProjectCommand cmd)
        {
            var canAccess = await CanAccessProject(cmd.Project.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this project.");
            var project = mapper.Map<Project>(cmd.Project);
            project.ProponentName = cmd.UserInfo.BusinessName;
            //var id = (await projectRepository.Manage(new SaveProject { Project = project })).Id;
            //await projectRepository.Manage(new SubmitProject { Id = id });
            var id = Guid.NewGuid().ToString();
            return id;
        }

        public async Task<string> Handle(SaveProgressReportCommand cmd)
        {
            var canAccess = await CanAccessProgressReport(cmd.ProgressReport.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this progress report.");

            var existingProgressReport = (await reportRepository.Query(new ProgressReportsQuery { Id = cmd.ProgressReport.Id, BusinessId = cmd.UserInfo.BusinessId })).Items.SingleOrDefault();
            if (existingProgressReport == null) throw new NotFoundException("Application not found");

            var progressReport = mapper.Map<ProgressReportDetails>(cmd.ProgressReport);

            //is PreCreatedActivity or was copied from activity/report
            var mandatoryActivityIds = existingProgressReport.Workplan?.WorkplanActivities?.Where(a => a.ActivityType?.PreCreatedActivity == true || a.CopiedFromActivity == true || !string.IsNullOrEmpty(a.OriginalReportId)).Select(a => a.Id).ToList();
            var currentActivityIds = progressReport.Workplan?.WorkplanActivities?.Select(a => a.Id).ToList() ?? new List<string?>();
            if (mandatoryActivityIds != null && mandatoryActivityIds.Any(id => !currentActivityIds.Contains(id))) throw new BusinessValidationException("Not Allowed to remove activity");

            var id = (await reportRepository.Manage(new SaveProgressReport { ProgressReport = progressReport })).Id;
            return id;
        }

        public async Task<string> Handle(SubmitProgressReportCommand cmd)
        {
            var canAccess = await CanAccessProgressReport(cmd.ProgressReport.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this progress report.");
            var existingProgressReport = (await reportRepository.Query(new ProgressReportsQuery { Id = cmd.ProgressReport.Id, BusinessId = cmd.UserInfo.BusinessId })).Items.SingleOrDefault();
            if (existingProgressReport == null) throw new NotFoundException("Progress Report not found");

            var progressReport = mapper.Map<ProgressReportDetails>(cmd.ProgressReport);

            //is PreCreatedActivity or was copied from activity/report
            var mandatoryActivityIds = existingProgressReport.Workplan?.WorkplanActivities?.Where(a => a.ActivityType?.PreCreatedActivity == true || a.CopiedFromActivity == true || !string.IsNullOrEmpty(a.OriginalReportId)).Select(a => a.Id).ToList();
            var currentActivityIds = progressReport.Workplan?.WorkplanActivities?.Select(a => a.Id).ToList() ?? new List<string?>();
            if (mandatoryActivityIds != null && mandatoryActivityIds.Any(id => !currentActivityIds.Contains(id))) throw new BusinessValidationException("Not Allowed to remove activity");

            var id = (await reportRepository.Manage(new SaveProgressReport { ProgressReport = progressReport })).Id;
            await reportRepository.Manage(new SubmitProgressReport { Id = id });
            return id;
        }

        public async Task<string> Handle(SaveClaimCommand cmd)
        {
            var canAccess = await CanAccessClaim(cmd.Claim.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this claim.");

            var existingClaim = (await reportRepository.Query(new ClaimsQuery { Id = cmd.Claim.Id, BusinessId = cmd.UserInfo.BusinessId })).Items.SingleOrDefault();
            if (existingClaim == null) throw new NotFoundException("Claim not found");

            var claim = mapper.Map<ClaimDetails>(cmd.Claim);

            var id = (await reportRepository.Manage(new SaveClaim { Claim = claim })).Id;
            return id;
        }

        public async Task<string> Handle(SubmitClaimCommand cmd)
        {
            var canAccess = await CanAccessClaim(cmd.Claim.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this claim.");

            var existingClaim = (await reportRepository.Query(new ClaimsQuery { Id = cmd.Claim.Id, BusinessId = cmd.UserInfo.BusinessId })).Items.SingleOrDefault();
            if (existingClaim == null) throw new NotFoundException("Claim not found");

            var claim = mapper.Map<ClaimDetails>(cmd.Claim);
            var now = DateTime.UtcNow;
            if (claim.Invoices != null && claim.Invoices.Any(i => string.IsNullOrEmpty(i.InvoiceNumber))) throw new BusinessValidationException("InvoiceNumber is required");
            if (claim.Invoices != null && claim.Invoices.Any(i => i.Date > now)) throw new BusinessValidationException("Invoice date cannot be in the future");
            if (claim.Invoices != null && claim.Invoices.Any(i => i.Date > i.PaymentDate)) throw new BusinessValidationException("Payment date cannot be before invoice date");
            if (claim.Invoices != null && claim.Invoices.Any(i => i.TaxRebate > i.GrossAmount)) throw new BusinessValidationException("Tax Rebate cannot be greater than Gross Amount");
            if (claim.Invoices != null && claim.Invoices.Any(i => i.ClaimAmount > i.GrossAmount)) throw new BusinessValidationException("Claim Amount cannot be greater than Gross Amount");
            if (claim.Invoices != null && claim.Invoices.Any(i => i.TotalPST > i.GrossAmount)) throw new BusinessValidationException("PST cannot be greater than Gross Amount");
            if (claim.Invoices != null && claim.Invoices.Any(i => i.TotalGST > i.GrossAmount)) throw new BusinessValidationException("GST cannot be greater than Gross Amount");


            var id = (await reportRepository.Manage(new SaveClaim { Claim = claim })).Id;
            await reportRepository.Manage(new SubmitClaim { Id = id });
            return id;
        }

        public async Task<string> Handle(CreateInvoiceCommand cmd)
        {
            var canAccess = await CanAccessClaim(cmd.ClaimId, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this claim.");

            var existingClaim = (await reportRepository.Query(new ClaimsQuery { Id = cmd.ClaimId, BusinessId = cmd.UserInfo.BusinessId })).Items.SingleOrDefault();
            if (existingClaim == null) throw new NotFoundException("Claim not found");

            var id = (await reportRepository.Manage(new CreateInvoice { ClaimId = cmd.ClaimId, InvoiceId = cmd.InvoiceId })).Id;
            return id;
        }

        public async Task<string> Handle(DeleteInvoiceCommand cmd)
        {
            var canAccess = await CanAccessClaim(cmd.ClaimId, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this claim.");

            var existingClaim = (await reportRepository.Query(new ClaimsQuery { Id = cmd.ClaimId, BusinessId = cmd.UserInfo.BusinessId })).Items.SingleOrDefault();
            if (existingClaim == null) throw new NotFoundException("Claim not found");
            if (existingClaim.Invoices == null) throw new NotFoundException("Invoice not found");

            var existingInvoice = existingClaim.Invoices.Where(i => i.Id == cmd.InvoiceId).SingleOrDefault();
            if (existingInvoice == null) throw new NotFoundException("Invoice not found");

            if (existingInvoice.Attachments != null)
            {
                foreach (var attachment in existingInvoice.Attachments)
                {
                    await DeleteInvoiceDocument(new DeleteAttachmentCommand { Id = attachment.Id, UserInfo = cmd.UserInfo });
                }
            }

            var id = (await reportRepository.Manage(new DeleteInvoice { InvoiceId = cmd.InvoiceId })).Id;
            return id;
        }

        public async Task<string> Handle(CreateInterimReportCommand cmd)
        {
            var canAccess = await CanAccessProject(cmd.ProjectId, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this project.");
            var project = (await projectRepository.Query(new ProjectsQuery { Id = cmd.ProjectId, BusinessId = cmd.UserInfo.BusinessId })).Items.SingleOrDefault();
            if (project == null) throw new NotFoundException("Project not found");
            if (project.StartDate == null) throw new BusinessValidationException("Invalid Report Start Date");

            var validationRes = await Handle(new ValidateCanCreateReportCommand { ProjectId = cmd.ProjectId, ReportType = cmd.ReportType, UserInfo = cmd.UserInfo });
            if (!validationRes.CanCreate) throw new BusinessValidationException(validationRes.Description);

            var reportPeriodName = validationRes.Description;

            //if (project.InterimReports == null || project.InterimReports.Count() == 0)
            //{
            //    reportPeriodName = GetReportPeriod(project.ReportingScheduleType, project.StartDate.Value);
            //}
            //else
            //{
            //    var lastReport = project.InterimReports.OrderByDescending(r => r.ReportDate).First();
            //    if (lastReport.ReportDate == null) throw new BusinessValidationException($"Invalid Report Date for report {lastReport.Id}");
            //    reportPeriodName = GetNextReportPeriod(project.ReportingScheduleType, lastReport.ReportDate.Value);
            //}

            var id = (await reportRepository.Manage(new CreateProjectReport { ProjectId = cmd.ProjectId, ReportPeriodName = reportPeriodName, ReportType = cmd.ReportType })).Id;
            return id;
        }

        public async Task<ValidateCanCreateReportResult> Handle(ValidateCanCreateReportCommand cmd)
        {
            var canAccess = await CanAccessProject(cmd.ProjectId, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this project.");
            var project = (await projectRepository.Query(new ProjectsQuery { Id = cmd.ProjectId, BusinessId = cmd.UserInfo.BusinessId })).Items.SingleOrDefault();
            if (project == null) throw new NotFoundException("Project not found");
            if (project.StartDate == null) throw new BusinessValidationException("Invalid Project Start Date");
            if (project.ReportingScheduleType == null) throw new BusinessValidationException("Invalid Project Reporting Schedule");
            //if (string.IsNullOrEmpty(project.FirstReportPeriod)) throw new BusinessValidationException("Invalid First Report Period");

            bool canCreate = false;
            string description = string.Empty;

            if (project.InterimReports == null || project.InterimReports.Count() == 0)
            {
                //First Interim Report
                canCreate = true;
                var defaultPeriod = project.ReportingScheduleType == ReportingScheduleType.Quarterly ? "2025-Q1" : project.ReportingScheduleType == ReportingScheduleType.Monthly ? "2025-Month-1" : string.Empty;
                description = project.FirstReportPeriod ?? defaultPeriod;
                if (string.IsNullOrEmpty(description)) throw new BusinessValidationException("Error determining report period");
                //GetReportPeriodFromDate(project.ReportingScheduleType, project.StartDate.Value);
            }
            else
            {
                //Subsequent Interim Report
                var lastReport = project.InterimReports.OrderByDescending(r => r.ReportDate).First();
                if (lastReport.ReportDate == null) throw new BusinessValidationException($"Invalid Report Date for report {lastReport.Id}");
                if (lastReport.Status == InterimReportStatus.Approved)
                {
                    canCreate = true;
                    //description = GetNextReportPeriod(project.ReportingScheduleType, lastReport.ReportDate.Value);
                    description = !string.IsNullOrEmpty(lastReport.ReportPeriod) ? GetNextReportPeriodFromString(project.ReportingScheduleType, lastReport.ReportPeriod) : GetNextReportPeriodFromDate(project.ReportingScheduleType, lastReport.ReportDate.Value);
                }
                else
                {
                    description = !string.IsNullOrEmpty(lastReport.ReportPeriod) ? lastReport.ReportPeriod : GetReportPeriodFromDate(project.ReportingScheduleType, lastReport.ReportDate.Value);
                    //description = GetReportPeriod(project.ReportingScheduleType, lastReport.ReportDate.Value);
                }
            }

            return new ValidateCanCreateReportResult { CanCreate = canCreate, Description = description };
        }


        public async Task<FileQueryResult> Handle(DownloadAttachment cmd)
        {
            var recordType = (await documentRepository.Query(new DocumentQuery { Id = cmd.Id })).Document.RecordType;

            switch (recordType)
            {
                case RecordType.FullProposal: return await DownloadApplicationDocument(cmd);
                case RecordType.ProgressReport: return await DownloadProgressReportDocument(cmd);
                default: throw new BusinessValidationException("Unsupported Record Type");
            }
        }

        public async Task<DeclarationQueryResult> Handle(DeclarationQuery _)
        {
            var res = await applicationRepository.Query(new Resources.Applications.DeclarationQuery());
            return new DeclarationQueryResult { Items = mapper.Map<IEnumerable<DeclarationInfo>>(res.Items) };
        }

        public async Task<EntitiesQueryResult> Handle(EntitiesQuery _)
        {
            var res = await applicationRepository.Query(new Resources.Applications.EntitiesQuery());
            return mapper.Map<EntitiesQueryResult>(res);
        }

        private async Task<FileQueryResult> DownloadApplicationDocument(DownloadAttachment cmd)
        {
            var canAccess = await CanAccessApplicationFromDocumentId(cmd.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");
            var recordId = (await documentRepository.Query(new DocumentQuery { Id = cmd.Id })).RecordId;

            var res = await s3Provider.HandleQuery(new FileQuery { Key = cmd.Id, Folder = $"{RecordType.FullProposal.ToDescriptionString()}/{recordId}" });
            return (FileQueryResult)res;
        }

        private async Task<FileQueryResult> DownloadProgressReportDocument(DownloadAttachment cmd)
        {
            var canAccess = await CanAccessProgressReportFromDocumentId(cmd.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this progress report.");
            var recordId = (await documentRepository.Query(new DocumentQuery { Id = cmd.Id })).RecordId;

            var res = await s3Provider.HandleQuery(new FileQuery { Key = cmd.Id, Folder = $"{RecordType.ProgressReport.ToDescriptionString()}/{recordId}" });
            return (FileQueryResult)res;
        }

        private async Task<string> UploadApplicationDocument(UploadAttachmentCommand cmd)
        {
            var canAccess = await CanAccessApplication(cmd.AttachmentInfo.RecordId, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");
            var application = (await applicationRepository.Query(new ApplicationsQuery { Id = cmd.AttachmentInfo.RecordId })).Items.SingleOrDefault();
            if (application == null) throw new NotFoundException("Application not found");
            if (!ApplicationInEditableStatus(application)) throw new BusinessValidationException("Can only edit attachments when application is in Draft");
            if (cmd.AttachmentInfo.DocumentType != DocumentType.OtherSupportingDocument && application.Attachments != null && application.Attachments.Any(a => a.DocumentType == cmd.AttachmentInfo.DocumentType))
            {
                throw new BusinessValidationException($"A document with type {cmd.AttachmentInfo.DocumentType.ToDescriptionString()} already exists on the application {cmd.AttachmentInfo.RecordId}");
            }

            var newDocId = Guid.NewGuid().ToString();

            await s3Provider.HandleCommand(new UploadFileCommand { Key = newDocId, File = cmd.AttachmentInfo.File, Folder = $"{cmd.AttachmentInfo.RecordType.ToDescriptionString()}/{application.CrmId}" });
            var documentRes = (await documentRepository.Manage(new CreateApplicationDocument { NewDocId = newDocId, ApplicationId = cmd.AttachmentInfo.RecordId, Document = new Document { Name = cmd.AttachmentInfo.File.FileName, DocumentType = cmd.AttachmentInfo.DocumentType, Size = GetFileSize(cmd.AttachmentInfo.File.Content) } }));
            return documentRes.Id;
        }

        private async Task<string> UploadProgressReportDocument(UploadAttachmentCommand cmd)
        {
            var canAccess = await CanAccessProgressReport(cmd.AttachmentInfo.RecordId, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this progress report.");
            var progressReport = (await reportRepository.Query(new ProgressReportsQuery { Id = cmd.AttachmentInfo.RecordId })).Items.SingleOrDefault();
            if (progressReport == null) throw new NotFoundException("Progress Report not found");
            //if (!ApplicationInEditableStatus(progressReport)) throw new BusinessValidationException("Can only edit attachments when application is in Draft");
            if (cmd.AttachmentInfo.DocumentType != DocumentType.OtherSupportingDocument && progressReport.Attachments != null && progressReport.Attachments.Any(a => a.DocumentType == cmd.AttachmentInfo.DocumentType))
            {
                throw new BusinessValidationException($"A document with type {cmd.AttachmentInfo.DocumentType.ToDescriptionString()} already exists on the application {cmd.AttachmentInfo.RecordId}");
            }

            var newDocId = Guid.NewGuid().ToString();

            await s3Provider.HandleCommand(new UploadFileCommand { Key = newDocId, File = cmd.AttachmentInfo.File, Folder = $"{cmd.AttachmentInfo.RecordType.ToDescriptionString()}/{progressReport.CrmId}" });
            var documentRes = (await documentRepository.Manage(new CreateProgressReportDocument { NewDocId = newDocId, ProgressReportId = cmd.AttachmentInfo.RecordId, Document = new Document { Name = cmd.AttachmentInfo.File.FileName, DocumentType = cmd.AttachmentInfo.DocumentType, Size = GetFileSize(cmd.AttachmentInfo.File.Content) } }));
            return documentRes.Id;
        }

        private async Task<string> UploadInvoiceDocument(UploadAttachmentCommand cmd)
        {
            var canAccess = await CanAccessInvoiceFromDocumentId(cmd.AttachmentInfo.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this invoice.");
            var invoice = (await reportRepository.Query(new InvoicesQuery { Id = cmd.AttachmentInfo.RecordId })).Items.SingleOrDefault();
            if (invoice == null) throw new NotFoundException("Invoice not found");
            //if (!ApplicationInEditableStatus(progressReport)) throw new BusinessValidationException("Can only edit attachments when application is in Draft");
            if (cmd.AttachmentInfo.DocumentType != DocumentType.OtherSupportingDocument && invoice.Attachments != null && invoice.Attachments.Any(a => a.DocumentType == cmd.AttachmentInfo.DocumentType))
            {
                throw new BusinessValidationException($"A document with type {cmd.AttachmentInfo.DocumentType.ToDescriptionString()} already exists on the invoice {cmd.AttachmentInfo.RecordId}");
            }

            var newDocId = Guid.NewGuid().ToString();

            await s3Provider.HandleCommand(new UploadFileCommand { Key = newDocId, File = cmd.AttachmentInfo.File, Folder = $"{cmd.AttachmentInfo.RecordType.ToDescriptionString()}/{invoice.Id}" });
            var documentRes = (await documentRepository.Manage(new CreateInvoiceDocument { NewDocId = newDocId, InvoiceId = cmd.AttachmentInfo.RecordId, Document = new Document { Name = cmd.AttachmentInfo.File.FileName, DocumentType = cmd.AttachmentInfo.DocumentType, Size = GetFileSize(cmd.AttachmentInfo.File.Content) } }));
            return documentRes.Id;
        }

        private async Task<string> DeleteApplicationDocument(DeleteAttachmentCommand cmd)
        {
            var canAccess = await CanAccessApplicationFromDocumentId(cmd.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");
            var documentRes = await documentRepository.Manage(new DeleteApplicationDocument { Id = cmd.Id });
            await s3Provider.HandleCommand(new UpdateTagsCommand { Key = cmd.Id, Folder = $"{RecordType.FullProposal.ToDescriptionString()}/{documentRes.RecordId}", FileTag = GetDeletedFileTag() });
            return documentRes.Id;
        }

        private async Task<string> DeleteProgressReportDocument(DeleteAttachmentCommand cmd)
        {
            var canAccess = await CanAccessProgressReportFromDocumentId(cmd.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this progress report.");
            var documentRes = await documentRepository.Manage(new DeleteProgressReportDocument { Id = cmd.Id });
            await s3Provider.HandleCommand(new UpdateTagsCommand { Key = cmd.Id, Folder = $"{RecordType.ProgressReport.ToDescriptionString()}/{documentRes.RecordId}", FileTag = GetDeletedFileTag() });
            return documentRes.Id;
        }

        private async Task<string> DeleteInvoiceDocument(DeleteAttachmentCommand cmd)
        {
            var canAccess = await CanAccessInvoiceFromDocumentId(cmd.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this invoice.");
            var documentRes = await documentRepository.Manage(new DeleteInvoiceDocument { Id = cmd.Id });
            await s3Provider.HandleCommand(new UpdateTagsCommand { Key = cmd.Id, Folder = $"{RecordType.Invoice.ToDescriptionString()}/{documentRes.RecordId}", FileTag = GetDeletedFileTag() });
            return documentRes.Id;
        }

        private string GetFileSize(byte[] file)
        {
            float bytes = file.Length;
            if (bytes < 1024) return $"{bytes.ToString("0.00")} B";
            bytes = bytes / 1024f;
            if (bytes < 1024) return $"{bytes.ToString("0.00")} KB";
            bytes = bytes / 1024f;
            if (bytes < 1024) return $"{bytes.ToString("0.00")} MB";
            bytes = bytes / 1024f;
            if (bytes < 1024) return $"{bytes.ToString("0.00")} GB";
            bytes = bytes / 1024f;
            return $"{bytes.ToString("0.00")} TB";
        }

        private bool ApplicationInEditableStatus(Application application)
        {
            return application.Status == ApplicationStatus.DraftProponent || application.Status == ApplicationStatus.DraftStaff || application.Status == ApplicationStatus.Withdrawn;
        }

        private string GetNextReportPeriodFromString(ReportingScheduleType? type, string period)
        {
            switch (type)
            {
                case ReportingScheduleType.Quarterly:
                    return GetNextFiscalQuarter(period);
                case ReportingScheduleType.Monthly:
                    return GetNextMonthString(period);
                default: return string.Empty;
            }
        }

        private string GetNextReportPeriodFromDate(ReportingScheduleType? type, DateTime date)
        {
            switch (type)
            {
                case ReportingScheduleType.Quarterly:
                    return GetNextFiscalQuarter(GetFiscalQuarterString(date));
                case ReportingScheduleType.Monthly:
                    return GetNextMonthString(GetMonthString(date));
                default: return string.Empty;
            }
        }

        private string GetReportPeriodFromDate(ReportingScheduleType? type, DateTime date)
        {
            switch (type)
            {
                case ReportingScheduleType.Quarterly:
                    return GetFiscalQuarterString(date);
                case ReportingScheduleType.Monthly:
                    return GetMonthString(date);
                default: return string.Empty;
            }
        }

        private static string GetMonthString(DateTime date)
        {
            string year = date.Year.ToString();
            int monthNumber = date.Month;

            return $"{year}-Month-{monthNumber}";
        }

        private static string GetNextMonthString(string monthString)
        {
            string[] parts = monthString.Split('-');
            if (parts.Length != 3 || !int.TryParse(parts[0], out int year) || parts[1] != "Month" || !int.TryParse(parts[2], out int month))
            {
                throw new ArgumentException("Invalid month format. Expected format: YYYY-Month-N (e.g., 2025-Month-2)");
            }

            // Increment month
            month++;

            // If we exceed Month-12, move to next year and reset to Month-1
            if (month > 12)
            {
                month = 1;
                year++;
            }

            return $"{year}-Month-{month}";
        }

        private static string GetFiscalQuarterString(DateTime date, int fiscalYearStartMonth = 4) //April
        {
            int year = date.Year;
            int monthOffset = (date.Month - fiscalYearStartMonth + 12) % 12;
            int quarter = (monthOffset / 3) + 1;

            // Adjust the fiscal year if the date falls before the fiscal start month
            if (date.Month < fiscalYearStartMonth)
            {
                year--;
            }

            return $"{year}-Q{quarter}";
        }

        private static string GetNextFiscalQuarter(string fiscalQuarter)
        {
            string[] parts = fiscalQuarter.Split('-');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int year) || !parts[1].StartsWith("Q") || !int.TryParse(parts[1].Substring(1), out int quarter))
            {
                throw new ArgumentException("Invalid fiscal quarter format. Expected format: YYYY-QN (e.g., 2025-Q2)");
            }

            // Increment quarter
            quarter++;

            // If we exceed Q4, move to next year and reset to Q1
            if (quarter > 4)
            {
                quarter = 1;
                year++;
            }

            return $"{year}-Q{quarter}";
        }

        private async Task<bool> CanAccessApplication(string? id, string? businessId)
        {
            if (string.IsNullOrEmpty(businessId)) throw new ArgumentNullException("Missing user's BusinessId");
            if (string.IsNullOrEmpty(id)) return true;
            return await applicationRepository.CanAccessApplication(id, businessId);
        }

        private async Task<bool> CanAccessApplicationFromDocumentId(string? id, string? businessId)
        {
            if (string.IsNullOrEmpty(businessId)) throw new ArgumentNullException("Missing user's BusinessId");
            if (string.IsNullOrEmpty(id)) return true;
            return await applicationRepository.CanAccessApplicationFromDocumentId(id, businessId);
        }

        private async Task<bool> CanAccessProject(string? id, string? businessId)
        {
            if (string.IsNullOrEmpty(businessId)) throw new ArgumentNullException("Missing user's BusinessId");
            if (string.IsNullOrEmpty(id)) return true;
            return await projectRepository.CanAccessProject(id, businessId);
        }

        private async Task<bool> CanAccessReport(string? id, string? businessId)
        {
            if (string.IsNullOrEmpty(businessId)) throw new ArgumentNullException("Missing user's BusinessId");
            if (string.IsNullOrEmpty(id)) return true;
            return await reportRepository.CanAccessReport(id, businessId);
        }

        private async Task<bool> CanAccessClaim(string? id, string? businessId)
        {
            if (string.IsNullOrEmpty(businessId)) throw new ArgumentNullException("Missing user's BusinessId");
            if (string.IsNullOrEmpty(id)) return true;
            return await reportRepository.CanAccessClaim(id, businessId);
        }

        private async Task<bool> CanAccessProgressReport(string? id, string? businessId)
        {
            if (string.IsNullOrEmpty(businessId)) throw new ArgumentNullException("Missing user's BusinessId");
            if (string.IsNullOrEmpty(id)) return true;
            return await reportRepository.CanAccessProgressReport(id, businessId);
        }

        private async Task<bool> CanAccessProgressReportFromDocumentId(string? id, string? businessId)
        {
            if (string.IsNullOrEmpty(businessId)) throw new ArgumentNullException("Missing user's BusinessId");
            if (string.IsNullOrEmpty(id)) return true;
            return await reportRepository.CanAccessProgressReportFromDocumentId(id, businessId);
        }

        private async Task<bool> CanAccessForecast(string? id, string? businessId)
        {
            if (string.IsNullOrEmpty(businessId)) throw new ArgumentNullException("Missing user's BusinessId");
            if (string.IsNullOrEmpty(id)) return true;
            return await reportRepository.CanAccessForecast(id, businessId);
        }

        private async Task<bool> CanAccessInvoiceFromDocumentId(string? id, string? businessId)
        {
            if (string.IsNullOrEmpty(businessId)) throw new ArgumentNullException("Missing user's BusinessId");
            if (string.IsNullOrEmpty(id)) return true;
            return await reportRepository.CanAccessInvoiceFromDocumentId(id, businessId);
        }

        private FilterOptions ParseFilter(string? filter)
        {
            if (string.IsNullOrEmpty(filter)) return new FilterOptions();

            var ret = new FilterOptions();

            var parts = filter.Split(',');
            foreach (var part in parts)
            {
                var name = part.Split('=')[0]?.ToLower();
                var value = part.Split("=")[1];

                switch (name)
                {
                    case "programtype":
                        {
                            ret.ProgramType = value.ToUpper();
                            break;
                        }
                    case "applicationtype":
                        {
                            if (value.ToLower() == "fp") value = "Full Proposal";
                            if (value.ToLower() == "eoi") value = "EOI";
                            ret.ApplicationType = value;
                            break;
                        }
                    case "status":
                        {
                            value = Regex.Replace(value, @"\*", "");
                            var selectedStatuses = value.Split("\\|");
                            var statuses = new List<int>();
                            foreach (var currStatus in selectedStatuses)
                            {
                                var submissionStatus = Enum.Parse<SubmissionPortalStatus>(currStatus);
                                var applicationStatuses = IntakeStatusMapper(submissionStatus);
                                statuses = statuses.Concat(applicationStatuses).ToList();
                            }
                            ret.Statuses = statuses;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            return ret;
        }

        private string GetOrderBy(string? orderBy)
        {
            if (string.IsNullOrEmpty(orderBy)) return "drr_name desc";
            orderBy = orderBy.ToLower();
            var descending = false;
            if (orderBy.Contains(" desc"))
            {
                descending = true;
                orderBy = Regex.Replace(orderBy, @" desc", "");
                orderBy = Regex.Replace(orderBy, @" asc", "");
            }

            var dir = descending ? " desc" : "";

            switch (orderBy)
            {
                case "id": return "drr_name" + dir;
                case "projecttitle": return "drr_projecttitle" + dir;
                case "applicationtype": return "drr_applicationtypename" + dir;
                case "programtype": return "drr_programname" + dir;
                case "status": return "statuscode" + dir;
                case "fundingrequest": return "drr_eligibleamount" + dir;
                case "modifieddate": return "modifiedon" + dir;
                case "submitteddate": return "drr_submitteddate" + dir;
                default: return "drr_name";
            }
        }

        private List<int> IntakeStatusMapper(SubmissionPortalStatus? status)
        {
            var ret = new List<int>();
            switch (status)
            {
                case SubmissionPortalStatus.Draft:
                    {
                        ret.Add((int)(int)ApplicationStatusOptionSet.DraftStaff);
                        ret.Add((int)ApplicationStatusOptionSet.DraftProponent);
                        break;
                    }
                case SubmissionPortalStatus.UnderReview:
                    {
                        ret.Add((int)ApplicationStatusOptionSet.Submitted);
                        ret.Add((int)ApplicationStatusOptionSet.InReview);
                        break;
                    }
                case SubmissionPortalStatus.EligibleInvited:
                    {
                        ret.Add((int)ApplicationStatusOptionSet.Invited);
                        break;
                    }
                case SubmissionPortalStatus.EligiblePending:
                    {
                        ret.Add((int)ApplicationStatusOptionSet.InPool);
                        break;
                    }
                case SubmissionPortalStatus.Ineligible:
                    {
                        ret.Add((int)ApplicationStatusOptionSet.Ineligible);
                        break;
                    }
                case SubmissionPortalStatus.Withdrawn:
                    {
                        ret.Add((int)ApplicationStatusOptionSet.Withdrawn);
                        break;
                    }
                case SubmissionPortalStatus.Closed:
                    {
                        ret.Add((int)ApplicationStatusOptionSet.Closed);
                        break;
                    }
                case SubmissionPortalStatus.FullProposalSubmitted:
                    {
                        ret.Add((int)ApplicationStatusOptionSet.FPSubmitted);
                        break;
                    }
                case SubmissionPortalStatus.Approved:
                    {
                        ret.Add((int)ApplicationStatusOptionSet.Approved);
                        break;
                    }
                case SubmissionPortalStatus.ApprovedInPrinciple:
                    {
                        ret.Add((int)ApplicationStatusOptionSet.ApprovedInPrinciple);
                        break;
                    }
                default: break;
            }
            return ret;
        }
    }
}
