using AutoMapper;
using EMCR.DRR.Managers.Intake;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EMCR.Tests.Integration.DRR.Managers.Intake
{
#pragma warning disable CS8604 // Possible null reference argument.
    public class ConfigurationTests
    {
        private readonly IIntakeManager manager;
        private readonly IMapper mapper;

        public ConfigurationTests()
        {
            var host = EMBC.Tests.Integration.DRR.Application.Host;
            manager = host.Services.GetRequiredService<IIntakeManager>();
            mapper = host.Services.GetRequiredService<IMapper>();
        }

        [Test]
        public async Task CanQueryEntitiesData()
        {
            var res = await manager.Handle(new EntitiesQuery());
            res.AffectedParties.Count().ShouldBeGreaterThan(0);
        }

        [Test]
        public async Task CanQueryDeclarations()
        {
            var res = mapper.Map<IEnumerable<EMCR.DRR.Controllers.DeclarationInfo>>((await manager.Handle(new DeclarationQuery())).Items);
            res.Count().ShouldBeGreaterThan(0);
        }
    }
}
#pragma warning restore CS8604 // Possible null reference argument.
