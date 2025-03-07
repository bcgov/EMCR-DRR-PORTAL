using System.Reflection;
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

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        public static DraftFpApplication FillInTestFpApplication(DraftFpApplication originalFp, ContactDetails? submitter = null)
        {
            var tempFp = new Faker<DraftFpApplication>("en_CA").WithApplicationRules(submitter).Generate();

            //Overwrite null/empty properties from original FP
            foreach (PropertyInfo prop in typeof(DraftFpApplication).GetProperties())
            {
                object currentValue = prop.GetValue(originalFp);
                object defaultValue = prop.PropertyType.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null;

                bool shouldReplace =
                currentValue == null ||
                Equals(currentValue, defaultValue) ||
                (currentValue is IEnumerable<object> collection && !collection.Any());

                if (shouldReplace)
                {
                    prop.SetValue(originalFp, prop.GetValue(tempFp));
                }
            }
            return originalFp;
        }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
    }
}
