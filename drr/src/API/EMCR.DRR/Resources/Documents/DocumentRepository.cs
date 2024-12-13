using AutoMapper;
using EMCR.DRR.Dynamics;
using EMCR.DRR.Managers.Intake;
using EMCR.DRR.Resources.Applications;
using EMCR.Utilities.Extensions;
using Microsoft.Dynamics.CRM;

namespace EMCR.DRR.API.Resources.Documents
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly IDRRContextFactory dRRContextFactory;
        private readonly IMapper mapper;

        public DocumentRepository(IDRRContextFactory dRRContextFactory, IMapper mapper)
        {
            this.dRRContextFactory = dRRContextFactory;
            this.mapper = mapper;
        }

        public async Task<ManageDocumentCommandResult> Manage(ManageDocumentCommand cmd)
        {
            return cmd switch
            {
                CreateDocument c => await Handle(c),
                DeleteDocument c => await Handle(c),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

        public async Task<QueryDocumentCommandResult> Query(QueryDocumentCommand query)
        {
            return query switch
            {
                DocumentQuery q => await HandleDocumentQuery(q),
                _ => throw new NotSupportedException($"{query.GetType().Name} is not supported")
            };
        }

#pragma warning disable CS8601 // Possible null reference assignment.
        public async Task<QueryDocumentCommandResult> HandleDocumentQuery(DocumentQuery query)
        {
            var readCtx = dRRContextFactory.CreateReadOnly();
            var document = await readCtx.bcgov_documenturls.Where(d => d.bcgov_documenturlid == Guid.Parse(query.Id)).SingleOrDefaultAsync();
            return new QueryDocumentCommandResult { ApplicationId = document._bcgov_application_value.ToString(), Document = mapper.Map<Document>(document) };
        }

        public async Task<ManageDocumentCommandResult> Handle(CreateDocument cmd)
        {
            var ctx = dRRContextFactory.Create();
            var bcGovDocument = mapper.Map<bcgov_documenturl>(cmd.Document);
            bcGovDocument.bcgov_documenturlid = Guid.Parse(cmd.NewDocId);
            var application = await ctx.drr_applications.Where(a => a.drr_name == cmd.ApplicationId).SingleOrDefaultAsync();
            if (application.statuscode != (int)ApplicationStatusOptionSet.DraftProponent)
            {
                application.statuscode = (int)ApplicationStatusOptionSet.DraftProponent;
                ctx.UpdateObject(application);
            }
            bcGovDocument.bcgov_url = $"drr_application/{application.drr_applicationid}";
            bcGovDocument.bcgov_origincode = (int?)OriginOptionSet.Web;
            bcGovDocument.bcgov_filesize = cmd.Document.Size;
            bcGovDocument.bcgov_receiveddate = DateTime.UtcNow;
            //Set Document Type
            ctx.AddTobcgov_documenturls(bcGovDocument);
            ctx.AddLink(application, nameof(application.bcgov_drr_application_bcgov_documenturl_Application), bcGovDocument);
            ctx.SetLink(bcGovDocument, nameof(bcGovDocument.bcgov_Application), application);
            var documentType = await ctx.bcgov_documenttypes.Where(t => t.bcgov_name == cmd.Document.DocumentType.ToDescriptionString()).SingleOrDefaultAsync();
            if (documentType == null) documentType = await ctx.bcgov_documenttypes.Where(t => t.bcgov_name == DocumentType.OtherSupportingDocument.ToDescriptionString()).SingleOrDefaultAsync();
            ctx.SetLink(bcGovDocument, nameof(bcGovDocument.bcgov_DocumentType), documentType);
            await ctx.SaveChangesAsync();

            return new ManageDocumentCommandResult { Id = bcGovDocument.bcgov_documenturlid.ToString(), ApplicationId = application.drr_applicationid.ToString() };
        }

        public async Task<ManageDocumentCommandResult> Handle(DeleteDocument cmd)
        {
            var ctx = dRRContextFactory.Create();

            var document = await ctx.bcgov_documenturls.Where(d => d.bcgov_documenturlid == Guid.Parse(cmd.Id)).SingleOrDefaultAsync();
            if (document != null) ctx.DeleteObject(document);

            await ctx.SaveChangesAsync();

            return new ManageDocumentCommandResult { Id = cmd.Id, ApplicationId = document?._bcgov_application_value.ToString() };
        }
    }
#pragma warning restore CS8601 // Possible null reference assignment.
}
