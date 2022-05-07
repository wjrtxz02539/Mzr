namespace BlazorWeb.Models.Chart
{
    public class TimeLineValue
    {
        public DateTime UtcTime { get; set; }
        public int Count { get; set; }

        public DateTime LocalTime => UtcTime.ToLocalTime();
    }
}
