using AutoMapper;
using EMCR.DRR.API.Resources.Projects;
using EMCR.DRR.API.Services;
using EMCR.DRR.Dynamics;
using EMCR.DRR.Managers.Intake;
using EMCR.DRR.Resources.Applications;
using EMCR.Utilities.Extensions;
using Microsoft.Dynamics.CRM;

namespace EMCR.DRR.API.Resources.Requests
{
    public class RequestRepository : IRequestRepository
    {
        private readonly IDRRContextFactory dRRContextFactory;
        private readonly IMapper mapper;

        public RequestRepository(IDRRContextFactory dRRContextFactory, IMapper mapper)
        {
            this.mapper = mapper;
            this.dRRContextFactory = dRRContextFactory;
        }

        public async Task<bool> CanAccessRequest(string id, string businessId)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();
            var existingRequest = await readCtx.drr_requests.Expand(a => a.drr_ProjectId).Where(a => a.drr_name == id).SingleOrDefaultAsync();
            if (existingRequest == null) return true;
            readCtx.AttachTo(nameof(readCtx.drr_projects), existingRequest.drr_ProjectId);
            await readCtx.LoadPropertyAsync(existingRequest.drr_ProjectId, nameof(drr_project.drr_ProponentName));
            return (!string.IsNullOrEmpty(existingRequest.drr_ProjectId.drr_ProponentName.drr_bceidguid)) && existingRequest.drr_ProjectId.drr_ProponentName.drr_bceidguid.Equals(businessId);
        }

        public async Task<bool> CanAccessRequestFromDocumentId(string id, string businessId, bool forUpdate)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();
            var document = await readCtx.bcgov_documenturls.Expand(d => d.drr_RequestId).Where(a => a.bcgov_documenturlid == Guid.Parse(id)).SingleOrDefaultAsync();
            var existingRequest = await readCtx.drr_requests.Expand(a => a.drr_ProjectId).Where(a => a.drr_requestid == document._drr_requestid_value).SingleOrDefaultAsync();
            if (existingRequest == null) return true;
            if (forUpdate && existingRequest.statuscode == (int)RequestStatusOptionSet.Inactive) return false;
            readCtx.AttachTo(nameof(readCtx.drr_projects), existingRequest.drr_ProjectId);
            await readCtx.LoadPropertyAsync(existingRequest.drr_ProjectId, nameof(drr_project.drr_ProponentName));
            return (!string.IsNullOrEmpty(existingRequest.drr_ProjectId.drr_ProponentName.drr_bceidguid)) && existingRequest.drr_ProjectId.drr_ProponentName.drr_bceidguid.Equals(businessId);
        }

        public async Task<ManageRequestCommandResult> Manage(ManageRequestCommand cmd)
        {
            return cmd switch
            {
                CreateConditionRequest c => await HandleCreateConditionRequest(c),
                SaveConditionRequest c => await HandleSaveConditionRequest(c),
                SubmitConditionRequest c => await HandleSubmitConditionRequest(c),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

#pragma warning disable CS8601 // Possible null reference assignment.
        public async Task<ManageRequestCommandResult> HandleCreateConditionRequest(CreateConditionRequest cmd)
        {
            var ctx = dRRContextFactory.Create();
            var request = new drr_request
            {
                drr_requestid = Guid.NewGuid(),
                drr_requesttype = (int)RequestTypeOptionSet.Condition,
                statuscode = (int)RequestStatusOptionSet.DraftProponent
            };
            ctx.AddTodrr_requests(request);
            var condition = await ctx.drr_projectconditions.Expand(c => c.drr_Project).Where(c => c.drr_name == cmd.ConditionId).SingleOrDefaultAsync();
            if (condition == null) throw new NotFoundException("Condition not found");
            ctx.SetLink(request, nameof(request.drr_ProjectConditionId), condition);
            ctx.SetLink(request, nameof(request.drr_ProjectId), condition.drr_Project);
            await ctx.SaveChangesAsync();

            var res = ctx.drr_requests.Where(r => r.drr_requestid == request.drr_requestid).Select(r => r.drr_name).Single();
            return new ManageRequestCommandResult { Id = res };
        }
#pragma warning restore CS8601 // Possible null reference assignment.

        public async Task<ManageRequestCommandResult> HandleSaveConditionRequest(SaveConditionRequest cmd)
        {
            var ctx = dRRContextFactory.Create();
            var existingRequest = await ctx.drr_requests.Where(p => p.drr_name == cmd.Request.Id).SingleOrDefaultAsync();
            if (existingRequest == null) throw new NotFoundException("Condition Request not found");

            var loadTasks = new List<Task>
            {
                ctx.LoadPropertyAsync(existingRequest, nameof(drr_request.drr_ProjectConditionId)),
                ctx.LoadPropertyAsync(existingRequest, nameof(drr_request.drr_AuthorizedRepresentativeContactId)),
                ctx.LoadPropertyAsync(existingRequest, nameof(drr_request.drr_request_bcgov_documenturl_RequestId)),
            };

            await Task.WhenAll(loadTasks);

            ctx.DetachAll();
            var drrRequest = mapper.Map<drr_request>(cmd.Request);
            drrRequest.drr_requestid = existingRequest.drr_requestid;

            foreach (var doc in drrRequest.drr_request_bcgov_documenturl_RequestId)
            {
                var curr = existingRequest.drr_request_bcgov_documenturl_RequestId.SingleOrDefault(d => d.bcgov_documenturlid == doc.bcgov_documenturlid);
                if (curr != null) curr.bcgov_documentcomments = doc.bcgov_documentcomments;
            }

            drrRequest.drr_request_bcgov_documenturl_RequestId = existingRequest.drr_request_bcgov_documenturl_RequestId;

            ctx.AttachTo(nameof(ctx.drr_requests), drrRequest);

            var authorizedRep = drrRequest.drr_AuthorizedRepresentativeContactId;
            if (authorizedRep != null) SaveAuthrizedRepresentative(ctx, drrRequest, authorizedRep, existingRequest.drr_AuthorizedRepresentativeContactId);
            await SetDeclarations(ctx, drrRequest, "Condition Request");
            UpdateRequestDocuments(ctx, drrRequest, existingRequest);

            ctx.UpdateObject(drrRequest);
            await ctx.SaveChangesAsync();
            ctx.DetachAll();

            return new ManageRequestCommandResult { Id = existingRequest.drr_name };
        }

        public async Task<ManageRequestCommandResult> HandleSubmitConditionRequest(SubmitConditionRequest cmd)
        {
            var ctx = dRRContextFactory.Create();
            var request = await ctx.drr_requests.Where(a => a.drr_name == cmd.Id).SingleOrDefaultAsync();
            request.statuscode = (int)RequestStatusOptionSet.Submitted;
            //request.drr_submissiondate = DateTime.UtcNow;
            ctx.UpdateObject(request);
            await ctx.SaveChangesAsync();
            ctx.DetachAll();

            var owner = await GetDMAPIntakeTeam(ctx);
            if (owner != null)
            {
                request = await ctx.drr_requests.Where(a => a.drr_name == cmd.Id).SingleOrDefaultAsync();
                ctx.SetLink(request, nameof(drr_request.ownerid), owner);
                await ctx.SaveChangesAsync();
            }
            ctx.DetachAll();
            return new ManageRequestCommandResult { Id = cmd.Id };
        }

        private static void UpdateRequestDocuments(DRRContext drrContext, drr_request request, drr_request existingRequest)
        {
            foreach (var doc in request.drr_request_bcgov_documenturl_RequestId)
            {
                if (doc != null)
                {
                    drrContext.AttachTo(nameof(drrContext.bcgov_documenturls), doc);
                    drrContext.UpdateObject(doc);
                }
            }
        }

        private static async Task SetDeclarations(DRRContext drrContext, drr_request request, string ApplicationTypeName)
        {
            var accuracyDeclaration = (await drrContext.drr_legaldeclarations.Where(d => d.statecode == (int)EntityState.Active && d.drr_declarationtype == (int)DeclarationTypeOptionSet.AccuracyOfInformation && d.drr_formtype == (int)FormTypeOptionSet.Application && d.drr_ApplicationType.drr_name == ApplicationTypeName).GetAllPagesAsync()).FirstOrDefault();
            var representativeDeclaration = (await drrContext.drr_legaldeclarations.Where(d => d.statecode == (int)EntityState.Active && d.drr_declarationtype == (int)DeclarationTypeOptionSet.AuthorizedRepresentative && d.drr_formtype == (int)FormTypeOptionSet.Application && d.drr_ApplicationType.drr_name == ApplicationTypeName).GetAllPagesAsync()).FirstOrDefault();

            if (accuracyDeclaration != null)
            {
                drrContext.SetLink(request, nameof(drr_request.drr_AccuracyofInformationDeclarationId), accuracyDeclaration);
            }

            if (representativeDeclaration != null)
            {
                drrContext.SetLink(request, nameof(drr_request.drr_AuthorizedRepresentativeDeclarationId), representativeDeclaration);
            }
        }

        private static void SaveAuthrizedRepresentative(DRRContext drrContext, drr_request request, contact authorizedRep, contact existingRep)
        {
            if (existingRep == null || existingRep.contactid == null || authorizedRep.contactid != existingRep.contactid)
            {
                drrContext.AddTocontacts(authorizedRep);
                drrContext.AddLink(authorizedRep, nameof(authorizedRep.drr_contact_drr_request_AuthorizedRepresentativeContactId), request);
                drrContext.SetLink(request, nameof(drr_request.drr_AuthorizedRepresentativeContactId), authorizedRep);
            }
            else
            {
                drrContext.AttachTo(nameof(drrContext.contacts), authorizedRep);
                drrContext.UpdateObject(authorizedRep);
            }
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

            var requestsQuery = readCtx.drr_requests.Expand(r => r.drr_ProjectConditionId).Expand(r => r.drr_ProjectId)
                .Where(a => a.statuscode != (int)InvoiceStatusOptionSet.Inactive);
            if (!string.IsNullOrEmpty(query.Id)) requestsQuery = requestsQuery.Where(a => a.drr_name == query.Id);
            if (!string.IsNullOrEmpty(query.ConditionId)) requestsQuery = requestsQuery.Where(a => a.drr_ProjectConditionId.drr_name == query.ConditionId);
            if (!string.IsNullOrEmpty(query.ProjectId)) requestsQuery = requestsQuery.Where(a => a.drr_ProjectId.drr_name == query.ProjectId);

            var results = (await requestsQuery.GetAllPagesAsync(ct)).ToList();
            var length = results.Count;

            await Parallel.ForEachAsync(results, ct, async (request, ct) => await ParallelLoadRequest(readCtx, request, ct));

            return new RequestQueryResult { Items = mapper.Map<IEnumerable<Request>>(results), Length = length };
        }

        private static async Task ParallelLoadRequest(DRRContext ctx, drr_request request, CancellationToken ct)
        {
            ctx.AttachTo(nameof(DRRContext.drr_requests), request);
            var loadTasks = new List<Task>
            {
                ctx.LoadPropertyAsync(request, nameof(drr_request.drr_ProjectConditionId), ct),
                ctx.LoadPropertyAsync(request, nameof(drr_request.drr_AuthorizedRepresentativeContactId), ct),
                ctx.LoadPropertyAsync(request, nameof(drr_request.drr_request_bcgov_documenturl_RequestId), ct)
            };

            await Task.WhenAll(loadTasks);

            ctx.AttachTo(nameof(DRRContext.drr_projectconditions), request.drr_ProjectConditionId);

            var secondLoadTasks = new List<Task>{
                ParallelLoadRequestDocumentTypes(ctx, request, ct),
                ctx.LoadPropertyAsync(request.drr_ProjectConditionId, nameof(drr_projectcondition.drr_Condition), ct),
            };

            await Task.WhenAll(secondLoadTasks);
        }

        private static async Task ParallelLoadRequestDocumentTypes(DRRContext ctx, drr_request request, CancellationToken ct)
        {
            await request.drr_request_bcgov_documenturl_RequestId.ForEachAsync(5, async doc =>
            {
                ctx.AttachTo(nameof(DRRContext.bcgov_documenturls), doc);
                await ctx.LoadPropertyAsync(doc, nameof(bcgov_documenturl.bcgov_DocumentType), ct);
            });
        }

        private static async Task<team?> GetDMAPIntakeTeam(DRRContext ctx)
        {
            var DMAPTeamName = "DMAP Intake Team";
            return await ctx.teams.Where(su => su.name == DMAPTeamName).SingleOrDefaultAsync();
        }
    }
}
