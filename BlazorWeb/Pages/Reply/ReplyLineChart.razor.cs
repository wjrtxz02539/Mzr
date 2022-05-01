using BlazorWeb.Pages.User;
using BlazorWeb.Utils;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Mzr.Share.Models.Bilibili;
using System.Diagnostics;

namespace BlazorWeb.Pages.Reply
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
        public DateTime? EndTime { get; set; } = null;
        [Parameter]
        public List<ChartSeries> Series { get; set; } = new();
        [Parameter]
        public bool ControllerEnabled { get; set; } = true;
        [Parameter]
        public ChartOptions ChartOptions { get; set; } = new()
        {
        };
        [Parameter]
        public string[]? XAxisLabels { get; set; } = null;
        [Parameter]
        public ChartType ChartType { get; set; } = ChartType.Line;
        [Parameter]
        public string ChartWidth { get; set; } = "80%";
        [Parameter]
        public string ChartHeight { get; set; } = "80%";

        private List<long> threadIds = new();
        private List<long> userIds = new();
        private List<long> upIds = new();
        private string? contentQuery = null;
        private DateTime? startTime = null;
        private DateTime? endTime = null;
        private List<ChartSeries> series = null!;
        private string[] xAxisLabels = null!;

        protected override async Task OnInitializedAsync()
        {
            threadIds = ThreadIds;
            userIds = UserIds;
            upIds = UpIds;
            contentQuery = ContentQuery;

            var dateRange = DateTimeTools.GetSpecificWholeDayRangeUtc(EndTime ?? DateTime.Now);
            startTime = dateRange.Item1;
            endTime = dateRange.Item2;
            series = Series;
            xAxisLabels = XAxisLabels ?? Enumerable.Range(0, 24).Select(x => x.ToString()).ToArray();


            if (series.Count == 0)
            {
                await LoadData();
            }
        }

        private void OnUsersSelected(List<BiliUser> users)
        {
            userIds = users.Select(x => x.UserId).ToList();
        }

        private void OnUpsSelected(List<BiliUser> users)
        {
            upIds = users.Select(x => x.UserId).ToList();
        }

        private async Task LoadData()
        {
            var watch = new Stopwatch();
            watch.Start();
            if ((endTime - startTime) > TimeSpan.FromDays(31))
            {
                throw new ArgumentException("时间范围最大仅支持三十天");
            }

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

            series.Clear();

            foreach (var query in queryList)
            {
                var item = new ChartSeries()
                {
                    Name = await GetReplyQueryName(query),
                    Data = await replyService.HourlyAggregateAsync(
                                            userId: query.UserId,
                                            threadId: query.ThreadId,
                                            upId: query.UpId,
                                            contentQuery: query.ContentQuery,
                                            startTime: startTime,
                                            endTime: endTime
                                        )
                };
                series.Add(item);
            }

            if (queryList.Count == 0)
                series.Add(new()
                {
                    Name = "评论数",
                    Data = await replyService.HourlyAggregateAsync(startTime: startTime, endTime: endTime)
                });

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

        private void AddParametersToDict(string name, object[] values, ref List<ReplyQuery> data)
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

        private void DateChanged(DateTime? date)
        {
            if (date == null)
                return;

            var range = DateTimeTools.GetSpecificWholeDayRangeUtc(DateTimeTools.SetLocalTimeZone(date.Value));
            startTime = range.Item1;
            endTime = range.Item2;

        }

        private void SelectUsers()
        {
            var parameters = new DialogParameters();

            var callback = new EventCallbackFactory().Create<List<BiliUser>>(this, OnUsersSelected);
            parameters.Add("UsersSelected", callback);

            dialogService.Show<UserSelectDialog>("选择用户", parameters);
        }

        private void SelectUps()
        {
            var parameters = new DialogParameters();

            var callback = new EventCallbackFactory().Create<List<BiliUser>>(this, OnUpsSelected);
            parameters.Add("UpsSelected", callback);

            dialogService.Show<UpSelectDialog>("选择UP", parameters);
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
