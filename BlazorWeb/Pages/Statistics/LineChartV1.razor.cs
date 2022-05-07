using Blazorise.Charts;
using BlazorWeb.Models.Chart;
using Microsoft.AspNetCore.Components;

namespace BlazorWeb.Pages.Statistics
{
    public partial class LineChartV1
    {
        [Parameter]
        public TimeLineDefinition? ChartDefinition { get; set; } = null;
        [Parameter]
        public string Title { get; set; } = string.Empty;


        private LineChart<double> lineChart = null!;
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
            await lineChart.Clear();
            if (ChartDefinition != null)
            {
                await lineChart.AddLabels(ChartDefinition.Labels);
                var colorSet = chartColors.ToHashSet();
                foreach(var dataSet in ChartDefinition.DataSets)
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

                    var tempSet = new LineChartDataset<double>
                    {
                        Label = dataSet.Label,
                        Data = dataSet.Data,
                        Fill = dataSet.Fill,
                        PointRadius = dataSet.PointRadius,
                        BackgroundColor = color,
                        BorderColor = color,
                    };
                    await lineChart.AddDataSet(tempSet);
                }
                await lineChart.Update();
            }
        }

    }
}
