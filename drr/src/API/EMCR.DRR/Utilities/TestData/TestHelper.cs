using Bogus;
using EMCR.DRR.Controllers;

namespace EMCR.DRR.API.Utilities.TestData
{
    public static class TestHelper
    {
        public static DraftEoiApplication CreateNewTestEOIApplication(ContactDetails? submitter = null)
        {
            var eoi = new Faker<DraftEoiApplication>("en_CA").WithApplicationRules(submitter).Generate();
            return eoi;
        }
    }
}
