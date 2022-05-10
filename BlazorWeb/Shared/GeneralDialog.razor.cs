using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace BlazorWeb.Shared
{
    public partial class GeneralDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;

        [Parameter]
        public string? Content { get; set; } = null;

        [Parameter]
        public string? InputContent { get; set; } = null;

        [Parameter]
        public string? InputValue { get; set; } = null;

        [Parameter]
        public string? CancelText { get; set; } = null;

        [Parameter]
        public string? SubmitText { get; set; } = null;

        [Parameter]
        public EventCallback<string?> SubmitCallback { get; set; }

        private MudInput<string> inputField = null!;
        private async Task OnSubmitClick(MouseEventArgs e)
        {
            if (SubmitCallback.HasDelegate)
            {
                await SubmitCallback.InvokeAsync(inputField.Value);
            }
            MudDialog.Close(DialogResult.Ok(true));
        }

        private void OnCancelClick() => MudDialog.Cancel();
    }
}
