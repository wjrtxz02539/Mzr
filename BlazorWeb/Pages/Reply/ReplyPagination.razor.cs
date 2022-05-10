using BlazorWeb.Shared;
using BlazorWeb.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Mzr.Share.Models.Bilibili;
using System.Diagnostics;

namespace BlazorWeb.Pages.Reply
{
    public partial class ReplyPagination
    {
        [Parameter]
        [SupplyParameterFromQuery(Name = "threadId")]
        public long? ThreadId { get; set; } = null;
        [Parameter]
        [SupplyParameterFromQuery(Name = "parentId")]
        public long? ParentId { get; set; } = null;
        [Parameter]
        [SupplyParameterFromQuery(Name = "rootId")]
        public long? RootId { get; set; } = null;
        [Parameter]
        [SupplyParameterFromQuery(Name = "upId")]
        public long? UpId { get; set; } = null;
        [Parameter]
        [SupplyParameterFromQuery(Name = "userId")]
        public long? UserId { get; set; } = null;
        [Parameter]
        [SupplyParameterFromQuery(Name = "dialogId")]
        public long? DialogId { get; set; } = null;
        [Parameter]
        [SupplyParameterFromQuery(Name = "sort")]
        public string Sort { get; set; } = "-time";
        [Parameter]
        [SupplyParameterFromQuery(Name = "startTime")]
        public DateTime? StartTime { get; set; } = null;
        [Parameter]
        [SupplyParameterFromQuery(Name = "endTime")]
        public DateTime? EndTime { get; set; } = null;

        private MudTable<BiliReply> table = default!;

        private string? replyQuery = null;
        private string sort = null!;
        private int page = 1;
        private int size = 10;
        private bool loading = true;
        private DateTime? startTime;
        private DateTime? endTime;
        private DateRange dateRange => new DateRange(startTime?.ToLocalTime(), endTime?.ToLocalTime());
        private MudDateRangePicker dateRangePicker = null!;
        private MudTextField<string> queryField = null!;

        protected override void OnInitialized()
        {
            sort = Sort;
            startTime = StartTime;
            endTime = EndTime;
        }

        private bool shouldStrictDate => !(ThreadId != null || ParentId != null || RootId != null || UserId != null || DialogId != null);
        private string dateQueryLabel => shouldStrictDate ? "时间段（最多7天）" : "时间段";

        private void StrictDateQuery()
        {
            if (!shouldStrictDate)
                return;

            if (!startTime.HasValue && !endTime.HasValue)
            {
                endTime = DateTime.UtcNow;
                startTime = endTime.Value.AddDays(-7).AddTicks(1);
            }
            else if (startTime.HasValue && !endTime.HasValue)
                endTime = startTime!.Value.AddDays(7).AddTicks(-1);
            else if (!startTime.HasValue && endTime.HasValue)
                startTime = endTime!.Value.AddDays(-7).AddTicks(1);
            else if ((endTime - startTime) > TimeSpan.FromDays(31))
                endTime = startTime!.Value.AddDays(7).AddTicks(-1);

            return;
        }

        private async Task<TableData<BiliReply>> ServerDataReload(TableState state)
        {
            StrictDateQuery();
            loading = true;
            StateHasChanged();

            var watch = new Stopwatch();
            watch.Start();

            if (!string.IsNullOrEmpty(state.SortLabel))
            {
                sort = state.SortLabel;
                if (state.SortDirection == MudBlazor.SortDirection.Descending)
                    sort = $"-{sort}";
            }

            page = state.Page + 1;
            size = state.PageSize;

            var response = await replyService.PaginationAsync(
                userId: UserId,
                threadId: ThreadId,
                upId: UpId,
                dialogId: DialogId,
                page: page,
                size: size,
                sort: sort,
                contentQuery: replyQuery,
                startTime: startTime,
                endTime: endTime,
                root: RootId,
                parent: ParentId
            );

            watch.Stop();
            var parameters = new Dictionary<string, object?>()
        {
            {"userId", UserId},
            {"threadId", ThreadId},
            {"upId", UpId},
            {"dialogId", DialogId},
            {"root", RootId},
            {"parent", ParentId},
            {"page", state.Page + 1},
            {"size", state.PageSize},
            {"sort", sort},
            { "contentQuery", replyQuery },
            {"startTime", startTime },
            {"endTime", endTime}
        };

            await webUserService.Log("ReplyPagination", parameters, status: "Success", elapsed: watch.ElapsedMilliseconds, url: nav.Uri);

            return new() { Items = response.Items, TotalItems = response.MetaData.TotalCount };
        }


        private void DateRangeChanged(DateRange dateRange)
        {
            if (dateRange.Start.HasValue)
            {
                dateRange.Start = DateTimeTools.SetLocalTimeZone(dateRange.Start.Value);
                startTime = dateRange.Start?.ToUniversalTime();
            }

            if (dateRange.End.HasValue)
            {
                dateRange.End = DateTimeTools.SetLocalTimeZone(dateRange.End.Value);
                endTime = dateRange.End?.ToUniversalTime();
            }
        }

        private void OnQueryClick()
        {
            DateRangeChanged(dateRangePicker.DateRange);
            replyQuery = string.IsNullOrEmpty(queryField.Value) ? null : queryField.Value;

            table.ReloadServerData();
        }

        private void OnDownloadClick(MouseEventArgs e)
        {
            OnQueryClick();

            var parameters = new DialogParameters
            {
                { "Content", "下载评论数量（根据当前页面开始往后计数）" },
                { "InputType", typeof(int) },
                { "InputContent", "下载数量" },
                { "InputValue", 100 },
                { "CancelText", "取消" },
                { "SubmitText", "下载" },
                { "SubmitCallback", new EventCallbackFactory().Create<object?>(this, DownloadDialogCallback) }
            };

            loading = false;
            StateHasChanged();
            dialogService.Show<GeneralDialog>("下载评论", parameters);
        }

        private async void DownloadDialogCallback(object value)
        {
            var downloadSize = (int)value;
            if (downloadSize == 0)
                return;
            if (webUserService.webUser == null)
                return;

            var result = await replyService.SubmitExportTask(
                username: webUserService.webUser.Username,
                userId: UserId,
                threadId: ThreadId,
                upId: UpId,
                dialogId: DialogId,
                skip: (page - 1) * size,
                size: downloadSize,
                sort: sort,
                contentQuery: replyQuery,
                startTime: startTime,
                endTime: endTime,
                root: RootId,
                parent: ParentId
                );

            var parameters = new DialogParameters
            {
                { "SubmitText", "确认" },
            };

            if (result != null)
            {
                parameters.Add("Content", "下载任务添加成功，请去文件页面，查看具体任务进度及文件下载。");
                dialogService.Show<GeneralDialog>("成功", parameters);
            }
            else
            {
                parameters.Add("Content", $"下载任务添加失败，当前导出队列长度为 {statusService.ExportRunningDict.Count}，请稍后再试。");
                dialogService.Show<GeneralDialog>("失败", parameters);
            }

        }
    }
}
