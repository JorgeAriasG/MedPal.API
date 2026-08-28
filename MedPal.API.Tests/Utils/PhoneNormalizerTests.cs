using FluentAssertions;
using MedPal.API.Utils;
using Xunit;

namespace MedPal.API.Tests.Utils
{
    public class PhoneNormalizerTests
    {
        [Theory]
        [InlineData("521234567890", "+521234567890")]
        [InlineData("+521234567890", "+521234567890")]
        [InlineData("52 1234 5678 90", "+521234567890")]
        [InlineData("(52) 1234-5678-90", "+521234567890")]
        [InlineData("1234567890", "+521234567890")]
        [InlineData("00521234567890", "+521234567890")]
        [InlineData("0441234567890", "+521234567890")]
        public void ToE164_MexicanPhones_ShouldNormalizeCorrectly(string input, string expected)
        {
            var result = PhoneNormalizer.ToE164(input);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("12345")]
        [InlineData("123456789")]
        public void ToE164_InvalidOrShortPhones_ShouldReturnNull(string? input)
        {
            var result = PhoneNormalizer.ToE164(input);
            result.Should().BeNull();
        }

        [Fact]
        public void ToE164_TelcelElevenDigit_ShouldRemoveExtraOne()
        {
            // Telcel quirk: 521 + 10 digits = 13 chars, but the 1 is redundant
            var result = PhoneNormalizer.ToE164("5211234567890");
            result.Should().Be("+521234567890");
        }

        [Fact]
        public void ToE164_WithSpacesAndDashes_ShouldStripNonDigits()
        {
            var result = PhoneNormalizer.ToE164("521 234-567-890");
            result.Should().Be("+521234567890");
        }
    }
}
