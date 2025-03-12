using System.Reflection;
using Bogus;
using EMCR.DRR.Controllers;

namespace EMCR.DRR.API.Utilities.TestData
{
    public static class TestHelper
    {
        public static DraftEoiApplication CreateNewTestEOIApplication(string prefix = "autotest-", ContactDetails? submitter = null)
        {
            var eoi = new Faker<DraftEoiApplication>("en_CA").WithApplicationRules(prefix, submitter).Generate();
            return eoi;
        }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        public static DraftFpApplication FillInTestFpApplication(DraftFpApplication originalFp, string prefix = "autotest-", ContactDetails? submitter = null)
        {
            var tempFp = new Faker<DraftFpApplication>("en_CA").WithApplicationRules(prefix, originalFp, submitter).Generate();

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

            originalFp.Standards = tempFp.Standards;
            originalFp.IncreasedOrTransferred = tempFp.IncreasedOrTransferred;
            if (originalFp.Standards != null) originalFp.Standards = originalFp.Standards.DistinctBy(s => s.Category);

            if (originalFp.ProposedActivities != null)
            {
                foreach (var activity in originalFp.ProposedActivities)
                {
                    if (!activity.StartDate.HasValue) activity.StartDate = DateTime.UtcNow.AddDays(1);
                    if (!activity.EndDate.HasValue) activity.EndDate = DateTime.UtcNow.AddDays(5);
                }
            }

            if (originalFp.Attachments != null)
            {
                foreach (var attachment in originalFp.Attachments)
                {
                    attachment.Comments = "attachment comment";
                }
            }

            return originalFp;
        }

        public static IEnumerable<Invoice> CreateInvoices(DateTime startDate, DateTime endDate, CostCategory[] categoryOptions, decimal? total = null, string prefix = "autotest-")
        {
            var length = new Random().Next(2, 6);
            if (total == null) total = 100000;
            var amounts = GenerateRandomNumbersWithTargetSum((int)total, length);
            var ret = new Invoice[length];
            for (int i = 0; i < length; i++)
            {
                ret[i] = new Faker<Invoice>("en_CA").WithInvoiceRules(i + 1, startDate, endDate, categoryOptions, prefix, amounts[i]);
            }

            return ret;
        }

        public static Invoice CreateNewTestInvoice(string id, int index, DateTime startDate, DateTime endDate, CostCategory[] categoryOptions, string prefix = "autotest-", decimal? total = null)
        {
            var invoice = new Faker<Invoice>("en_CA").WithInvoiceRules(index, startDate, endDate, categoryOptions, prefix, total).Generate();
            invoice.Id = id;
            return invoice;
        }

        public static int[] GenerateRandomNumbersWithTargetSum(int target, int count)
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
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
    }
}
