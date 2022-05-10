using BlazorWeb.Models.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop;
using MudBlazor;

namespace BlazorWeb.Pages
{
    public partial class File
    {
        [Parameter]
        public string? Username { get; set; } = null;

        [Parameter]
        public string Sort { get; set; } = "-created_time";

        private MudTable<WebFile> table = null!;
        private bool loading = true;
        private string? username = null;
        private string sort = null!;

        protected override void OnInitialized()
        {
            if (webUserService.webUser == null)
            {
                nav.NavigateTo("MicrosoftIdentity/Account/SignOut", forceLoad: true);
                return;
            }

            if (webUserService.webUser.Role != WebUserRole.Admin)
                Username = webUserService.webUser.Username;

            username = Username;
            sort = Sort;
        }
        private async Task<TableData<WebFile>> ServerDataReload(TableState state)
        {
            loading = true;
            StateHasChanged();

            if (!string.IsNullOrEmpty(state.SortLabel))
            {
                sort = state.SortLabel;
                if (state.SortDirection == SortDirection.Descending)
                    sort = $"-{sort}";
            }

            var response = await fileService.PaginationAsync(
                username: username,
                page: state.Page + 1,
                pageSize: state.PageSize,
                sort: sort
                );

            loading = false;
            StateHasChanged();

            return new() { Items = response.Items, TotalItems = response.MetaData.TotalCount };
        }
        private async Task OnDeleteClick(WebFile file)
        {
            await fileService.DeleteAsync(file);
            await Task.Delay(1000);
            await table.ReloadServerData();
        }

        private async Task OnDownloadClick(WebFile file)
        {
            if (file.Status != WebFileStatusEnum.Success)
                return;

            if (file.GridfsId == default)
                return;

            loading = false;
            StateHasChanged();

            using var stream = await fileService.OpenDownloadStreamAsync(file);
            using var streamRef = new DotNetStreamReference(stream: stream);

            await JS.InvokeVoidAsync("downloadFileFromStream", file.Filename, streamRef);
        }
    }
}
