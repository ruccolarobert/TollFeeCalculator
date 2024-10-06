using FluentAssertions;

namespace TollFeeCalculator.Tests
{
    public class UnitTests
    {
        [Fact]
        public void GetTollFee_WhenVehicleIsNull_ShouldThrowException()
        {
            // Arrange
            TollCalculator tollCalculator = new TollCalculator();
            DateTime[] timestamps = new DateTime[] { new DateTime(2024, 10, 3, 6, 0, 0) };

            // Act
            Action act = () => tollCalculator.GetTollFee(null, timestamps);

            // Assert
            act.Should().Throw<NullReferenceException>();
        }
    }
}
