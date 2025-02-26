using System.Text;
using AutoMapper;
using EMCR.DRR.API.Model;
using EMCR.DRR.API.Services.S3;
using EMCR.DRR.API.Utilities.TestData;
using EMCR.DRR.Controllers;
using EMCR.DRR.Dynamics;
using EMCR.DRR.Managers.Intake;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EMCR.Tests.Integration.DRR.Managers.Intake
{
    public class SubmissionTests
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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8629 // Nullable value type may be null.
        public SubmissionTests()
        {
            var host = Application.Host;
            manager = host.Services.GetRequiredService<IIntakeManager>();
            mapper = host.Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task CanCreateEOIApplication()
        {
            var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();

            var application = TestHelper.CreateNewTestEOIApplication();
            var id = await manager.Handle(new EoiSaveApplicationCommand { Application = mapper.Map<EoiApplication>(application), UserInfo = userInfo });
            id.ShouldNotBeEmpty();
        }

        [Test]
        public async Task CanSubmitEOIApplication()
        {
            var application = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            application.Status = SubmissionPortalStatus.UnderReview;
            application.AuthorizedRepresentativeStatement = true;
            application.FOIPPAConfirmation = true;
            application.InformationAccuracyStatement = true;

            var id = await manager.Handle(new EoiSubmitApplicationCommand { Application = application, UserInfo = GetTestUserInfo() });
            id.ShouldNotBeEmpty();

            var savedApplication = (await manager.Handle(new DrrApplicationsQuery { Id = id, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();
            savedApplication.Id.ShouldBe(id);
            savedApplication.AuthorizedRepresentativeStatement.ShouldBe(true);
            savedApplication.FOIPPAConfirmation.ShouldBe(true);
            savedApplication.InformationAccuracyStatement.ShouldBe(true);
            savedApplication.Status.ShouldBe(ApplicationStatus.Submitted);
            savedApplication.AdditionalContact1.ShouldNotBeNull();
            savedApplication.SubmittedDate.ShouldNotBeNull();
            savedApplication.InfrastructureImpacted.ShouldHaveSingleItem().Impact.ShouldNotBeNullOrEmpty();
        }

        [Test]
        public async Task CanSubmitFPApplication()
        {
            var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();

            var eoi = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            eoi.Status = SubmissionPortalStatus.EligibleInvited;
            eoi.AuthorizedRepresentativeStatement = true;
            eoi.FOIPPAConfirmation = true;
            eoi.InformationAccuracyStatement = true;

            var eoiId = await manager.Handle(new EoiSubmitApplicationCommand { Application = eoi, UserInfo = userInfo });
            eoiId.ShouldNotBeEmpty();

            var fpId = await manager.Handle(new CreateFpFromEoiCommand { EoiId = eoiId, UserInfo = userInfo, ScreenerQuestions = CreateScreenerQuestions() });
            fpId.ShouldNotBeEmpty();

            var body = DateTime.Now.ToString();
            byte[] bytes = Encoding.ASCII.GetBytes(body);
            var projectWorkplanFile = new S3File { FileName = "autotest-dpw.txt", Content = bytes, ContentType = "text/plain", };
            var costEstimateFile = new S3File { FileName = "autotest-dce.txt", Content = bytes, ContentType = "text/plain", };

            //await manager.Handle(new UploadAttachmentCommand { AttachmentInfo = new AttachmentInfo { ApplicationId = fpId, File = projectWorkplanFile, DocumentType = EMCR.DRR.Managers.Intake.DocumentType.DetailedProjectWorkplan }, UserInfo = GetTestUserInfo() });
            await manager.Handle(new UploadAttachmentCommand { AttachmentInfo = new AttachmentInfo { RecordId = fpId, RecordType = EMCR.DRR.Managers.Intake.RecordType.FullProposal, File = costEstimateFile, DocumentType = EMCR.DRR.Managers.Intake.DocumentType.DetailedCostEstimate }, UserInfo = GetTestUserInfo() });

            var fullProposal = (await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault();
            fullProposal.Id.ShouldBe(fpId);
            fullProposal.EoiId.ShouldBe(eoiId);

            var fpToSubmit = mapper.Map<FpApplication>(FillInFullProposal(mapper.Map<DraftFpApplication>(fullProposal)));
            fpToSubmit.Submitter = eoi.Submitter;
            fpToSubmit.AuthorizedRepresentativeStatement = true;
            fpToSubmit.InformationAccuracyStatement = true;

            foreach (var activity in fpToSubmit.ProposedActivities)
            {
                if (!activity.StartDate.HasValue) activity.StartDate = DateTime.UtcNow.AddDays(1);
                if (!activity.EndDate.HasValue) activity.EndDate = DateTime.UtcNow.AddDays(5);
            }

            await manager.Handle(new FpSubmitApplicationCommand { Application = fpToSubmit, UserInfo = userInfo });

            var submittedFP = (await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault();
            submittedFP.Status.ShouldBe(ApplicationStatus.Submitted);
            submittedFP.AuthorizedRepresentativeStatement.ShouldBe(true);
            submittedFP.InformationAccuracyStatement.ShouldBe(true);
        }

        [Test]
        public async Task QueryApplications_CanGetSpecificPage()
        {
            var queryOptions = new QueryOptions { Page = 2, PageSize = 15 };
            //var queryOptions = new QueryOptions { Page = 1, PageSize = 100 };
            var queryRes = await manager.Handle(new DrrApplicationsQuery { BusinessId = GetTestUserInfo().BusinessId, QueryOptions = queryOptions });
            var applications = queryRes.Items;
            var submissions = mapper.Map<IEnumerable<Submission>>(applications);
            submissions.Count().ShouldBeLessThanOrEqualTo(15);
        }

        [Test]
        public async Task QueryApplications_CanFilterByField()
        {
            var queryOptions = new QueryOptions { Filter = "programType=DRIF,applicationType=FP,status=*UnderReview\\|EligiblePending" };
            var queryRes = await manager.Handle(new DrrApplicationsQuery { BusinessId = GetTestUserInfo().BusinessId, QueryOptions = queryOptions });
            var applications = queryRes.Items;
            var submissions = mapper.Map<IEnumerable<Submission>>(applications);
            //submissions.Count().ShouldBe(20);
            submissions.ShouldAllBe(s => s.ProgramType == ProgramType.DRIF);
            submissions.ShouldAllBe(s => s.ApplicationType == ApplicationType.FP);
            submissions.ShouldAllBe(s => s.Status == SubmissionPortalStatus.UnderReview || s.Status == SubmissionPortalStatus.EligiblePending);
        }

        [Test]
        public async Task QueryApplications_CanSortResults()
        {
            var queryOptions = new QueryOptions { OrderBy = "fundingrequest desc" };
            var queryRes = await manager.Handle(new DrrApplicationsQuery { BusinessId = GetTestUserInfo().BusinessId, QueryOptions = queryOptions });
            var applications = queryRes.Items;
            var submissions = mapper.Map<IEnumerable<Submission>>(applications);
            submissions.Count().ShouldBeLessThanOrEqualTo(20);
        }

        [Test]
        public async Task CanQueryApplications()
        {
            var application = CreateNewTestEOIApplication();
            var id = await manager.Handle(new EoiSaveApplicationCommand { Application = mapper.Map<EoiApplication>(application), UserInfo = GetTestUserInfo() });
            id.ShouldNotBeEmpty();

            var secondApplication = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            secondApplication.Status = SubmissionPortalStatus.UnderReview;
            secondApplication.AuthorizedRepresentativeStatement = true;
            secondApplication.FOIPPAConfirmation = true;
            secondApplication.InformationAccuracyStatement = true;

            var secondId = await manager.Handle(new EoiSubmitApplicationCommand { Application = secondApplication, UserInfo = GetTestUserInfo() });
            secondId.ShouldNotBeEmpty();

            var thirdApplication = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            thirdApplication.Status = SubmissionPortalStatus.EligibleInvited;
            thirdApplication.AuthorizedRepresentativeStatement = true;
            thirdApplication.FOIPPAConfirmation = true;
            thirdApplication.InformationAccuracyStatement = true;

            var thirdId = await manager.Handle(new EoiSubmitApplicationCommand { Application = thirdApplication, UserInfo = GetTestUserInfo() });
            thirdId.ShouldNotBeEmpty();

            var fpId = await manager.Handle(new CreateFpFromEoiCommand { EoiId = thirdId, UserInfo = GetTestUserInfo(), ScreenerQuestions = CreateScreenerQuestions() });
            fpId.ShouldNotBeEmpty();

            var queryRes = await manager.Handle(new DrrApplicationsQuery { BusinessId = GetTestUserInfo().BusinessId });
            var applications = queryRes.Items;
            var submissions = mapper.Map<IEnumerable<Submission>>(applications);
            submissions.ShouldContain(s => s.Id == id);
            submissions.ShouldContain(s => s.Id == secondId);
            submissions.ShouldContain(s => s.Id == thirdId);
            submissions.Single(s => s.Id == thirdId).ExistingFpId.ShouldNotBeNull();
            submissions.ShouldAllBe(s => s.ProgramType == ProgramType.DRIF);
        }

        [Test]
        public async Task CanCreateDraftEOIApplication()
        {
            var uniqueSignature = TestPrefix + "-" + Guid.NewGuid().ToString().Substring(0, 4);
            var application = new DraftEoiApplication
            {
                Status = SubmissionPortalStatus.Draft,
                //RelatedHazards = new[] { EMCR.DRR.Controllers.Hazards.Other },
            };
            var userInfo = new UserInfo
            {
                BusinessId = $"{uniqueSignature}_business-bceid",
                BusinessName = $"{uniqueSignature}_business-name",
                UserId = $"{uniqueSignature}_user-bceid"
            };
            var id = await manager.Handle(new EoiSaveApplicationCommand { Application = mapper.Map<EoiApplication>(application), UserInfo = userInfo });
            id.ShouldNotBeEmpty();


            var savedApplication = (await manager.Handle(new DrrApplicationsQuery { Id = id, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault();
            savedApplication.Id.ShouldBe(id);
            savedApplication.OwnershipDeclaration.ShouldBeNull();
            savedApplication.AuthorizedRepresentativeStatement.ShouldBe(false);
            savedApplication.SubmittedDate.ShouldBeNull();
        }

        [Test]
        public async Task SaveEOI_BlankInfrastructure_BlankRecordNotCreated()
        {
            var application = CreateNewTestEOIApplication();

            application.InfrastructureImpacted = new[]
            {
                new InfrastructureImpacted
                {
                    Infrastructure = string.Empty,
                    Impact = string.Empty,
                },
                    null
            };

            var id = await manager.Handle(new EoiSaveApplicationCommand { Application = mapper.Map<EoiApplication>(application), UserInfo = GetTestUserInfo() });
            id.ShouldNotBeEmpty();

            var savedApplication = (await manager.Handle(new DrrApplicationsQuery { Id = id, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();
            savedApplication.InfrastructureImpacted.Count().ShouldBe(0);
        }

        [Test]
        public async Task SubmitMultipleApplications_SameSubmitter_MultipleSubmitterContactCreated()
        {
            var uniqueSignature = TestPrefix + "-" + Guid.NewGuid().ToString().Substring(0, 4);
            var application = CreateNewTestEOIApplication();
            application.Status = SubmissionPortalStatus.UnderReview;
            application.ProjectTitle = "First Submission";
            var id = await manager.Handle(new EoiSubmitApplicationCommand { Application = mapper.Map<EoiApplication>(application), UserInfo = GetTestUserInfo() });
            id.ShouldNotBeEmpty();

            var secondApplication = CreateNewTestEOIApplication();
            secondApplication.Status = SubmissionPortalStatus.UnderReview;
            secondApplication.ProjectTitle = "Second Submission";
            secondApplication.Submitter = application.Submitter;
            var secondId = await manager.Handle(new EoiSubmitApplicationCommand { Application = mapper.Map<EoiApplication>(secondApplication), UserInfo = GetTestUserInfo() });
            secondId.ShouldNotBeEmpty();

            var host = Application.Host;
            var drrCtxFactory = host.Services.GetRequiredService<IDRRContextFactory>();
            var ctx = drrCtxFactory.CreateReadOnly();

            var submitters = ctx.contacts.Where(c => c.drr_userid == TestUserId).ToList();
            submitters.Count.ShouldBeGreaterThan(1);
        }

        [Test]
        public async Task CanCreateFpFromEoi()
        {
            var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();

            var eoi = mapper.Map<EoiApplication>(TestHelper.CreateNewTestEOIApplication());
            //var eoi = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            eoi.Status = SubmissionPortalStatus.EligibleInvited;
            eoi.AuthorizedRepresentativeStatement = true;
            eoi.FOIPPAConfirmation = true;
            eoi.InformationAccuracyStatement = true;

            var eoiId = await manager.Handle(new EoiSubmitApplicationCommand { Application = eoi, UserInfo = userInfo });
            eoiId.ShouldNotBeEmpty();

            var screenerQuestions = CreateScreenerQuestions();
            screenerQuestions.FirstNationsAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable;
            screenerQuestions.LocalGovernmentAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable;
            screenerQuestions.EngagedWithFirstNationsOccurred = false;

            var fpId = await manager.Handle(new CreateFpFromEoiCommand { EoiId = eoiId, UserInfo = userInfo, ScreenerQuestions = screenerQuestions });
            fpId.ShouldNotBeEmpty();

            var fullProposal = (await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault();
            fullProposal.Id.ShouldBe(fpId);

            fullProposal.RegionalProject.ShouldBeNull();
            fullProposal.MainDeliverable.ShouldBe(eoi.ScopeStatement);
            fullProposal.HaveAuthorityToDevelop.ShouldBe(screenerQuestions.HaveAuthorityToDevelop);
            fullProposal.FirstNationsAuthorizedByPartners.ShouldBe(screenerQuestions.FirstNationsAuthorizedByPartners);
            fullProposal.LocalGovernmentAuthorizedByPartners.ShouldBe(screenerQuestions.LocalGovernmentAuthorizedByPartners);
            //fullProposal.FoundationWorkCompleted.ShouldBe(screenerQuestions.FoundationWorkCompleted);
            fullProposal.EngagedWithFirstNationsOccurred.ShouldBeNull();
            fullProposal.IncorporateFutureClimateConditions.ShouldBe(screenerQuestions.IncorporateFutureClimateConditions);
            fullProposal.MeetsRegulatoryRequirements.ShouldBe(screenerQuestions.MeetsRegulatoryRequirements);
            fullProposal.MeetsEligibilityRequirements.ShouldBe(screenerQuestions.MeetsEligibilityRequirements);
            fullProposal.InfrastructureImpacted.Count().ShouldBe(eoi.InfrastructureImpacted.Count());
        }

        [Test]
        public async Task DeleteFP_ThrowsError()
        {
            var eoi = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            eoi.Status = SubmissionPortalStatus.EligibleInvited;
            eoi.AuthorizedRepresentativeStatement = true;
            eoi.FOIPPAConfirmation = true;
            eoi.InformationAccuracyStatement = true;

            var eoiId = await manager.Handle(new EoiSubmitApplicationCommand { Application = eoi, UserInfo = GetTestUserInfo() });
            eoiId.ShouldNotBeEmpty();

            var screenerQuestions = CreateScreenerQuestions();
            screenerQuestions.FirstNationsAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable;
            screenerQuestions.LocalGovernmentAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable;
            screenerQuestions.EngagedWithFirstNationsOccurred = false;

            var fpId = await manager.Handle(new CreateFpFromEoiCommand { EoiId = eoiId, UserInfo = GetTestUserInfo(), ScreenerQuestions = screenerQuestions });
            fpId.ShouldNotBeEmpty();

            var fullProposal = (await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();
            fullProposal.Id.ShouldBe(fpId);

            Should.Throw<Exception>(() => manager.Handle(new DeleteApplicationCommand { Id = fpId, UserInfo = GetTestUserInfo() }));
        }

        [Test]
        public async Task CanQueryFp()
        {
            var fp = (await manager.Handle(new DrrApplicationsQuery { Id = "DRIF-FP-4492", BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();
            var ret = mapper.Map<DraftFpApplication>(fp);
            ret.ProposedActivities.ShouldNotBeEmpty();
        }

        [Test]
        public async Task CanUpdateFp()
        {
            var eoi = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            eoi.Status = SubmissionPortalStatus.EligibleInvited;
            eoi.AuthorizedRepresentativeStatement = true;
            eoi.FOIPPAConfirmation = true;
            eoi.InformationAccuracyStatement = true;

            var eoiId = await manager.Handle(new EoiSubmitApplicationCommand { Application = eoi, UserInfo = GetTestUserInfo() });
            eoiId.ShouldNotBeEmpty();

            var fpId = await manager.Handle(new CreateFpFromEoiCommand { EoiId = eoiId, UserInfo = GetTestUserInfo(), ScreenerQuestions = CreateScreenerQuestions() });
            fpId.ShouldNotBeEmpty();

            var fullProposal = (await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();
            fullProposal.Id.ShouldBe(fpId);
            fullProposal.EoiId.ShouldBe(eoiId);
            fullProposal.HowWasNeedIdentified.ShouldBe(eoi.RationaleForSolution);
            var fpToUpdate = FillInFullProposal(mapper.Map<DraftFpApplication>(fullProposal));
            fpToUpdate.ProposedActivities.First().Deliverables = "project deliverables";
            fpToUpdate.ProposedActivities.First().StartDate = DateTime.UtcNow.AddDays(1);

            foreach (var activity in fpToUpdate.ProposedActivities)
            {
                if (!activity.StartDate.HasValue) activity.StartDate = DateTime.UtcNow.AddDays(1);
                if (!activity.EndDate.HasValue) activity.EndDate = DateTime.UtcNow.AddDays(5);
            }

            await manager.Handle(new FpSaveApplicationCommand { Application = mapper.Map<FpApplication>(fpToUpdate), UserInfo = GetTestUserInfo() });

            var updatedFp = (await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();
            updatedFp.RegionalProject.ShouldBe(true);
            updatedFp.IsInfrastructureImpacted.ShouldBe(true);
            updatedFp.EstimatedPeopleImpactedFP.ShouldBe(EMCR.DRR.Managers.Intake.EstimatedNumberOfPeopleFP.FiveHundredToOneK);
            updatedFp.Standards.ShouldContain(s => s.Category == "Other");
            updatedFp.Standards.Single(s => s.Category == "Other").IsCategorySelected.ShouldBe(true);
            updatedFp.Professionals.ShouldContain(p => p.Name == "professional1");
            updatedFp.LocalGovernmentAuthorizedByPartners.ShouldBe(EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable);
            ((int)updatedFp.OperationAndMaintenance).ShouldBe((int)fpToUpdate.OperationAndMaintenance);
            updatedFp.ClimateAssessmentTools.ShouldNotBeEmpty();
            updatedFp.ClimateAssessmentComments.ShouldBe("climate assessment comments");
            updatedFp.IncreasedOrTransferred.ShouldNotBeEmpty();
            updatedFp.CostEstimates.ShouldNotBeEmpty();
            updatedFp.IntendToSecureFunding.ShouldBe(fpToUpdate.IntendToSecureFunding);

            var ret = mapper.Map<DraftFpApplication>(updatedFp);
            ret.FoundationalOrPreviousWorks.ShouldContain("autotest-verification-method");
            ret.AffectedParties.ShouldContain("party 1");
            ret.Permits.ShouldContain("permit 1");
            ret.ClimateAssessmentTools.ShouldContain("tool 1");
            ret.Professionals.ShouldContain("professional1");
            ret.CostReductions.ShouldContain("cost reduction 1");
            ret.CoBenefits.ShouldContain("benefit 1");
            ret.IncreasedResiliency.ShouldContain("resiliency 1");
            ret.ComplexityRisks.ShouldContain("complexity risk 1");
            ret.ReadinessRisks.ShouldContain("readiness risk 1");
            ret.SensitivityRisks.ShouldContain("sensitivity risk 1");
            ret.CapacityRisks.ShouldContain("capacity risk 1");
            ret.CostConsiderations.ShouldContain("cost consideration 1");
            ret.PreviousResponseComments.ShouldBe(fpToUpdate.PreviousResponseComments);
            ret.MeetsEligibilityRequirements.ShouldBe(fpToUpdate.MeetsEligibilityRequirements);
            ret.MeetsEligibilityComments.ShouldBe(fpToUpdate.MeetsEligibilityComments);
            ret.TotalProjectCost.ShouldBe(fpToUpdate.TotalProjectCost);
            ret.HowWasNeedIdentified.ShouldBe(fpToUpdate.HowWasNeedIdentified);
            ret.Contingency.ShouldBe(fpToUpdate.Contingency);
            ret.ProposedActivities.Count().ShouldBe(fpToUpdate.ProposedActivities.Count());
            ret.CostEstimateClass.ShouldBe(fpToUpdate.CostEstimateClass);
            ret.TotalProjectCostChangeComments.ShouldBe(fpToUpdate.TotalProjectCostChangeComments);
            //ret.TotalEligibleCosts.ShouldBe(fpToUpdate.TotalEligibleCosts);
        }

        [Test]
        public async Task UpdateFp_EmptyFields_ValuesAreCleared()
        {
            var eoi = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            eoi.Status = SubmissionPortalStatus.EligibleInvited;
            eoi.AuthorizedRepresentativeStatement = true;
            eoi.FOIPPAConfirmation = true;
            eoi.InformationAccuracyStatement = true;

            var eoiId = await manager.Handle(new EoiSubmitApplicationCommand { Application = eoi, UserInfo = GetTestUserInfo() });
            eoiId.ShouldNotBeEmpty();

            var fpId = await manager.Handle(new CreateFpFromEoiCommand { EoiId = eoiId, UserInfo = GetTestUserInfo(), ScreenerQuestions = CreateScreenerQuestions() });
            fpId.ShouldNotBeEmpty();

            var fullProposal = (await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();
            fullProposal.Id.ShouldBe(fpId);
            fullProposal.EoiId.ShouldBe(eoiId);
            fullProposal.HowWasNeedIdentified.ShouldBe(eoi.RationaleForSolution);

            var fpToUpdate = FillInFullProposal(mapper.Map<DraftFpApplication>(fullProposal));
            await manager.Handle(new FpSaveApplicationCommand { Application = mapper.Map<FpApplication>(fpToUpdate), UserInfo = GetTestUserInfo() });

            var updatedFp = (await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();
            updatedFp.RegionalProject.ShouldBe(true);
            updatedFp.IsInfrastructureImpacted.ShouldBe(true);
            updatedFp.EstimatedPeopleImpactedFP.ShouldBe(EMCR.DRR.Managers.Intake.EstimatedNumberOfPeopleFP.FiveHundredToOneK);
            updatedFp.Standards.ShouldContain(s => s.Category == "Other");
            updatedFp.Standards.Single(s => s.Category == "Other").IsCategorySelected.ShouldBe(true);
            updatedFp.Professionals.ShouldContain(p => p.Name == "professional1");
            updatedFp.LocalGovernmentAuthorizedByPartners.ShouldBe(EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable);
            ((int)updatedFp.OperationAndMaintenance).ShouldBe((int)fpToUpdate.OperationAndMaintenance);
            updatedFp.ClimateAssessmentTools.ShouldNotBeEmpty();
            updatedFp.ClimateAssessmentComments.ShouldBe("climate assessment comments");
            updatedFp.IncreasedOrTransferred.ShouldNotBeEmpty();
            updatedFp.IntendToSecureFunding.ShouldBe(fpToUpdate.IntendToSecureFunding);

            var fpToUpdate2 = ClearFullProposal(mapper.Map<DraftFpApplication>(updatedFp));
            await manager.Handle(new FpSaveApplicationCommand { Application = mapper.Map<FpApplication>(fpToUpdate2), UserInfo = GetTestUserInfo() });

            var twiceUpdatedFp = (await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();

            var ret = mapper.Map<DraftFpApplication>(twiceUpdatedFp);
            ret.RegionalProjectComments.ShouldBeNull();
            ret.Area.ShouldBeNull();
            ret.StartDate.ShouldBeNull();
            ret.EndDate.ShouldBeNull();
            ret.Standards.ShouldAllBe(s => s.Standards.Count() == 0);
            ret.RelatedHazards.ShouldBeEmpty();
            ret.Professionals.ShouldBeEmpty();
            ret.ClimateAssessmentTools.ShouldBeEmpty();
            ret.ClimateAssessmentComments.ShouldBeNull();
            ret.IncreasedOrTransferred.ShouldBeEmpty();
            ret.IntendToSecureFunding.ShouldBeNull();
            ret.PreviousResponseCost.ShouldBeNull();
            ret.PreviousResponseComments.ShouldBeNull();
        }

        [Test]
        public async Task UpdateFP_ListsUpdateCorrectly()
        {
            var eoi = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            eoi.Status = SubmissionPortalStatus.EligibleInvited;
            eoi.AuthorizedRepresentativeStatement = true;
            eoi.FOIPPAConfirmation = true;
            eoi.InformationAccuracyStatement = true;

            var eoiId = await manager.Handle(new EoiSubmitApplicationCommand { Application = eoi, UserInfo = GetTestUserInfo() });
            eoiId.ShouldNotBeEmpty();

            var fpId = await manager.Handle(new CreateFpFromEoiCommand { EoiId = eoiId, UserInfo = GetTestUserInfo(), ScreenerQuestions = CreateScreenerQuestions() });
            fpId.ShouldNotBeEmpty();

            var fullProposal = (await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();
            fullProposal.Id.ShouldBe(fpId);
            fullProposal.EoiId.ShouldBe(eoiId);

            var fpToUpdate = FillInFullProposal(mapper.Map<DraftFpApplication>(fullProposal));
            await manager.Handle(new FpSaveApplicationCommand { Application = mapper.Map<FpApplication>(fpToUpdate), UserInfo = GetTestUserInfo() });

            var updatedFp = (await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();
            updatedFp.RegionalProject.ShouldBe(true);
            updatedFp.Standards.ShouldContain(s => s.Category == "Other");
            updatedFp.Professionals.ShouldContain(p => p.Name == "professional1");
            updatedFp.LocalGovernmentAuthorizedByPartners.ShouldBe(EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable);

            fpToUpdate = FillInFullProposal(mapper.Map<DraftFpApplication>(updatedFp));
            fpToUpdate.ProposedActivities = fpToUpdate.ProposedActivities.Where(a => a.Activity != EMCR.DRR.Controllers.ActivityType.Mapping).ToList();

            await manager.Handle(new FpSaveApplicationCommand { Application = mapper.Map<FpApplication>(fpToUpdate), UserInfo = GetTestUserInfo() });

            var twiceUpdatedFp = (await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();
            var ret = mapper.Map<DraftFpApplication>(twiceUpdatedFp);
            ret.Professionals.Count().ShouldBe(fpToUpdate.Professionals.Count());
            ret.Standards.Count().ShouldBe(6);
            ret.ProposedActivities.Count().ShouldBe(fpToUpdate.ProposedActivities.Count());
            ret.FoundationalOrPreviousWorks.Count().ShouldBe(fpToUpdate.FoundationalOrPreviousWorks.Count());
            ret.AffectedParties.Count().ShouldBe(fpToUpdate.AffectedParties.Count());
            ret.CostReductions.Count().ShouldBe(fpToUpdate.CostReductions.Count());
            ret.CoBenefits.Count().ShouldBe(fpToUpdate.CoBenefits.Count());
            ret.IncreasedResiliency.Count().ShouldBe(fpToUpdate.IncreasedResiliency.Count());
            ret.ComplexityRisks.Count().ShouldBe(fpToUpdate.ComplexityRisks.Count());
            ret.ReadinessRisks.Count().ShouldBe(fpToUpdate.ReadinessRisks.Count());
            ret.SensitivityRisks.Count().ShouldBe(fpToUpdate.SensitivityRisks.Count());
            ret.CapacityRisks.Count().ShouldBe(fpToUpdate.CapacityRisks.Count());
            ret.CostEstimates.Count().ShouldBe(fpToUpdate.CostEstimates.Count());
            ////ret.TransferRisks.Count().ShouldBe(fpToUpdate.TransferRisks.Count());
            ret.YearOverYearFunding.Count().ShouldBe(fpToUpdate.YearOverYearFunding.Count());
            //ret.CostConsiderations.Count().ShouldBe(fpToUpdate.CostConsiderations.Count());
        }

        [Test]
        public async Task UpdateFp_ContingencyCostCategoryStream1_ThrowsError()
        {
            var eoi = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            eoi.Status = SubmissionPortalStatus.EligibleInvited;
            eoi.AuthorizedRepresentativeStatement = true;
            eoi.FOIPPAConfirmation = true;
            eoi.InformationAccuracyStatement = true;

            var eoiId = await manager.Handle(new EoiSubmitApplicationCommand { Application = eoi, UserInfo = GetTestUserInfo() });
            eoiId.ShouldNotBeEmpty();

            var fpId = await manager.Handle(new CreateFpFromEoiCommand { EoiId = eoiId, UserInfo = GetTestUserInfo(), ScreenerQuestions = CreateScreenerQuestions() });
            fpId.ShouldNotBeEmpty();

            var fullProposal = (await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();
            fullProposal.Id.ShouldBe(fpId);
            fullProposal.EoiId.ShouldBe(eoiId);
            fullProposal.HowWasNeedIdentified.ShouldBe(eoi.RationaleForSolution);
            var fpToUpdate = FillInFullProposal(mapper.Map<DraftFpApplication>(fullProposal));
            fpToUpdate.FundingStream = EMCR.DRR.Controllers.FundingStream.Stream1;
            fpToUpdate.CostEstimates = fpToUpdate.CostEstimates.Append(new EMCR.DRR.Controllers.CostEstimate
            {
                TaskName = "cost estimate task 2",
                CostCategory = EMCR.DRR.Controllers.CostCategory.Contingency,
                Description = "cost estimate description",
                Resources = EMCR.DRR.Controllers.ResourceCategory.ProjectSupport,
                Units = EMCR.DRR.Controllers.CostUnit.SquareKilometer,
                Quantity = 1,
                UnitRate = (decimal?)10,
                TotalCost = 10,
            });

            Should.Throw<Exception>(() => manager.Handle(new FpSaveApplicationCommand { Application = mapper.Map<FpApplication>(fpToUpdate), UserInfo = GetTestUserInfo() }));
        }

        [Test]
        public async Task WithdrawApplication_StatusDraft_ThrowsError()
        {
            var application = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            application.Status = SubmissionPortalStatus.Draft;

            var id = await manager.Handle(new EoiSaveApplicationCommand { Application = application, UserInfo = GetTestUserInfo() });
            id.ShouldNotBeEmpty();

            Should.Throw<Exception>(() => manager.Handle(new WithdrawApplicationCommand { Id = id, UserInfo = GetTestUserInfo() }));
        }

        [Test]
        public async Task WithdrawApplication_StatusSubmitted_ApplicationWithdrawn()
        {
            var application = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            application.Status = SubmissionPortalStatus.UnderReview;
            application.AuthorizedRepresentativeStatement = true;
            application.FOIPPAConfirmation = true;
            application.InformationAccuracyStatement = true;

            var id = await manager.Handle(new EoiSubmitApplicationCommand { Application = application, UserInfo = GetTestUserInfo() });
            id.ShouldNotBeEmpty();

            await manager.Handle(new WithdrawApplicationCommand { Id = id, UserInfo = GetTestUserInfo() });
            var withdrawnApplication = (await manager.Handle(new DrrApplicationsQuery { Id = id, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();
            withdrawnApplication.Status.ShouldBe(ApplicationStatus.Withdrawn);
        }

        [Test]
        public async Task DeleteApplication_StatusSubmitted_ThrowsError()
        {
            var application = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            application.Status = SubmissionPortalStatus.UnderReview;
            application.AuthorizedRepresentativeStatement = true;
            application.FOIPPAConfirmation = true;
            application.InformationAccuracyStatement = true;

            var id = await manager.Handle(new EoiSaveApplicationCommand { Application = application, UserInfo = GetTestUserInfo() });
            id.ShouldNotBeEmpty();

            Should.Throw<Exception>(() => manager.Handle(new DeleteApplicationCommand { Id = id, UserInfo = GetTestUserInfo() }));
        }

        [Test]
        public async Task DeleteApplication_StatusDraft_StatusUpdatedToDeleted()
        {
            var application = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            application.Status = SubmissionPortalStatus.Draft;

            var id = await manager.Handle(new EoiSaveApplicationCommand { Application = application, UserInfo = GetTestUserInfo() });
            id.ShouldNotBeEmpty();

            var res = await manager.Handle(new DeleteApplicationCommand { Id = id, UserInfo = GetTestUserInfo() });
            res.ShouldBe(id);

            var deletedApplication = (await manager.Handle(new DrrApplicationsQuery { Id = id, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();
            deletedApplication.ShouldBeNull();
        }

        [Test]
        public async Task CanAddAttachment()
        {
            var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();

            var eoi = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            eoi.Status = SubmissionPortalStatus.EligibleInvited;
            eoi.AuthorizedRepresentativeStatement = true;
            eoi.FOIPPAConfirmation = true;
            eoi.InformationAccuracyStatement = true;

            var eoiId = await manager.Handle(new EoiSubmitApplicationCommand { Application = eoi, UserInfo = userInfo });
            eoiId.ShouldNotBeEmpty();

            var screenerQuestions = CreateScreenerQuestions();
            screenerQuestions.FirstNationsAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable;
            screenerQuestions.LocalGovernmentAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable;
            screenerQuestions.EngagedWithFirstNationsOccurred = false;

            var fpId = await manager.Handle(new CreateFpFromEoiCommand { EoiId = eoiId, UserInfo = userInfo, ScreenerQuestions = screenerQuestions });
            fpId.ShouldNotBeEmpty();

            var body = DateTime.Now.ToString();
            var fileName = "autotest.txt";
            byte[] bytes = Encoding.ASCII.GetBytes(body);
            var file = new S3File { FileName = fileName, Content = bytes, ContentType = "text/plain", };

            var documentId = await manager.Handle(new UploadAttachmentCommand { AttachmentInfo = new AttachmentInfo { RecordId = fpId, RecordType = EMCR.DRR.Managers.Intake.RecordType.FullProposal, File = file, DocumentType = EMCR.DRR.Managers.Intake.DocumentType.SitePlan }, UserInfo = GetTestUserInfo() });
            var fullProposal = mapper.Map<DraftFpApplication>((await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            fullProposal.HaveResolution = true;
            fullProposal.Attachments.Count().ShouldBe(1);
            fullProposal.Attachments.First().DocumentType.ShouldBe(EMCR.DRR.API.Model.DocumentType.SitePlan);
            fullProposal.Attachments.First().Comments = "site plan comments";

            await manager.Handle(new FpSaveApplicationCommand { Application = mapper.Map<FpApplication>(fullProposal), UserInfo = GetTestUserInfo() });

            var updatedFp = (await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();
            updatedFp.Attachments.First().Comments.ShouldBe("site plan comments");
            updatedFp.HaveResolution.ShouldBe(true);
        }

        [Test]
        public async Task CanDeleteAttachment()
        {
            var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();

            var eoi = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            eoi.Status = SubmissionPortalStatus.EligibleInvited;
            eoi.AuthorizedRepresentativeStatement = true;
            eoi.FOIPPAConfirmation = true;
            eoi.InformationAccuracyStatement = true;

            var eoiId = await manager.Handle(new EoiSubmitApplicationCommand { Application = eoi, UserInfo = userInfo });
            eoiId.ShouldNotBeEmpty();

            var screenerQuestions = CreateScreenerQuestions();
            screenerQuestions.FirstNationsAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable;
            screenerQuestions.LocalGovernmentAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable;
            screenerQuestions.EngagedWithFirstNationsOccurred = false;

            var fpId = await manager.Handle(new CreateFpFromEoiCommand { EoiId = eoiId, UserInfo = userInfo, ScreenerQuestions = screenerQuestions });
            fpId.ShouldNotBeEmpty();

            var body = DateTime.Now.ToString();
            var fileName = "autotest.txt";
            byte[] bytes = Encoding.ASCII.GetBytes(body);
            var file = new S3File { FileName = fileName, Content = bytes, ContentType = "text/plain", };

            var documentId = await manager.Handle(new UploadAttachmentCommand { AttachmentInfo = new AttachmentInfo { RecordId = fpId, RecordType = EMCR.DRR.Managers.Intake.RecordType.FullProposal, File = file, DocumentType = EMCR.DRR.Managers.Intake.DocumentType.SitePlan }, UserInfo = GetTestUserInfo() });
            var fullProposal = mapper.Map<DraftFpApplication>((await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            fullProposal.Attachments.Count().ShouldBe(1);

            await manager.Handle(new DeleteAttachmentCommand { Id = documentId, UserInfo = GetTestUserInfo() });

            var updatedFp = (await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = GetTestUserInfo().BusinessId })).Items.SingleOrDefault();
            updatedFp.Attachments.Count().ShouldBe(0);
        }

        [Test]
        public async Task AddAttachment_NoDocumentType_DefaultsToOther()
        {
            var userInfo = GetTestUserInfo();
            //var userInfo = GetCRAFTUserInfo();

            var eoi = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            eoi.Status = SubmissionPortalStatus.EligibleInvited;
            eoi.AuthorizedRepresentativeStatement = true;
            eoi.FOIPPAConfirmation = true;
            eoi.InformationAccuracyStatement = true;

            var eoiId = await manager.Handle(new EoiSubmitApplicationCommand { Application = eoi, UserInfo = userInfo });
            eoiId.ShouldNotBeEmpty();

            var screenerQuestions = CreateScreenerQuestions();
            screenerQuestions.FirstNationsAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable;
            screenerQuestions.LocalGovernmentAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable;
            screenerQuestions.EngagedWithFirstNationsOccurred = false;

            var fpId = await manager.Handle(new CreateFpFromEoiCommand { EoiId = eoiId, UserInfo = userInfo, ScreenerQuestions = screenerQuestions });
            fpId.ShouldNotBeEmpty();

            var body = DateTime.Now.ToString();
            var fileName = "autotest.txt";
            byte[] bytes = Encoding.ASCII.GetBytes(body);
            var file = new S3File { FileName = fileName, Content = bytes, ContentType = "text/plain", };

            var documentId = await manager.Handle(new UploadAttachmentCommand { AttachmentInfo = new AttachmentInfo { RecordId = fpId, RecordType = EMCR.DRR.Managers.Intake.RecordType.FullProposal, File = file }, UserInfo = GetTestUserInfo() });
            var fullProposal = mapper.Map<DraftFpApplication>((await manager.Handle(new DrrApplicationsQuery { Id = fpId, BusinessId = userInfo.BusinessId })).Items.SingleOrDefault());
            fullProposal.HaveResolution = true;
            fullProposal.Attachments.Count().ShouldBe(1);
            fullProposal.Attachments.First().DocumentType.ShouldBe(EMCR.DRR.API.Model.DocumentType.OtherSupportingDocument);
        }

        [Test]
        public async Task AddAttachment_ExistingDocumentType_ThrowsError()
        {
            var userInfo = GetTestUserInfo();

            var eoi = mapper.Map<EoiApplication>(CreateNewTestEOIApplication());
            eoi.Status = SubmissionPortalStatus.EligibleInvited;
            eoi.AuthorizedRepresentativeStatement = true;
            eoi.FOIPPAConfirmation = true;
            eoi.InformationAccuracyStatement = true;

            var eoiId = await manager.Handle(new EoiSubmitApplicationCommand { Application = eoi, UserInfo = userInfo });
            eoiId.ShouldNotBeEmpty();

            var screenerQuestions = CreateScreenerQuestions();
            screenerQuestions.FirstNationsAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable;
            screenerQuestions.LocalGovernmentAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable;
            screenerQuestions.EngagedWithFirstNationsOccurred = false;

            var fpId = await manager.Handle(new CreateFpFromEoiCommand { EoiId = eoiId, UserInfo = userInfo, ScreenerQuestions = screenerQuestions });
            fpId.ShouldNotBeEmpty();

            var body = DateTime.Now.ToString();
            var fileName = "autotest.txt";
            byte[] bytes = Encoding.ASCII.GetBytes(body);
            var file = new S3File { FileName = fileName, Content = bytes, ContentType = "text/plain", };

            var documentId = await manager.Handle(new UploadAttachmentCommand { AttachmentInfo = new AttachmentInfo { RecordId = fpId, RecordType = EMCR.DRR.Managers.Intake.RecordType.FullProposal, File = file, DocumentType = EMCR.DRR.Managers.Intake.DocumentType.SitePlan }, UserInfo = GetTestUserInfo() });
            Should.Throw<Exception>(() => manager.Handle(new UploadAttachmentCommand { AttachmentInfo = new AttachmentInfo { RecordId = fpId, RecordType = EMCR.DRR.Managers.Intake.RecordType.FullProposal, File = file, DocumentType = EMCR.DRR.Managers.Intake.DocumentType.SitePlan }, UserInfo = GetTestUserInfo() }));
        }

#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning restore CS8629 // Nullable value type may be null.
        private EMCR.DRR.Managers.Intake.ScreenerQuestions CreateScreenerQuestions()
        {
            return new EMCR.DRR.Managers.Intake.ScreenerQuestions
            {
                ProjectWorkplan = true,
                ProjectSchedule = true,
                CostEstimate = true,
                SitePlan = EMCR.DRR.Managers.Intake.YesNoOption.Yes,
                HaveAuthorityToDevelop = true,
                FirstNationsAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.Yes,
                LocalGovernmentAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.Yes,
                FoundationWorkCompleted = EMCR.DRR.Managers.Intake.YesNoOption.Yes,
                EngagedWithFirstNationsOccurred = true,
                IncorporateFutureClimateConditions = true,
                MeetsRegulatoryRequirements = true,
                MeetsEligibilityRequirements = true,
            };
        }

        private DraftEoiApplication CreateNewTestEOIApplication()
        {
            var uniqueSignature = TestPrefix + "-" + Guid.NewGuid().ToString().Substring(0, 4);
            return new DraftEoiApplication
            {
                Status = SubmissionPortalStatus.Draft,

                //Proponent Information - 1
                ProponentType = EMCR.DRR.Controllers.ProponentType.LocalGovernment,
                Submitter = CreateNewTestContact(uniqueSignature, "submitter"),
                ProjectContact = CreateNewTestContact(uniqueSignature, "proj"),
                AdditionalContacts = new[]
                {
                    CreateNewTestContact(uniqueSignature, "add1"),
                    CreateNewTestContact(uniqueSignature, "add2"),
                },
                PartneringProponents = new[]
                {
                    $"{uniqueSignature}_partner1",
                    $"{uniqueSignature}_partner2"
                },

                //Project Information - 2
                FundingStream = EMCR.DRR.Controllers.FundingStream.Stream1,
                ProjectTitle = "Project Title",
                Stream = EMCR.DRR.Controllers.ProjectType.New,
                ScopeStatement = "scope",
                RelatedHazards = new[]
                {
                    EMCR.DRR.Controllers.Hazards.Flood,
                    EMCR.DRR.Controllers.Hazards.Tsunami,
                    EMCR.DRR.Controllers.Hazards.Other
                },
                OtherHazardsDescription = "Other Description",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(14),

                //Funding Information - 3
                EstimatedTotal = 1000,
                FundingRequest = 100,
                HaveOtherFunding = true,
                OtherFunding = new[]
                {
                    new EMCR.DRR.Controllers.FundingInformation
                    {
                        Name = $"{uniqueSignature}_Self",
                        Amount = 100,
                        Type = EMCR.DRR.Controllers.FundingType.SelfFunding,
                    },
                    new EMCR.DRR.Controllers.FundingInformation
                    {
                        Name = $"{uniqueSignature}_Prov",
                        Amount = 200,
                        Type = EMCR.DRR.Controllers.FundingType.Prov,
                    },
                    new EMCR.DRR.Controllers.FundingInformation
                    {
                        Name = $"{uniqueSignature}_Other",
                        Amount = 300,
                        Type = EMCR.DRR.Controllers.FundingType.OtherGrants,
                        OtherDescription = "other funding reason"
                    },
                },
                RemainingAmount = 300,
                IntendToSecureFunding = "Funding Reasons",

                //Location Information - 4
                OwnershipDeclaration = false,
                OwnershipDescription = "ownership description",
                LocationDescription = "location description",

                //Project Detail - 5
                RationaleForFunding = "rationale for funding",
                EstimatedPeopleImpacted = EMCR.DRR.Controllers.EstimatedNumberOfPeople.OneToTenK,
                CommunityImpact = "community impact",
                IsInfrastructureImpacted = true,
                InfrastructureImpacted = new[] { new InfrastructureImpacted { Infrastructure = $"{uniqueSignature}_infrastructure1", Impact = "impact" } },
                DisasterRiskUnderstanding = "helps many people",
                AdditionalBackgroundInformation = "additional background info",
                AddressRisksAndHazards = "fix risks",
                ProjectDescription = "project description",
                AdditionalSolutionInformation = "additional solution info",
                RationaleForSolution = "rational for solution",

                //Engagement Plan - 6
                FirstNationsEngagement = "Engagement Proposal",
                NeighbourEngagement = "engage with neighbours",
                AdditionalEngagementInformation = "additional engagement info",

                //Other Supporting Information - 7
                ClimateAdaptation = "Climate Adaptation",
                OtherInformation = "Other Info",

                //Declaration - 8
                //InformationAccuracyStatement = true,
                //FOIPPAConfirmation = true,
                //AuthorizedRepresentativeStatement = true
            };
        }

        private DraftFpApplication FillInFullProposal(DraftFpApplication application)
        {
            //Proponent & Project Information - 1
            application.RegionalProject = true;
            application.RegionalProjectComments = "regional comments";
            application.MainDeliverable = "main deliverable";

            //Ownership & Authorization - 2
            application.HaveAuthorityToDevelop = true;
            application.OperationAndMaintenance = EMCR.DRR.Controllers.YesNoOption.Yes;
            application.OperationAndMaintenanceComments = "operation and maint. comments";
            application.FirstNationsAuthorizedByPartners = EMCR.DRR.Controllers.YesNoOption.No;
            application.LocalGovernmentAuthorizedByPartners = EMCR.DRR.Controllers.YesNoOption.NotApplicable;
            application.AuthorizationOrEndorsementComments = "authority or endorsement comments";

            //Project Area - 3
            application.Area = 123;
            application.Units = EMCR.DRR.Controllers.AreaUnits.Acres;
            application.AreaDescription = "area description";
            application.IsInfrastructureImpacted = true;
            application.EstimatedPeopleImpactedFP = EMCR.DRR.Controllers.EstimatedNumberOfPeopleFP.FiveHundredToOneK;

            //Project Plan - 4
#pragma warning disable CS8604 // Possible null reference argument.
            application.ProposedActivities = application.ProposedActivities.Concat(new[]
            {
                new EMCR.DRR.Controllers.ProposedActivity {Id = Guid.NewGuid().ToString(), StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5), Activity = EMCR.DRR.Controllers.ActivityType.Mapping, Deliverables = "mapping deliverable", Tasks = "mapping tasks" },
                new EMCR.DRR.Controllers.ProposedActivity {StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5), Activity = EMCR.DRR.Controllers.ActivityType.Construction, Deliverables = "construction deliverable", Tasks = "construction tasks" },
            });
#pragma warning restore CS8604 // Possible null reference argument.
            application.FoundationalOrPreviousWorks = new[] { "autotest-verification-method" };
            application.HowWasNeedIdentified = "need identified";
            application.ProjectAlternateOptions = "some alternate options";

            //Project Engagement - 5
            application.EngagedWithFirstNationsComments = "first nations comments";
            application.OtherEngagement = EMCR.DRR.Controllers.YesNoOption.Yes;
            application.AffectedParties = new[] { "party 1", "party 2" };
            application.OtherEngagementComments = "other engagement comments";
            application.CollaborationComments = "collaboration comments";

            //Climate Adaptation - 6
            application.IncorporateFutureClimateConditions = true;
            application.ClimateAssessment = true;
            application.ClimateAssessmentTools = new[] { "tool 1", "tool 2" };
            application.ClimateAssessmentComments = "climate assessment comments";

            //Permits Regulations & Standards - 7
            application.Permits = new[] { "permit 1", "permit 2" };
            application.StandardsAcceptable = EMCR.DRR.Controllers.YesNoOption.NotApplicable;
            application.Standards = new[] {
                new EMCR.DRR.Controllers.StandardInfo { IsCategorySelected = true, Category = "Environment - Water (includes Rivers, Flooding, etc.)", Standards = new [] { "BC Water Sustainability Act", "Water Survey Canada", "other water env standard" } },
                new EMCR.DRR.Controllers.StandardInfo { IsCategorySelected = true, Category = "Other", Standards = new [] { "other_standard1"} },
            };
            application.StandardsComments = "standards comments";
            application.ProfessionalGuidance = true;
            application.Professionals = new[] { "professional1", "professional2" };
            application.ProfessionalGuidanceComments = "professional guidance comments";
            application.KnowledgeHolders = "knowledge holders";
            application.MeetsRegulatoryRequirements = false;
            application.MeetsRegulatoryComments = "regulations comments";
            application.MeetsEligibilityRequirements = false;
            application.MeetsEligibilityComments = "eligibility comments";

            //Project Outcomes - 8
            application.PublicBenefit = false;
            application.PublicBenefitComments = "public benefit comments";
            application.FutureCostReduction = true;
            application.CostReductions = new[] { "cost reduction 1", "cost reduction 2" };
            application.CostReductionComments = "cost reduction comments";
            application.ProduceCoBenefits = true;
            application.CoBenefits = new[] { "benefit 1", "benefit 2" };
            application.CoBenefitComments = "benefit comments";
            application.IncreasedResiliency = new[] { "resiliency 1", "resiliency 2" };
            application.IncreasedResiliencyComments = "resiliency comments";

            //Project Risks - 9
            application.ComplexityRiskMitigated = true;
            application.ComplexityRisks = new[] { "complexity risk 1", "complexity risk 2" };
            application.ComplexityRiskComments = "risk comments";
            application.ReadinessRiskMitigated = true;
            application.ReadinessRisks = new[] { "readiness risk 1", "readiness risk 2" };
            application.ReadinessRiskComments = "readiness comments";
            application.SensitivityRiskMitigated = true;
            application.SensitivityRisks = new[] { "sensitivity risk 1", "sensitivity risk 2" };
            application.SensitivityRiskComments = "sensitivity comments";
            application.CapacityRiskMitigated = true;
            application.CapacityRisks = new[] { "capacity risk 1", "capacity risk 2" };
            application.CapacityRiskComments = "capacity comments";
            application.RiskTransferMigigated = true;
            application.IncreasedOrTransferred = new[] { EMCR.DRR.Controllers.IncreasedOrTransferred.Increased };
            application.IncreasedOrTransferredComments = "transfer comments";

            //Budget - 10
            application.TotalProjectCost = 1000;
            application.TotalProjectCostChangeComments = "project cost change comment";
            application.YearOverYearFunding = new[] { new EMCR.DRR.Controllers.YearOverYearFunding { Amount = 400, Year = "2024/2025" } };
            application.EligibleFundingRequest = 400;
            application.TotalDrifFundingRequest = 400;
            application.RemainingAmount = 0;
            application.DiscrepancyComment = "discrepancy comment";
            //application.CostEffective = false;
            application.IntendToSecureFunding = "intend to secure funding";
            application.CostEffectiveComments = "cost effective comments";
            application.PreviousResponse = EMCR.DRR.Controllers.YesNoOption.No;
            application.PreviousResponseCost = 1200;
            application.PreviousResponseComments = "previous response comments";
            application.CostConsiderationsApplied = false;
            application.CostConsiderations = new[] { "cost consideration 1", "cost consideration 2" };
            application.CostConsiderationsComments = "cost consideration comments";
            application.CostEstimates = application.CostEstimates?.Concat(new[] { new EMCR.DRR.Controllers.CostEstimate {
                TaskName = "cost estimate task 1",
                CostCategory = EMCR.DRR.Controllers.CostCategory.Communications,
                Description = "cost estimate description",
                Resources = EMCR.DRR.Controllers.ResourceCategory.ProjectSupport,
                Units = EMCR.DRR.Controllers.CostUnit.SquareKilometer,
                Quantity = 5,
                UnitRate = (decimal?)10.15,
                TotalCost = 50,
                }
            });
            application.CostEstimateClass = EMCR.DRR.Controllers.CostEstimateClass.ClassA;
            application.Contingency = 10;
            application.TotalEligibleCosts = 55;

            return application;
        }

        private DraftFpApplication ClearFullProposal(DraftFpApplication application)
        {
            //Proponent & Project Information - 1
            application.RegionalProject = null;
            application.RegionalProjectComments = null;
            application.MainDeliverable = string.Empty;

            //Ownership & Authorization - 2
            application.HaveAuthorityToDevelop = null;
            application.OperationAndMaintenance = null;
            application.OperationAndMaintenanceComments = string.Empty;
            application.FirstNationsAuthorizedByPartners = null;
            application.LocalGovernmentAuthorizedByPartners = null;
            application.AuthorizationOrEndorsementComments = string.Empty;

            //Project Area - 3
            application.Area = null;
            application.Units = null;
            application.RelatedHazards = Array.Empty<EMCR.DRR.Controllers.Hazards>();
            application.AreaDescription = string.Empty;
            application.IsInfrastructureImpacted = null;
            application.EstimatedPeopleImpactedFP = null;
            application.InfrastructureImpacted = Array.Empty<InfrastructureImpacted>();

            //Project Plan - 4
            application.StartDate = null;
            application.EndDate = null;
            application.ProposedActivities = Array.Empty<EMCR.DRR.Controllers.ProposedActivity>();
            application.FoundationalOrPreviousWorks = Array.Empty<string>();
            application.HowWasNeedIdentified = string.Empty;
            application.ProjectAlternateOptions = string.Empty;

            //Project Engagement - 5
            application.EngagedWithFirstNationsComments = string.Empty;
            application.OtherEngagement = null;
            application.AffectedParties = Array.Empty<string>();
            application.OtherEngagementComments = string.Empty;
            application.CollaborationComments = string.Empty;

            //Climate Adaptation - 6
            application.IncorporateFutureClimateConditions = null;
            application.ClimateAssessment = null;
            application.ClimateAssessmentTools = Array.Empty<string>();
            application.ClimateAssessmentComments = string.Empty;

            //Permits Regulations & Standards - 7
            application.StandardsAcceptable = null;
            application.Standards = Array.Empty<EMCR.DRR.Controllers.StandardInfo>();
            application.StandardsComments = string.Empty;
            application.ProfessionalGuidance = null;
            application.Professionals = Array.Empty<string>();
            application.ProfessionalGuidanceComments = string.Empty;
            application.KnowledgeHolders = string.Empty;
            application.MeetsRegulatoryRequirements = null;
            application.MeetsRegulatoryComments = string.Empty;
            application.MeetsEligibilityRequirements = null;
            application.MeetsEligibilityComments = string.Empty;

            //Project Outcomes - 8
            application.PublicBenefit = null;
            application.PublicBenefitComments = string.Empty;
            application.FutureCostReduction = null;
            application.CostReductions = Array.Empty<string>();
            application.CostReductionComments = string.Empty;
            application.ProduceCoBenefits = null;
            application.CoBenefits = Array.Empty<string>();
            application.CoBenefitComments = string.Empty;
            application.IncreasedResiliency = Array.Empty<string>();
            application.IncreasedResiliencyComments = string.Empty;

            //Project Risks - 9
            application.ComplexityRiskMitigated = null;
            application.ComplexityRisks = Array.Empty<string>();
            application.ComplexityRiskComments = string.Empty;
            application.ReadinessRiskMitigated = null;
            application.ReadinessRisks = Array.Empty<string>();
            application.ReadinessRiskComments = string.Empty;
            application.SensitivityRiskMitigated = null;
            application.SensitivityRisks = Array.Empty<string>();
            application.SensitivityRiskComments = string.Empty;
            application.CapacityRiskMitigated = null;
            application.CapacityRisks = Array.Empty<string>();
            application.CapacityRiskComments = string.Empty;
            application.RiskTransferMigigated = null;
            application.IncreasedOrTransferred = Array.Empty<EMCR.DRR.Controllers.IncreasedOrTransferred>();
            application.IncreasedOrTransferredComments = string.Empty;

            //Budget - 10
            application.TotalProjectCost = null;
            application.YearOverYearFunding = Array.Empty<EMCR.DRR.Controllers.YearOverYearFunding>();
            //application.TotalDrifFundingRequest = 5000;
            //application.RemainingAmount = 4300;
            application.DiscrepancyComment = string.Empty;
            //application.CostEffective = false;
            application.IntendToSecureFunding = string.Empty;
            application.CostEffectiveComments = string.Empty;
            application.PreviousResponse = null;
            application.PreviousResponseCost = null;
            application.PreviousResponseComments = string.Empty;
            application.CostConsiderationsApplied = true;
            application.CostConsiderations = Array.Empty<string>();
            application.CostConsiderationsComments = string.Empty;

            return application;
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
    }
}
