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

        [Fact]
        public void GetTollFee_WhenPassagesFromDifferentDates_ShouldThrowException()
        {
            // Arrange
            TollCalculator tollCalculator = new TollCalculator();
            DateTime[] timestamps = [new(2024, 10, 3, 6, 0, 0), new(2024, 10, 4, 6, 0, 0)];

            // Act
            Action act = () => tollCalculator.GetTollFee(new Car(), timestamps);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void BatchPassages_WhenPassagesAreWithin60Minutes_ShouldReturnSingleBatch()
        {
            // Arrange
            TollCalculator tollCalculator = new TollCalculator();
            DateTime[] timestamps = [new DateTime(2024, 10, 3, 6, 0, 0), new DateTime(2024, 10, 3, 6, 59, 59)];

            // Act
            var batches = tollCalculator.BatchPassages(timestamps).ToList();

            // Assert
            batches.Should().HaveCount(1);
            batches.First().Should().HaveCount(2);
        }

        [Fact]
        public void BatchPassages_WhenPassagesAreMoreThan60MinutesApart_ShouldReturnMultipleBatches()
        {
            // Arrange
            TollCalculator tollCalculator = new TollCalculator();
            DateTime[] timestamps = [new DateTime(2024, 10, 3, 6, 0, 0), new DateTime(2024, 10, 3, 7, 0, 0)];

            // Act
            var batches = tollCalculator.BatchPassages(timestamps).ToList();

            // Assert
            batches.Should().HaveCount(2);
            batches.First().Should().HaveCount(1);
            batches.Last().Should().HaveCount(1);
        }
    }
}
