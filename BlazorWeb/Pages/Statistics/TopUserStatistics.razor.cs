using BlazorWeb.Data;
using Microsoft.AspNetCore.Components;
using Mzr.Share.Models.Bilibili;

namespace BlazorWeb.Pages.Statistics
{
    public partial class TopUserStatistics
    {
        [Parameter]
        public long? UpId { get; set; } = null;
        [Parameter]
        public long? ThreadId { get; set; } = null;
        [Parameter]
        public DateTime? StartTime { get; set; } = null;
        [Parameter]
        public DateTime? EndTime { get; set; } = null;
        [Parameter]
        public int Size { get; set; } = 10;

        [Inject]
        private BiliReplyService replyService { get; set; } = null!;

        private long? upId { get; set; } = null;
        private long? threadId { get; set; } = null;
        private DateTime? startTime { get; set; } = null;
        private DateTime? endTime { get; set; } = null;
        private int size { get; set; } = 10;
        private List<Tuple<int, long, BiliUser?>> items = new();
        private bool shouldStrictDate => !(ThreadId != null || UpId != null);

        protected override async Task OnInitializedAsync()
        {
            upId = UpId;
            threadId = ThreadId;
            startTime = StartTime;
            endTime = EndTime;
            size = Size;

            await LoadData();
        }

        private async Task LoadData()
        {
            if(shouldStrictDate)
                StrictDateQuery();

            items = await replyService.GetTopUsersAsync(
                startTime: startTime,
                endTime: endTime,
                upId: upId,
                threadId: threadId,
                limit: size);
        }

        private void StrictDateQuery()
        {
            if (!startTime.HasValue && !endTime.HasValue)
            {
                endTime = DateTime.UtcNow;
                startTime = endTime.Value.AddDays(-31).AddTicks(1);
            }
            else if (startTime.HasValue && !endTime.HasValue)
                endTime = startTime!.Value.AddDays(31).AddTicks(-1);
            else if (!startTime.HasValue && endTime.HasValue)
                startTime = endTime!.Value.AddDays(-31).AddTicks(1);
            else if ((endTime - startTime) > TimeSpan.FromDays(31))
                endTime = startTime!.Value.AddDays(31).AddTicks(-1);
            return;
        }
    }
}
