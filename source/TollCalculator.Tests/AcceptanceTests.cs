using FluentAssertions;
using Moq;
using TollFeeCaclulator.Utils;

namespace TollFeeCalculator.Tests
{
    public class AcceptanceTests
    {
        private static readonly Mock<IHolidayService> _holidayServiceMock = new Mock<IHolidayService>();
        private static readonly TollCalculator _tollCalculator = new(_holidayServiceMock.Object);

        // Varje passage genom en betalstation i Göteborg kostar 8, 13 eller 18 kronor beroende på tidpunkt.
        // Tider           Belopp
        // 06:00–06:29     8 kr
        // 06:30–06:59     13 kr
        // 07:00–07:59     18 kr
        // 08:00–08:29     13 kr
        // 08:30–14:59     8 kr
        // 15:00–15:29     13 kr
        // 15:30–16:59     18 kr
        // 17:00–17:59     13 kr
        // 18:00–18:29     8 kr
        // 18:30–05:59     0 kr
        [Theory]
        [InlineData(6, 0, 0, 8)]
        [InlineData(6, 29, 59, 8)]
        [InlineData(6, 30, 0, 13)]
        [InlineData(6, 59, 59, 13)]
        [InlineData(7, 0, 0, 18)]
        [InlineData(7, 59, 59, 18)]
        [InlineData(8, 0, 0, 13)]
        [InlineData(8, 29, 59, 13)]
        [InlineData(8, 30, 0, 8)]
        [InlineData(14, 59, 59, 8)]
        [InlineData(15, 0, 0, 13)]
        [InlineData(15, 29, 59, 13)]
        [InlineData(15, 30, 0, 18)]
        [InlineData(16, 59, 59, 18)]
        [InlineData(17, 0, 0, 13)]
        [InlineData(17, 59, 59, 13)]
        [InlineData(18, 0, 0, 8)]
        [InlineData(18, 29, 59, 8)]
        [InlineData(18, 30, 0, 0)]
        [InlineData(5, 59, 59, 0)]
        public void GetTollFee_WhenCarPassesAToll_ShouldCalculateTheExpectedCost(int hour, int minute, int second, int expectedFee)
        {
            // Arrange
            var timestamp = new DateTime(2024, 10, 3, hour, minute, second);

            // Act
            var fee = _tollCalculator.GetTollFee(new Car(), [timestamp]);
            
            // Assert
            fee.Should().Be(expectedFee);
        }



        // Det maximala beloppet per dag och fordon är 60 kronor.
        [Fact]
        public void GetTollFee_WhenCarPassesTollsMultipleTimesInOneDay_ShouldNotExceed60SEK()
        {
            // Arrange
            var timestamps = new DateTime[]
            {
                new(2024, 10, 3, 6, 30, 0),  // 13
                new(2024, 10, 3, 7, 31, 0),  // 18
                new(2024, 10, 3, 9, 0, 0),   // 8
                new(2024, 10, 3, 15, 30, 0), // 18
                new(2024, 10, 3, 16, 31, 0), // 18
                new(2024, 10, 3, 17, 32, 0), // 13
            };                               // Total: 88

            // Act
            var fee = _tollCalculator.GetTollFee(new Car(), timestamps);
            
            // Assert
            fee.Should().Be(60);
        }

        // Trängselskatt tas ut för fordon som passerar en betalstation måndag till fredag mellan 06.00 och 18.29.
        // done?

        // Skatt tas inte ut lördagar,
        [Fact]
        public void GetTollFee_WhenCarPassesTollOnSaturday_ShouldNotCharge()
        {
            // Arrange
            var timestamp = new DateTime(2024, 10, 5, 6, 30, 0);

            // Act
            var fee = _tollCalculator.GetTollFee(new Car(), [timestamp]);
            
            // Assert
            fee.Should().Be(0);
        }

        // helgdagar, dagar före helgdag eller
        [Theory]
        [InlineData(2013, 1, 1)]
        [InlineData(2013, 1, 19)] // Saturday
        [InlineData(2013, 1, 20)] // Sunday
        public void GetTollFee_WhenCarPassesTollOnPublicHoliday_ShouldNotCharge(int year, int month, int day)
        {
            // Arrange
            var tollCalculator2013 = new TollCalculator(new Holiday2013Service());
            var timestamp = new DateTime(year, month, day, 6, 30, 0);

            // Act
            var fee = tollCalculator2013.GetTollFee(new Car(), [timestamp]);
            
            // Assert
            fee.Should().Be(0);
        }

        // under juli månad.
        [Fact]
        public void GetTollFee_WhenCarPassesTollInJuly_ShouldNotCharge()
        {
            // Arrange
            var timestamp = new DateTime(2024, 7, 5, 6, 30, 0);

            // Act
            var fee = _tollCalculator.GetTollFee(new Car(), [timestamp]);
            
            // Assert
            fee.Should().Be(0);
        }

        // Vissa fordon är undantagna från trängselskatt. (Exemple Motorcykel)
        [Fact]
        public void GetTollFee_WhenMotorbikePassesToll_ShouldNotCharge()
        {
            // Arrange
            var timestamp = new DateTime(2024, 10, 3, 6, 30, 0);

            // Act
            var fee = _tollCalculator.GetTollFee(new Motorbike(), [timestamp]);
            
            // Assert
            fee.Should().Be(0);
        }

        // En bil som passerar flera betalstationer inom 60 minuter bara beskattas en gång.
        // Det belopp som då ska betalas är det högsta beloppet av de passagerna.
        [Fact]
        public void GetTollFee_WhenCarPassesMultipleTollsWithin60Minutes_ShouldChargeTheHighestFee()
        {
            // Arrange
            var timestamps = new DateTime[]
            {
                new(2024, 10, 3, 6, 30, 0),  // 13
                new(2024, 10, 3, 7, 0, 0),   // 18
            };                               // Total: 31

            // Act
            var fee = _tollCalculator.GetTollFee(new Car(), timestamps);
            
            // Assert
            fee.Should().Be(18);
        }
    }
}