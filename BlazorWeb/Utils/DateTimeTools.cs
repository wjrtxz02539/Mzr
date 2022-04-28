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
            return result;
        }
    }
}
