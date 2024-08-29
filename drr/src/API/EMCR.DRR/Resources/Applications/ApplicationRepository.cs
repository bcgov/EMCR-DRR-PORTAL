using AutoMapper;
using EMCR.DRR.API.Services;
using EMCR.DRR.Dynamics;
using EMCR.DRR.Managers.Intake;
using Microsoft.Dynamics.CRM;

namespace EMCR.DRR.Resources.Applications
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly IDRRContextFactory dRRContextFactory;
        private readonly IMapper mapper;

        public ApplicationRepository(IDRRContextFactory dRRContextFactory, IMapper mapper)
        {
            this.mapper = mapper;
            this.dRRContextFactory = dRRContextFactory;
        }

        public async Task<ManageApplicationCommandResult> Manage(ManageApplicationCommand cmd)
        {
            return cmd switch
            {
                SubmitApplication c => await HandleSubmitEOIApplication(c),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

        public async Task<ApplicationQueryResult> Query(ApplicationQuery query)
        {
            return query switch
            {
                ApplicationsQuery q => await HandleQueryApplication(q),
                _ => throw new NotSupportedException($"{query.GetType().Name} is not supported")
            };
        }

        public async Task<DeclarationQueryResult> Query(DeclarationQuery query)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();

            var results = await readCtx.drr_legaldeclarations.Where(d => d.statecode == (int)EntityState.Active).GetAllPagesAsync();
            var items = mapper.Map<IEnumerable<DeclarationInfo>>(results);
            return new DeclarationQueryResult { Items = items };
        }

        public async Task<bool> CanAccessApplication(string id, string businessId)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();
            var existingApplication = await readCtx.drr_applications.Expand(a => a.drr_Primary_Proponent_Name).Where(a => a.drr_name == id).SingleOrDefaultAsync();
            if (existingApplication == null) return true;
            return (!string.IsNullOrEmpty(existingApplication.drr_Primary_Proponent_Name.drr_bceidguid)) && existingApplication.drr_Primary_Proponent_Name.drr_bceidguid.Equals(businessId);
        }

        private async Task<ApplicationQueryResult> HandleQueryApplication(ApplicationsQuery query)
        {
            var ct = new CancellationTokenSource().Token;
            var readCtx = dRRContextFactory.CreateReadOnly();

            var applicationsQuery = readCtx.drr_applications.Where(a => a.statecode == (int)EntityState.Active);
            if (!string.IsNullOrEmpty(query.Id)) applicationsQuery = applicationsQuery.Where(a => a.drr_name == query.Id);
            if (!string.IsNullOrEmpty(query.BusinessId)) applicationsQuery = applicationsQuery.Where(a => a.drr_Primary_Proponent_Name.drr_bceidguid == query.BusinessId);

            var results = await applicationsQuery.GetAllPagesAsync(ct);

            results = results.ToArray();

            var partnerProponentsOnly = false;
            if (string.IsNullOrEmpty(query.Id)) partnerProponentsOnly = true;

            await Parallel.ForEachAsync(results, ct, async (a, ct) => await ParallelLoadApplicationAsync(readCtx, partnerProponentsOnly, a, ct));
            var items = mapper.Map<IEnumerable<Application>>(results.OrderBy(a => a.drr_name));
            return new ApplicationQueryResult { Items = items };
        }

        public async Task<ManageApplicationCommandResult> HandleSubmitEOIApplication(SubmitApplication cmd)
        {
            var ctx = dRRContextFactory.Create();
            if (string.IsNullOrEmpty(cmd.Application.Id))
            {
                return new ManageApplicationCommandResult { Id = await Create(ctx, cmd.Application) };
            }
            else
            {
                return new ManageApplicationCommandResult { Id = await Update(ctx, cmd.Application) };
            }
        }

        private async Task<string> Create(DRRContext ctx, Application application)
        {
            var drrApplication = mapper.Map<drr_application>(application);
            drrApplication.drr_applicationid = Guid.NewGuid();
            ctx.AddTodrr_applications(drrApplication);
            return await SaveApplication(ctx, drrApplication, application);
        }

        private async Task<string> Update(DRRContext ctx, Application application)
        {
            var currentApplication = await ctx.drr_applications
                .Expand(a => a.drr_Primary_Proponent_Name)
                .Expand(a => a.drr_SubmitterContact)
                .Expand(a => a.drr_PrimaryProjectContact)
                .Expand(a => a.drr_AdditionalContact1)
                .Expand(a => a.drr_AdditionalContact2)
                .Expand(a => a.drr_application_fundingsource_Application)
                .Expand(a => a.drr_drr_application_drr_criticalinfrastructureimpacted_Application)
                .Where(a => a.drr_name == application.Id)
                .SingleOrDefaultAsync();

            if (currentApplication == null) throw new NotFoundException("Application not found");

            var partnerAccounts = (await ctx.connections.Expand(c => c.record2id_account).Where(c => c.record1id_drr_application.drr_applicationid == currentApplication.drr_applicationid).GetAllPagesAsync()).Select(p => p.record2id_account).ToList();
            ctx.DetachAll();
            RemoveOldData(ctx, currentApplication, partnerAccounts);

            var drrApplication = mapper.Map<drr_application>(application);
            drrApplication.drr_applicationid = currentApplication.drr_applicationid;

            ctx.AttachTo(nameof(ctx.drr_applications), drrApplication);
            ctx.UpdateObject(drrApplication);
            return await SaveApplication(ctx, drrApplication, application);
        }

        private void RemoveOldData(DRRContext ctx, drr_application drrApplication, IEnumerable<account> partnerAccounts)
        {
            foreach (var account in partnerAccounts)
            {
                ctx.AttachTo(nameof(ctx.accounts), account);
                ctx.DeleteObject(account);
            }

            if (drrApplication.drr_PrimaryProjectContact != null)
            {
                ctx.AttachTo(nameof(ctx.contacts), drrApplication.drr_PrimaryProjectContact);
                ctx.DeleteObject(drrApplication.drr_PrimaryProjectContact);
            }
            if (drrApplication.drr_AdditionalContact1 != null)
            {
                ctx.AttachTo(nameof(ctx.contacts), drrApplication.drr_AdditionalContact1);
                ctx.DeleteObject(drrApplication.drr_AdditionalContact1);
            }
            if (drrApplication.drr_AdditionalContact2 != null)
            {
                ctx.AttachTo(nameof(ctx.contacts), drrApplication.drr_AdditionalContact2);
                ctx.DeleteObject(drrApplication.drr_AdditionalContact2);
            }
            foreach (var fund in drrApplication.drr_application_fundingsource_Application)
            {
                ctx.AttachTo(nameof(ctx.drr_fundingsources), fund);
                ctx.DeleteObject(fund);
            }
            foreach (var infrastructure in drrApplication.drr_drr_application_drr_criticalinfrastructureimpacted_Application)
            {
                ctx.AttachTo(nameof(ctx.drr_criticalinfrastructureimpacteds), infrastructure);
                ctx.DeleteObject(infrastructure);
            }
        }

        private async Task<string> SaveApplication(DRRContext ctx, drr_application drrApplication, Application application)
        {
            var primaryProponent = drrApplication.drr_Primary_Proponent_Name;
            primaryProponent = await CheckForExistingProponent(ctx, primaryProponent, application);

            var submitter = drrApplication.drr_SubmitterContact;
            var primaryProjectContact = drrApplication.drr_PrimaryProjectContact;
            var additionalContact1 = drrApplication.drr_AdditionalContact1;
            var additionalContact2 = drrApplication.drr_AdditionalContact2;

            AssignPrimaryProponent(ctx, drrApplication, primaryProponent);
            if (submitter != null) AddSubmitter(ctx, drrApplication, submitter);
            if (primaryProjectContact != null) AddPrimaryProjectContact(ctx, drrApplication, primaryProjectContact);
            if (additionalContact1 != null) AddAdditionalContact1(ctx, drrApplication, additionalContact1);
            if (additionalContact2 != null) AddAdditionalContact2(ctx, drrApplication, additionalContact2);
            AddFundinSources(ctx, drrApplication);
            AddInfrastructureImpacted(ctx, drrApplication);
            SetApplicationType(ctx, drrApplication, "EOI");
            SetProgram(ctx, drrApplication, "DRIF");
            await SetDeclarations(ctx, drrApplication);

            var partnerAccounts = mapper.Map<IEnumerable<account>>(application.PartneringProponents);
            foreach (var account in partnerAccounts)
            {
                ctx.AddToaccounts(account);
            }

            await ctx.SaveChangesAsync();
            if (partnerAccounts.Count() > 0)
            {
                CreatePartnerConnections(ctx, drrApplication, partnerAccounts);
                await ctx.SaveChangesAsync();
            }

            //get the autogenerated application number
            var drrApplicationNumber = ctx.drr_applications.Where(a => a.drr_applicationid == drrApplication.drr_applicationid).Select(a => a.drr_name).Single();

            ctx.DetachAll();
            return drrApplicationNumber;
        }

        private async Task<account> CheckForExistingProponent(DRRContext ctx, account proponent, Application application)
        {
            var existingProponent = string.IsNullOrEmpty(application.BCeIDBusinessId) ? null : await ctx.accounts.Where(a => a.drr_bceidguid == application.BCeIDBusinessId).SingleOrDefaultAsync();
            if (existingProponent == null)
            {
                ctx.AddToaccounts(proponent);
            }
            else
            {
                proponent = existingProponent;
            }
            return proponent;
        }

        private static async Task SetDeclarations(DRRContext drrContext, drr_application application)
        {
            var accuracyDeclaration = (await drrContext.drr_legaldeclarations.Where(d => d.statecode == (int)EntityState.Active && d.drr_declarationtype == (int)DeclarationTypeOptionSet.AccuracyOfInformation).GetAllPagesAsync()).FirstOrDefault();
            var representativeDeclaration = (await drrContext.drr_legaldeclarations.Where(d => d.statecode == (int)EntityState.Active && d.drr_declarationtype == (int)DeclarationTypeOptionSet.AuthorizedRepresentative).GetAllPagesAsync()).FirstOrDefault();

            if (accuracyDeclaration != null)
            {
                drrContext.SetLink(application, nameof(drr_application.drr_AccuracyofInformationDeclaration), accuracyDeclaration);
            }

            if (representativeDeclaration != null)
            {
                drrContext.SetLink(application, nameof(drr_application.drr_AuthorizedRepresentativeDeclaration), representativeDeclaration);
            }
        }

        private static void AssignPrimaryProponent(DRRContext drrContext, drr_application application, account primaryProponent)
        {
            drrContext.AddLink(primaryProponent, nameof(primaryProponent.drr_account_drr_application_PrimaryProponentName), application);
        }

        private static void AddSubmitter(DRRContext drrContext, drr_application application, contact submitter)
        {
            drrContext.AddTocontacts(submitter);
            drrContext.AddLink(submitter, nameof(submitter.drr_contact_drr_application_SubmitterContact), application);
        }

        private static void AddPrimaryProjectContact(DRRContext drrContext, drr_application application, contact primaryProjectContact)
        {
            drrContext.AddTocontacts(primaryProjectContact);
            drrContext.AddLink(primaryProjectContact, nameof(primaryProjectContact.drr_contact_drr_application_PrimaryProjectContact), application);
        }

        private static void AddAdditionalContact1(DRRContext drrContext, drr_application application, contact additionalContact1)
        {
            if (additionalContact1 == null || string.IsNullOrEmpty(additionalContact1.firstname)) return;
            drrContext.AddTocontacts(additionalContact1);
            drrContext.AddLink(additionalContact1, nameof(additionalContact1.drr_contact_drr_application_AdditionalContact1), application);
        }

        private static void AddAdditionalContact2(DRRContext drrContext, drr_application application, contact additionalContact2)
        {
            if (additionalContact2 == null || string.IsNullOrEmpty(additionalContact2.firstname)) return;
            drrContext.AddTocontacts(additionalContact2);
            drrContext.AddLink(additionalContact2, nameof(additionalContact2.drr_contact_drr_application_AdditionalContact2), application);
        }

        private static void CreatePartnerConnections(DRRContext drrContext, drr_application application, IEnumerable<account> partnerAccounts)
        {
            var connectionRole = drrContext.connectionroles.Where(r => r.name == "Partner").SingleOrDefault();
            foreach (var account in partnerAccounts)
            {
                var connection = new connection
                {
                    name = account.name,
                };
                drrContext.AddToconnections(connection);
                drrContext.SetLink(connection, nameof(connection.record2roleid), connectionRole);
                drrContext.SetLink(connection, nameof(connection.record2id_account), account);
                drrContext.SetLink(connection, nameof(connection.record1id_drr_application), application);
            }
        }

        private static void AddFundinSources(DRRContext drrContext, drr_application application)
        {
            foreach (var fund in application.drr_application_fundingsource_Application)
            {
                if (fund != null)
                {
                    drrContext.AddTodrr_fundingsources(fund);
                    drrContext.AddLink(application, nameof(application.drr_application_fundingsource_Application), fund);
                    drrContext.SetLink(fund, nameof(fund.drr_Application), application);
                }
            }
        }

        private static void AddInfrastructureImpacted(DRRContext drrContext, drr_application application)
        {
            foreach (var infrastructure in application.drr_drr_application_drr_criticalinfrastructureimpacted_Application)
            {
                if (infrastructure != null && !string.IsNullOrEmpty(infrastructure.drr_name))
                {
                    drrContext.AddTodrr_criticalinfrastructureimpacteds(infrastructure);
                    drrContext.AddLink(application, nameof(application.drr_drr_application_drr_criticalinfrastructureimpacted_Application), infrastructure);
                    drrContext.SetLink(infrastructure, nameof(infrastructure.drr_Application), application);
                }
            }
        }

        private static void SetApplicationType(DRRContext drrContext, drr_application application, string ApplicationTypeName)
        {
            var applicationType = drrContext.drr_applicationtypes.Where(type => type.drr_name == ApplicationTypeName).SingleOrDefault();
            drrContext.AddLink(applicationType, nameof(applicationType.drr_drr_applicationtype_drr_application_ApplicationType), application);
            drrContext.SetLink(application, nameof(application.drr_ApplicationType), applicationType);
        }

        private static void SetProgram(DRRContext drrContext, drr_application application, string ProgramName)
        {
            var program = drrContext.drr_programs.Where(p => p.drr_name == ProgramName).SingleOrDefault();
            drrContext.AddLink(program, nameof(program.drr_drr_program_drr_application_Program), application);
            drrContext.SetLink(application, nameof(application.drr_Program), program);
        }

        private static async Task ParallelLoadApplicationAsync(DRRContext ctx, bool partnerProponentsOnly, drr_application application, CancellationToken ct)
        {
            ctx.AttachTo(nameof(DRRContext.drr_applications), application);

            var loadTasks = new List<Task>
            {
                ctx.LoadPropertyAsync(application, nameof(drr_application.drr_application_connections1), ct),
            };

            if (!partnerProponentsOnly)
            {
                loadTasks = loadTasks.Concat(new List<Task>
                {
                    ctx.LoadPropertyAsync(application, nameof(drr_application.drr_Primary_Proponent_Name), ct),
                    ctx.LoadPropertyAsync(application, nameof(drr_application.drr_SubmitterContact), ct),
                    ctx.LoadPropertyAsync(application, nameof(drr_application.drr_PrimaryProjectContact), ct),
                    ctx.LoadPropertyAsync(application, nameof(drr_application.drr_AdditionalContact1), ct),
                    ctx.LoadPropertyAsync(application, nameof(drr_application.drr_AdditionalContact2), ct),
                    ctx.LoadPropertyAsync(application, nameof(drr_application.drr_application_contact_Application), ct),
                    ctx.LoadPropertyAsync(application, nameof(drr_application.drr_application_fundingsource_Application), ct),
                    ctx.LoadPropertyAsync(application, nameof(drr_application.drr_drr_application_drr_criticalinfrastructureimpacted_Application), ct),
                }).ToList();
            }

            await Task.WhenAll(loadTasks);
        }
    }
}
