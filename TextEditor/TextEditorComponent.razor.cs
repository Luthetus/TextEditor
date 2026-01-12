using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text;

namespace TextEditor;

public sealed partial class TextEditorComponent : ComponentBase
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;
    
    [Parameter, EditorRequired]
    public TextEditorModel TextEditorModel { get; set; } = null!;
    
    protected override void OnParametersSet()
    {
        if (TextEditorModel is null)
            throw new NotImplementedException($"{nameof(TextEditorModel)} cannot be null");
        base.OnParametersSet();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            JsRuntime.InvokeAsync<TextEditorMeasurements>();
        }
    }
    
    private async Task FocusOnClick()
    {
        await JsRuntime.InvokeVoidAsync("ideTextEditor.setFocus");
    }
    
    private void OnKeydown(KeyboardEventArgs e)
    {
        TextEditorModel.Content.Append(e.Key);
    }
}
