using Bogus;
using EMCR.DRR.Controllers;

namespace EMCR.DRR.API.Utilities.TestData
{
    public static class BogusExtensions
    {
        private static readonly string prefix = "autotest-";

#pragma warning disable CS8629 // Nullable value type may be null.
        public static Faker<DraftEoiApplication> WithApplicationRules(this Faker<DraftEoiApplication> faker, ContactDetails? submitter = null)
        {
            return faker
            .RuleFor(a => a.Id, f => null)
            .RuleFor(a => a.Status, f => EMCR.DRR.API.Model.SubmissionPortalStatus.Draft)

            //Proponent Information - 1
            .RuleFor(a => a.ProponentType, f => f.Random.Enum<ProponentType>())
            .RuleFor(a => a.Submitter, f => submitter ?? new Faker<ContactDetails>("en_CA").WithContactDetailsRules().Generate())
            .RuleFor(a => a.ProjectContact, f => new Faker<ContactDetails>("en_CA").WithContactDetailsRules().Generate())
            .RuleFor(a => a.AdditionalContacts, f =>
            {
                var contactFaker = new Faker<ContactDetails>("en_CA");
                return contactFaker.WithContactDetailsRules().GenerateBetween(1, 2);
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
            .RuleFor(a => a.OtherFunding, (f, a) => CreateOtherFunding(f, (bool)a.HaveOtherFunding, f.Random.Number(0, (int)(a.EstimatedTotal - a.FundingRequest))))
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
            .RuleFor(a => a.InfrastructureImpacted, (f, a) => a.IsInfrastructureImpacted == true ? new Faker<InfrastructureImpacted>("en_CA").WithInfrastructureImpactedRules().GenerateBetween(1, 5) : Array.Empty<InfrastructureImpacted>())
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

        public static Faker<ContactDetails> WithContactDetailsRules(this Faker<ContactDetails> faker)
        {
            return faker
                .RuleFor(f => f.FirstName, f => prefix + f.Person.FirstName)
                .RuleFor(f => f.LastName, f => prefix + f.Person.LastName)
                .RuleFor(f => f.Title, f => f.Name.JobTitle())
                .RuleFor(f => f.Department, f => f.Commerce.Department())
                .RuleFor(f => f.Phone, f => f.Person.Phone)
                .RuleFor(f => f.Email, f => f.Person.Email)
                ;
        }

        public static Faker<FundingInformation> WithFundingInformationRules(this Faker<FundingInformation> faker, int amount)
        {
            return faker
                .RuleFor(f => f.Name, f => prefix + f.Person.FirstName)
                .RuleFor(f => f.Type, f => f.Random.Enum<FundingType>())
                .RuleFor(f => f.Amount, f => amount)
                .RuleFor(f => f.OtherDescription, f => f.Lorem.Sentence())
                ;
        }

        public static Faker<InfrastructureImpacted> WithInfrastructureImpactedRules(this Faker<InfrastructureImpacted> faker)
        {
            return faker
                .RuleFor(f => f.Infrastructure, f => prefix + f.Company.CompanyName())
                .RuleFor(f => f.Impact, f => f.Lorem.Sentence())
                ;
        }

        private static IEnumerable<FundingInformation> CreateOtherFunding(Faker f, bool haveOtherFunding, int total)
        {
            if (!haveOtherFunding) return Enumerable.Empty<FundingInformation>();

            var length = f.Random.Number(1, 6);
            var amounts = GenerateRandomNumbersWithTargetSum(total, length);
            var ret = new FundingInformation[length];
            for (int i = 0; i < length; i++)
            {
                ret[i] = new Faker<FundingInformation>("en_CA").WithFundingInformationRules(amounts[i]);
            }

            return ret;
        }

        private static int[] GenerateRandomNumbersWithTargetSum(int target, int count)
        {
            Random rand = new Random();
            int[] points = new int[count + 1];

            // Generate random partition points
            for (int i = 1; i < count; i++)
            {
                points[i] = rand.Next(1, target);
            }
            points[count] = target;
            Array.Sort(points);

            // Compute differences to get numbers that sum to target
            int[] result = new int[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = points[i + 1] - points[i];
            }

            return result;
        }
#pragma warning restore CS8629 // Nullable value type may be null.
    }
}
