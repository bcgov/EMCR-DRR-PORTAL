using Bogus;
using EMCR.DRR.Controllers;
using EMCR.Utilities.Extensions;

namespace EMCR.DRR.API.Utilities.TestData
{
    public static class BogusExtensions
    {
        //private static readonly string prefix = "autotest-";

#pragma warning disable CS8629 // Nullable value type may be null.
        public static Faker<DraftEoiApplication> WithApplicationRules(this Faker<DraftEoiApplication> faker, string prefix, ContactDetails? submitter = null)
        {
            return faker
            .RuleFor(a => a.Id, f => null)
            .RuleFor(a => a.Status, f => EMCR.DRR.API.Model.SubmissionPortalStatus.Draft)

            //Proponent Information - 1
            .RuleFor(a => a.ProponentType, f => f.Random.Enum<ProponentType>())
            .RuleFor(a => a.AuthorizedRepresentative, f => submitter ?? new Faker<ContactDetails>("en_CA").WithContactDetailsRules(prefix).Generate())
            .RuleFor(a => a.ProjectContact, f => new Faker<ContactDetails>("en_CA").WithContactDetailsRules(prefix).Generate())
            .RuleFor(a => a.AdditionalContacts, f =>
            {
                var contactFaker = new Faker<ContactDetails>("en_CA");
                return contactFaker.WithContactDetailsRules(prefix).GenerateBetween(1, 2);
            })
            .RuleFor(a => a.PartneringProponents, f => Enumerable.Range(0, f.Random.Int(0, 5)).Select(x => f.Company.CompanyName()).ToList())

            //Project Information - 2
            .RuleFor(a => a.FundingStream, f => f.Random.Enum<FundingStream>())
            .RuleFor(a => a.ProjectTitle, f => prefix + f.Name.JobArea())
            .RuleFor(a => a.Stream, f => f.Random.Enum<ProjectType>())
            .RuleFor(a => a.ScopeStatement, f => f.Lorem.Sentence())
            .RuleFor(a => a.RelatedHazards, f => Enumerable.Range(1, f.Random.Int(1, 8)).Select(x => f.Random.Enum<Hazards>()).Distinct().ToList())
            .RuleFor(a => a.OtherHazardsDescription, f => f.Lorem.Sentence())
            .RuleFor(a => a.StartDate, f => DateTime.UtcNow)
            .RuleFor(a => a.EndDate, f => DateTime.UtcNow.AddDays(f.Random.Number(5, 60)))

            //Funding Information - 3
            .RuleFor(a => a.EstimatedTotal, f => f.Random.Number(10, 1000) * 1000)
            .RuleFor(a => a.FundingRequest, (f, a) => f.Random.Number(10, (int)a.EstimatedTotal.Value / 1000) * 1000)
            .RuleFor(a => a.HaveOtherFunding, f => f.Random.Bool())
            //.RuleFor(a => a.HaveOtherFunding, f => true)
            .RuleFor(a => a.OtherFunding, (f, a) => CreateOtherFunding(f, (bool)a.HaveOtherFunding, f.Random.Number(0, (int)(a.EstimatedTotal - a.FundingRequest)), prefix))
            .RuleFor(a => a.RemainingAmount, (f, a) => a.EstimatedTotal - (a.FundingRequest + a.OtherFunding.Select(fund => fund.Amount).Sum()))
            .RuleFor(a => a.IntendToSecureFunding, f => f.Lorem.Sentence())

            //Location Information - 4
            .RuleFor(a => a.OwnershipDeclaration, f => f.Random.Bool())
            .RuleFor(a => a.OwnershipDescription, f => f.Lorem.Sentence())
            .RuleFor(a => a.LocationDescription, f => f.Lorem.Sentence())

            //Project Detail - 5
            .RuleFor(a => a.RationaleForFunding, f => f.Lorem.Sentence())
            .RuleFor(a => a.EstimatedPeopleImpacted, f => f.Random.Enum<EstimatedNumberOfPeople>())
            .RuleFor(a => a.CommunityImpact, f => f.Lorem.Sentence())
            .RuleFor(a => a.IsInfrastructureImpacted, f => f.Random.Bool())
            .RuleFor(a => a.InfrastructureImpacted, (f, a) => a.IsInfrastructureImpacted == true ? new Faker<InfrastructureImpacted>("en_CA").WithInfrastructureImpactedRules(prefix).GenerateBetween(1, 5) : Array.Empty<InfrastructureImpacted>())
            .RuleFor(a => a.DisasterRiskUnderstanding, f => f.Lorem.Sentence())
            .RuleFor(a => a.AdditionalBackgroundInformation, f => f.Lorem.Sentence())
            .RuleFor(a => a.AddressRisksAndHazards, f => f.Lorem.Sentence())
            .RuleFor(a => a.ProjectDescription, f => f.Lorem.Sentence())
            .RuleFor(a => a.AdditionalSolutionInformation, f => f.Lorem.Sentence())
            .RuleFor(a => a.RationaleForSolution, f => f.Lorem.Sentence())

            //Engagement Plan - 6
            .RuleFor(a => a.FirstNationsEngagement, f => f.Lorem.Sentence())
            .RuleFor(a => a.NeighbourEngagement, f => f.Lorem.Sentence())
            .RuleFor(a => a.AdditionalEngagementInformation, f => f.Lorem.Sentence())

            //Other Supporting Information - 7
            .RuleFor(a => a.ClimateAdaptation, f => f.Lorem.Sentence())
            .RuleFor(a => a.OtherInformation, f => f.Lorem.Sentence())
            ;
        }

        public static Faker<DraftFpApplication> WithApplicationRules(this Faker<DraftFpApplication> faker, string prefix, DraftFpApplication? originalFp = null, ContactDetails? submitter = null)
        {
            var otherFundingTotal = originalFp != null ? originalFp.OtherFunding.Select(f => f.Amount).Sum() : 0;
            var professionalOptions = new[]
            {
                "British Columbia Land Surveyor",
                "Professional agrologist",
                "Professional archaeologist",
                "Professional engineer",
                "Professional forester / forest technologist",
                "Professional geoscientist"
            };
            return faker
            .RuleFor(a => a.Id, f => null)
            .RuleFor(a => a.EoiId, f => null)
            .RuleFor(a => a.Status, f => EMCR.DRR.API.Model.SubmissionPortalStatus.Draft)
            .RuleFor(a => a.AuthorizedRepresentative, f => submitter ?? new Faker<ContactDetails>("en_CA").WithContactDetailsRules(prefix).Generate())

            //Proponent & Project Information - 1
            .RuleFor(a => a.ProjectContact, f => new Faker<ContactDetails>("en_CA").WithContactDetailsRules(prefix).Generate())
            .RuleFor(a => a.AdditionalContacts, f =>
            {
                var contactFaker = new Faker<ContactDetails>("en_CA");
                return contactFaker.WithContactDetailsRules(prefix).GenerateBetween(1, 2);
            })
            .RuleFor(a => a.PartneringProponents, f => Enumerable.Range(0, f.Random.Int(0, 5)).Select(x => f.Company.CompanyName()).ToList())
            .RuleFor(a => a.RegionalProject, f => f.Random.Bool())
            .RuleFor(a => a.RegionalProjectComments, (f, a) => a.RegionalProject == true ? f.Lorem.Sentence() : null)
            .RuleFor(a => a.MainDeliverable, f => f.Lorem.Sentence())


            //Ownership & Authorization - 2
            .RuleFor(a => a.OwnershipDeclaration, f => f.Random.Bool())
            .RuleFor(a => a.OwnershipDescription, f => f.Lorem.Sentence())
            .RuleFor(a => a.HaveAuthorityToDevelop, f => f.Random.Bool())
            .RuleFor(a => a.OperationAndMaintenance, f => f.Random.Enum<YesNoOption>())
            .RuleFor(a => a.OperationAndMaintenanceComments, (f, a) => f.Lorem.Sentence())
            .RuleFor(a => a.FirstNationsAuthorizedByPartners, f => f.Random.Enum<YesNoOption>())
            .RuleFor(a => a.LocalGovernmentAuthorizedByPartners, f => f.Random.Enum<YesNoOption>())
            .RuleFor(a => a.AuthorizationOrEndorsementComments, f => f.Lorem.Sentence())


            //Project Area - 3
            .RuleFor(a => a.LocationDescription, f => f.Lorem.Sentence())
            .RuleFor(a => a.Area, f => f.Random.Number(0, 1000))
            .RuleFor(a => a.Units, f => f.Random.Enum<AreaUnits>())
            .RuleFor(a => a.RelatedHazards, f => Enumerable.Range(1, f.Random.Int(1, 8)).Select(x => f.Random.Enum<Hazards>()).Distinct().ToList())
            .RuleFor(a => a.OtherHazardsDescription, f => f.Lorem.Sentence())
            .RuleFor(a => a.AreaDescription, f => f.Lorem.Sentence())
            .RuleFor(a => a.CommunityImpact, f => f.Lorem.Sentence())
            .RuleFor(a => a.EstimatedPeopleImpactedFP, f => f.Random.Enum<EstimatedNumberOfPeopleFP>())
            .RuleFor(a => a.IsInfrastructureImpacted, f => f.Random.Bool())
            .RuleFor(a => a.InfrastructureImpacted, (f, a) => a.IsInfrastructureImpacted == true ? new Faker<InfrastructureImpacted>("en_CA").WithInfrastructureImpactedRules(prefix).GenerateBetween(1, 5) : Array.Empty<InfrastructureImpacted>())


            //Project Plan - 4
            .RuleFor(a => a.StartDate, f => DateTime.UtcNow)
            .RuleFor(a => a.EndDate, f => DateTime.UtcNow.AddDays(f.Random.Number(5, 60)))
            .RuleFor(a => a.ProjectDescription, f => f.Lorem.Sentence())
            .RuleFor(a => a.ProposedActivities, (f, a) => new Faker<ProposedActivity>("en_CA").WithProposedActivityRules().GenerateBetween(1, 5))
            .RuleFor(a => a.FoundationalOrPreviousWorks, f => Enumerable.Range(1, f.Random.Int(1, 5)).Select(x => f.Lorem.Word()).ToList())
            .RuleFor(a => a.HowWasNeedIdentified, f => f.Lorem.Sentence())
            .RuleFor(a => a.AddressRisksAndHazards, f => f.Lorem.Sentence())
            .RuleFor(a => a.DisasterRiskUnderstanding, f => f.Lorem.Sentence())
            .RuleFor(a => a.RationaleForFunding, f => f.Lorem.Sentence())
            .RuleFor(a => a.ProjectAlternateOptions, f => f.Lorem.Sentence())

            //Project Engagement - 5
            .RuleFor(a => a.EngagedWithFirstNationsOccurred, f => f.Random.Bool())
            .RuleFor(a => a.EngagedWithFirstNationsComments, (f, a) => f.Lorem.Sentence())
            .RuleFor(a => a.OtherEngagement, f => f.Random.Enum<YesNoOption>())
            .RuleFor(a => a.AffectedParties, (f, a) => a.OtherEngagement == YesNoOption.Yes ? Enumerable.Range(0, f.Random.Int(1, 5)).Select(x => f.Company.CompanyName()).ToList() : null)
            .RuleFor(a => a.OtherEngagementComments, f => f.Lorem.Sentence())
            .RuleFor(a => a.CollaborationComments, f => f.Lorem.Sentence())


            //Climate Adaptation - 6
            .RuleFor(a => a.IncorporateFutureClimateConditions, f => f.Random.Bool())
            .RuleFor(a => a.ClimateAdaptation, f => f.Lorem.Sentence())
            .RuleFor(a => a.ClimateAssessment, f => f.Random.Bool())
            .RuleFor(a => a.ClimateAssessmentTools, (f, a) => a.ClimateAssessment == true ? Enumerable.Range(0, f.Random.Int(1, 5)).Select(x => f.Lorem.Word()).ToList() : null)
            .RuleFor(a => a.ClimateAssessmentComments, (f, a) => a.ClimateAssessment == true ? f.Lorem.Sentence() : null)


            //Permits Regulations & Standards - 7
            .RuleFor(a => a.Permits, f => Enumerable.Range(0, f.Random.Int(0, 5)).Select(x => f.Lorem.Word()).ToList())
            .RuleFor(a => a.StandardsAcceptable, f => f.Random.Enum<YesNoOption>())
            .RuleFor(a => a.Standards, (f, a) => new Faker<StandardInfo>("en_CA").WithStandardInfoRules().GenerateBetween(1, 5))
            .RuleFor(a => a.StandardsComments, f => f.Lorem.Sentence())
            .RuleFor(a => a.ProfessionalGuidance, f => f.Random.Bool())
            .RuleFor(a => a.Professionals, (f, a) => a.ProfessionalGuidance == true ? (Enumerable.Range(0, f.Random.Int(1, 5)).Select(x => f.PickRandom(professionalOptions))).Distinct().ToList() : null)
            .RuleFor(a => a.ProfessionalGuidanceComments, f => f.Lorem.Sentence())
            .RuleFor(a => a.KnowledgeHolders, f => f.Lorem.Sentence())
            .RuleFor(a => a.MeetsRegulatoryRequirements, f => f.Random.Bool())
            .RuleFor(a => a.MeetsRegulatoryComments, (f, a) => a.MeetsRegulatoryRequirements == true ? f.Lorem.Sentence() : null)
            .RuleFor(a => a.MeetsEligibilityRequirements, f => f.Random.Bool())
            .RuleFor(a => a.MeetsEligibilityComments, (f, a) => a.MeetsEligibilityRequirements == true ? f.Lorem.Sentence() : null)


            //Project Outcomes - 8
            .RuleFor(a => a.PublicBenefit, f => f.Random.Bool())
            .RuleFor(a => a.PublicBenefitComments, f => f.Lorem.Sentence())
            .RuleFor(a => a.FutureCostReduction, f => f.Random.Bool())
            .RuleFor(a => a.CostReductions, (f, a) => a.FutureCostReduction == true ? Enumerable.Range(0, f.Random.Int(1, 5)).Select(x => f.Lorem.Word()).ToList() : null)
            .RuleFor(a => a.CostReductionComments, (f, a) => a.FutureCostReduction == true ? f.Lorem.Sentence() : null)
            .RuleFor(a => a.ProduceCoBenefits, f => f.Random.Bool())
            .RuleFor(a => a.CoBenefits, (f, a) => a.ProduceCoBenefits == true ? Enumerable.Range(0, f.Random.Int(1, 5)).Select(x => f.Lorem.Word()).ToList() : null)
            .RuleFor(a => a.CoBenefitComments, (f, a) => a.ProduceCoBenefits == true ? f.Lorem.Sentence() : null)
            .RuleFor(a => a.IncreasedResiliency, f => Enumerable.Range(1, f.Random.Int(1, 5)).Select(x => f.Lorem.Word()).ToList())
            .RuleFor(a => a.IncreasedResiliencyComments, f => f.Lorem.Sentence())


            //Project Risks - 9
            .RuleFor(a => a.ComplexityRiskMitigated, f => f.Random.Bool())
            .RuleFor(a => a.ComplexityRisks, (f, a) => a.ComplexityRiskMitigated == true ? Enumerable.Range(0, f.Random.Int(1, 5)).Select(x => f.Lorem.Word()).ToList() : null)
            .RuleFor(a => a.ComplexityRiskComments, (f, a) => a.ComplexityRiskMitigated == true ? f.Lorem.Sentence() : null)
            .RuleFor(a => a.ReadinessRiskMitigated, f => f.Random.Bool())
            .RuleFor(a => a.ReadinessRisks, (f, a) => a.ReadinessRiskMitigated == true ? Enumerable.Range(0, f.Random.Int(1, 5)).Select(x => f.Lorem.Word()).ToList() : null)
            .RuleFor(a => a.ReadinessRiskComments, (f, a) => a.ReadinessRiskMitigated == true ? f.Lorem.Sentence() : null)
            .RuleFor(a => a.SensitivityRiskMitigated, f => f.Random.Bool())
            .RuleFor(a => a.SensitivityRisks, (f, a) => a.SensitivityRiskMitigated == true ? Enumerable.Range(0, f.Random.Int(1, 5)).Select(x => f.Lorem.Word()).ToList() : null)
            .RuleFor(a => a.SensitivityRiskComments, (f, a) => a.SensitivityRiskMitigated == true ? f.Lorem.Sentence() : null)
            .RuleFor(a => a.CapacityRiskMitigated, f => f.Random.Bool())
            .RuleFor(a => a.CapacityRisks, (f, a) => a.CapacityRiskMitigated == true ? Enumerable.Range(0, f.Random.Int(1, 5)).Select(x => f.Lorem.Word()).ToList() : null)
            .RuleFor(a => a.CapacityRiskComments, (f, a) => a.CapacityRiskMitigated == true ? f.Lorem.Sentence() : null)
            .RuleFor(a => a.RiskTransferMigigated, f => f.Random.Bool())
            .RuleFor(a => a.IncreasedOrTransferred, (f, a) => a.RiskTransferMigigated == true ? Enumerable.Range(1, f.Random.Int(1, 2)).Select(x => f.Random.Enum<IncreasedOrTransferred>()).Distinct().ToList() : null)
            .RuleFor(a => a.IncreasedOrTransferredComments, (f, a) => a.RiskTransferMigigated == true ? f.Lorem.Sentence() : null)


            //Budget - 10
            .RuleFor(a => a.FundingStream, f => f.Random.Enum<FundingStream>())
            .RuleFor(a => a.TotalProjectCost, f => originalFp != null && originalFp.TotalProjectCost != null ? originalFp.TotalProjectCost - otherFundingTotal : f.Random.Number(10, 1000) * 1000)
            .RuleFor(a => a.TotalProjectCostChangeComments, f => f.Lorem.Sentence())
            .RuleFor(a => a.TotalDrifFundingRequest, (f, a) => a.TotalProjectCost)
            .RuleFor(a => a.EligibleFundingRequest, (f, a) => a.TotalProjectCost)
            .RuleFor(a => a.RemainingAmount, (f, a) => a.EligibleFundingRequest - a.TotalDrifFundingRequest)
            .RuleFor(a => a.YearOverYearFunding, (f, a) => CreateYearOverYearFunding(f, (int)a.EligibleFundingRequest))
            .RuleFor(a => a.DiscrepancyComment, (f, a) => a.RemainingAmount > 0 ? f.Lorem.Sentence() : null)
            .RuleFor(a => a.IntendToSecureFunding, f => f.Lorem.Sentence())
            .RuleFor(a => a.CostEffectiveComments, f => f.Lorem.Sentence())
            .RuleFor(a => a.PreviousResponse, f => f.Random.Enum<YesNoOption>())
            .RuleFor(a => a.PreviousResponseCost, (f, a) => a.PreviousResponse == YesNoOption.Yes ? f.Random.Number(10, 1000) * 10 : null)
            .RuleFor(a => a.PreviousResponseComments, (f, a) => f.Lorem.Sentence())
            .RuleFor(a => a.CostConsiderationsApplied, f => f.Random.Bool())
            .RuleFor(a => a.CostConsiderations, (f, a) => a.CostConsiderationsApplied == true ? Enumerable.Range(0, f.Random.Int(1, 5)).Select(x => f.Lorem.Word()).ToList() : null)
            .RuleFor(a => a.CostConsiderationsComments, (f, a) => a.CostConsiderationsApplied == true ? f.Lorem.Sentence() : null)
            .RuleFor(a => a.CostEstimateClass, f => f.Random.Enum<CostEstimateClass>())
            .RuleFor(a => a.CostEstimates, (f, a) => CreateCostEstimates(f, (int)a.EligibleFundingRequest, a.FundingStream.Value))
            .RuleFor(a => a.EstimatesMatchFundingRequest, f => false)
            .RuleFor(a => a.Contingency, f => f.Random.Int(0, 25))
            .RuleFor(a => a.ContingencyComments, f => f.Lorem.Sentence())
            .RuleFor(a => a.TotalEligibleCosts, (f, a) => a.TotalDrifFundingRequest)


            //Attachments - 11
            .RuleFor(a => a.HaveResolution, f => f.Random.Bool())
            .RuleFor(a => a.Attachments, f => [])

            ;
        }

        public static Faker<Invoice> WithInvoiceRules(this Faker<Invoice> faker, int index, DateTime startDate, DateTime endDate, CostCategory[] categoryOptions, string prefix, decimal? total = null)
        {
            int midPoint = (endDate - startDate).Days / 2;

            return faker
                .RuleFor(i => i.Id, f => Guid.NewGuid().ToString())
                .RuleFor(i => i.InvoiceNumber, f => $"{prefix}Inv-{index}")
                .RuleFor(i => i.Date, f => f.Date.Between(startDate, DateTime.UtcNow))
                .RuleFor(i => i.WorkStartDate, f => f.Date.Between(startDate, startDate.AddDays(midPoint)))
                .RuleFor(i => i.WorkEndDate, (f, i) => f.Date.Between(i.WorkStartDate.Value, endDate))
                .RuleFor(i => i.PaymentDate, (f, i) => f.Date.Between(i.Date.Value, DateTime.UtcNow))
                .RuleFor(i => i.CostCategory, f => f.PickRandom(categoryOptions))
                .RuleFor(i => i.SupplierName, f => f.Company.CompanyName())
                .RuleFor(i => i.Description, f => f.Lorem.Sentence())
                .RuleFor(i => i.GrossAmount, f => f.Random.Decimal(0, total ?? 50000))
                .RuleFor(i => i.TaxRebate, (f, i) => f.Random.Decimal(0, i.GrossAmount.Value * (decimal)0.1))
                .RuleFor(i => i.ClaimAmount, (f, i) => f.Random.Decimal(i.GrossAmount.Value * (decimal)0.8, i.GrossAmount.Value))
                .RuleFor(i => i.TotalPST, (f, i) => f.Random.Decimal(0, i.GrossAmount.Value * (decimal)0.05))
                .RuleFor(i => i.TotalGST, (f, i) => f.Random.Decimal(0, i.GrossAmount.Value * (decimal)0.05))
                .RuleFor(i => i.Attachments, f => [])
                ;
        }

        public static Faker<ContactDetails> WithContactDetailsRules(this Faker<ContactDetails> faker, string prefix)
        {
            return faker
                .RuleFor(c => c.FirstName, f => prefix + f.Person.FirstName)
                .RuleFor(c => c.LastName, f => prefix + f.Person.LastName)
                .RuleFor(c => c.Title, f => f.Name.JobTitle())
                .RuleFor(c => c.Department, f => f.Commerce.Department())
                .RuleFor(c => c.Phone, f => f.Person.Phone)
                .RuleFor(c => c.Email, f => f.Person.Email)
                ;
        }

        public static Faker<FundingInformation> WithFundingInformationRules(this Faker<FundingInformation> faker, int amount, string prefix)
        {
            return faker
                .RuleFor(fund => fund.Name, f => prefix + f.Person.FirstName)
                .RuleFor(fund => fund.Type, f => f.Random.Enum<FundingType>())
                .RuleFor(fund => fund.Amount, f => amount)
                .RuleFor(fund => fund.OtherDescription, f => f.Lorem.Sentence())
                ;
        }

        public static Faker<InfrastructureImpacted> WithInfrastructureImpactedRules(this Faker<InfrastructureImpacted> faker, string prefix)
        {
            return faker
                .RuleFor(i => i.Infrastructure, f => prefix + f.Company.CompanyName())
                .RuleFor(i => i.Impact, f => f.Lorem.Sentence(f.Random.Int(3, 5)))
                ;
        }

        public static Faker<ProposedActivity> WithProposedActivityRules(this Faker<ProposedActivity> faker)
        {
            return faker
                .RuleFor(a => a.Id, f => null)
                .RuleFor(a => a.Activity, f => f.Random.Enum<ActivityType>())
                .RuleFor(a => a.StartDate, f => DateTime.UtcNow)
                .RuleFor(a => a.EndDate, f => DateTime.UtcNow.AddDays(f.Random.Number(5, 60)))
                .RuleFor(a => a.Tasks, f => f.Lorem.Sentence())
                .RuleFor(a => a.Deliverables, f => f.Lorem.Sentence())
                ;
        }

        public static Faker<StandardInfo> WithStandardInfoRules(this Faker<StandardInfo> faker)
        {
            return faker
                .RuleFor(s => s.IsCategorySelected, f => f.Random.Bool())
                .RuleFor(s => s.Category, (f, s) => s.IsCategorySelected == true ? CategoryNames.TakeRandom(1).First() : null)
                .RuleFor(s => s.Standards, f => Enumerable.Range(0, f.Random.Int(0, 5)).Select(x => f.Lorem.Word()).ToList())
                ;
        }

        private static List<int> GetDivisors(int n)
        {
            var divisors = new SortedSet<int>(); // Sorted to keep order

            for (int i = 1; i * i <= n; i++)
            {
                if (n % i == 0)
                {
                    divisors.Add(i);        // Add the smaller divisor
                    divisors.Add(n / i);    // Add the corresponding larger divisor
                }
            }

            return divisors.ToList();
        }

        public static Faker<CostEstimate> WithCostEstimateRules(this Faker<CostEstimate> faker, int amount, CostCategory[] categoryOptions)
        {
            var divisors = GetDivisors(amount);
            divisors = divisors.Where(d => d <= 20).ToList();
            var dMin = 1;
            var dMax = divisors.Count() - 1;
            if (dMax < dMin)
            {
                dMax = 0;
                dMin = dMax;
            }
            return faker
                .RuleFor(a => a.Id, f => null)
                .RuleFor(a => a.TaskName, f => f.Lorem.Word())
                .RuleFor(a => a.CostCategory, f => f.PickRandom(categoryOptions))
                .RuleFor(a => a.Description, f => f.Lorem.Sentence())
                .RuleFor(a => a.Resources, f => f.Random.Enum<ResourceCategory>())
                .RuleFor(a => a.Units, f => f.Random.Enum<CostUnit>())
                .RuleFor(a => a.Quantity, f => divisors.ElementAt(f.Random.Int(dMin, dMax)))
                .RuleFor(a => a.UnitRate, (f, a) => amount / a.Quantity)
                .RuleFor(a => a.TotalCost, (f, a) => amount)
                ;
        }

        public static Faker<YearOverYearFunding> WithYearOverYearFundingRules(this Faker<YearOverYearFunding> faker, int amount, int index)
        {
            return faker
                .RuleFor(fund => fund.Year, f => Years.ElementAt(index))
                .RuleFor(fund => fund.Amount, f => amount)
                ;
        }

        private static IEnumerable<YearOverYearFunding> CreateYearOverYearFunding(Faker f, int total)
        {
            var length = f.Random.Number(1, 4);
            var amounts = TestHelper.GenerateRandomNumbersWithTargetSum(total, length);
            var ret = new YearOverYearFunding[length];
            for (int i = 0; i < length; i++)
            {
                ret[i] = new Faker<YearOverYearFunding>("en_CA").WithYearOverYearFundingRules(amounts[i], i);
            }

            return ret;
        }

        private static IEnumerable<CostEstimate> CreateCostEstimates(Faker f, int total, FundingStream stream)
        {
            var categoryOptions = Enum.GetValues(typeof(CostCategory)).Cast<CostCategory>().ToArray();
            if (stream == FundingStream.Stream1) categoryOptions = categoryOptions.Where(e => e != CostCategory.Contingency).ToArray();
            var length = f.Random.Number(1, 4);
            var amounts = TestHelper.GenerateRandomNumbersWithTargetSum(total, length);
            var ret = new CostEstimate[length];
            for (int i = 0; i < length; i++)
            {
                ret[i] = new Faker<CostEstimate>("en_CA").WithCostEstimateRules(amounts[i], categoryOptions);
            }

            return ret;
        }

        private static IEnumerable<FundingInformation> CreateOtherFunding(Faker f, bool haveOtherFunding, int total, string prefix)
        {
            if (!haveOtherFunding) return Enumerable.Empty<FundingInformation>();

            var length = f.Random.Number(1, 6);
            var amounts = TestHelper.GenerateRandomNumbersWithTargetSum(total, length);
            var ret = new FundingInformation[length];
            for (int i = 0; i < length; i++)
            {
                ret[i] = new Faker<FundingInformation>("en_CA").WithFundingInformationRules(amounts[i], prefix);
            }

            return ret;
        }

        private static IEnumerable<string> CategoryNames = new[]
        {
            "Archaeology",
            "Environment - Mapping and Landscape",
            "Environment - Seismic",
            "Environment - Water (includes Rivers, Flooding, etc.)",
            "Financial",
            "Other"
        };

        private static IEnumerable<string> Years = new[]
        {
            "2024/2025",
            "2025/2026",
            "2026/2027",
            "2027/2028",
            "2028/2029",
            "2029/2030",
        };
#pragma warning restore CS8629 // Nullable value type may be null.
    }
}
