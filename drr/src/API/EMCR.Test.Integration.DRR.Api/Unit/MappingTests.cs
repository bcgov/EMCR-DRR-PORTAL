using AutoMapper;

namespace EMCR.Tests.Unit.DRR
{
    public class MappingTests
    {
        private readonly MapperConfiguration mapperConfig;

        public MappingTests()
        {
            mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps("EMCR.DRR.API");
            });
        }

        [Test]
        public void ValidateAutoMapperMappings()
        {
            mapperConfig.AssertConfigurationIsValid();
        }
    }
}
