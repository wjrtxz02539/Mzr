using BlazorWeb.Models.Chart;

namespace BlazorWeb.Utils
{
    public class ChartTools
    {
        public static Tuple<List<string>, List<double>> ParseToLineChart(List<TimeLineValue> values)
        {
            Tuple<List<string>, List<double>> result = new(new(), new());
            foreach(var value in values)
            {
                result.Item1.Add(value.LocalTime.ToString("yyyy-MM-dd HH"));
                result.Item2.Add(value.Count);
            }
            return result;
            /*
            if(values.Count < 2)
            {
                foreach(var value in values)
                {
                    result.Item1.Add(value.LocalTime.ToString("yyyy-MM-dd_HH"));
                    result.Item2.Add(value.Count);
                }
                return result;
            }

            var prefixYear = values[0].LocalTime.Year != values[^1].LocalTime.Year;
            var prefixMonth = values[0].LocalTime.Month != values[^1].LocalTime.Month || prefixYear;
            var prefixDay = values[0].LocalTime.Day != values[^1].LocalTime.Day || prefixMonth;

            int currentYear = -1;
            int currentMonth = -1;
            int currentDay = -1;
            foreach(var value in values)
            {
                var labelParts = new List<int>();
                if (prefixYear && currentYear != value.LocalTime.Year)
                {
                    currentYear = value.LocalTime.Year;
                    labelParts.Add(currentYear);
                }

                if (prefixMonth && currentMonth != value.LocalTime.Month)
                {
                    currentMonth = value.LocalTime.Month;
                    labelParts.Add(currentMonth);
                }

                if (prefixDay && currentDay != value.LocalTime.Day)
                {
                    currentDay = value.LocalTime.Day;
                    labelParts.Add(currentDay);
                }

                if (labelParts.Count > 0)
                    result.Item1.Add($"{string.Join('-', labelParts.Select(x => $"{x: 02}").ToArray())} {value.LocalTime.Hour:02}");
                else
                    result.Item1.Add(value.LocalTime.Hour.ToString());
            }
            */
        }
    }
}
