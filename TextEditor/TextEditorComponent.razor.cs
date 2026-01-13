using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace TextEditor;

public sealed partial class TextEditorComponent : ComponentBase, IDisposable
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;
    
    [Parameter, EditorRequired]
    public TextEditorModel Model { get; set; } = null!;
    
    private DotNetObjectReference<TextEditorComponent>? _dotNetHelper;
    /// <summary>
    /// No persistence of TextEditorModel comes with the library.
    ///
    /// But the presumption here is that someone might choose to persist a TextEditorModel,
    /// and that they'd like to know what the measurements last were,
    /// because those measurements relate to the virtualized content that was displayed.
    ///
    /// In 'OnParametersSet()' the Model has its 'Measurements' property set to that of the component's '_measurements' field.
    /// Additionally this set is performed within 'InitializeAndTakeMeasurements()'.
    /// </summary>
    private TextEditorMeasurements _measurements;
    /// <summary>
    /// This field most closely relates to whether the non-Blazor UI events were added via JavaScript or not.
    /// </summary>
    private bool _failedToInitialize;
    
    private TextEditorTooltip _tooltip;
    private bool _tooltipOccurred;
    private double _tooltipClientX;
    private double _tooltipClientY;
    
    public TextEditorMeasurements Measurements => _measurements;
    public bool FailedToInitialize => _failedToInitialize;
    
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
            await InitializeAndTakeMeasurements();
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public async Task InitializeAndTakeMeasurements()
    {
        _measurements = await JsRuntime.InvokeAsync<TextEditorMeasurements>("ideTextEditor.initializeAndTakeMeasurements", _dotNetHelper);
        Model.Measurements = _measurements;
        _failedToInitialize = _measurements.IsDefault();
    }
    
    public async Task TakeMeasurements()
    {
        _measurements = await JsRuntime.InvokeAsync<TextEditorMeasurements>("ideTextEditor.takeMeasurements");
        Model.Measurements = _measurements;
    }
    
    [JSInvokable]
    public void ReceiveKeyboardDebounce()
    {
        Model.ReceiveKeyboardDebounce();
        StateHasChanged();
    }
    
    [JSInvokable]
    public void OnKeydown(string key)
    {
        // TODO: perhaps use code for the software implemented dvorak and etc... people. I'm not sure if all ways of doing other layouts change 'Key'.
        //
        // TODO: another such scenario that I recall was for Linux, I think it was the gnome tweaks settings to make capslock act as an escape key,...
        //       ...I'm not sure the details but something about it needed a special case.
        //
        if (key.Length == 1)
        {
            Model.InsertText(key);
        }
    
        switch (key)
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
        
        StateHasChanged();
    }
    
    [JSInvokable]
    public void OnMouseDown(
        long buttons,
        double clientX,
        double clientY,
        bool shiftKey)
    {
        var (characterIndex, lineIndex) = GetRelativeIndices(clientX, clientY);
        Model.PositionIndex = characterIndex;
        StateHasChanged();
    }
    
    [JSInvokable]
    public void OnMouseMove(
        long buttons,
        double clientX,
        double clientY,
        bool shiftKey)
    {
        var (characterIndex, lineIndex) = GetRelativeIndices(clientX, clientY);
        Model.PositionIndex = characterIndex;
        StateHasChanged();
    }
    
    [JSInvokable]
    public void ReceiveTooltip(double clientX, double clientY)
    {
        if (Model.TooltipList is null)
            return;
    
        var (characterIndex, lineIndex) = GetRelativeIndices(clientX, clientY);
        
        var tooltipFound = false;
        
        foreach (var tooltip in Model.TooltipList)
        {
            if (tooltip.StartPositionIndex <= characterIndex && tooltip.EndPositionIndex > characterIndex)
            {
                tooltipFound = true;
                _tooltip = tooltip;
                _tooltipOccurred = true;
                _tooltipClientX = clientX;
                _tooltipClientY = clientY;
            }
        }
        
        if (!tooltipFound)
        {
            // Check whether there previously was a tooltip being shown.
            // Because if two tooltip events result in no tooltip found, then there is no need to re-render.
            if (_tooltipOccurred)
            {
                _tooltipOccurred = false;
                StateHasChanged();
            }
        }
        else
        {
            // If there is tooltip found, re-render everytime.
            StateHasChanged();
        }
    }
    
    /// <summary>
    /// Careful, this method and <see cref="GetRelativeCoordinates"/> take similar arguments
    ///
    /// Furthermore, this method in particular takes the original clientX and clientY.
    /// Do NOT pass the results of "the other method" to this one.
    /// </summary>
    private (int characterIndex, int lineIndex) GetRelativeIndices(double clientX, double clientY)
    {
        // rX => relativeX
        // rY => relativeY
        double rX;
        double rY;
        
        rX = clientX - Model.Measurements.EditorLeft;
        rY = clientY - Model.Measurements.EditorTop;
        
        if (rX < 0) rX = 0;
        if (rY < 0) rY = 0;
        
        var characterIndex = (int)Math.Round(rX / Model.Measurements.CharacterWidth, MidpointRounding.AwayFromZero);
        if (characterIndex > Model.Content.Length)
            characterIndex = Model.Content.Length;
        
        return
            (
                characterIndex,
                (int)rY
            );
    }
    
    /// <summary>
    /// Careful, this method and <see cref="GetRelativeIndices"/> take similar arguments
    /// </summary>
    private (double rX, double rY) GetRelativeCoordinates(double clientX, double clientY)
    {
        // I don't understand how method inlining works so
        // I'm gonna explicitly duplicate this code in GetRelativeIndices(...)
        
        // rX => relativeX
        // rY => relativeY
        double rX;
        double rY;
        
        rX = clientX - Model.Measurements.EditorLeft;
        rY = clientY - Model.Measurements.EditorTop;
        
        if (rX < 0) rX = 0;
        if (rY < 0) rY = 0;
        
        return (rX, rY);
    }
    
    public void Dispose()
    {
        _dotNetHelper?.Dispose();
    }
}
