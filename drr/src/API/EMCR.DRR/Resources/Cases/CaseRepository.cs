using AutoMapper;
using EMCR.DRR.Dynamics;
using EMCR.DRR.Resources.Applications;
using Microsoft.Dynamics.CRM;

namespace EMCR.DRR.API.Resources.Cases
{
    public class CaseRepository : ICaseRepository
    {
        private readonly IDRRContextFactory dRRContextFactory;
        private readonly IMapper mapper;

        public CaseRepository(IDRRContextFactory dRRContextFactory, IMapper mapper)
        {
            this.dRRContextFactory = dRRContextFactory;
            this.mapper = mapper;
        }

        public async Task<ManageCaseCommandResult> Manage(ManageCaseCommand cmd)
        {
            return cmd switch
            {
                GenerateFpFromEoi c => await Handle(c),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

        public async Task<QueryCaseCommandResult> Query(QueryCaseCommand query)
        {
            return query switch
            {
                CaseQuery q => await HandleCaseQuery(q),
                _ => throw new NotSupportedException($"{query.GetType().Name} is not supported")
            };
        }

        private async Task<QueryCaseCommandResult> HandleCaseQuery(CaseQuery query)
        {
            var ct = new CancellationTokenSource().Token;
            var readCtx = dRRContextFactory.CreateReadOnly();

            var caseQuery = readCtx.incidents.Expand(i => i.drr_EOIApplication).Expand(i => i.drr_FullProposalApplication).Where(a => a.statecode == (int)EntityState.Active);
            if (!string.IsNullOrEmpty(query.EoiId)) caseQuery = caseQuery.Where(i => i.drr_EOIApplication.drr_name == query.EoiId);
            if (!string.IsNullOrEmpty(query.FpId)) caseQuery = caseQuery.Where(i => i.drr_FullProposalApplication.drr_name == query.FpId);

            var results = (await caseQuery.GetAllPagesAsync(ct)).ToArray();
            var items = mapper.Map<IEnumerable<Case>>(results);
            return new QueryCaseCommandResult { Items = items };
        }

        private async Task<ManageCaseCommandResult> Handle(GenerateFpFromEoi cmd)
        {
            var ctx = dRRContextFactory.Create();
            var incident = await ctx.incidents.Where(i => i.drr_EOIApplication.drr_name == cmd.EoiId).SingleOrDefaultAsync();
            incident.drr_createfullproposal = (int)DRRTwoOptions.Yes;
            ctx.UpdateObject(incident);
            await ctx.SaveChangesAsync();
            ctx.DetachAll();
            var updatedIncident = await ctx.incidents.Expand(i => i.drr_FullProposalApplication).Where(i => i.drr_EOIApplication.drr_name == cmd.EoiId).SingleOrDefaultAsync();
            return new ManageCaseCommandResult { Id = updatedIncident.drr_FullProposalApplication.drr_name };
        }
    }
}
