using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.OData.Client;
using Microsoft.OData.Extensions.Client;

namespace EMCR.DRR.Dynamics
{
    public class DynamicsODataClientHandler : IODataClientHandler
    {
        private readonly DRRContextOptions options;
        private readonly ISecurityTokenProvider tokenProvider;
        private string? authToken;
        
        //These fields will still be sent to CRM even if the value is null - needed to be able to wipe these fields
        //TODO - find a better way...
        private string[] SetPropertyExemptions = new[] { 
            //Application
            "drr_hazards",
            "drr_anticipatedprojectstartdate",
            "drr_anticipatedprojectenddate",
            "drr_estimated_total_project_cost",
            "drr_estimateddriffundingprogramrequest",
            "drr_remaining_amount",
            "drr_estimatedsizeofprojectarea",
            "drr_eligibleamount",
            "drr_totaldrifprogramfundingrequest",
            "drr_cost",
            "drr_contingency",
            "drr_totaleligiblecosts",
            "drr_increasedortransferred",

            //Funding Information
            "drr_estimated_amount",

            //Proposed Activity
            "drr_anticipatedstartdate",
            "drr_anticipatedenddate",

            //Detailed Cost Estimate
            "drr_unitrate",
            "drr_quantity",
            "drr_totalcost",

            //Funding Request
            "drr_drifprogramfundingrequest",

            //Progress Report
            "drr_percentageofprojectcompleteasofreportdate",
            "drr_percentconstructioncompleteatreportdate",
            "drr_dateofannouncement",
            "drr_plannedstartdate",
            "drr_plannedcompletiondate",
            "drr_actualstartdate",
            "drr_actualcompletiondate",
            "drr_dateinstalled",
            "drr_dateremoved",
            "drr_plannedeventdate",
            "drr_dateoccurred",

            //Requests
            "drr_descriptionofrequest",
        };

        public DynamicsODataClientHandler(IOptions<DRRContextOptions> options, ISecurityTokenProvider tokenProvider)
        {
            this.options = options.Value;
            this.tokenProvider = tokenProvider;
        }

        public void OnClientCreated(ClientCreatedArgs args)
        {
            authToken = tokenProvider.AcquireToken().ConfigureAwait(false).GetAwaiter().GetResult();
            var client = args.ODataClient;
            client.SaveChangesDefaultOptions = SaveChangesOptions.BatchWithSingleChangeset;
            client.EntityParameterSendOption = EntityParameterSendOption.SendOnlySetProperties;
            client.Configurations.RequestPipeline.OnEntryStarting((arg) =>
            {
                // do not send reference properties and null values to Dynamics - added some exception fields where we do want to allow null for purposes of clearing that field in crm
                arg.Entry.Properties = arg.Entry.Properties.Where((prop) =>
                !prop.Name.StartsWith('_') &&
                (prop.Value != null || SetPropertyExemptions.Contains(prop.Name)));
            });
            client.BuildingRequest += Client_BuildingRequest;
            client.SendingRequest2 += Client_SendingRequest2;
        }

        private void Client_SendingRequest2(object sender, SendingRequest2EventArgs e)
        {
            e.RequestMessage.SetHeader("Authorization", $"Bearer {authToken}");
        }

        private void Client_BuildingRequest(object sender, BuildingRequestEventArgs e)
        {
            if (e.RequestUri != null)
                e.RequestUri = RewriteRequestUri((DataServiceContext)sender, options.DynamicsApiEndpoint ?? null!, e.RequestUri);
        }

        private static Uri RewriteRequestUri(DataServiceContext ctx, Uri endpointUri, Uri requestUri) =>
           requestUri.IsAbsoluteUri
                 ? new Uri(endpointUri, (endpointUri.AbsolutePath == "/" ? string.Empty : endpointUri.AbsolutePath) + requestUri.AbsolutePath + requestUri.Query)
                 : new Uri(endpointUri, (endpointUri.AbsolutePath == "/" ? string.Empty : endpointUri.AbsolutePath) + ctx.BaseUri.AbsolutePath + requestUri.ToString());
    }
}
