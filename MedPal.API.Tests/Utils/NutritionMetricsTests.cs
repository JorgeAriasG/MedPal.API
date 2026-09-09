using FluentAssertions;
using MedPal.API.Utils;

namespace MedPal.API.Tests.Utils
{
    public class NutritionMetricsTests
    {
        [Theory]
        [InlineData(70, 175, 22.9)]
        [InlineData(80, 170, 27.7)]
        [InlineData(90, 190, 24.9)]
        [InlineData(60, 160, 23.4)]
        public void CalculateBmi_UsesHeightInCentimeters(decimal weight, decimal heightCm, decimal expected)
        {
            NutritionMetrics.CalculateBmi(weight, heightCm).Should().Be(expected);
        }

        [Fact]
        public void CalculateBmi_WithoutWeight_ReturnsNull()
        {
            NutritionMetrics.CalculateBmi(null, 175).Should().BeNull();
        }

        [Fact]
        public void CalculateBmi_WithoutHeight_ReturnsNull()
        {
            NutritionMetrics.CalculateBmi(70, null).Should().BeNull();
        }

        [Fact]
        public void CalculateBmi_WithZeroHeight_ReturnsNull()
        {
            NutritionMetrics.CalculateBmi(70, 0).Should().BeNull();
        }

        [Theory]
        [InlineData(80, 175, 0.46)]
        [InlineData(95, 180, 0.53)]
        public void CalculateWaistHeightRatio_UsesCmOnBothSides(decimal waistCm, decimal heightCm, decimal expected)
        {
            NutritionMetrics.CalculateWaistHeightRatio(waistCm, heightCm).Should().Be(expected);
        }

        [Fact]
        public void CalculateWaistHeightRatio_WithoutHeight_ReturnsNull()
        {
            NutritionMetrics.CalculateWaistHeightRatio(80, null).Should().BeNull();
        }

        [Theory]
        [InlineData(80, 104, 0.769)]
        [InlineData(85, 90, 0.944)]
        public void CalculateWaistHipRatio_UsesCmOnBothSides(decimal waistCm, decimal hipCm, decimal expected)
        {
            NutritionMetrics.CalculateWaistHipRatio(waistCm, hipCm).Should().Be(expected);
        }

        [Fact]
        public void CalculateWaistHipRatio_WithoutHip_ReturnsNull()
        {
            NutritionMetrics.CalculateWaistHipRatio(80, null).Should().BeNull();
        }

        [Fact]
        public void SamePhysicalPatient_ProducesIdenticalResultsAcrossEndpointSurfaces()
        {
            // Both create and update endpoints delegate to this canonical helper,
            // so the exact same values must be produced regardless of entry point.
            var bmi = NutritionMetrics.CalculateBmi(70, 175);
            var waistHeightRatio = NutritionMetrics.CalculateWaistHeightRatio(80, 175);
            var waistHipRatio = NutritionMetrics.CalculateWaistHipRatio(80, 104);

            bmi.Should().Be(22.9m);
            waistHeightRatio.Should().Be(0.46m);
            waistHipRatio.Should().Be(0.769m);
        }

        [Fact]
        public void HeightOneMeterSeventyBehavesAsCentimeters_NotMeters()
        {
            // Regression: a value of 1.75 must NOT be interpreted as meters.
            // Feeding 1.75 as if it were cm would inflate BMI to a nonsensical value.
            NutritionMetrics.CalculateBmi(70, 1.75m).Should().BeGreaterThan(1000m);
            NutritionMetrics.CalculateBmi(70, 175m).Should().Be(22.9m);
        }
    }
}