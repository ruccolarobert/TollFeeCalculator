namespace TollFeeCalculator
{
    public class TollCalculator // TODO: define interface for toll calculator?
    {
        private const int MAX_FEE = 18;

        /**
         * Calculate the total toll fee for one day
         *
         * @param vehicle - the vehicle
         * @param dates   - date and time of all passes on one day
         * @return - the total toll fee for that day
         */

        public int GetTollFee(Vehicle vehicle, DateTime[] dates) // TODO: if permitted, change signature of dates to IEnumerable<DateTime>?
        {
            if (IsTollFreeVehicle(vehicle)) return 0;

            var datesByDay = dates.GroupBy(date => date.Date);
            if (datesByDay.Count() > 1) throw new ArgumentException("All dates must be within the same day.");

            DateTime intervalStart = dates[0];
            if (IsTollFreeDate(intervalStart)) return 0;

            var batches = BatchPassages(dates);
            var totalFee = 0;
            foreach (var batch in batches)
            {
                var maxFee = MaxFeeForBatch(batch);
                totalFee += maxFee;
            }

            if (totalFee > 60) totalFee = 60;
            return totalFee;
        }

        private bool IsTollFreeVehicle(Vehicle vehicle)
        {
            if (vehicle == null) throw new NullReferenceException($"{nameof(Vehicle)} undefined.");
            String vehicleType = vehicle.GetVehicleType(); // TODO: convert to switch statement, or perhaps better extend interface to hold this information
            return vehicleType.Equals(TollFreeVehicles.Motorbike.ToString()) ||
                   vehicleType.Equals(TollFreeVehicles.Tractor.ToString()) ||
                   vehicleType.Equals(TollFreeVehicles.Emergency.ToString()) ||
                   vehicleType.Equals(TollFreeVehicles.Diplomat.ToString()) ||
                   vehicleType.Equals(TollFreeVehicles.Foreign.ToString()) ||
                   vehicleType.Equals(TollFreeVehicles.Military.ToString());
        }

        private int GetTollFee(DateTime date)
        {
            // TODO: simplify this whole method.

            int hour = date.Hour;
            int minute = date.Minute;

            if (hour == 6 && minute >= 0 && minute <= 29) return 8;
            else if (hour == 6 && minute >= 30 && minute <= 59) return 13;
            else if (hour == 7 && minute >= 0 && minute <= 59) return 18;
            else if (hour == 8 && minute >= 0 && minute <= 29) return 13;
            else if (hour >= 8 && hour <= 14 && minute >= 30 && minute <= 59) return 8;
            else if (hour == 15 && minute >= 0 && minute <= 29) return 13;
            else if (hour == 15 && minute >= 0 || hour == 16 && minute <= 59) return 18;
            else if (hour == 17 && minute >= 0 && minute <= 59) return 13;
            else if (hour == 18 && minute >= 0 && minute <= 29) return 8;
            else return 0;
        }

        private static Boolean IsTollFreeDate(DateTime date)
        {
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            if (month == 7) return true;

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

            if (year == 2013) // TODO: support more years than this. Abstract this into a service interface
            {
                if (month == 1 && day == 1 ||
                    month == 3 && (day == 28 || day == 29) ||
                    month == 4 && (day == 1 || day == 30) ||
                    month == 5 && (day == 1 || day == 8 || day == 9) ||
                    month == 6 && (day == 5 || day == 6 || day == 21) ||
                    month == 7 ||
                    month == 11 && day == 1 ||
                    month == 12 && (day == 24 || day == 25 || day == 26 || day == 31))
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * Groups the timestamps into batches of 60 minutes
         *
         * @param timestamps - all timestamps
         * @return - the timestamps grouped into 60 minute batches
         */
        internal IEnumerable<List<DateTime>> BatchPassages(DateTime[] timestamps)
        {
            var sortedTimestamps = timestamps.OrderBy(timestamp => timestamp);

            var tempLeader = sortedTimestamps.First();
            var tempGroup = new List<DateTime> { tempLeader };
            foreach (var timestamp in sortedTimestamps.Skip(1))
            {
                if ((timestamp - tempLeader).TotalMinutes < 60)
                {
                    tempGroup.Add(timestamp);
                }
                else
                {
                    yield return tempGroup;
                    tempLeader = timestamp;
                    tempGroup = [timestamp];
                }
            }

            yield return tempGroup; // yield last group
        }

        internal int MaxFeeForBatch(IEnumerable<DateTime> timestamps)
        {
            var maxFee = 0;
            foreach (var timestamp in timestamps)
            {
                var fee = GetTollFee(timestamp);
                if (fee == MAX_FEE) return fee;
                if (fee > maxFee) maxFee = fee;
            }

            return maxFee;
        }

        private enum TollFreeVehicles
        {
            Motorbike = 0,
            Tractor = 1,
            Emergency = 2,
            Diplomat = 3,
            Foreign = 4,
            Military = 5
        }
    }
}
