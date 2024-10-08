namespace TollFeeCaclulator.Utils
{
    public interface IHolidayService
    {
        bool IsHoliday(DateTime date);
    }
    
    public class Holiday2013Service : IHolidayService
    {
        public bool IsHoliday(DateTime date)
        {
            var year = date.Year;
            var month = date.Month;
            var day = date.Day;

            if (year == 2013)
            {
                return
                    month == 1 && day == 1 ||
                    month == 3 && (day == 28 || day == 29) ||
                    month == 4 && (day == 1 || day == 30) ||
                    month == 5 && (day == 1 || day == 8 || day == 9) ||
                    month == 6 && (day == 5 || day == 6 || day == 21) ||
                    month == 11 && day == 1 ||
                    month == 12 && (day == 24 || day == 25 || day == 26 || day == 31);
            }
            return false;
        }
    }
}
