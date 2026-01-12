using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace TextEditor;

public sealed partial class TextEditorComponent : ComponentBase
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;
    
    [Parameter, EditorRequired]
    public TextEditorModel Model { get; set; } = null!;
    
    protected override void OnParametersSet()
    {
        if (Model is null)
            throw new NotImplementedException($"The Blazor parameter '{nameof(Model)}' cannot be null");
        base.OnParametersSet();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Model.Measurements = await JsRuntime.InvokeAsync<TextEditorMeasurements>("ideTextEditor.getTextEditorMeasurements");
            await InvokeAsync(StateHasChanged);
        }
    }
    
    private async Task FocusOnClick()
    {
        await JsRuntime.InvokeVoidAsync("ideTextEditor.setFocus");
    }
    
    private void OnKeydown(KeyboardEventArgs e)
    {
        // TODO: perhaps use code for the software implemented dvorak and etc... people. I'm not sure if all ways of doing other layouts change 'Key'.
        //
        // TODO: another such scenario that I recall was for Linux, I think it was the gnome tweaks settings to make capslock act as an escape key,...
        //       ...I'm not sure the details but something about it needed a special case.
        //
        if (e.Key.Length == 1)
        {
            Model.Content.Append(e.Key);
        }
    
        switch (e.Key)
        {
            case "ArrowLeft":
                Model.MoveCursor(MoveCursorKind.ArrowLeft);
                break;
            case "ArrowDown":
                Model.MoveCursor(MoveCursorKind.ArrowDown);
                break;
            case "ArrowUp":
                Model.MoveCursor(MoveCursorKind.ArrowUp);
                break;
            case "ArrowRight":
                Model.MoveCursor(MoveCursorKind.ArrowRight);
                break;
        }
        
    }
    
    private void OnMouseDown(MouseEventArgs e)
    {
        // rX => relativeX
        // rY => relativeY
        double rX;
        double rY;
        
        rX = e.ClientX - Model.Measurements.EditorLeft;
        rY = e.ClientY - Model.Measurements.EditorTop;
        
        if (rX < 0) rX = 0;
        if (rY < 0) rY = 0;
        
        var characterIndex = (int)Math.Round(rX / Model.Measurements.CharacterWidth, MidpointRounding.AwayFromZero);
        Model.PositionIndex = characterIndex;
    }
}
