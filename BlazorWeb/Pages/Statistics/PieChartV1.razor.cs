using Blazorise.Charts;
using Microsoft.AspNetCore.Components;

namespace BlazorWeb.Pages.Statistics
{
    public partial class PieChartV1
    {
        [Parameter]
        public List<Tuple<string, long>> DataSet { get; set; } = new();
        [Parameter]
        public string Title { get; set; } = string.Empty;

        private PieChart<long> pieChart = null!;

        private readonly List<ChartColor> chartColors = new()
        {
            ChartColor.FromRgba(255, 99, 132, 0.7f),
            ChartColor.FromRgba(54, 162, 235, 0.7f),
            ChartColor.FromRgba(255, 206, 86, 0.7f),
            ChartColor.FromRgba(75, 192, 192, 0.7f),
            ChartColor.FromRgba(153, 102, 255, 0.7f),
            ChartColor.FromRgba(255, 159, 64, 0.7f)
        };
        private Random random = new Random();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await Draw();
            }
        }

        public async Task Draw()
        {
            await pieChart.Clear();
            if (DataSet.Count > 0)
            {
                var labels = DataSet.Select(x => x.Item1).ToList();

                var colorSet = chartColors.ToHashSet();
                var dataList = new List<long>();
                var colorList = new List<string>();
                foreach (var item in DataSet)
                {
                    string color;
                    if (colorSet.Count == 0)
                        color = ChartColor.FromRgba((byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255), 0.7f).ToJsRgba();
                    else
                    {
                        var colorTemp = colorSet.First();
                        colorSet.Remove(colorTemp);
                        color = colorTemp.ToJsRgba();
                    }
                    dataList.Add(item.Item2);
                    colorList.Add(color);
                }

                var dataSet = new PieChartDataset<long>
                {
                    Label = "Up",
                    Data = dataList,
                    BackgroundColor = colorList
                };

                await pieChart.AddLabelsDatasetsAndUpdate(labels, dataSet);
            }
        }
    }
}
