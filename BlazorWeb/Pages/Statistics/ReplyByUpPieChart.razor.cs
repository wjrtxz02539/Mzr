using BlazorWeb.Models.Chart;
using BlazorWeb.Pages.User;
using BlazorWeb.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Mzr.Share.Models.Bilibili;
using System.Diagnostics;

namespace BlazorWeb.Pages.Statistics
{
    public partial class ReplyByUpPieChart
    {
        [Parameter]
        public long UserId { get; set; }
        [Parameter]
        public string? ContentQuery { get; set; } = null;
        [Parameter]
        public DateTime? StartTime { get; set; } = null;
        [Parameter]
        public DateTime? EndTime { get; set; } = null;
        [Parameter]
        public bool ControlEnabled { get; set; } = false;
        [Parameter]
        public string? Title { get; set; } = null;

        private long userId;
        private string? contentQuery = null;
        private DateTime startTime;
        private DateTime endTime;
        private DateRange DateRangeValue => new(startTime.ToLocalTime(), endTime.ToLocalTime());
        private MudDateRangePicker dateRangePicker = null!;
        private MudTextField<string> textField = null!;
        private PieChartV1? pieChart = null;
        private List<Tuple<string, long>> replyByUpStatistics = new();

        protected override async Task OnInitializedAsync()
        {
            userId = UserId;
            contentQuery = ContentQuery;

            SetTimeQuery(StartTime, EndTime);

            await LoadData();
        }

        private void SetTimeQuery(DateTime? start, DateTime? end)
        {
            if (start == null && end == null)
            {
                endTime = DateTime.UtcNow;
                startTime = DateTime.UtcNow.AddDays(-30);
            }
            else if (start != null && end != null)
            {
                startTime = start.Value;
                endTime = end.Value;
                if ((endTime - startTime).TotalDays > 31)
                {
                    startTime = endTime.AddDays(-30);
                }
            }
            else if (start != null)
            {
                endTime = start.Value.AddDays(30);
                startTime = start.Value;
            }
            else if (end != null)
            {
                startTime = end.Value.AddDays(-30);
                endTime = end.Value;
            }
        }

        private async Task OnQueryClick(MouseEventArgs e)
        {
            SetTimeQuery(dateRangePicker.DateRange.Start, dateRangePicker.DateRange.End);
            contentQuery = string.IsNullOrEmpty(textField.Value) ? null : textField.Value;
            await LoadData();
            if (pieChart != null)
                await pieChart.Draw();
        }

        private async Task LoadData()
        {
            await LoadReplyByUpPieChartAsync();
            StateHasChanged();
        }
        private async Task LoadReplyByUpPieChartAsync()
        {
            replyByUpStatistics.Clear();
            var data = await replyService.ReplyCountGroupByUpAsync(userId: userId, contentQuery: contentQuery, startTime: startTime, endTime: endTime);
            foreach (var item in data)
            {
                replyByUpStatistics.Add(new(item.Item1.Username, item.Item2));
            }
        }
    }
}
