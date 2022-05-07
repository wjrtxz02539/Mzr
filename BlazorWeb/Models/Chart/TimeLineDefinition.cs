namespace BlazorWeb.Models.Chart
{
    public class TimeLineDefinition
    {
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public List<DateTime> TimeSeries { get; } = new();

        public List<TimeLineDataSet> DataSets { get; } = new();
        public string[] Labels { get; }

        public TimeLineDefinition(DateTime startTime, DateTime endTime)
        {
            StartTime = new DateTime(year: startTime.Year, month: startTime.Month, day: startTime.Day, hour: startTime.Hour, minute: 0, second: 0);
            EndTime = new DateTime(year: endTime.Year, month: endTime.Month, day: endTime.Day, hour: endTime.Hour, minute: 0, second: 0).AddHours(1).AddTicks(-1);

            var hours = (EndTime - StartTime).TotalHours;
            var result = new List<string>();
            for (var hour = 0; hour < hours; hour++)
            {
                var tempTime = StartTime.AddHours(hour);
                result.Add(tempTime.ToLocalTime().ToString("yy-MM-dd HH"));
                TimeSeries.Add(tempTime);
            }

            Labels = result.ToArray();
        }

        public void AddDataSet(TimeLineDataSet dataSet)
        {
            dataSet.Format(TimeSeries);
            DataSets.Add(dataSet);
        }
    }
}
