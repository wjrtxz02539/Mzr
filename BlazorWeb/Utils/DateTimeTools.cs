namespace BlazorWeb.Utils
{
    public class DateTimeTools
    {
        public static DateTime SetTimeZone(DateTime date, TimeZoneInfo timeZone)
        {
            return new DateTimeOffset(date).ToOffset(timeZone.BaseUtcOffset).DateTime;
        }

        public static DateTime SetLocalTimeZone(DateTime date)
        {
            var result = SetTimeZone(date, TimeZoneInfo.Local);
            return DateTime.SpecifyKind(result, DateTimeKind.Local);
        }

        public static Tuple<DateTime, DateTime> GetSpecificWholeDayRangeUtc(DateTime date)
        {
            var startTime = new DateTime(year: date.Year, month: date.Month, day: date.Day);
            var endTime = startTime.AddDays(1).AddTicks(-1);

            if (date.Kind == DateTimeKind.Utc)
            {
                startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
                endTime = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);
            }
            else if (date.Kind == DateTimeKind.Local)
            {
                startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Local).ToUniversalTime();
                endTime = DateTime.SpecifyKind(endTime, DateTimeKind.Local).ToUniversalTime();
            }
            else
            {
                throw new ArgumentException("Should specific the date.Kind.");
            }

            return Tuple.Create(startTime, endTime);
        }

        public static Tuple<DateTime, DateTime> Get24HDateRangeUtc()
        {
            var endTime = DateTime.UtcNow;
            var startTime = endTime.AddHours(-24);
            return Tuple.Create(startTime, endTime);
        }
    }
}
