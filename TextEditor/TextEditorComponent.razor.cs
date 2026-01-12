using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text;

namespace TextEditor;

public sealed partial class TextEditorComponent : ComponentBase
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    private StringBuilder _content = new("test");
    
    private async Task FocusOnClick()
    {
        await JsRuntime.InvokeVoidAsync("ideTextEditor.setFocus");
    }
    
    private void OnKeydown(KeyboardEventArgs e)
    {
        _content.Append(e.Key);
    }
}

