using BlazorWeb.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Mzr.Share.Models.Bilibili;
using System.Diagnostics;

namespace BlazorWeb.Pages.Dynamic
{
    public partial class DynamicPagination
    {
        [Parameter]
        [SupplyParameterFromQuery(Name = "userId")]
        public long? UserId { get; set; } = null;
        [Parameter]
        [SupplyParameterFromQuery(Name = "descriptionQuery")]
        public string? DescriptionQuery { get; set; } = null;
        [Parameter]
        [SupplyParameterFromQuery(Name = "sort")]
        public string Sort { get; set; } = "-time";
        [Parameter]
        [SupplyParameterFromQuery(Name = "startTime")]
        public DateTime? StartTime { get; set; } = null;
        [Parameter]
        [SupplyParameterFromQuery(Name = "endTime")]
        public DateTime? EndTime { get; set; } = null;

        private List<BiliDynamic> pagedData = new();
        private MudTable<BiliDynamic> table = default!;
        private string dynamicCardTooltipStyle = string.Empty;

        private Dictionary<long, BiliUser?> userDict = new();
        private Dictionary<long, BiliDynamicRunRecord?> recordDict = new();
        private string? descriptionQuery = null;
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
            descriptionQuery = DescriptionQuery;
            startTime = StartTime;
            endTime = EndTime;
        }

        private async Task<TableData<BiliDynamic>> ServerDataReload(TableState state)
        {
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

            var response = await dynamicService.PaginationAsync(
                userId: UserId,
                page: state.Page + 1,
                size: state.PageSize,
                sort: sort ?? string.Empty,
                descriptionQuery: descriptionQuery,
                startTime: startTime,
                endTime: endTime
            );

            userDict.Clear();
            recordDict.Clear();
            foreach (var item in response.Items)
            {
                if (!userDict.ContainsKey(item.UserId))
                    userDict.TryAdd(item.UserId, await userService.GetByUserId(item.UserId));
                recordDict.TryAdd(item.DynamicId, await dynamicService.GetLatestRunRecord(item.DynamicId));
            }

            watch.Stop();
            var parameters = new Dictionary<string, object?>()
        {
            {"userId", UserId},
            {"page", state.Page + 1},
            {"size", state.PageSize},
            {"sort", sort ?? string.Empty},
            { "descriptionQuery", descriptionQuery },
            {"startTime", startTime },
            {"endTime", endTime}
        };

            await webUserService.Log("DynamicPagination", parameters, status: "Success", elapsed: watch.ElapsedMilliseconds, url: nav.Uri);

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

        private BiliUser? GetUser(long userId)
        {
            if (userDict.TryGetValue(userId, out BiliUser? value))
                return value;
            return value;
        }

        private BiliDynamicRunRecord? GetRunRecord(long dynamicId)
        {
            if (recordDict.TryGetValue(dynamicId, out BiliDynamicRunRecord? value))
                return value;
            return value;
        }

        private void OnQueryClick(MouseEventArgs e)
        {
            DateRangeChanged(dateRangePicker.DateRange);
            descriptionQuery = string.IsNullOrEmpty(queryField.Value) ? null : queryField.Value;
            table.ReloadServerData();
        }
    }
}
