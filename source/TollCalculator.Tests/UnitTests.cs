using FluentAssertions;
using Moq;
using TollFeeCaclulator.Utils;

namespace TollFeeCalculator.Tests
{
    public class UnitTests
    {
        private static readonly Mock<IHolidayService> _holidayServiceMock = new Mock<IHolidayService>();
        private static readonly TollCalculator _tollCalculator = new(_holidayServiceMock.Object);

        [Fact]
        public void GetTollFee_WhenVehicleIsNull_ShouldThrowException()
        {
            // Arrange
            DateTime[] timestamps = new DateTime[] { new DateTime(2024, 10, 3, 6, 0, 0) };

            // Act
            Action act = () => _tollCalculator.GetTollFee(null, timestamps);

            // Assert
            act.Should().Throw<NullReferenceException>();
        }

        [Fact]
        public void GetTollFee_WhenPassagesFromDifferentDates_ShouldThrowException()
        {
            // Arrange
            DateTime[] timestamps = [new(2024, 10, 3, 6, 0, 0), new(2024, 10, 4, 6, 0, 0)];

            // Act
            Action act = () => _tollCalculator.GetTollFee(new Car(), timestamps);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void BatchPassages_WhenPassagesAreWithin60Minutes_ShouldReturnSingleBatch()
        {
            // Arrange
            DateTime[] timestamps = [new DateTime(2024, 10, 3, 6, 0, 0), new DateTime(2024, 10, 3, 6, 59, 59)];

            // Act
            var batches = _tollCalculator.BatchPassages(timestamps).ToList();

            // Assert
            batches.Should().HaveCount(1);
            batches.First().Should().HaveCount(2);
        }

        [Fact]
        public void BatchPassages_WhenPassagesAreMoreThan60MinutesApart_ShouldReturnMultipleBatches()
        {
            // Arrange
            DateTime[] timestamps = [new DateTime(2024, 10, 3, 6, 0, 0), new DateTime(2024, 10, 3, 7, 0, 0)];

            // Act
            var batches = _tollCalculator.BatchPassages(timestamps).ToList();

            // Assert
            batches.Should().HaveCount(2);
            batches.First().Should().HaveCount(1);
            batches.Last().Should().HaveCount(1);
        }

        [Fact]
        public void MaxFeeForBatch_WhenPassagesAreWithin60Minutes_ShouldReturnTheHighestFee()
        {
            // Arrange
            DateTime[] timestamps = [
                new DateTime(2024, 10, 3, 6, 0, 0), // 8
                new DateTime(2024, 10, 3, 6, 59, 59)]; // 13

            // Act
            var fee = _tollCalculator.MaxFeeForBatch(timestamps);

            // Assert
            fee.Should().Be(13);
        }
    }
}
