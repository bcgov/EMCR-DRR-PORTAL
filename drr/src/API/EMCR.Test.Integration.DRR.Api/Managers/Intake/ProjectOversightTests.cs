using System.Text;
using AutoMapper;
using EMCR.DRR.API.Services.S3;
using EMCR.DRR.Controllers;
using EMCR.DRR.Managers.Intake;
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

        private UserInfo GetTestUserInfo()
        {
            return new UserInfo { BusinessId = TestBusinessId, BusinessName = TestBusinessName, UserId = TestUserId };
        }

        private UserInfo GetCRAFTUserInfo()
        {
            return new UserInfo { BusinessId = CRAFTD1BusinessId, BusinessName = CRAFTD1BusinessName, UserId = CRAFTD1UserId };
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
            projects.Count().ShouldBeGreaterThan(1);
            //projects.ShouldAllBe(s => s.ProgramType == ProgramType.DRIF);
        }

        [Test]
        public async Task QueryProjects_CanFilterById()
        {
            var queryOptions = new QueryOptions { Filter = "programType=DRIF,applicationType=FP,status=*UnderReview\\|EligiblePending" };
            var queryRes = await manager.Handle(new DrrProjectsQuery { Id = "DRIF-PRJ-1015", BusinessId = GetCRAFTUserInfo().BusinessId, QueryOptions = queryOptions });
            var projects = mapper.Map<IEnumerable<DraftDrrProject>>(queryRes.Items);
            projects.Count().ShouldBe(1);
            projects.First().Claims.Last().ReportPeriod.ShouldNotBeNullOrEmpty();
        }

        [Test]
        public async Task QueryReports_CanFilterById()
        {
            var queryRes = await manager.Handle(new DrrReportsQuery { Id = "DRIF-REP-1146", BusinessId = GetTestUserInfo().BusinessId });
            var reports = mapper.Map<IEnumerable<EMCR.DRR.Controllers.InterimReport>>(queryRes.Items);
            reports.Count().ShouldBe(1);
        }

        [Test]
        public async Task QueryClaims_CanFilterById()
        {
            var queryRes = await manager.Handle(new DrrClaimsQuery { Id = "DRIF-CLAIM-1000", BusinessId = GetTestUserInfo().BusinessId });
            var claims = mapper.Map<IEnumerable<EMCR.DRR.Controllers.ProjectClaim>>(queryRes.Items);
            claims.Count().ShouldBe(1);
        }

        [Test]
        public async Task QueryProgressReports_CanFilterById()
        {
            var queryRes = await manager.Handle(new DrrProgressReportsQuery { Id = "DRIF-PR-1058", BusinessId = GetCRAFTUserInfo().BusinessId });
            var prs = mapper.Map<IEnumerable<EMCR.DRR.Controllers.DraftProgressReport>>(queryRes.Items);
            prs.Count().ShouldBe(1);
            var progressReport = prs.Single();
            progressReport.ProjectType.ShouldBe(EMCR.DRR.Controllers.InterimProjectType.Stream1);
        }

        [Test]
        public async Task QueryForecasts_CanFilterById()
        {
            var queryRes = await manager.Handle(new DrrForecastsQuery { Id = "DRIF-FORECAST-1001", BusinessId = GetTestUserInfo().BusinessId });
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

            //Console.WriteLine(progressReport.Id);
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
        public async Task ValidateCanCreateReport_ValidationFalse()
        {
            var queryOptions = new QueryOptions { Filter = "programType=DRIF,applicationType=FP,status=*UnderReview\\|EligiblePending" };
            var queryRes = await manager.Handle(new DrrProjectsQuery { Id = "DRIF-PRJ-1015", BusinessId = GetCRAFTUserInfo().BusinessId, QueryOptions = queryOptions });
            var project = queryRes.Items.SingleOrDefault();
            var res = await manager.Handle(new ValidateCanCreateReportCommand { ProjectId = project.Id, ReportType = EMCR.DRR.Managers.Intake.ReportType.Interim, UserInfo = GetCRAFTUserInfo() });
            res.CanCreate.ShouldBe(false);
        }

        //[Test]
        //public async Task CanCreateReport()
        //{
        //    var queryOptions = new QueryOptions { Filter = "programType=DRIF,applicationType=FP,status=*UnderReview\\|EligiblePending" };
        //    var queryRes = await manager.Handle(new DrrProjectsQuery { Id = "DRIF-PRJ-1013", BusinessId = GetCRAFTUserInfo().BusinessId, QueryOptions = queryOptions });
        //    var project = queryRes.Items.SingleOrDefault();
        //    var res = await manager.Handle(new CreateInterimReportCommand { ProjectId = project.Id, ReportType = EMCR.DRR.Managers.Intake.ReportType.Interim, UserInfo = GetCRAFTUserInfo() });
        //    project = (await manager.Handle(new DrrProjectsQuery { Id = "DRIF-PRJ-1013", BusinessId = GetCRAFTUserInfo().BusinessId, QueryOptions = queryOptions })).Items.SingleOrDefault();
        //    project.InterimReports.First().ProgressReport.ShouldNotBeNull();
        //    project.InterimReports.First().ProjectClaim.ShouldNotBeNull();
        //    project.InterimReports.First().Forecast.ShouldNotBeNull();
        //    res.ShouldNotBeNullOrEmpty();
        //}

        //[Test]
        //public async Task CanSubmitProgressReport()
        //{
        //    var progressReportId = "DRIF-PR-1044";
        //    var uniqueSignature = TestPrefix + "-" + Guid.NewGuid().ToString().Substring(0, 4);
        //    var progressReport = mapper.Map<EMCR.DRR.Controllers.ProgressReport>((await manager.Handle(new DrrProgressReportsQuery { Id = progressReportId, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault());

        //    progressReport = FillInProgressReport(progressReport, uniqueSignature);

        //    //Console.WriteLine(progressReport.Id);
        //    await manager.Handle(new SubmitProgressReportCommand { ProgressReport = progressReport, UserInfo = GetTestUserInfo() });


        //    var submittedProgressReport = mapper.Map<EMCR.DRR.Controllers.ProgressReport>((await manager.Handle(new DrrProgressReportsQuery { Id = progressReport.Id, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault());
        //    submittedProgressReport.Workplan.MediaAnnouncementComment.ShouldBe(progressReport.Workplan.MediaAnnouncementComment);
        //    submittedProgressReport.Workplan.ProjectProgress.ShouldBe(progressReport.Workplan.ProjectProgress);
        //    submittedProgressReport.Workplan.MediaAnnouncement.ShouldBe(progressReport.Workplan.MediaAnnouncement);
        //    submittedProgressReport.Workplan.OtherDelayReason.ShouldBe(progressReport.Workplan.OtherDelayReason);
        //    submittedProgressReport.EventInformation.PastEvents.Count().ShouldBe(1);
        //    submittedProgressReport.EventInformation.UpcomingEvents.Count().ShouldBe(1);
        //    submittedProgressReport.DateSubmitted.ShouldNotBeNull();
        //    submittedProgressReport.Status.ShouldBe(EMCR.DRR.Controllers.ProgressReportStatus.Submitted);
        //}

        [Test]
        public async Task CanAddAttachment()
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
        public async Task CanDownloadAttachment()
        {
            //var userInfo = GetTestUserInfo();
            var userInfo = GetCRAFTUserInfo();

            var document = (FileQueryResult)(await manager.Handle(new DownloadAttachment { Id = "fed185a3-b079-4a4c-9680-36b220352cdc", UserInfo = userInfo }));
            document.File.FileName.ShouldNotBeNull();

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


            if (progressReport.Workplan.WorkplanActivities.Count() > 0)
                progressReport.Workplan.WorkplanActivities = progressReport.Workplan.WorkplanActivities = progressReport.Workplan.WorkplanActivities.Take(progressReport.Workplan.WorkplanActivities.Count() - 1).ToArray();
            progressReport.Workplan.WorkplanActivities = progressReport.Workplan.WorkplanActivities.Append(new WorkplanActivity
            {
                Activity = EMCR.DRR.Controllers.ActivityType.Mapping,
                ActualCompletionDate = DateTime.UtcNow.AddDays(11),
                ActualStartDate = DateTime.UtcNow.AddDays(4),
                Comment = $"{uniqueSignature} - mapping comment",
                Id = Guid.NewGuid().ToString(),
                PlannedCompletionDate = DateTime.UtcNow.AddDays(10),
                PlannedStartDate = DateTime.UtcNow.AddDays(3),
                Status = EMCR.DRR.Controllers.WorkplanStatus.NotStarted,
            }).ToArray();
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
