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
        private string? sort = null;
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

            var response = await replyService.PaginationAsync(
                userId: UserId,
                threadId: ThreadId,
                upId: UpId,
                dialogId: DialogId,
                page: state.Page + 1,
                size: state.PageSize,
                sort: sort ?? string.Empty,
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
            {"sort", sort ?? string.Empty},
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

        private void OnQueryClick(MouseEventArgs e)
        {
            DateRangeChanged(dateRangePicker.DateRange);
            replyQuery = string.IsNullOrEmpty(queryField.Value) ? null : queryField.Value;

            table.ReloadServerData();
        }
    }
}
