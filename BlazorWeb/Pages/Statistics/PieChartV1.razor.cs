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
                await pieChart.AddLabels(DataSet.Select(x => x.Item1));

                var dataSet = new PieChartDataset<long>()
                {
                    Data = DataSet.Select(x => x.Item2).ToList()
                };
                await pieChart.Update();
            }
        }
    }
}
