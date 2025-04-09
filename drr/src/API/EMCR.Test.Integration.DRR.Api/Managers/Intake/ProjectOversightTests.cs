using System.Text;
using AutoMapper;
using EMCR.DRR.API.Resources.Projects;
using EMCR.DRR.API.Services.S3;
using EMCR.DRR.API.Utilities.TestData;
using EMCR.DRR.Controllers;
using EMCR.DRR.Dynamics;
using EMCR.DRR.Managers.Intake;
using EMCR.DRR.Resources.Applications;
using Microsoft.Dynamics.CRM;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EMCR.Tests.Integration.DRR.Managers.Intake
{
    public class ProjectOversightTests
    {
        private string TestPrefix = "autotest-dev";
        private string TestBusinessId = "autotest-dev-business-bceid";
        private string TestBusinessName = "autotest-dev-business-name";
        private string TestUserId = "autotest-dev-user-bceid";

        private string CRAFTD1BusinessId = "9F4430C64A2546C08B1F129F4071C1B4";
        private string CRAFTD1BusinessName = "EMCR CRAFT BCeID DEV";
        private string CRAFTD1UserId = "FAAA14A088F94B78B121C8A025F7304D";

        private string CRAFTD2BusinessId = "727C37D2C2CD44ED9F379624FF960465";
        private string CRAFTD2BusinessName = "CRAFT Development Community 2";
        private string CRAFTD2UserId = "...";

        private string TestProjectId = "DRIF-PRJ-1052";

        private UserInfo GetTestUserInfo()
        {
            return new UserInfo { BusinessId = TestBusinessId, BusinessName = TestBusinessName, UserId = TestUserId };
        }

        private UserInfo GetCRAFTUserInfo()
        {
            return new UserInfo { BusinessId = CRAFTD1BusinessId, BusinessName = CRAFTD1BusinessName, UserId = CRAFTD1UserId };
        }
        private UserInfo GetCRAFT2UserInfo()
        {
            return new UserInfo { BusinessId = CRAFTD2BusinessId, BusinessName = CRAFTD2BusinessName, UserId = CRAFTD2UserId };
        }
        private readonly IIntakeManager manager;
        private readonly IMapper mapper;

        public ProjectOversightTests()
        {
            var host = Application.Host;
            manager = host.Services.GetRequiredService<IIntakeManager>();
            mapper = host.Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task QueryProjects()
        {
            var queryRes = await manager.Handle(new DrrProjectsQuery { BusinessId = GetTestUserInfo().BusinessId });
            var projects = mapper.Map<IEnumerable<DraftDrrProject>>(queryRes.Items);
            projects.Count().ShouldBeGreaterThanOrEqualTo(1);
            projects.ShouldAllBe(s => s.Status != EMCR.DRR.Controllers.ProjectStatus.NotStarted);
        }

#pragma warning disable CS8604 // Possible null reference argument.
        [Test]
        public async Task QueryProjects_CanFilterById()
        {
            //var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();
            var userInfo = GetCRAFT2UserInfo();

            var queryOptions = new QueryOptions { Filter = "programType=DRIF,applicationType=FP,status=*UnderReview\\|EligiblePending" };
            var queryRes = await manager.Handle(new DrrProjectsQuery { Id = "DRIF-PRJ-1129", BusinessId = userInfo.BusinessId, QueryOptions = queryOptions });
            var projects = mapper.Map<IEnumerable<DraftDrrProject>>(queryRes.Items);
            projects.Count().ShouldBe(1);
            projects.First().Claims.Last().ReportPeriod.ShouldNotBeNullOrEmpty();
        }
#pragma warning restore CS8604 // Possible null reference argument.

        [Test]
        public async Task QueryReports_CanFilterById()
        {
            var queryRes = await manager.Handle(new DrrReportsQuery { Id = "DRIF-REP-1146", BusinessId = GetTestUserInfo().BusinessId });
            var reports = mapper.Map<IEnumerable<EMCR.DRR.Controllers.InterimReport>>(queryRes.Items);
            reports.Count().ShouldBe(1);
        }

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        [Test]
        public async Task QueryClaims_PreviousClaimTotal_PopulatesCorrectly()
        {
            var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();

            var project = (await manager.Handle(new DrrProjectsQuery { Id = TestProjectId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault();
            for (var i = 0; i < project.InterimReports.Count() - 1; ++i)
            {
                var currentClaimId = project.InterimReports.ElementAt(i).ProjectClaim.Id;
                var previousClaimId = project.InterimReports.ElementAt(i + 1).ProjectClaim.Id;
                var currentClaim = mapper.Map<EMCR.DRR.Controllers.ProjectClaim>((await manager.Handle(new DrrClaimsQuery { Id = currentClaimId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
                var previousClaim = mapper.Map<EMCR.DRR.Controllers.ProjectClaim>((await manager.Handle(new DrrClaimsQuery { Id = previousClaimId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());

                if ((currentClaim.PreviousClaimTotal == null || currentClaim.PreviousClaimTotal == 0) && (previousClaim.TotalClaimed == null || previousClaim.TotalClaimed == 0)) continue;
                currentClaim.PreviousClaimTotal.ShouldBe(previousClaim.TotalClaimed);
            }
        }

        //[Test]
        //public async Task TEMP_SetWPActivityCopiedActivity()
        //{
        //    await SetWorkplanActivityCopiedActivity("DRIF-WA-1959");
        //}

        [Test]
        public async Task QueryClaims_CanFilterById()
        {
            var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();

            var queryRes = await manager.Handle(new DrrClaimsQuery { Id = "DRIF-CLAIM-1166", BusinessId = userInfo.BusinessId });
            var claims = mapper.Map<IEnumerable<DraftProjectClaim>>(queryRes.Items);
            claims.Count().ShouldBe(1);
            var claim = claims.SingleOrDefault();
            claim.Invoices.Count().ShouldBeGreaterThan(0);
            claim.ProjectType.ShouldNotBeNull();
            claim.TotalProjectAmount.ShouldNotBeNull();
            claim.TotalClaimed.ShouldNotBeNull();
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.

        [Test]
        public async Task QueryProgressReports_CanFilterById()
        {
            //var userInfo = GetTestUserInfo();
            var userInfo = GetCRAFTUserInfo();
            //var userInfo = GetCRAFT2UserInfo();

            var queryRes = await manager.Handle(new DrrProgressReportsQuery { Id = "DRIF-PR-1058", BusinessId = userInfo.BusinessId });
            var prs = mapper.Map<IEnumerable<DraftProgressReport>>(queryRes.Items);
            prs.Count().ShouldBe(1);
            var progressReport = prs.Single();
            //progressReport.ProjectType.ShouldBe(EMCR.DRR.Controllers.InterimProjectType.Stream2);
            progressReport.ReportPeriod.ShouldNotBeNullOrEmpty();
        }

        [Test]
        public async Task QueryForecasts_CanFilterById()
        {
            //var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();
            var userInfo = GetCRAFT2UserInfo();

            var queryRes = await manager.Handle(new DrrForecastsQuery { Id = "FORECAST-1117", BusinessId = userInfo.BusinessId });
            var forecasts = mapper.Map<IEnumerable<EMCR.DRR.Controllers.DraftForecast>>(queryRes.Items);
            forecasts.Count().ShouldBe(1);
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8601 // Possible null reference assignment.
        [Test]
        public async Task CanUpdateProgressReport()
        {
            //var userInfo = GetTestUserInfo();
            var userInfo = GetCRAFTUserInfo();

            var progressReportId = "DRIF-PR-1058";
            var uniqueSignature = TestPrefix + "-" + Guid.NewGuid().ToString().Substring(0, 4);
            var progressReport = mapper.Map<EMCR.DRR.Controllers.ProgressReport>((await manager.Handle(new DrrProgressReportsQuery { Id = progressReportId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());

            progressReport = FillInProgressReport(progressReport, uniqueSignature);
            progressReport.Status = EMCR.DRR.Controllers.ProgressReportStatus.Draft;

            Console.WriteLine(progressReport.Id);
            await manager.Handle(new SaveProgressReportCommand { ProgressReport = progressReport, UserInfo = userInfo });


            var updatedProgressReport = mapper.Map<EMCR.DRR.Controllers.DraftProgressReport>((await manager.Handle(new DrrProgressReportsQuery { Id = progressReport.Id, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            updatedProgressReport.Workplan.MediaAnnouncementComment.ShouldBe(progressReport.Workplan.MediaAnnouncementComment);
            updatedProgressReport.Workplan.ProjectCompletionPercentage.ShouldBe(progressReport.Workplan.ProjectCompletionPercentage);
            updatedProgressReport.Workplan.ProjectProgress.ShouldBe(progressReport.Workplan.ProjectProgress);
            updatedProgressReport.Workplan.MediaAnnouncement.ShouldBe(progressReport.Workplan.MediaAnnouncement);
            //updatedProgressReport.Workplan.MediaAnnouncementDate.ShouldBe(progressReport.Workplan.MediaAnnouncementDate);
            updatedProgressReport.Workplan.OtherDelayReason.ShouldBe(progressReport.Workplan.OtherDelayReason);
            updatedProgressReport.EventInformation.PastEvents.Count().ShouldBe(1);
            updatedProgressReport.EventInformation.UpcomingEvents.Count().ShouldBe(1);
        }

        [Test]
        public async Task CanSubmitProgressReport()
        {
            var uniqueSignature = TestPrefix + "-" + Guid.NewGuid().ToString().Substring(0, 4);
            var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();
            var project = (await manager.Handle(new DrrProjectsQuery { Id = TestProjectId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault();
            if (!project.InterimReports.Any())
            {
                await CanCreateReport();
                project = (await manager.Handle(new DrrProjectsQuery { Id = TestProjectId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault();
            }

            var interimReport = project.InterimReports.First();
            var progressReportId = interimReport.ProgressReport.Id;
            var progressReport = mapper.Map<EMCR.DRR.Controllers.ProgressReport>((await manager.Handle(new DrrProgressReportsQuery { Id = progressReportId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            progressReport = FillInProgressReport(progressReport, uniqueSignature);
            progressReport.Status = EMCR.DRR.Controllers.ProgressReportStatus.Draft;
            progressReport.AuthorizedRepresentativeStatement = true;
            progressReport.InformationAccuracyStatement = true;
            progressReport.AuthorizedRepresentative = new EMCR.DRR.Controllers.ContactDetails
            {
                FirstName = "Joe",
                LastName = "autotest",
                Department = "dep",
                Title = "title",
                Email = "email@test.com",
                Phone = "6041234567"
            };

            await manager.Handle(new SubmitProgressReportCommand { ProgressReport = progressReport, UserInfo = userInfo });

            Console.WriteLine(progressReport.Id);
            var updatedProgressReport = mapper.Map<DraftProgressReport>((await manager.Handle(new DrrProgressReportsQuery { Id = progressReport.Id, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            updatedProgressReport.Status.ShouldBe(EMCR.DRR.Controllers.ProgressReportStatus.Submitted);
        }

#pragma warning disable CS8629 // Nullable value type may be null.
        [Test]
        public async Task CanUpdateClaim()
        {
            var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();

            var project = (await manager.Handle(new DrrProjectsQuery { Id = TestProjectId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault();
            if (!project.InterimReports.Any() || project.InterimReports.First().ProjectClaim == null || project.InterimReports.First().ProjectClaim.Status == EMCR.DRR.Managers.Intake.ClaimStatus.Submitted)
            {
                await CanCreateReport();
                project = (await manager.Handle(new DrrProjectsQuery { Id = TestProjectId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault();
            }

            var interimReport = project.InterimReports.First();

            var claimId = interimReport.ProjectClaim.Id;
            Console.WriteLine(claimId);
            var uniqueSignature = TestPrefix + "-" + Guid.NewGuid().ToString().Substring(0, 4);
            var claim = mapper.Map<EMCR.DRR.Controllers.ProjectClaim>((await manager.Handle(new DrrClaimsQuery { Id = claimId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            var categoryOptions = claim?.PreviousClaims != null ? claim.PreviousClaims.Select(c => c.CostCategory.Value).ToArray() : Enum.GetValues(typeof(EMCR.DRR.Controllers.CostCategory)).Cast<EMCR.DRR.Controllers.CostCategory>().Where(e => e != EMCR.DRR.Controllers.CostCategory.Contingency).ToArray();

            if (claim.Invoices.Any())
            {
                foreach (var invoice in claim.Invoices)
                {
                    await manager.Handle(new DeleteInvoiceCommand { ClaimId = claim.Id, InvoiceId = invoice.Id, UserInfo = userInfo });
                }
                claim.Invoices = [];
            }

            claim = FillInClaim(claim, uniqueSignature);
            claim.Status = EMCR.DRR.Controllers.ClaimStatus.Draft;

            if (claim.AuthorizedRepresentative == null)
            {
                claim.AuthorizedRepresentative = CreateNewTestContact(uniqueSignature, "submitter");
            }

            await manager.Handle(new SaveClaimCommand { Claim = claim, UserInfo = userInfo });
            var invoiceTotal = new Random().Next(20, 101) * 1000;
            DateTime startDate = project.StartDate ?? DateTime.UtcNow;
            DateTime endDate = project.EndDate ?? DateTime.UtcNow.AddDays(30);
            var invoices = TestHelper.CreateInvoices(startDate, endDate, categoryOptions, invoiceTotal);
            for (int i = 0; i < invoices.Count(); i++)
            {
                await manager.Handle(new CreateInvoiceCommand { ClaimId = claim.Id, InvoiceId = Guid.NewGuid().ToString(), UserInfo = userInfo });
            }

            var updatedClaim = mapper.Map<DraftProjectClaim>((await manager.Handle(new DrrClaimsQuery { Id = claim.Id, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());

            updatedClaim.Status.ShouldBe(claim.Status);
            updatedClaim.Invoices.Count().ShouldBe(invoices.Count());
            for (int i = 0; i < invoices.Count(); i++)
            {
                invoices.ElementAt(i).Id = updatedClaim.Invoices.ElementAt(i).Id;
            }

            updatedClaim.Invoices = invoices;
            await manager.Handle(new SaveClaimCommand { Claim = mapper.Map<EMCR.DRR.Controllers.ProjectClaim>(updatedClaim), UserInfo = userInfo });
            var twiceUpdatedClaim = mapper.Map<DraftProjectClaim>((await manager.Handle(new DrrClaimsQuery { Id = updatedClaim.Id, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            twiceUpdatedClaim.Invoices.Count().ShouldBe(updatedClaim.Invoices.Count());
        }

        [Test]
        public async Task CanSubmitClaim()
        {
            var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();

            var project = (await manager.Handle(new DrrProjectsQuery { Id = TestProjectId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault();
            if (!project.InterimReports.Any() || project.InterimReports.First().ProjectClaim == null || project.InterimReports.First().ProjectClaim.Status == EMCR.DRR.Managers.Intake.ClaimStatus.Submitted)
            {
                await CanCreateReport();
                project = (await manager.Handle(new DrrProjectsQuery { Id = TestProjectId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault();
            }

            var interimReport = project.InterimReports.First();

            var claimId = interimReport.ProjectClaim.Id;
            Console.WriteLine(claimId);
            var uniqueSignature = TestPrefix + "-" + Guid.NewGuid().ToString().Substring(0, 4);
            var claim = mapper.Map<EMCR.DRR.Controllers.ProjectClaim>((await manager.Handle(new DrrClaimsQuery { Id = claimId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            var categoryOptions = claim?.PreviousClaims != null ? claim.PreviousClaims.Select(c => c.CostCategory.Value).ToArray() : Enum.GetValues(typeof(EMCR.DRR.Controllers.CostCategory)).Cast<EMCR.DRR.Controllers.CostCategory>().Where(e => e != EMCR.DRR.Controllers.CostCategory.Contingency).ToArray();

            if (claim.Invoices.Any())
            {
                foreach (var invoice in claim.Invoices)
                {
                    await manager.Handle(new DeleteInvoiceCommand { ClaimId = claim.Id, InvoiceId = invoice.Id, UserInfo = userInfo });
                }
                claim.Invoices = [];
            }

            claim = FillInClaim(claim, uniqueSignature);
            claim.Status = EMCR.DRR.Controllers.ClaimStatus.Draft;

            if (claim.AuthorizedRepresentative == null)
            {
                claim.AuthorizedRepresentative = CreateNewTestContact(uniqueSignature, "submitter");
            }

            await manager.Handle(new SaveClaimCommand { Claim = claim, UserInfo = userInfo });
            var invoiceTotal = new Random().Next(20, 101) * 1000;
            DateTime startDate = project.StartDate ?? DateTime.UtcNow;
            DateTime endDate = project.EndDate ?? DateTime.UtcNow.AddDays(30);
            var invoices = TestHelper.CreateInvoices(startDate, endDate, categoryOptions, invoiceTotal);
            for (int i = 0; i < invoices.Count(); i++)
            {
                await manager.Handle(new CreateInvoiceCommand { ClaimId = claim.Id, InvoiceId = Guid.NewGuid().ToString(), UserInfo = userInfo });
            }

            var updatedClaim = mapper.Map<DraftProjectClaim>((await manager.Handle(new DrrClaimsQuery { Id = claim.Id, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());

            updatedClaim.Status.ShouldBe(claim.Status);
            updatedClaim.Invoices.Count().ShouldBe(invoices.Count());
            for (int i = 0; i < invoices.Count(); i++)
            {
                invoices.ElementAt(i).Id = updatedClaim.Invoices.ElementAt(i).Id;
            }

            updatedClaim.Invoices = invoices;
            var claimToSubmit = mapper.Map<EMCR.DRR.Controllers.ProjectClaim>(updatedClaim);
            claimToSubmit.AuthorizedRepresentativeStatement = true;
            claimToSubmit.InformationAccuracyStatement = true;
            await manager.Handle(new SubmitClaimCommand { Claim = claimToSubmit, UserInfo = userInfo });
            var submittedClaim = mapper.Map<EMCR.DRR.Controllers.ProjectClaim>((await manager.Handle(new DrrClaimsQuery { Id = updatedClaim.Id, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            submittedClaim.Invoices.Count().ShouldBe(updatedClaim.Invoices.Count());
            submittedClaim.Status.ShouldBe(EMCR.DRR.Controllers.ClaimStatus.Submitted);
            submittedClaim.AuthorizedRepresentative.ShouldNotBeNull();
            submittedClaim.AuthorizedRepresentativeStatement.ShouldBe(true);
            submittedClaim.InformationAccuracyStatement.ShouldBe(true);
        }

        [Test]
        public async Task CanUpdateForecast()
        {
            var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();

            var project = (await manager.Handle(new DrrProjectsQuery { Id = TestProjectId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault();
            if (!project.InterimReports.Any() || project.InterimReports.First().Forecast == null || project.InterimReports.First().Forecast.Status == EMCR.DRR.Managers.Intake.ForecastStatus.Submitted)
            {
                await CanCreateReport();
                project = (await manager.Handle(new DrrProjectsQuery { Id = TestProjectId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault();
            }

            var interimReport = project.InterimReports.First();

            var forecastId = interimReport.Forecast.Id;
            Console.WriteLine(forecastId);
            var uniqueSignature = TestPrefix + "-" + Guid.NewGuid().ToString().Substring(0, 4);
            var forecast = mapper.Map<EMCR.DRR.Controllers.Forecast>((await manager.Handle(new DrrForecastsQuery { Id = forecastId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());

            if (forecast.ForecastItems != null)
            {
                foreach (var item in forecast.ForecastItems)
                {
                    item.TotalProjectedExpenditure = 500;
                    item.ClaimsOnThisReport = 400;
                }
            }

            forecast.Status = EMCR.DRR.Controllers.ForecastStatus.Draft;
            forecast.OriginalForecast = 33333;
            forecast.VarianceComment = "variance comment";

            if (forecast.AuthorizedRepresentative == null)
            {
                forecast.AuthorizedRepresentative = CreateNewTestContact(uniqueSignature, "submitter");
            }

            await manager.Handle(new SaveForecastCommand { Forecast = forecast, UserInfo = userInfo });

            var updatedForecast = mapper.Map<DraftForecast>((await manager.Handle(new DrrForecastsQuery { Id = forecast.Id, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());

            updatedForecast.ForecastItems.Count().ShouldBe(forecast.ForecastItems.Count());
            updatedForecast.VarianceComment.ShouldBe(forecast.VarianceComment);
            updatedForecast.Status.ShouldBe(forecast.Status);
        }

        [Test]
        public async Task CanSubmitForecast()
        {
            var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();

            var project = (await manager.Handle(new DrrProjectsQuery { Id = TestProjectId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault();
            if (!project.InterimReports.Any() || project.InterimReports.First().Forecast == null || project.InterimReports.First().Forecast.Status == EMCR.DRR.Managers.Intake.ForecastStatus.Submitted)
            {
                await CanCreateReport();
                project = (await manager.Handle(new DrrProjectsQuery { Id = TestProjectId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault();
            }

            var interimReport = project.InterimReports.First();

            var forecastId = interimReport.Forecast.Id;
            Console.WriteLine(forecastId);
            var uniqueSignature = TestPrefix + "-" + Guid.NewGuid().ToString().Substring(0, 4);
            var forecast = mapper.Map<EMCR.DRR.Controllers.Forecast>((await manager.Handle(new DrrForecastsQuery { Id = forecastId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());

            if (forecast.ForecastItems != null)
            {
                foreach (var item in forecast.ForecastItems)
                {
                    item.TotalProjectedExpenditure = 500;
                    item.ClaimsOnThisReport = 400;
                }
            }

            forecast.Status = EMCR.DRR.Controllers.ForecastStatus.Draft;
            forecast.OriginalForecast = 33333;
            forecast.VarianceComment = "variance comment";

            if (forecast.AuthorizedRepresentative == null)
            {
                forecast.AuthorizedRepresentative = CreateNewTestContact(uniqueSignature, "submitter");
            }
            forecast.AuthorizedRepresentativeStatement = true;
            forecast.InformationAccuracyStatement = true;

            await manager.Handle(new SubmitForecastCommand { Forecast = forecast, UserInfo = userInfo });

            var updatedForecast = mapper.Map<DraftForecast>((await manager.Handle(new DrrForecastsQuery { Id = forecast.Id, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());

            updatedForecast.ForecastItems.Count().ShouldBe(forecast.ForecastItems.Count());
            updatedForecast.VarianceComment.ShouldBe(forecast.VarianceComment);
            updatedForecast.Status.ShouldBe(EMCR.DRR.Controllers.ForecastStatus.Submitted);
        }
#pragma warning restore CS8629 // Nullable value type may be null.

        //[Test]
        //public async Task UpdateClaim100Invoices()
        //{
        //    //var userInfo = GetTestUserInfo();
        //    var userInfo = GetCRAFTUserInfo();

        //    var claimId = "DRIF-CLAIM-1039";
        //    var uniqueSignature = TestPrefix + "-" + Guid.NewGuid().ToString().Substring(0, 4);
        //    var claim = mapper.Map<EMCR.DRR.Controllers.ProjectClaim>((await manager.Handle(new DrrClaimsQuery { Id = claimId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());

        //    claim = FillInClaim(claim, uniqueSignature);
        //    claim.Status = EMCR.DRR.Controllers.ClaimStatus.Draft;

        //    //Console.WriteLine(progressReport.Id);
        //    await manager.Handle(new SaveClaimCommand { Claim = claim, UserInfo = userInfo });
        //    for (var i = 0; i < 100; i++)
        //    {
        //        var invoiceId = await manager.Handle(new CreateInvoiceCommand { ClaimId = claim.Id, InvoiceId = Guid.NewGuid().ToString(), UserInfo = userInfo });
        //    }

        //    var updatedClaim = mapper.Map<DraftProjectClaim>((await manager.Handle(new DrrClaimsQuery { Id = claim.Id, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
        //    updatedClaim.Status.ShouldBe(claim.Status);
        //    updatedClaim.Invoices.Count().ShouldBeGreaterThan(90);
        //    updatedClaim.Invoices = [];

        //    Stopwatch stopwatch = new Stopwatch();
        //    // Start the stopwatch
        //    stopwatch.Start();

        //    await manager.Handle(new SaveClaimCommand { Claim = claim, UserInfo = userInfo });

        //    stopwatch.Stop();
        //    // Get the elapsed time in milliseconds
        //    Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} milliseconds");

        //    var twiceUpdatedClaim = mapper.Map<DraftProjectClaim>((await manager.Handle(new DrrClaimsQuery { Id = claim.Id, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
        //    twiceUpdatedClaim.Invoices.ShouldBeEmpty();
        //}

        [Test]
        public async Task CanAddAttachmentsToClaimInvoice()
        {
            //var userInfo = GetTestUserInfo();
            var userInfo = GetCRAFTUserInfo();

            var claimId = "DRIF-CLAIM-1039";
            var claim = mapper.Map<EMCR.DRR.Controllers.ProjectClaim>((await manager.Handle(new DrrClaimsQuery { Id = claimId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            var invoice = claim.Invoices.FirstOrDefault();
            if (invoice == null)
            {
                var invoiceId = await manager.Handle(new CreateInvoiceCommand { ClaimId = claim.Id, InvoiceId = Guid.NewGuid().ToString(), UserInfo = userInfo });
                claim = mapper.Map<EMCR.DRR.Controllers.ProjectClaim>((await manager.Handle(new DrrClaimsQuery { Id = claimId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
                invoice = claim.Invoices.FirstOrDefault();
            }
            foreach (var doc in invoice.Attachments)
            {
                await manager.Handle(new DeleteAttachmentCommand { Id = doc.Id, UserInfo = userInfo });
            }

            var body = DateTime.Now.ToString();
            var fileName = "autotest-invoice.txt";
            var fileName2 = "autotest-pop.txt";
            byte[] bytes = Encoding.ASCII.GetBytes(body);
            var file = new S3File { FileName = fileName, Content = bytes, ContentType = "text/plain", };
            var file2 = new S3File { FileName = fileName2, Content = bytes, ContentType = "text/plain", };

            var documentId = await manager.Handle(new UploadAttachmentCommand { AttachmentInfo = new AttachmentInfo { RecordId = invoice.Id, RecordType = EMCR.DRR.Managers.Intake.RecordType.Invoice, File = file, DocumentType = EMCR.DRR.Managers.Intake.DocumentType.Invoice }, UserInfo = userInfo });
            var documentId2 = await manager.Handle(new UploadAttachmentCommand { AttachmentInfo = new AttachmentInfo { RecordId = invoice.Id, RecordType = EMCR.DRR.Managers.Intake.RecordType.Invoice, File = file, DocumentType = EMCR.DRR.Managers.Intake.DocumentType.ProofOfPayment }, UserInfo = userInfo });

            var claimToUpdate = mapper.Map<EMCR.DRR.Controllers.ProjectClaim>((await manager.Handle(new DrrClaimsQuery { Id = claimId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            var invoiceToUpdate = claimToUpdate.Invoices.SingleOrDefault(i => i.Id == invoice.Id);
            invoiceToUpdate.Attachments.Count().ShouldBe(2);
            var invDocToUpdate = invoiceToUpdate.Attachments.Where(a => a.DocumentType == EMCR.DRR.API.Model.DocumentType.Invoice).SingleOrDefault();
            invoiceToUpdate.Attachments.Where(a => a.DocumentType == EMCR.DRR.API.Model.DocumentType.Invoice).ToArray().Length.ShouldBe(1);
            invoiceToUpdate.Attachments.Where(a => a.DocumentType == EMCR.DRR.API.Model.DocumentType.ProofOfPayment).ToArray().Length.ShouldBe(1);
            invDocToUpdate.Comments = "invoice comments";

            await manager.Handle(new SaveClaimCommand { Claim = claimToUpdate, UserInfo = userInfo });


            var updatedClaim = mapper.Map<EMCR.DRR.Controllers.ProjectClaim>((await manager.Handle(new DrrClaimsQuery { Id = claimId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            var updatedInvoice = claimToUpdate.Invoices.SingleOrDefault(i => i.Id == invoice.Id);
            var invDoc = updatedInvoice.Attachments.Where(a => a.DocumentType == EMCR.DRR.API.Model.DocumentType.Invoice).SingleOrDefault();
            invDoc.Comments.ShouldBe(invDocToUpdate.Comments);

        }

        [Test]
        public async Task ValidateCanCreateReport_ValidationFalse()
        {
            //var userInfo = GetTestUserInfo();
            var userInfo = GetCRAFTUserInfo();
            //var userInfo = GetCRAFT2UserInfo();

            var queryOptions = new QueryOptions { Filter = "programType=DRIF,applicationType=FP,status=*UnderReview\\|EligiblePending" };
            var queryRes = await manager.Handle(new DrrProjectsQuery { Id = "DRIF-PRJ-1015", BusinessId = userInfo.BusinessId, QueryOptions = queryOptions });
            //var queryRes = await manager.Handle(new DrrProjectsQuery { Id = "DRIF-PRJ-1108", BusinessId = userInfo.BusinessId, QueryOptions = queryOptions });
            var project = queryRes.Items.SingleOrDefault();
            var res = await manager.Handle(new ValidateCanCreateReportCommand { ProjectId = project.Id, ReportType = EMCR.DRR.Managers.Intake.ReportType.Interim, UserInfo = userInfo });
            res.CanCreate.ShouldBe(false);
        }

        [Test]
        public async Task CanCreateReport()
        {
            var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();

            //await ClearAllReportsForProject(TestProjectId);
            await AllReportsApprovedForProject(TestProjectId);
            var queryOptions = new QueryOptions { Filter = "programType=DRIF,applicationType=FP,status=*UnderReview\\|EligiblePending" };
            var project = (await manager.Handle(new DrrProjectsQuery { Id = TestProjectId, BusinessId = userInfo.BusinessId, QueryOptions = queryOptions })).Items.SingleOrDefault();
            project.InterimReports.ShouldAllBe(r => r.Status == EMCR.DRR.Managers.Intake.InterimReportStatus.Approved);
            var res = await manager.Handle(new CreateInterimReportCommand { ProjectId = project.Id, ReportType = EMCR.DRR.Managers.Intake.ReportType.Interim, UserInfo = userInfo });
            project = (await manager.Handle(new DrrProjectsQuery { Id = TestProjectId, BusinessId = userInfo.BusinessId, QueryOptions = queryOptions })).Items.SingleOrDefault();
            project.InterimReports.First().ProgressReport.ShouldNotBeNull();
            project.InterimReports.First().ProjectClaim.ShouldNotBeNull();
            project.InterimReports.First().Forecast.ShouldNotBeNull();
            Console.WriteLine(project.InterimReports.First().Id);
        }

        private async Task AllReportsApprovedForProject(string projectId)
        {
            var host = Application.Host;
            var factory = host.Services.GetRequiredService<IDRRContextFactory>();
            var ctx = factory.Create();
            var project = (await ctx.drr_projects.Expand(p => p.drr_drr_project_drr_projectreport_Project).Where(p => p.drr_name == projectId).GetAllPagesAsync()).SingleOrDefault();
            foreach (var report in project.drr_drr_project_drr_projectreport_Project)
            {
                //ctx.AttachTo(nameof(ctx.drr_projectreports), report);
                report.statuscode = (int)ProjectReportStatusOptionSet.Approved;
                ctx.UpdateObject(report);
            }
            await ctx.SaveChangesAsync();
        }

        private async Task SetWorkplanActivityCopiedActivity(string activityId)
        {
            var host = Application.Host;
            var factory = host.Services.GetRequiredService<IDRRContextFactory>();
            var ctx = factory.Create();
            var wp = (await ctx.drr_projectworkplanactivities.Where(p => p.drr_name == activityId).GetAllPagesAsync()).SingleOrDefault();
            wp.drr_copiedactivity = (int)DRRTwoOptions.Yes;
            ctx.UpdateObject(wp);
            await ctx.SaveChangesAsync();
        }

        private async Task ClearAllReportsForProject(string projectId)
        {
            var host = Application.Host;
            var factory = host.Services.GetRequiredService<IDRRContextFactory>();
            var ctx = factory.Create();
            var project = (await ctx.drr_projects.Expand(p => p.drr_drr_project_drr_projectreport_Project).Where(p => p.drr_name == projectId).GetAllPagesAsync()).SingleOrDefault();
            foreach (var report in project.drr_drr_project_drr_projectreport_Project)
            {
                //ctx.AttachTo(nameof(ctx.drr_projectreports), report);
                ctx.DeleteObject(report);
            }
            await ctx.SaveChangesAsync();
        }

        [Test]
        public async Task CanAddAttachmentToProgressReport()
        {
            //var userInfo = GetTestUserInfo();
            var userInfo = GetCRAFTUserInfo();

            var progressReportId = "DRIF-PR-1058";
            var progressReport = mapper.Map<EMCR.DRR.Controllers.DraftProgressReport>((await manager.Handle(new DrrProgressReportsQuery { Id = progressReportId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            foreach (var doc in progressReport.Attachments)
            {
                await manager.Handle(new DeleteAttachmentCommand { Id = doc.Id, UserInfo = userInfo });
            }

            var body = DateTime.Now.ToString();
            var fileName = "autotest.txt";
            byte[] bytes = Encoding.ASCII.GetBytes(body);
            var file = new S3File { FileName = fileName, Content = bytes, ContentType = "text/plain", };

            var documentId = await manager.Handle(new UploadAttachmentCommand { AttachmentInfo = new AttachmentInfo { RecordId = progressReport.Id, RecordType = EMCR.DRR.Managers.Intake.RecordType.ProgressReport, File = file, DocumentType = EMCR.DRR.Managers.Intake.DocumentType.ProgressReport }, UserInfo = userInfo });

            var progressReportToUpdate = mapper.Map<EMCR.DRR.Controllers.ProgressReport>((await manager.Handle(new DrrProgressReportsQuery { Id = progressReportId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            progressReportToUpdate.Attachments.Count().ShouldBe(1);
            progressReportToUpdate.Attachments.First().DocumentType.ShouldBe(EMCR.DRR.API.Model.DocumentType.ProgressReport);
            progressReportToUpdate.Attachments.First().Comments = "progress report comments";

            await manager.Handle(new SaveProgressReportCommand { ProgressReport = progressReportToUpdate, UserInfo = userInfo });

            var updatedProgressReport = mapper.Map<EMCR.DRR.Controllers.DraftProgressReport>((await manager.Handle(new DrrProgressReportsQuery { Id = progressReportToUpdate.Id, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            updatedProgressReport.Attachments.First().Comments.ShouldBe(progressReportToUpdate.Attachments.First().Comments);

        }

        [Test]
        public async Task CanAddAttachmentToForecast()
        {
            var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();

            var forecastId = "FORECAST-1154";
            var forecast = mapper.Map<DraftForecast>((await manager.Handle(new DrrForecastsQuery { Id = forecastId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            foreach (var doc in forecast.Attachments)
            {
                await manager.Handle(new DeleteAttachmentCommand { Id = doc.Id, UserInfo = userInfo });
            }

            var body = DateTime.Now.ToString();
            var fileName = "autotest.txt";
            byte[] bytes = Encoding.ASCII.GetBytes(body);
            var file = new S3File { FileName = fileName, Content = bytes, ContentType = "text/plain", };

            var documentId = await manager.Handle(new UploadAttachmentCommand { AttachmentInfo = new AttachmentInfo { RecordId = forecast.Id, RecordType = EMCR.DRR.Managers.Intake.RecordType.ForecastReport, File = file, DocumentType = EMCR.DRR.Managers.Intake.DocumentType.ForecastReport }, UserInfo = userInfo });

            var forecastToUpdate = mapper.Map<EMCR.DRR.Controllers.Forecast>((await manager.Handle(new DrrForecastsQuery { Id = forecastId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            forecastToUpdate.Attachments.Count().ShouldBe(1);
            forecastToUpdate.Attachments.Single().DocumentType.ShouldBe(EMCR.DRR.API.Model.DocumentType.ForecastReport);
            forecastToUpdate.Attachments.Single().Comments = "forecast report comments";

            await manager.Handle(new SaveForecastCommand { Forecast = forecastToUpdate, UserInfo = userInfo });

            var updatedForecast = mapper.Map<DraftForecast>((await manager.Handle(new DrrForecastsQuery { Id = forecastToUpdate.Id, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            updatedForecast.Attachments.Single().Comments.ShouldBe(forecastToUpdate.Attachments.Single().Comments);
        }

        [Test]
        public async Task CanDownloadAttachment()
        {
            //var userInfo = GetTestUserInfo();
            var userInfo = GetCRAFTUserInfo();

            var document = (FileQueryResult)(await manager.Handle(new DownloadAttachment { Id = "fed185a3-b079-4a4c-9680-36b220352cdc", UserInfo = userInfo }));
            document.File.FileName.ShouldNotBeNull();

        }

        private EMCR.DRR.Controllers.ProjectClaim FillInClaim(EMCR.DRR.Controllers.ProjectClaim claim, string uniqueSignature = "autotest")
        {
            claim.HaveClaimExpenses = true;
            claim.ClaimComment = $"{uniqueSignature} - claim comment";
            //claim.ClaimAmount = 5000;
            return claim;
        }

        private EMCR.DRR.Controllers.ProgressReport FillInProgressReport(EMCR.DRR.Controllers.ProgressReport progressReport, string uniqueSignature = "autotest")
        {
            progressReport.Workplan.MediaAnnouncementComment = $"{uniqueSignature} - media comment";

            progressReport.Workplan.ProjectProgress = ProjectProgressStatus.BehindSchedule;
            progressReport.Workplan.DelayReason = Delay.Other;
            progressReport.Workplan.OtherDelayReason = "we are slow";
            progressReport.Workplan.BehindScheduleMitigatingComments = "mitigation steps";
            progressReport.Workplan.ProjectCompletionPercentage = (decimal?)12.5;
            progressReport.Workplan.ConstructionCompletionPercentage = (decimal?)35.7;
            progressReport.Workplan.SignageRequired = true;
            progressReport.Workplan.MediaAnnouncement = true;
            progressReport.Workplan.MediaAnnouncementDate = DateTime.UtcNow.AddDays(3);
            progressReport.Workplan.MediaAnnouncementComment = "media announcement description";
            progressReport.Workplan.OutstandingIssues = true;
            progressReport.Workplan.OutstandingIssuesComments = "issues description";
            progressReport.Workplan.FundingSourcesChanged = true;
            progressReport.Workplan.FundingSourcesChangedComment = "funding change description";


            if (!progressReport.Workplan.WorkplanActivities.Any(a => a.Activity == EMCR.DRR.Controllers.ActivityType.PermitToConstruct))
            {
                progressReport.Workplan.WorkplanActivities = progressReport.Workplan.WorkplanActivities.Append(new WorkplanActivity
                {
                    Activity = EMCR.DRR.Controllers.ActivityType.PermitToConstruct,
                    PlannedStartDate = DateTime.UtcNow.AddDays(3),
                    ActualStartDate = DateTime.UtcNow.AddDays(4),
                    Comment = $"{uniqueSignature} - permit to construct comment",
                    Id = Guid.NewGuid().ToString(),
                    Status = EMCR.DRR.Controllers.WorkplanStatus.NoLongerNeeded,
                }).ToArray();
            }
            if (progressReport.Workplan.FundingSignage.Count() > 0) progressReport.Workplan.FundingSignage = progressReport.Workplan.FundingSignage.Take(progressReport.Workplan.FundingSignage.Count() - 1).ToArray();
            progressReport.Workplan.FundingSignage = progressReport.Workplan.FundingSignage.Append(new EMCR.DRR.Controllers.FundingSignage
            {
                Id = Guid.NewGuid().ToString(),
                Type = EMCR.DRR.Controllers.SignageType.Temporary,
                DateInstalled = DateTime.UtcNow.AddDays(3),
                DateRemoved = DateTime.UtcNow.AddDays(7),
                BeenApproved = false,
            }).ToArray();


            progressReport.EventInformation.EventsOccurredSinceLastReport = true;
            if (progressReport.EventInformation.PastEvents.Count() > 0) progressReport.EventInformation.PastEvents = progressReport.EventInformation.PastEvents.Take(progressReport.EventInformation.PastEvents.Count() - 1).ToArray();
            progressReport.EventInformation.PastEvents = progressReport.EventInformation.PastEvents.Append(new PastEvent
            {
                Details = $"{uniqueSignature} - past event details",
                Date = DateTime.UtcNow.AddDays(-2),
            }).ToArray();

            progressReport.EventInformation.AnyUpcomingEvents = true;
            if (progressReport.EventInformation.UpcomingEvents.Count() > 0) progressReport.EventInformation.UpcomingEvents = progressReport.EventInformation.UpcomingEvents.Take(progressReport.EventInformation.UpcomingEvents.Count() - 1).ToArray();
            progressReport.EventInformation.UpcomingEvents = progressReport.EventInformation.UpcomingEvents.Append(new EMCR.DRR.Controllers.ProjectEvent
            {
                Details = $"{uniqueSignature} - upcoming event details",
                Date = DateTime.UtcNow.AddDays(2),
                ProvincialRepresentativeRequest = true,
                //ProvincialRepresentativeRequestComment = "representative comment",
                Contact = CreateNewTestContact(uniqueSignature, "event")
            }).ToArray();


            return progressReport;
        }

        private EMCR.DRR.Controllers.ContactDetails CreateNewTestContact(string uniqueSignature, string namePrefix)
        {
            return new EMCR.DRR.Controllers.ContactDetails
            {
                FirstName = $"{uniqueSignature}_{namePrefix}_first",
                LastName = $"{uniqueSignature}_{namePrefix}_last",
                Email = "test@test.com",
                Phone = "604-123-4567",
                Department = "Position",
                Title = "Title"
            };
        }
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}
