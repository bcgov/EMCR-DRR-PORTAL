﻿using System.Security.Claims;
using System.Text.Json;
using BCeIDService;
using EMCR.Utilities;
using Microsoft.Extensions.Caching.Distributed;

namespace EMBC.DRR.API.Services;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable IDE0022 // Use block body for method
#pragma warning disable CS8603 // Possible null reference return.
public interface IUserService
{
    Task<ClaimsPrincipal> GetPrincipal(ClaimsPrincipal sourcePrincipal = null);
    Task<AccountDetails> GetAccountDetails(ClaimsPrincipal sourcePrincipal);
}

public class UserService(IDistributedCache cache, IHttpContextAccessor httpContext, IConfiguration configuration) : IUserService
{
    private ClaimsPrincipal? currentPrincipal => httpContext.HttpContext?.User;
    private static string GetCurrentBusinessName(ClaimsPrincipal principal) => principal.FindFirstValue("bceid_business_name");
    private static string GetCurrentBusinessId(ClaimsPrincipal principal) => principal.FindFirstValue("bceid_business_guid");
    private static string GetCurrentUserId(ClaimsPrincipal principal) => principal.FindFirstValue("bceid_user_guid");
    private static string GetCurrentUserName(ClaimsPrincipal principal) => principal.FindFirstValue("bceid_username");

    public async Task<ClaimsPrincipal> GetPrincipal(ClaimsPrincipal? sourcePrincipal = null)
    {
        if (sourcePrincipal == null) sourcePrincipal = currentPrincipal;
        var userId = GetCurrentUserId(sourcePrincipal);


        var cacheKey = $"user:{userId}";
        var profile = await cache.GetOrSet(cacheKey, async () => await GetAccountDetails(sourcePrincipal), TimeSpan.FromMinutes(10));
        if (profile == null) return sourcePrincipal;

        Claim[] drrClaims =
        [
            new Claim("user_info", JsonSerializer.Serialize(profile)),
        ];
        return new ClaimsPrincipal(new ClaimsIdentity(sourcePrincipal.Identity, sourcePrincipal.Claims.Concat(drrClaims)));
    }

    public async Task<AccountDetails> GetAccountDetails(ClaimsPrincipal sourcePrincipal)
    {
        var userId = GetCurrentUserId(sourcePrincipal);
        BCeIDServiceSoapClient client = new BCeIDServiceSoapClient(
                configuration.GetValue<string>("BCeID:SOAPURL"),
                TimeSpan.FromSeconds(60),
                configuration.GetValue<string>("BCeID:serviceAccountName"),
                configuration.GetValue<string>("BCeID:serviceAcountPassword")
                );

        var accountDetails = await client.getAccountDetailAsync(new AccountDetailRequest
        {
            onlineServiceId = configuration.GetValue<string>("BCeID:serviceAccountId"),
            requesterAccountTypeCode = BCeIDAccountTypeCode.Business,
            requesterUserGuid = userId,
            userGuid = userId,
            accountTypeCode = BCeIDAccountTypeCode.Business,
        });

        return new AccountDetails
        {
            BusinessName = GetCurrentBusinessName(sourcePrincipal),
            FirstName = accountDetails.account.individualIdentity.name.firstname.value,
            LastName = accountDetails.account.individualIdentity.name.surname.value,
            Title = accountDetails.account.internalIdentity.title.value,
            Department = accountDetails.account.internalIdentity.department.value,
            Phone = accountDetails.account.contact.telephone.value,
            Email = accountDetails.account.contact.email.value,
        };
    }
}
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore IDE0022 // Use block body for method
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

public class AccountDetails
{
    public required string BusinessName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Title { get; set; }
    public string? Department { get; set; }
    public string? Phone { get; set; }
    public required string Email { get; set; }
}
