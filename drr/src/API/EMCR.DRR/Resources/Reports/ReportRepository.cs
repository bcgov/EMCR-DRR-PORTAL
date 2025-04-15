using AutoMapper;
using EMCR.DRR.API.Resources.Projects;
using EMCR.DRR.API.Services;
using EMCR.DRR.Dynamics;
using EMCR.DRR.Managers.Intake;
using EMCR.DRR.Resources.Applications;
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

        public async Task<bool> CanAccessProgressReportFromDocumentId(string id, string businessId, bool forUpdate)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();
            var document = await readCtx.bcgov_documenturls.Expand(d => d.bcgov_ProgressReport).Where(a => a.bcgov_documenturlid == Guid.Parse(id)).SingleOrDefaultAsync();
            var existingProgressReport = await readCtx.drr_projectprogresses.Expand(a => a.drr_Project).Where(a => a.drr_projectprogressid == document.bcgov_ProgressReport.drr_projectprogressid).SingleOrDefaultAsync();
            if (existingProgressReport == null) return true;
            if (forUpdate && existingProgressReport.statuscode == (int)ProjectProgressReportStatusOptionSet.Submitted) return false;
            readCtx.AttachTo(nameof(readCtx.drr_projects), existingProgressReport.drr_Project);
            await readCtx.LoadPropertyAsync(existingProgressReport.drr_Project, nameof(drr_project.drr_ProponentName));
            return (!string.IsNullOrEmpty(existingProgressReport.drr_Project.drr_ProponentName.drr_bceidguid)) && existingProgressReport.drr_Project.drr_ProponentName.drr_bceidguid.Equals(businessId);
        }

        public async Task<bool> CanAccessClaim(string id, string businessId)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();
            var existingClaim = await readCtx.drr_projectclaims.Expand(a => a.drr_Project).Where(a => a.drr_name == id).SingleOrDefaultAsync();
            if (existingClaim == null) return true;
            readCtx.AttachTo(nameof(readCtx.drr_projects), existingClaim.drr_Project);
            await readCtx.LoadPropertyAsync(existingClaim.drr_Project, nameof(drr_project.drr_ProponentName));
            return (!string.IsNullOrEmpty(existingClaim.drr_Project.drr_ProponentName.drr_bceidguid)) && existingClaim.drr_Project.drr_ProponentName.drr_bceidguid.Equals(businessId);
        }

        public async Task<bool> CanAccessForecast(string id, string businessId)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();
            var existingForecast = await readCtx.drr_projectbudgetforecasts.Expand(a => a.drr_Project).Where(a => a.drr_name == id).SingleOrDefaultAsync();
            if (existingForecast == null) return true;
            readCtx.AttachTo(nameof(readCtx.drr_projects), existingForecast.drr_Project);
            await readCtx.LoadPropertyAsync(existingForecast.drr_Project, nameof(drr_project.drr_ProponentName));
            return (!string.IsNullOrEmpty(existingForecast.drr_Project.drr_ProponentName.drr_bceidguid)) && existingForecast.drr_Project.drr_ProponentName.drr_bceidguid.Equals(businessId);
        }

        public async Task<bool> CanAccessForecastFromDocumentId(string id, string businessId, bool forUpdate)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();
            var document = await readCtx.bcgov_documenturls.Expand(d => d.bcgov_projectbudgetforecastid).Where(a => a.bcgov_documenturlid == Guid.Parse(id)).SingleOrDefaultAsync();
            var existingForecast = await readCtx.drr_projectbudgetforecasts.Expand(a => a.drr_Project).Where(a => a.drr_projectbudgetforecastid == document.bcgov_projectbudgetforecastid.drr_projectbudgetforecastid).SingleOrDefaultAsync();
            if (existingForecast == null) return true;
            if (forUpdate && existingForecast.statuscode == (int)ForecastStatusOptionSet.Submitted) return false;
            readCtx.AttachTo(nameof(readCtx.drr_projects), existingForecast.drr_Project);
            await readCtx.LoadPropertyAsync(existingForecast.drr_Project, nameof(drr_project.drr_ProponentName));
            return (!string.IsNullOrEmpty(existingForecast.drr_Project.drr_ProponentName.drr_bceidguid)) && existingForecast.drr_Project.drr_ProponentName.drr_bceidguid.Equals(businessId);
        }

        public async Task<bool> CanAccessInvoiceFromDocumentId(string id, string businessId, bool forUpdate)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();
            var document = await readCtx.bcgov_documenturls.Expand(d => d.bcgov_ProjectExpenditure).Where(a => a.bcgov_documenturlid == Guid.Parse(id)).SingleOrDefaultAsync();
            var existingInvoice = await readCtx.drr_projectexpenditures.Expand(a => a.drr_Project).Expand(a => a.drr_Claim).Where(a => a.drr_projectexpenditureid == document.bcgov_ProjectExpenditure.drr_projectexpenditureid).SingleOrDefaultAsync();
            if (existingInvoice == null) return true;
            if (existingInvoice.drr_Claim != null && forUpdate && existingInvoice.drr_Claim.statuscode == (int)ProjectClaimStatusOptionSet.Submitted) return false;
            readCtx.AttachTo(nameof(readCtx.drr_projects), existingInvoice.drr_Project);
            await readCtx.LoadPropertyAsync(existingInvoice.drr_Project, nameof(drr_project.drr_ProponentName));
            return (!string.IsNullOrEmpty(existingInvoice.drr_Project.drr_ProponentName.drr_bceidguid)) && existingInvoice.drr_Project.drr_ProponentName.drr_bceidguid.Equals(businessId);
        }

        public async Task<bool> CanAccessReport(string id, string businessId)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();
            var existingReport = await readCtx.drr_projectreports.Expand(a => a.drr_Project).Where(a => a.drr_name == id).SingleOrDefaultAsync();
            if (existingReport == null) return true;
            readCtx.AttachTo(nameof(readCtx.drr_projects), existingReport.drr_Project);
            await readCtx.LoadPropertyAsync(existingReport.drr_Project, nameof(drr_project.drr_ProponentName));
            return (!string.IsNullOrEmpty(existingReport.drr_Project.drr_ProponentName.drr_bceidguid)) && existingReport.drr_Project.drr_ProponentName.drr_bceidguid.Equals(businessId);
        }

        public async Task<ManageReportCommandResult> Manage(ManageReportCommand cmd)
        {
            return cmd switch
            {
                SaveProgressReport c => await HandleSaveProgressReport(c),
                SubmitProgressReport c => await HandleSubmitProgressReport(c),
                CreateProjectReport c => await HandleCreateProjectReport(c),
                SaveClaim c => await HandleSaveClaim(c),
                SubmitClaim c => await HandleSubmitClaim(c),
                SaveForecast c => await HandleSaveForecast(c),
                SubmitForecast c => await HandleSubmitForecast(c),
                CreateInvoice c => await HandleCreateInvoice(c),
                DeleteInvoice c => await HandleDeleteInvoice(c),
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
                ctx.LoadPropertyAsync(existingProgressReport, nameof(drr_projectprogress.drr_AuthorizedRepresentativeContact)),
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

            RemoveOldProgressReportData(ctx, existingProgressReport, drrProgressReport);
            ctx.AttachTo(nameof(ctx.drr_projectprogresses), drrProgressReport);

            var projectActivityMasterListTask = LoadProjectActivityList(ctx, drrProgressReport);
            await Task.WhenAll([
                projectActivityMasterListTask,
            ]);
            var projectActivityMasterList = projectActivityMasterListTask.Result;
            var authorizedRep = drrProgressReport.drr_AuthorizedRepresentativeContact;
            if (authorizedRep != null) SaveProgressReportAuthrizedRepresentative(ctx, drrProgressReport, authorizedRep, existingProgressReport.drr_AuthorizedRepresentativeContact);


            AddWorkplanActivities(ctx, drrProgressReport, projectActivityMasterList, existingProgressReport);
            AddPastEvents(ctx, drrProgressReport, existingProgressReport);
            await AddUpcomingEvents(ctx, drrProgressReport, existingProgressReport);
            AddFundingSignage(ctx, drrProgressReport, existingProgressReport);
            UpdateProgressReportDocuments(ctx, drrProgressReport);
            await SetProgressReportDeclarations(ctx, drrProgressReport, "Progress Report");

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

        private static async Task SetProgressReportDeclarations(DRRContext drrContext, drr_projectprogress progressReport, string ApplicationTypeName)
        {
            var accuracyDeclaration = (await drrContext.drr_legaldeclarations.Where(d => d.statecode == (int)EntityState.Active && d.drr_declarationtype == (int)DeclarationTypeOptionSet.AccuracyOfInformation && d.drr_formtype == (int)FormTypeOptionSet.Report && d.drr_ApplicationType.drr_name == ApplicationTypeName).GetAllPagesAsync()).FirstOrDefault();
            var representativeDeclaration = (await drrContext.drr_legaldeclarations.Where(d => d.statecode == (int)EntityState.Active && d.drr_declarationtype == (int)DeclarationTypeOptionSet.AuthorizedRepresentative && d.drr_formtype == (int)FormTypeOptionSet.Report && d.drr_ApplicationType.drr_name == ApplicationTypeName).GetAllPagesAsync()).FirstOrDefault();

            if (accuracyDeclaration != null)
            {
                drrContext.SetLink(progressReport, nameof(drr_projectprogress.drr_AccuracyofInformationDeclaration), accuracyDeclaration);
            }

            if (representativeDeclaration != null)
            {
                drrContext.SetLink(progressReport, nameof(drr_projectprogress.drr_AuthorizedRepresentativeDeclaration), representativeDeclaration);
            }
        }

        //HandleSaveClaim
        public async Task<ManageReportCommandResult> HandleSaveClaim(SaveClaim cmd)
        {
            var ctx = dRRContextFactory.Create();
            var existingClaim = await ctx.drr_projectclaims.Where(p => p.drr_name == cmd.Claim.Id).SingleOrDefaultAsync();
            if (existingClaim == null) throw new NotFoundException("Claim not found");

            var loadTasks = new List<Task>
            {
                ctx.LoadPropertyAsync(existingClaim, nameof(drr_projectclaim.drr_drr_projectclaim_drr_projectexpenditure_Claim)),
                ctx.LoadPropertyAsync(existingClaim, nameof(drr_projectclaim.drr_AuthorizedRepresentativeContact)),
            };

            await Task.WhenAll(loadTasks);

            ctx.DetachAll();
            var drrClaim = mapper.Map<drr_projectclaim>(cmd.Claim);
            drrClaim.drr_projectclaimid = existingClaim.drr_projectclaimid;

            //RemoveOldClaimData(ctx, existingClaim, drrClaim); //if we use this, it would be for invoices - but we're doing separate commands to create/delete invoices
            ctx.AttachTo(nameof(ctx.drr_projectclaims), drrClaim);

            var authorizedRep = drrClaim.drr_AuthorizedRepresentativeContact;
            if (authorizedRep != null) SaveClaimAuthrizedRepresentative(ctx, drrClaim, authorizedRep, existingClaim.drr_AuthorizedRepresentativeContact);
            UpdateInvoices(ctx, drrClaim, existingClaim);
            await SetClaimDeclarations(ctx, drrClaim, "Claims Form");

            ctx.UpdateObject(drrClaim);
            await ctx.SaveChangesAsync();
            ctx.DetachAll();

            return new ManageReportCommandResult { Id = existingClaim.drr_name };
        }

        public async Task<ManageReportCommandResult> HandleSubmitClaim(SubmitClaim cmd)
        {
            var ctx = dRRContextFactory.Create();
            var claim = await ctx.drr_projectclaims.Where(a => a.drr_name == cmd.Id).SingleOrDefaultAsync();
            claim.statuscode = (int)ProjectClaimStatusOptionSet.Submitted;
            claim.drr_datesubmitted = DateTime.UtcNow;
            ctx.UpdateObject(claim);
            await ctx.SaveChangesAsync();
            ctx.DetachAll();
            return new ManageReportCommandResult { Id = cmd.Id };
        }

        private static async Task SetClaimDeclarations(DRRContext drrContext, drr_projectclaim claim, string ApplicationTypeName)
        {
            var accuracyDeclaration = (await drrContext.drr_legaldeclarations.Where(d => d.statecode == (int)EntityState.Active && d.drr_declarationtype == (int)DeclarationTypeOptionSet.AccuracyOfInformation && d.drr_formtype == (int)FormTypeOptionSet.Report && d.drr_ApplicationType.drr_name == ApplicationTypeName).GetAllPagesAsync()).FirstOrDefault();
            var representativeDeclaration = (await drrContext.drr_legaldeclarations.Where(d => d.statecode == (int)EntityState.Active && d.drr_declarationtype == (int)DeclarationTypeOptionSet.AuthorizedRepresentative && d.drr_formtype == (int)FormTypeOptionSet.Report && d.drr_ApplicationType.drr_name == ApplicationTypeName).GetAllPagesAsync()).FirstOrDefault();

            if (accuracyDeclaration != null)
            {
                drrContext.SetLink(claim, nameof(drr_projectclaim.drr_AccuracyofInformationDeclaration), accuracyDeclaration);
            }

            if (representativeDeclaration != null)
            {
                drrContext.SetLink(claim, nameof(drr_projectclaim.drr_AuthorizedRepresentativeDeclaration), representativeDeclaration);
            }
        }

        private static void SaveClaimAuthrizedRepresentative(DRRContext drrContext, drr_projectclaim claim, contact authorizedRep, contact existingRep)
        {
            if (existingRep == null || existingRep.contactid == null || authorizedRep.contactid != existingRep.contactid)
            {
                drrContext.AddTocontacts(authorizedRep);
                drrContext.AddLink(authorizedRep, nameof(authorizedRep.drr_contact_drr_projectclaim_AuthorizedRepresentativeContact), claim);
                drrContext.SetLink(claim, nameof(drr_projectclaim.drr_AuthorizedRepresentativeContact), authorizedRep);
            }
            else
            {
                drrContext.AttachTo(nameof(drrContext.contacts), authorizedRep);
                drrContext.UpdateObject(authorizedRep);
            }
        }

        public async Task<ManageReportCommandResult> HandleSaveForecast(SaveForecast cmd)
        {
            var ctx = dRRContextFactory.Create();
            var existingForecast = await ctx.drr_projectbudgetforecasts.Where(p => p.drr_name == cmd.Forecast.Id).SingleOrDefaultAsync();
            if (existingForecast == null) throw new NotFoundException("Forecast not found");

            var loadTasks = new List<Task>
            {
                ctx.LoadPropertyAsync(existingForecast, nameof(drr_projectbudgetforecast.drr_drr_projectbudgetforecast_drr_budgetforecastreportitem_ProjectBudgetForecast)),
                ctx.LoadPropertyAsync(existingForecast, nameof(drr_projectbudgetforecast.drr_AuthorizedRepresentativeContact)),
                ctx.LoadPropertyAsync(existingForecast, nameof(drr_projectbudgetforecast.bcgov_drr_projectbudgetforecast_bcgov_documenturl_projectbudgetforecastid)),
            };

            await Task.WhenAll(loadTasks);

            ctx.DetachAll();
            var drrForecast = mapper.Map<drr_projectbudgetforecast>(cmd.Forecast);
            drrForecast.drr_projectbudgetforecastid = existingForecast.drr_projectbudgetforecastid;

            foreach (var doc in drrForecast.bcgov_drr_projectbudgetforecast_bcgov_documenturl_projectbudgetforecastid)
            {
                var curr = existingForecast.bcgov_drr_projectbudgetforecast_bcgov_documenturl_projectbudgetforecastid.SingleOrDefault(d => d.bcgov_documenturlid == doc.bcgov_documenturlid);
                if (curr != null) curr.bcgov_documentcomments = doc.bcgov_documentcomments;
            }

            drrForecast.bcgov_drr_projectbudgetforecast_bcgov_documenturl_projectbudgetforecastid = existingForecast.bcgov_drr_projectbudgetforecast_bcgov_documenturl_projectbudgetforecastid;

            //RemoveOldForecastData(ctx, existingForecast, drrForecast); //maybe not needed?
            ctx.AttachTo(nameof(ctx.drr_projectbudgetforecasts), drrForecast);

            UpdateForecastItems(ctx, drrForecast, existingForecast);
            UpdateForecastReportDocuments(ctx, drrForecast);

            var authorizedRep = drrForecast.drr_AuthorizedRepresentativeContact;
            if (authorizedRep != null) SaveForecastAuthrizedRepresentative(ctx, drrForecast, authorizedRep, existingForecast.drr_AuthorizedRepresentativeContact);
            await SetForecastDeclarations(ctx, drrForecast, "Budget Forecast Report");

            ctx.UpdateObject(drrForecast);
            await ctx.SaveChangesAsync();
            ctx.DetachAll();

            return new ManageReportCommandResult { Id = existingForecast.drr_name };
        }

        public async Task<ManageReportCommandResult> HandleSubmitForecast(SubmitForecast cmd)
        {
            var ctx = dRRContextFactory.Create();
            var forecast = await ctx.drr_projectbudgetforecasts.Where(a => a.drr_name == cmd.Id).SingleOrDefaultAsync();
            forecast.statuscode = (int)ForecastStatusOptionSet.Submitted;
            forecast.drr_submissiondate = DateTime.UtcNow;
            ctx.UpdateObject(forecast);
            await ctx.SaveChangesAsync();
            ctx.DetachAll();
            return new ManageReportCommandResult { Id = cmd.Id };
        }

        private static async Task SetForecastDeclarations(DRRContext drrContext, drr_projectbudgetforecast forecast, string ApplicationTypeName)
        {
            var accuracyDeclaration = (await drrContext.drr_legaldeclarations.Where(d => d.statecode == (int)EntityState.Active && d.drr_declarationtype == (int)DeclarationTypeOptionSet.AccuracyOfInformation && d.drr_formtype == (int)FormTypeOptionSet.Report && d.drr_ApplicationType.drr_name == ApplicationTypeName).GetAllPagesAsync()).FirstOrDefault();
            var representativeDeclaration = (await drrContext.drr_legaldeclarations.Where(d => d.statecode == (int)EntityState.Active && d.drr_declarationtype == (int)DeclarationTypeOptionSet.AuthorizedRepresentative && d.drr_formtype == (int)FormTypeOptionSet.Report && d.drr_ApplicationType.drr_name == ApplicationTypeName).GetAllPagesAsync()).FirstOrDefault();

            if (accuracyDeclaration != null)
            {
                drrContext.SetLink(forecast, nameof(drr_projectbudgetforecast.drr_AccuracyofInformationDeclarationId), accuracyDeclaration);
            }

            if (representativeDeclaration != null)
            {
                drrContext.SetLink(forecast, nameof(drr_projectbudgetforecast.drr_AuthorizedRepresentativeDeclarationid), representativeDeclaration);
            }
        }

        private static void SaveForecastAuthrizedRepresentative(DRRContext drrContext, drr_projectbudgetforecast forecast, contact authorizedRep, contact existingRep)
        {
            if (existingRep == null || existingRep.contactid == null || authorizedRep.contactid != existingRep.contactid)
            {
                drrContext.AddTocontacts(authorizedRep);
                drrContext.AddLink(authorizedRep, nameof(authorizedRep.drr_contact_drr_projectbudgetforecast_AuthorizedRepresentativeContact), forecast);
                drrContext.SetLink(forecast, nameof(drr_projectbudgetforecast.drr_AuthorizedRepresentativeContact), authorizedRep);
            }
            else
            {
                drrContext.AttachTo(nameof(drrContext.contacts), authorizedRep);
                drrContext.UpdateObject(authorizedRep);
            }
        }

        public async Task<ManageReportCommandResult> HandleCreateInvoice(CreateInvoice cmd)
        {
            var ctx = dRRContextFactory.Create();
            var claim = await ctx.drr_projectclaims.Expand(c => c.drr_Project).Where(a => a.drr_name == cmd.ClaimId).SingleOrDefaultAsync();
            var invoice = new drr_projectexpenditure
            {
                drr_projectexpenditureid = Guid.Parse(cmd.InvoiceId),
            };
            ctx.AddTodrr_projectexpenditures(invoice);
            ctx.AddLink(claim, nameof(claim.drr_drr_projectclaim_drr_projectexpenditure_Claim), invoice);
            ctx.SetLink(invoice, nameof(invoice.drr_Claim), claim);
            ctx.SetLink(invoice, nameof(invoice.drr_Project), claim.drr_Project);
            await ctx.SaveChangesAsync();
            ctx.DetachAll();
            return new ManageReportCommandResult { Id = invoice.drr_projectexpenditureid.ToString() ?? string.Empty };
        }

        public async Task<ManageReportCommandResult> HandleDeleteInvoice(DeleteInvoice cmd)
        {
            var ctx = dRRContextFactory.Create();
            var invoice = await ctx.drr_projectexpenditures.Where(a => a.drr_projectexpenditureid == Guid.Parse(cmd.InvoiceId)).SingleOrDefaultAsync();
            if (invoice == null) throw new NotFoundException("Invoice not found");
            ctx.DeleteObject(invoice);
            await ctx.SaveChangesAsync();
            ctx.DetachAll();
            return new ManageReportCommandResult { Id = invoice.drr_projectexpenditureid.ToString() ?? string.Empty };
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
            return new ManageReportCommandResult { Id = createdReport.drr_name };

        }

        private static void SaveProgressReportAuthrizedRepresentative(DRRContext drrContext, drr_projectprogress progressReport, contact authorizedRep, contact existingRep)
        {
            if (existingRep == null || existingRep.contactid == null || authorizedRep.contactid != existingRep.contactid)
            {
                drrContext.AddTocontacts(authorizedRep);
                drrContext.AddLink(authorizedRep, nameof(authorizedRep.drr_contact_drr_projectprogress_AuthorizedRepresentativeContact), progressReport);
                drrContext.SetLink(progressReport, nameof(drr_projectprogress.drr_AuthorizedRepresentativeContact), authorizedRep);
            }
            else
            {
                drrContext.AttachTo(nameof(drrContext.contacts), authorizedRep);
                drrContext.UpdateObject(authorizedRep);
            }
        }

        private void RemoveOldProgressReportData(DRRContext ctx, drr_projectprogress existingProgressReport, drr_projectprogress drrProgressReport)
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

        private void RemoveOldClaimData(DRRContext ctx, drr_projectclaim existingClaim, drr_projectclaim drrClaim)
        {
            var invoicesToRemove = existingClaim.drr_drr_projectclaim_drr_projectexpenditure_Claim.Where(curr =>
            !drrClaim.drr_drr_projectclaim_drr_projectexpenditure_Claim.Any(updated => updated.drr_projectexpenditureid == curr.drr_projectexpenditureid)).ToList();

            foreach (var invoice in invoicesToRemove)
            {
                ctx.AttachTo(nameof(ctx.drr_projectexpenditures), invoice);
                ctx.DeleteObject(invoice);
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
                    if (activity.statuscode == (int)WorkplanStatusOptionSet.NoLongerNeeded) drrContext.DeactivateObject(activity, activity.statuscode.Value);
                    else if (activity.statuscode != null) drrContext.ActivateObject(activity, activity.statuscode.Value);
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

        private static void UpdateProgressReportDocuments(DRRContext drrContext, drr_projectprogress progressReport)
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

        private static void UpdateForecastReportDocuments(DRRContext drrContext, drr_projectbudgetforecast forecast)
        {
            foreach (var doc in forecast.bcgov_drr_projectbudgetforecast_bcgov_documenturl_projectbudgetforecastid)
            {
                if (doc != null)
                {
                    drrContext.AttachTo(nameof(drrContext.bcgov_documenturls), doc);
                    drrContext.UpdateObject(doc);
                }
            }
        }

        private static void UpdateInvoices(DRRContext drrContext, drr_projectclaim claim, drr_projectclaim existingClaim)
        {
            foreach (var invoice in claim.drr_drr_projectclaim_drr_projectexpenditure_Claim)
            {
                if (invoice != null)
                {
                    var existingInvoice = existingClaim.drr_drr_projectclaim_drr_projectexpenditure_Claim.Where(i => i.drr_name == invoice.drr_name).SingleOrDefault();
                    if (existingInvoice != null)
                    {
                        invoice.drr_projectexpenditureid = existingInvoice.drr_projectexpenditureid;
                        foreach (var doc in invoice.bcgov_drr_projectexpenditure_bcgov_documenturl_ProjectExpenditure)
                        {

                            var curr = existingInvoice.bcgov_drr_projectexpenditure_bcgov_documenturl_ProjectExpenditure.SingleOrDefault(d => d.bcgov_documenturlid == doc.bcgov_documenturlid);
                            if (curr != null) curr.bcgov_documentcomments = doc.bcgov_documentcomments;
                        }

                        invoice.bcgov_drr_projectexpenditure_bcgov_documenturl_ProjectExpenditure = existingInvoice.bcgov_drr_projectexpenditure_bcgov_documenturl_ProjectExpenditure;

                        foreach (var doc in invoice.bcgov_drr_projectexpenditure_bcgov_documenturl_ProjectExpenditure)
                        {
                            if (doc != null)
                            {
                                drrContext.AttachTo(nameof(drrContext.bcgov_documenturls), doc);
                                drrContext.UpdateObject(doc);
                            }
                        }
                    }

                    drrContext.AttachTo(nameof(drrContext.drr_projectexpenditures), invoice);
                    drrContext.UpdateObject(invoice);
                }
            }
        }

        private static void UpdateForecastItems(DRRContext drrContext, drr_projectbudgetforecast forecast, drr_projectbudgetforecast existingForecast)
        {
            foreach (var item in forecast.drr_drr_projectbudgetforecast_drr_budgetforecastreportitem_ProjectBudgetForecast)
            {
                if (item != null)
                {
                    var existingItem = existingForecast.drr_drr_projectbudgetforecast_drr_budgetforecastreportitem_ProjectBudgetForecast.Where(i => i.drr_budgetforecastreportitemid == item.drr_budgetforecastreportitemid).SingleOrDefault();
                    if (existingItem != null)
                    {
                        item.drr_budgetforecastreportitemid = existingItem.drr_budgetforecastreportitemid;
                        drrContext.AttachTo(nameof(drrContext.drr_budgetforecastreportitems), item);
                        drrContext.UpdateObject(item);
                    }
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

        public async Task<InvoiceQueryResult> Query(InvoiceQuery query)
        {
            return query switch
            {
                InvoicesQuery q => await HandleQueryInvoice(q),
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

            if (!string.IsNullOrEmpty(query.InvoiceId)) return await QueryClaimFromInvoice(readCtx, query);

            var claimsQuery = readCtx.drr_projectclaims
                .Where(a => a.statuscode != (int)ProjectClaimStatusOptionSet.Inactive);
            if (!string.IsNullOrEmpty(query.Id)) claimsQuery = claimsQuery.Where(a => a.drr_name == query.Id);

            var results = (await claimsQuery.GetAllPagesAsync(ct)).ToList();
            var length = results.Count;

            await Parallel.ForEachAsync(results, ct, async (claim, ct) => await ParallelLoadClaim(readCtx, claim, ct));
            return new ClaimQueryResult { Items = mapper.Map<IEnumerable<ClaimDetails>>(results), Length = length };
        }

        private async Task<ClaimQueryResult> QueryClaimFromInvoice(DRRContext readCtx, ClaimsQuery query)
        {
            var ct = new CancellationTokenSource().Token;
            if (string.IsNullOrEmpty(query.InvoiceId)) throw new NotFoundException("Claim not found");

            var invoiceQuery = readCtx.drr_projectexpenditures.Expand(i => i.drr_Claim)
                .Where(a => a.statuscode != (int)InvoiceStatusOptionSet.Inactive && a.drr_projectexpenditureid == Guid.Parse(query.InvoiceId));

            var results = (await invoiceQuery.GetAllPagesAsync(ct)).ToList();
            var length = results.Count;
            var claimResults = results.Select(i => i.drr_Claim).ToList();

            await Parallel.ForEachAsync(claimResults, ct, async (claim, ct) => await ParallelLoadClaim(readCtx, claim, ct));
            return new ClaimQueryResult { Items = mapper.Map<IEnumerable<ClaimDetails>>(claimResults), Length = length };
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
                .Where(a => a.statuscode != (int)ForecastStatusOptionSet.Inactive);
            if (!string.IsNullOrEmpty(query.Id)) forecastsQuery = forecastsQuery.Where(a => a.drr_name == query.Id);

            var results = (await forecastsQuery.GetAllPagesAsync(ct)).ToList();
            var length = results.Count;

            await Parallel.ForEachAsync(results, ct, async (f, ct) => await ParallelLoadForecast(readCtx, f, ct));
            return new ForecastQueryResult { Items = mapper.Map<IEnumerable<ForecastDetails>>(results), Length = length };
        }

        private async Task<InvoiceQueryResult> HandleQueryInvoice(InvoicesQuery query)
        {
            var ct = new CancellationTokenSource().Token;
            var readCtx = dRRContextFactory.CreateReadOnly();

            var invoicesQuery = readCtx.drr_projectexpenditures
                .Where(a => a.statuscode != (int)InvoiceStatusOptionSet.Inactive);
            if (!string.IsNullOrEmpty(query.Id)) invoicesQuery = invoicesQuery.Where(a => a.drr_projectexpenditureid == Guid.Parse(query.Id));

            var results = (await invoicesQuery.GetAllPagesAsync(ct)).ToList();
            var length = results.Count;

            return new InvoiceQueryResult { Items = mapper.Map<IEnumerable<Managers.Intake.Invoice>>(results), Length = length };
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
            if (report.drr_ClaimReport != null) report.drr_ClaimReport.drr_ProjectReport = report;
            if (report.drr_ProgressReport != null) report.drr_ProgressReport.drr_ProjectReport = report;
            if (report.drr_BudgetForecast != null) report.drr_BudgetForecast.drr_ProjectReport = report;
        }

        private static async Task ParallelLoadForecast(DRRContext ctx, drr_projectbudgetforecast forecast, CancellationToken ct)
        {
            ctx.AttachTo(nameof(DRRContext.drr_projectbudgetforecasts), forecast);
            var loadTasks = new List<Task>
            {
                ctx.LoadPropertyAsync(forecast, nameof(drr_projectbudgetforecast.drr_Project), ct),
                ctx.LoadPropertyAsync(forecast, nameof(drr_projectbudgetforecast.drr_ProjectReport), ct),
                ctx.LoadPropertyAsync(forecast, nameof(drr_projectbudgetforecast.drr_AuthorizedRepresentativeContact), ct),
                ctx.LoadPropertyAsync(forecast, nameof(drr_projectbudgetforecast.drr_drr_projectbudgetforecast_drr_budgetforecastreportitem_ProjectBudgetForecast), ct),
                ctx.LoadPropertyAsync(forecast, nameof(drr_projectbudgetforecast.bcgov_drr_projectbudgetforecast_bcgov_documenturl_projectbudgetforecastid), ct),
            };

            await Task.WhenAll(loadTasks);

            var secondLoadTasks = new List<Task>{
                ParallelLoadForecastItems(ctx, forecast, ct),
                ParallelLoadForecastReportDocumentTypes(ctx, forecast, ct),
            };

            if (forecast.drr_ProjectReport != null)
            {
                ctx.AttachTo(nameof(DRRContext.drr_projectreports), forecast.drr_ProjectReport);
                secondLoadTasks.Add(ctx.LoadPropertyAsync(forecast.drr_ProjectReport, nameof(drr_projectreport.drr_ReportPeriod), ct));
            }

            await Task.WhenAll(secondLoadTasks);
        }

        private static async Task ParallelLoadForecastItems(DRRContext ctx, drr_projectbudgetforecast forecast, CancellationToken ct)
        {
            await forecast.drr_drr_projectbudgetforecast_drr_budgetforecastreportitem_ProjectBudgetForecast.ForEachAsync(5, async item =>
            {
                ctx.AttachTo(nameof(DRRContext.drr_budgetforecastreportitems), item);
                await ctx.LoadPropertyAsync(item, nameof(drr_budgetforecastreportitem.drr_FiscalYear), ct);
            });
        }

        private static async Task ParallelLoadClaim(DRRContext ctx, drr_projectclaim claim, CancellationToken ct)
        {
            ctx.AttachTo(nameof(DRRContext.drr_projectclaims), claim);
            var loadTasks = new List<Task>
            {
                ctx.LoadPropertyAsync(claim, nameof(drr_projectclaim.drr_Project), ct),
                ctx.LoadPropertyAsync(claim, nameof(drr_projectclaim.drr_ProjectReport), ct),
                ctx.LoadPropertyAsync(claim, nameof(drr_projectclaim.drr_AuthorizedRepresentativeContact), ct),
                ctx.LoadPropertyAsync(claim, nameof(drr_projectclaim.drr_drr_projectclaim_drr_projectexpenditure_Claim), ct), //Invoices
                
                //These might be for future...
                //ctx.LoadPropertyAsync(claim, nameof(drr_projectclaim.drr_drr_projectclaim_drr_projectpayment_ProjectClaim), ct), //Payments
                //ctx.LoadPropertyAsync(claim, nameof(drr_projectclaim.bcgov_drr_projectprogress_bcgov_documenturl_ProgressReport), ct), //Attachments
            };

            await Task.WhenAll(loadTasks);

            var secondLoadTasks = new List<Task>{
                ParallelLoadInvoiceAttachments(ctx, claim, ct),
            };

            if (claim.drr_ProjectReport != null)
            {
                ctx.AttachTo(nameof(DRRContext.drr_projectreports), claim.drr_ProjectReport);
                secondLoadTasks.Add(ctx.LoadPropertyAsync(claim.drr_ProjectReport, nameof(drr_projectreport.drr_ReportPeriod), ct));
            }

            if (claim.drr_Project != null)
            {
                ctx.AttachTo(nameof(DRRContext.drr_projects), claim.drr_Project);
                secondLoadTasks.Add(ctx.LoadPropertyAsync(claim.drr_Project, nameof(drr_project.drr_FullProposalApplication), ct));
                secondLoadTasks.Add(ctx.LoadPropertyAsync(claim.drr_Project, nameof(drr_project.drr_drr_project_drr_projectclaim_Project), ct)); //All Project Claims for getting claim totals
            }

            await Task.WhenAll(secondLoadTasks);

            var thirdLoadTasks = new List<Task>{
                ParallelLoadAllClaimInvoices(ctx, claim, ct),
            };

            if (claim.drr_Project != null && claim.drr_Project.drr_FullProposalApplication != null)
            {
                ctx.AttachTo(nameof(DRRContext.drr_applications), claim.drr_Project.drr_FullProposalApplication);
                thirdLoadTasks.Add(ctx.LoadPropertyAsync(claim.drr_Project.drr_FullProposalApplication, nameof(drr_application.drr_drr_application_drr_detailedcostestimate_Application), ct));
            }

            await Task.WhenAll(thirdLoadTasks);

            //sort lists
            claim.drr_drr_projectclaim_drr_projectexpenditure_Claim = new System.Collections.ObjectModel.Collection<drr_projectexpenditure>(claim.drr_drr_projectclaim_drr_projectexpenditure_Claim.OrderBy(inv => inv.createdon).ToList());
        }

        private static async Task ParallelLoadAllClaimInvoices(DRRContext ctx, drr_projectclaim claim, CancellationToken ct)
        {
            await claim.drr_Project.drr_drr_project_drr_projectclaim_Project.ForEachAsync(5, async prjClaim =>
            {
                if (prjClaim.drr_projectclaimid != claim.drr_projectclaimid)
                {
                    ctx.AttachTo(nameof(DRRContext.drr_projectclaims), prjClaim);
                    await ctx.LoadPropertyAsync(prjClaim, nameof(drr_projectclaim.drr_drr_projectclaim_drr_projectexpenditure_Claim), ct);
                    await ctx.LoadPropertyAsync(prjClaim, nameof(drr_projectclaim.drr_ProjectReport), ct);
                }
                else
                {
                    prjClaim.drr_drr_projectclaim_drr_projectexpenditure_Claim = claim.drr_drr_projectclaim_drr_projectexpenditure_Claim;
                    prjClaim.drr_ProjectReport = claim.drr_ProjectReport;
                }
            });
        }

        private static async Task ParallelLoadInvoiceAttachments(DRRContext ctx, drr_projectclaim claim, CancellationToken ct)
        {
            await claim.drr_drr_projectclaim_drr_projectexpenditure_Claim.ForEachAsync(5, async invoice =>
            {
                ctx.AttachTo(nameof(DRRContext.drr_projectexpenditures), invoice);
                await ctx.LoadPropertyAsync(invoice, nameof(drr_projectexpenditure.bcgov_drr_projectexpenditure_bcgov_documenturl_ProjectExpenditure), ct);
                await invoice.bcgov_drr_projectexpenditure_bcgov_documenturl_ProjectExpenditure.ForEachAsync(5, async doc =>
                {
                    ctx.AttachTo(nameof(DRRContext.bcgov_documenturls), doc);
                    await ctx.LoadPropertyAsync(doc, nameof(bcgov_documenturl.bcgov_DocumentType), ct);
                });
            });
        }

        private static async Task ParallelLoadProgressReport(DRRContext ctx, drr_projectprogress pr, CancellationToken ct)
        {
            ctx.AttachTo(nameof(DRRContext.drr_projectprogresses), pr);
            var loadTasks = new List<Task>
            {
                ctx.LoadPropertyAsync(pr, nameof(drr_projectprogress.drr_Project), ct),
                ctx.LoadPropertyAsync(pr, nameof(drr_projectprogress.drr_ProjectReport), ct),
                ctx.LoadPropertyAsync(pr, nameof(drr_projectprogress.drr_AuthorizedRepresentativeContact), ct),
                ctx.LoadPropertyAsync(pr, nameof(drr_projectprogress.drr_drr_projectprogress_drr_projectworkplanactivity_ProjectProgressReport), ct),
                ctx.LoadPropertyAsync(pr, nameof(drr_projectprogress.drr_drr_projectprogress_drr_projectevent_ProjectProgress), ct),
                ctx.LoadPropertyAsync(pr, nameof(drr_projectprogress.drr_drr_projectprogress_drr_projectpastevent_ProjectProgress), ct),
                ctx.LoadPropertyAsync(pr, nameof(drr_projectprogress.drr_drr_projectprogress_drr_temporaryprovincialfundingsignage_ProjectProgress), ct),
                ctx.LoadPropertyAsync(pr, nameof(drr_projectprogress.bcgov_drr_projectprogress_bcgov_documenturl_ProgressReport), ct),
            };

            await Task.WhenAll(loadTasks);

            var secondLoadTasks = new List<Task>{
                ParallelLoadActivityTypes(ctx, pr, ct),
                ParallelLoadEventContacts(ctx, pr, ct),
                ParallelLoadProgressReportDocumentTypes(ctx, pr, ct),
            };

            if (pr.drr_ProjectReport != null)
            {
                ctx.AttachTo(nameof(DRRContext.drr_projectreports), pr.drr_ProjectReport);
                secondLoadTasks.Add(ctx.LoadPropertyAsync(pr.drr_ProjectReport, nameof(drr_projectreport.drr_ReportPeriod), ct));
            }

            await Task.WhenAll(secondLoadTasks);
        }

        private static async Task ParallelLoadActivityTypes(DRRContext ctx, drr_projectprogress pr, CancellationToken ct)
        {
            await pr.drr_drr_projectprogress_drr_projectworkplanactivity_ProjectProgressReport.ForEachAsync(5, async wa =>
            {
                ctx.AttachTo(nameof(DRRContext.drr_projectworkplanactivities), wa);
                var loadTasks = new List<Task>
                {
                    ctx.LoadPropertyAsync(wa, nameof(drr_projectworkplanactivity.drr_ActivityType), ct),
                    ctx.LoadPropertyAsync(wa, nameof(drr_projectworkplanactivity.drr_CopiedfromReport), ct),
                };

                await Task.WhenAll(loadTasks);
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

        private static async Task ParallelLoadProgressReportDocumentTypes(DRRContext ctx, drr_projectprogress pr, CancellationToken ct)
        {
            await pr.bcgov_drr_projectprogress_bcgov_documenturl_ProgressReport.ForEachAsync(5, async doc =>
            {
                ctx.AttachTo(nameof(DRRContext.bcgov_documenturls), doc);
                await ctx.LoadPropertyAsync(doc, nameof(bcgov_documenturl.bcgov_DocumentType), ct);
            });
        }

        private static async Task ParallelLoadForecastReportDocumentTypes(DRRContext ctx, drr_projectbudgetforecast forecast, CancellationToken ct)
        {
            await forecast.bcgov_drr_projectbudgetforecast_bcgov_documenturl_projectbudgetforecastid.ForEachAsync(5, async doc =>
            {
                ctx.AttachTo(nameof(DRRContext.bcgov_documenturls), doc);
                await ctx.LoadPropertyAsync(doc, nameof(bcgov_documenturl.bcgov_DocumentType), ct);
            });
        }
    }
}
