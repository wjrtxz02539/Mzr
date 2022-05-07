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
    public partial class ReplyLineChart
    {
        [Parameter]
        public List<long> ThreadIds { get; set; } = new();
        [Parameter]
        public List<long> UserIds { get; set; } = new();
        [Parameter]
        public List<long> UpIds { get; set; } = new();
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

        private List<long> threadIds = new();
        private List<long> userIds = new();
        private List<long> upIds = new();
        private string? contentQuery = null;
        private DateTime startTime;
        private DateTime endTime;
        private DateRange DateRangeValue => new(startTime.ToLocalTime(), endTime.ToLocalTime());
        private MudDateRangePicker dateRangePicker = null!;
        private MudTextField<string> textField = null!;
        private TimeLineDefinition? chartDefinition = null;
        private LineChartV1? lineChart = null;

        protected override async Task OnInitializedAsync()
        {
            threadIds = ThreadIds;
            userIds = UserIds;
            upIds = UpIds;
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
            if (lineChart != null)
                await lineChart.Draw();
        }

        private async Task LoadData()
        {
            var watch = new Stopwatch();
            watch.Start();

            chartDefinition = new TimeLineDefinition(startTime: startTime, endTime: endTime);
            var queryList = new List<ReplyQuery>();
            if (threadIds?.Count > 0)
                AddParametersToDict("ThreadId", threadIds.Select(x => (object)x).ToArray(), ref queryList);
            if (userIds?.Count > 0)
                AddParametersToDict("UserId", userIds.Select(x => (object)x).ToArray(), ref queryList);
            if (upIds?.Count > 0)
                AddParametersToDict("UpId", upIds.Select(x => (object)x).ToArray(), ref queryList);

            if (!string.IsNullOrEmpty(contentQuery))
                foreach (var query in queryList)
                    query.ContentQuery = contentQuery;

            foreach (var query in queryList)
            {
                var dataSet = new TimeLineDataSet(
                    data: await replyService.TimeGroupAsync(userId: query.UserId,
                                            threadId: query.ThreadId,
                                            upId: query.UpId,
                                            contentQuery: query.ContentQuery,
                                            startTime: startTime,
                                            endTime: endTime),
                    label: await GetReplyQueryName(query));
                chartDefinition.AddDataSet(dataSet);
            }

            if (queryList.Count == 0)
                chartDefinition.AddDataSet(new(data: await replyService.TimeGroupAsync(startTime: startTime, endTime: endTime), label: "评论数"));
            StateHasChanged();

            watch.Stop();
            var parameters = new Dictionary<string, object?>()
            {
                {"threadIds", threadIds},
                {"userIds", userIds},
                {"upIds", upIds},
                {"contentQuery", contentQuery},
                {"startTime", startTime },
                {"endTime", endTime}
            };

            await webUserService.Log("ReplyLineChart", parameters, status: "Success", elapsed: watch.ElapsedMilliseconds, url: nav.Uri);
        }

        private async Task<string> GetReplyQueryName(ReplyQuery query)
        {
            var parts = new List<string>();

            if (query.ThreadId.HasValue)
                parts.Add(query.ThreadId.Value.ToString());
            if (query.UpId.HasValue)
            {
                var user = await userService.GetByUserId(query.UpId.Value);
                if (user == null)
                    parts.Add(query.UpId.Value.ToString());
                else
                    parts.Add(user.Username);
            }
            if (query.UserId.HasValue)
            {
                var user = await userService.GetByUserId(query.UserId.Value);
                if (user == null)
                    parts.Add(query.UserId.Value.ToString());
                else
                    parts.Add(user.Username);
            }
            return string.Join('_', parts);
        }

        private static void AddParametersToDict(string name, object[] values, ref List<ReplyQuery> data)
        {
            if (data.Count == 0)
                foreach (var value in values)
                {
                    var item = new ReplyQuery();
                    item.Set(name, value);
                    data.Add(item);
                }
            else
            {
                var temp = data.Select(x => x.Clone()).ToList();
                data.Clear();
                foreach (var value in values)
                    foreach (var item in temp)
                    {
                        var copy = item.Clone();
                        copy.Set(name, value);
                        data.Add(copy);
                    }
            }
        }

        class ReplyQuery
        {
            public long? ThreadId = null;
            public long? UserId = null;
            public long? UpId = null;
            public string? ContentQuery = null;

            public ReplyQuery(long? threadId = null, long? userId = null, long? upId = null, string? contentQuery = null)
            {
                ThreadId = threadId;
                UserId = userId;
                UpId = upId;
                ContentQuery = contentQuery;
            }

            public void Set(string name, object value)
            {
                switch (name)
                {
                    case "ThreadId":
                        ThreadId = (long?)value;
                        break;
                    case "UserId":
                        UserId = (long?)value;
                        break;
                    case "UpId":
                        UpId = (long?)value;
                        break;
                    case "ContentQuery":
                        ContentQuery = (string?)value;
                        break;
                    default:
                        throw new Exception($"Unknown Reply Query parameter name: {name}.");
                }
            }

            public ReplyQuery Clone()
            {
                return new(ThreadId, UserId, UpId, ContentQuery);
            }
        }
    }
}
