using AutoMapper;
using MedPal.API.Mapping;
using Xunit;

namespace MedPal.API.Tests.Mapping
{
    public class AutoMapperTests
    {
        [Fact]
        public void AutoMapper_Configuration_IsValid()
        {
            // Arrange
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            // Assert
            config.AssertConfigurationIsValid();
        }
    }
}
