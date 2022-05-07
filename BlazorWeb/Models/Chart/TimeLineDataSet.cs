namespace BlazorWeb.Models.Chart
{
    public class TimeLineDataSet
    {
        public string Label { get; set; } = string.Empty;
        public List<double> Data { get; set; } = new();
        public bool Fill { get; set; } = false;
        public int PointRadius { get; set; } = 2;

        private List<TimeLineValue> rawData { get; set; } = new();

        public TimeLineDataSet(List<TimeLineValue> data, string? label = null)
        {
            rawData = data;
            if (!string.IsNullOrEmpty(label))
                Label = label;
        }

        public void Format(List<DateTime> timeSeries)
        {
            var index = 0;
            foreach(var time in timeSeries)
            {
                if (index < rawData.Count)
                {
                    if (time.Equals(rawData[index].UtcTime))
                    {
                        Data.Add(rawData[index].Count);
                        index++;
                    }
                    else
                        Data.Add(0);
                }
                else
                    Data.Add(0);
            }
        }
    }
}
