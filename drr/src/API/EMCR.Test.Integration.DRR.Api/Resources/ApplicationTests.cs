using EMCR.DRR.API.Resources.Cases;
using EMCR.DRR.Managers.Intake;
using EMCR.DRR.Resources.Applications;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EMCR.Tests.Integration.DRR.Resources
{
    public class ApplicationTests
    {
        private string TestPrefix = "autotest-dev";
        private string TestBusinessId = "autotest-dev-business-bceid";
        //private string TestUserId = "autotest-dev-user-bceid";
        private readonly IApplicationRepository applicationRepository;
        private readonly ICaseRepository caseRepository;

        public ApplicationTests()
        {
            var host = Application.Host;
            applicationRepository = host.Services.GetRequiredService<IApplicationRepository>();
            caseRepository = host.Services.GetRequiredService<ICaseRepository>();
        }

        [Test]
        public async Task CanCreateEOIApplication()
        {
            var originalApplication = CreateTestEOIApplication();
            var id = (await applicationRepository.Manage(new SaveApplication { Application = originalApplication })).Id;
            id.ShouldNotBeEmpty();

            var newApplication = (await applicationRepository.Query(new ApplicationsQuery { Id = id })).Items.ShouldHaveSingleItem();
            newApplication.ProjectTitle.ShouldNotBeEmpty();
            //verify submitter
            //verify project contact
            //verify additonal contacts
            //verify partnering proponents
            //verify funding sourcres
            //verify infrastructure
        }

        [Test]
        public async Task CanUpdateEOIApplication()
        {
            var originalApplication = CreateTestEOIApplication();
            var id = (await applicationRepository.Manage(new SaveApplication { Application = originalApplication })).Id;
            id.ShouldNotBeEmpty();

            var applicationToUpdate = (await applicationRepository.Query(new ApplicationsQuery { Id = id })).Items.ShouldHaveSingleItem();
            applicationToUpdate.ProjectTitle.ShouldNotBeEmpty();
            var currPrefix = applicationToUpdate.PartneringProponents.First().Name;
            currPrefix = currPrefix.Substring(0, currPrefix.IndexOf("_"));

            applicationToUpdate.PartneringProponents = new[]
                {
                   new PartneringProponent { Name = $"{currPrefix}_updated_partner1" },
                   new PartneringProponent { Name = $"{currPrefix}_updated_partner2" },
                };

            applicationToUpdate.InfrastructureImpacted = new[]
                {
                    new CriticalInfrastructure {Name= $"{currPrefix}_updated_infrastructure1", Impact = "updated impact" },
                    new CriticalInfrastructure {Name= $"{currPrefix}_updated_infrastructure2", Impact = "updated impact" },
                };

            await applicationRepository.Manage(new SaveApplication { Application = applicationToUpdate });

            var updatedApplication = (await applicationRepository.Query(new ApplicationsQuery { Id = id })).Items.ShouldHaveSingleItem();
            updatedApplication.InfrastructureImpacted.First().Name.ShouldContain("updated");
            updatedApplication.InfrastructureImpacted.Count().ShouldBe(2);
            updatedApplication.PartneringProponents.First().Name.ShouldContain("updated");
            updatedApplication.PartneringProponents.Count().ShouldBe(2);
            //verify submitter
            //verify project contact
            //verify additonal contacts
            //verify partnering proponents
            //verify funding sourcres
            //verify infrastructure
        }

        [Test]
        public async Task CanCreateFPApplication()
        {
            var originalApplication = CreateTestEOIApplication();
            originalApplication.SubmittedDate = DateTime.UtcNow;
            originalApplication.Status = ApplicationStatus.Invited;
            var eoiId = (await applicationRepository.Manage(new SaveApplication { Application = originalApplication })).Id;
            eoiId.ShouldNotBeEmpty();

            var eoiApplication = (await applicationRepository.Query(new ApplicationsQuery { Id = eoiId })).Items.ShouldHaveSingleItem();
            eoiApplication.ProjectTitle.ShouldNotBeEmpty();

            var fpId = (await caseRepository.Manage(new GenerateFpFromEoi { EoiId = eoiId, ScreenerQuestions = CreateScreenerQuestions() })).Id;
            var fpApplication = (await applicationRepository.Query(new ApplicationsQuery { Id = fpId })).Items.ShouldHaveSingleItem();
            fpApplication.ProjectTitle.ShouldNotBeEmpty();

            eoiApplication = (await applicationRepository.Query(new ApplicationsQuery { Id = eoiId })).Items.ShouldHaveSingleItem();
            eoiApplication.FpId.ShouldNotBeEmpty();
        }

        [Test]
        public async Task CanUpdateFPApplication()
        {
            var originalEOI = CreateTestEOIApplication();
            originalEOI.SubmittedDate = DateTime.UtcNow;
            originalEOI.Status = ApplicationStatus.Invited;
            var eoiId = (await applicationRepository.Manage(new SaveApplication { Application = originalEOI })).Id;
            eoiId.ShouldNotBeEmpty();

            var eoiApplication = (await applicationRepository.Query(new ApplicationsQuery { Id = eoiId })).Items.ShouldHaveSingleItem();
            eoiApplication.ProjectTitle.ShouldNotBeEmpty();

            var fpId = (await caseRepository.Manage(new GenerateFpFromEoi { EoiId = eoiId, ScreenerQuestions = CreateScreenerQuestions() })).Id;
            var fpApplication = (await applicationRepository.Query(new ApplicationsQuery { Id = fpId })).Items.ShouldHaveSingleItem();
            fpApplication.ProjectTitle.ShouldNotBeEmpty();

            fpApplication.Standards = new[]
            {
                new StandardInfo { Category = "Other", Standards = new[] {
                    new ProvincialStandard {Name = "Test 1"}, new ProvincialStandard {Name = "Some custom standard"}
                }},
            };

            await applicationRepository.Manage(new SaveApplication { Application = fpApplication });

            var updatedFpApplication = (await applicationRepository.Query(new ApplicationsQuery { Id = fpId })).Items.ShouldHaveSingleItem();
            updatedFpApplication.Standards.ShouldNotBeEmpty();
            updatedFpApplication.Standards.FirstOrDefault(s => s.Category == "Other").ShouldNotBeNull();
        }

        [Test]
        public async Task CanQueryDeclarations()
        {
            var declarations = (await applicationRepository.Query(new EMCR.DRR.Resources.Applications.DeclarationQuery { })).Items;
            declarations.ShouldNotBeEmpty();
        }

        [Test]
        public async Task CanQueryApplications()
        {
            var applications = (await applicationRepository.Query(new ApplicationsQuery { BusinessId = TestBusinessId })).Items;
            applications.ShouldNotBeEmpty();
        }

        private ScreenerQuestions CreateScreenerQuestions()
        {
            return new ScreenerQuestions
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

        private EMCR.DRR.Managers.Intake.Application CreateTestEOIApplication()
        {
            var uniqueSignature = TestPrefix + "-" + Guid.NewGuid().ToString().Substring(0, 4);
            return new EMCR.DRR.Managers.Intake.Application
            {
                ApplicationTypeName = "EOI",
                ProgramName = "DRIF",
                BCeIDBusinessId = TestBusinessId,
                Status = ApplicationStatus.DraftProponent,
                //Proponent Information
                ProponentType = ProponentType.LocalGovernment,
                ProponentName = $"{uniqueSignature}_applicant_name",
                AuthorizedRepresentative = CreateTestContact(uniqueSignature, "submitter", "user-bceid"),
                ProjectContact = CreateTestContact(uniqueSignature, "proj"),
                AdditionalContact1 = CreateTestContact(uniqueSignature, "add1"),
                //AdditionalContact2 = CreateTestContact(uniqueSignature),
                PartneringProponents = new[]
                {
                   new PartneringProponent { Name = $"{uniqueSignature}_partner1" },
                   new PartneringProponent { Name = $"{uniqueSignature}_partner2" },
                },

                //Project Information
                FundingStream = FundingStream.Stream1,
                ProjectTitle = "Project Title",
                ProjectType = ProjectType.New,
                ScopeStatement = "scope",
                RelatedHazards = new[]
                {
                    Hazards.Flood,
                    Hazards.Tsunami,
                    Hazards.Other
                },
                OtherHazardsDescription = "Other Description",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(14),

                //Funding Information
                EstimatedTotal = 1000,
                FundingRequest = 100,
                HaveOtherFunding = true,
                OtherFunding = new[]
                {
                    new FundingInformation
                    {
                        Name = $"{uniqueSignature}_self",
                        Amount = 100,
                        Type = FundingType.SelfFunding,
                    },
                    new FundingInformation
                    {
                        Name = $"{uniqueSignature}_prov",
                        Amount = 200,
                        Type = FundingType.Prov,
                    },
                    new FundingInformation
                    {
                        Name = $"{uniqueSignature}_other",
                        Amount = 300,
                        Type = FundingType.Other,
                        OtherDescription = "reason for other funding"
                    },
                },
                RemainingAmount = 600,
                IntendToSecureFunding = "Funding Reasons",

                //Location Information
                OwnershipDeclaration = true,
                OwnershipDescription = "owned",
                LocationDescription = "location description",

                //Project Detail
                RationaleForFunding = "rationale for funding",
                EstimatedPeopleImpacted = EstimatedNumberOfPeople.OneToTenK,
                CommunityImpact = "community impact",
                InfrastructureImpacted = new[]
                {
                    new CriticalInfrastructure {Name= $"{uniqueSignature}_infrastructure1", Impact = "impact" },
                    new CriticalInfrastructure {Name= $"{uniqueSignature}_infrastructure2", Impact = "impact" },
                },
                DisasterRiskUnderstanding = "helps many people",
                AdditionalBackgroundInformation = "additional background info",
                AddressRisksAndHazards = "fix risks",
                ProjectDescription = "project description",
                AdditionalSolutionInformation = "additional solution info",
                RationaleForSolution = "rational for solution",

                //Engagement Plan
                FirstNationsEngagement = "Engagement Proposal",
                NeighbourEngagement = "engage with neighbours",
                AdditionalEngagementInformation = "additional engagement info",

                //Other Supporting Information
                ClimateAdaptation = "Climate Adaptation",
                OtherInformation = "Other Info",

                //Declaration
                InformationAccuracyStatement = true,
                FOIPPAConfirmation = true,
                AuthorizedRepresentativeStatement = true
            };
        }

        private ContactDetails CreateTestContact(string uniqueSignature, string namePrefix, string? BCeId = null)
        {
            return new ContactDetails
            {
                BCeId = string.IsNullOrEmpty(BCeId) ? null : $"{uniqueSignature}_{BCeId}",
                FirstName = $"{uniqueSignature}_{namePrefix}_first",
                LastName = $"{uniqueSignature}_{namePrefix}_last",
                Email = "test@test.com",
                Phone = "6041234567",
                Department = "Position",
                Title = "Title"
            };
        }
    }


}
