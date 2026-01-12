using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace TextEditor;

public sealed partial class TextEditorComponent : ComponentBase, IDisposable
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;
    
    [Parameter, EditorRequired]
    public TextEditorModel Model { get; set; } = null!;
    
    private DotNetObjectReference<TextEditorViewModelSlimDisplay>? _dotNetHelper;
    /// <summary>
    /// No persistence of TextEditorModel comes with the library.
    ///
    /// But the presumption here is that someone might choose to persist a TextEditorModel,
    /// and that they'd like to know what the measurements last were,
    /// because those measurements relate to the virtualized content that was displayed.
    ///
    /// In 'OnParametersSet()' the Model has its 'Measurements' property set to that of the component's '_measurements' field.
    /// Additionally this set is performed within 'InitializeAndGetMeasurements()'.
    /// </summary>
    private TextEditorMeasurements _measurements;
    private bool _failedToInitialize;
    
    private TextEditorTooltip _tooltip;
    private bool _tooltipOccurred;
    private double _tooltipClientX;
    private double _tooltipClientY;
    
    protected override void OnInitialized()
    {
        _dotNetHelper = DotNetObjectReference.Create(this);
    }
    
    protected override void OnParametersSet()
    {
        if (Model is null)
            throw new NotImplementedException($"The Blazor parameter '{nameof(Model)}' cannot be null");
        
        Model.Measurements = _measurements;
            
        base.OnParametersSet();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeAndGetMeasurements();
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public async Task InitializeAndGetMeasurements()
    {
        _measurements = await JsRuntime.InvokeAsync<TextEditorMeasurements>("ideTextEditor.initializeAndGetMeasurements");
        Model.Measurements = _measurements;
        _failedToInitialize = _measurements.IsDefault();
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
    
    [JSInvokable]
    private void OnMouseMove(MouseEventArgs e)
    {
        
    }
    
    [JSInvokable]
    private void ReceiveTooltip(double clientX, double clientY)
    {
        if (Model.TooltipList is null)
            return;
    
        // rX => relativeX
        // rY => relativeY
        double rX;
        double rY;
        
        rX = e.ClientX - Model.Measurements.EditorLeft;
        rY = e.ClientY - Model.Measurements.EditorTop;
        
        if (rX < 0) rX = 0;
        if (rY < 0) rY = 0;
        
        var characterIndex = (int)Math.Round(rX / Model.Measurements.CharacterWidth, MidpointRounding.AwayFromZero);
        
        foreach (var tooltip in Model.TooltipList)
        {
            if (tooltip.PositionIndex == characterIndex)
            {
                _tooltip = tooltip;
                _tooltipOccurred = true;
                _tooltipClientX = clientX;
                _tooltipClientY = clientY;
            }
        }
    }
    
    public void Dispose()
    {
        _dotNetHelper?.Dispose();
    }
}
