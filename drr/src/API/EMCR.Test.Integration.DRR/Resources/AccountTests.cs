using EMCR.DRR.API.Resources.Accounts;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EMCR.Tests.Integration.DRR.Resources
{
    public class AccountTests
    {
        private string TestPrefix = "autotest-dev";

        [Test]
        public async Task CanCreateAccountIfNotExists()
        {
            var host = Application.Host;
            var accountRepository = host.Services.GetRequiredService<IAccountRepository>();
            var account = CreateTestAccount();
            var id = await accountRepository.Manage(new SaveAccountIfNotExists { Account = account });
            id.ShouldNotBeNull();
        }

        [Test]
        public async Task SaveAccountIfNotExists_AccountExists_NoDuplicateAccountCreated()
        {
            var host = Application.Host;
            var accountRepository = host.Services.GetRequiredService<IAccountRepository>();
            var account = CreateTestAccount();
            var id = (await accountRepository.Manage(new SaveAccountIfNotExists { Account = account })).Id;
            var secondId = (await accountRepository.Manage(new SaveAccountIfNotExists { Account = account })).Id;
            id.ShouldNotBeNull();
            id.ShouldBe(secondId);
        }

        private Account CreateTestAccount()
        {
            var uniqueSignature = TestPrefix + "-" + Guid.NewGuid().ToString().Substring(0, 4);
            return new Account
            {
                BCeIDBusinessId = $"{uniqueSignature}-business_bceid",
                Name = $"{uniqueSignature}-account_name",
                City = $"{uniqueSignature}-account_city",
            };
        }
    }
}
