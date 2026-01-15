using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace TextEditor;

public sealed partial class TextEditorComponent : ComponentBase, IDisposable
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;
    
    [Parameter, EditorRequired]
    public TextEditorModel Model { get; set; } = null!;

    private (int Small, int Large) OnMouseDown_Detail_Bounds;
    private int OnMouseDown_DetailRank3_OriginalLineIndex;

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
        _measurements = await JsRuntime.InvokeAsync<TextEditorMeasurements>("textEditor.initializeAndTakeMeasurements", _dotNetHelper);
        Model.Measurements = _measurements;
        _failedToInitialize = _measurements.IsDefault();
    }
    
    public async Task TakeMeasurements()
    {
        _measurements = await JsRuntime.InvokeAsync<TextEditorMeasurements>("textEditor.takeMeasurements");
        Model.Measurements = _measurements;
    }

    [JSInvokable]
    public void ReceiveKeyboardDebounce()
    {
        Model.ReceiveKeyboardDebounce();
        StateHasChanged();
    }

    /// <summary>
    /// This avoids the keydown event being async.
    /// </summary>
    [JSInvokable]
    public async Task ArbitraryCtrlKeybindAsync(string key)
    {
        switch (key)
        {
            case "a":
                Model.SelectAll();
                StateHasChanged();
                break;
            case "c":
                {
                    var selectedText = Model.GetSelection();
                    if (selectedText is not null)
                        await JsRuntime.InvokeVoidAsync("textEditor.setClipboard", selectedText);
                    break;
                }
        }
    }

    /// <summary>
    /// Paste might commonly be "held down" and repeated very frequently, and thus a synchronous method exists for it,
    /// rather than grouping it in an async method.
    /// </summary>
    /// <param name="text"></param>
    [JSInvokable]
    public void OnPaste(string text)
    {
        Model.InsertText(text);
        StateHasChanged();
    }

    [JSInvokable]
    public void OnKeydown(string key, bool shiftKey, bool ctrlKey)
    {
        // TODO: perhaps use code for the software implemented dvorak and etc... people. I'm not sure if all ways of doing other layouts change 'Key'.
        //
        // TODO: another such scenario that I recall was for Linux, I think it was the gnome tweaks settings to make capslock act as an escape key,...
        //       ...I'm not sure the details but something about it needed a special case.
        //
        if (key.Length == 1 && !ctrlKey)
        {
            Model.InsertText(key);
        }
    
        switch (key)
        {
            case "Enter":
                Model.InsertText("\n");
                break;
            case "Tab":
                Model.InsertText("\t");
                break;
            case "ArrowLeft":
                Model.MoveCursor(MoveCursorKind.ArrowLeft, shiftKey: shiftKey, ctrlKey: ctrlKey);
                break;
            case "ArrowDown":
                Model.MoveCursor(MoveCursorKind.ArrowDown, shiftKey: shiftKey, ctrlKey: ctrlKey);
                break;
            case "ArrowUp":
                Model.MoveCursor(MoveCursorKind.ArrowUp, shiftKey: shiftKey, ctrlKey: ctrlKey);
                break;
            case "ArrowRight":
                Model.MoveCursor(MoveCursorKind.ArrowRight, shiftKey: shiftKey, ctrlKey: ctrlKey);
                break;
            case "Home":
                Model.MoveCursor(MoveCursorKind.Home, shiftKey: shiftKey, ctrlKey: ctrlKey);
                break;
            case "End":
                Model.MoveCursor(MoveCursorKind.End, shiftKey: shiftKey, ctrlKey: ctrlKey);
                break;
            case "Delete":
                Model.DeleteTextAtPositionByCursor(DeleteByCursorKind.Delete, ctrlKey: ctrlKey);
                break;
            case "Backspace":
                Model.DeleteTextAtPositionByCursor(DeleteByCursorKind.Backspace, ctrlKey: ctrlKey);
                break;
        }
        
        StateHasChanged();
    }

    /// <summary>
    /// ExpandSelectionLeft must be invoked prior to ExpandSelectionRight in the case that both are necessary.
    /// </summary>
    private void ExpandSelectionLeft(CharacterKind leftCharacterKind, int lastValidColumnIndex)
    {
        var localPositionIndex = Model.PositionIndex;
        var localColumnIndex = Model.ColumnIndex;
        var count = 2;
        var originalCharacterKind = leftCharacterKind;

        while (localColumnIndex - count > -1)
        {
            if (Model.GetCharacterKind(Model[localPositionIndex - count]) == originalCharacterKind)
            {
                ++count;
            }
            else
            {
                --count;
                break;
            }
        }

        if (localColumnIndex - count <= -1)
        {
            --count;
        }

        Model.SelectionAnchor = Model.PositionIndex - count;
    }

    /// <summary>
    /// ExpandSelectionLeft must be invoked prior to ExpandSelectionRight in the case that both are necessary.
    /// </summary>
    private void ExpandSelectionRight(CharacterKind rightCharacterKind, int lastValidColumnIndex)
    {
        ++Model.ColumnIndex;
        ++Model.PositionIndex;
        var originalCharacterKind = rightCharacterKind;
        var localPositionIndex = Model.PositionIndex;
        var localColumnIndex = Model.ColumnIndex;
        while (localColumnIndex < lastValidColumnIndex)
        {
            if (Model.GetCharacterKind(Model[localPositionIndex]) == originalCharacterKind)
            {
                ++localColumnIndex;
                ++localPositionIndex;
            }
            else
            {
                break;
            }
        }
        Model.PositionIndex = localPositionIndex;
        Model.ColumnIndex = localColumnIndex;

        Model.SelectionEnd = Model.PositionIndex;
    }
    
    [JSInvokable]
    public void OnMouseDown(
        double relativeX,
        double relativeY,
        bool shiftKey,
        int detailRank)
    {
        if (detailRank == 1)
        {
            (Model.LineIndex, Model.ColumnIndex) = GetRelativeIndicesYFirst(relativeY, relativeX);
            Model.PositionIndex = Model.GetPositionIndex(Model.LineIndex, Model.ColumnIndex);
            if (!shiftKey)
                Model.SelectionAnchor = Model.PositionIndex;
            Model.SelectionEnd = Model.PositionIndex;
            StateHasChanged();
        }
        else if (detailRank == 2)
        {
            (Model.LineIndex, Model.ColumnIndex) = GetRelativeIndicesYFirst(relativeY, relativeX);
            Model.PositionIndex = Model.GetPositionIndex(Model.LineIndex, Model.ColumnIndex);
            if (!shiftKey)
                Model.SelectionAnchor = Model.PositionIndex;
            Model.SelectionEnd = Model.PositionIndex;

            var leftCharacterKind = CharacterKind.None;
            var rightCharacterKind = CharacterKind.None;

            var (lineIndex, linePosStart, linePosEnd) = Model.GetLineInformationExcludingLineEndingCharacterByPositionIndex(Model.PositionIndex);

            if (Model.ColumnIndex > 0)
            {
                leftCharacterKind = Model.GetCharacterKind(Model[Model.PositionIndex - 1]);
            }

            var lastValidColumnIndex = Model.GetLastValidColumnIndex(lineIndex);
            if (Model.ColumnIndex < lastValidColumnIndex)
            {
                rightCharacterKind = Model.GetCharacterKind(Model[Model.PositionIndex]);
            }

            if (leftCharacterKind > rightCharacterKind)
            {
                ExpandSelectionLeft(leftCharacterKind, lastValidColumnIndex);
            }
            else if (rightCharacterKind > leftCharacterKind)
            {
                ExpandSelectionRight(rightCharacterKind, lastValidColumnIndex);
            }
            else if (leftCharacterKind != CharacterKind.None && rightCharacterKind != CharacterKind.None)
            {
                ExpandSelectionLeft(leftCharacterKind, lastValidColumnIndex);
                ExpandSelectionRight(rightCharacterKind, lastValidColumnIndex);
            }
            OnMouseDown_Detail_Bounds = (Model.SelectionAnchor, Model.SelectionEnd);
            StateHasChanged();
        }
        else if (detailRank == 3)
        {
            var (lineIndex, linePosStart, linePosEnd) = Model.GetLineInformationExcludingLineEndingCharacterByPositionIndex(Model.PositionIndex);
            OnMouseDown_Detail_Bounds = (linePosStart, linePosEnd + 1);
            OnMouseDown_DetailRank3_OriginalLineIndex = lineIndex;

            Model.SelectionAnchor = linePosEnd + 1;
            Model.SelectionEnd = linePosStart;
            Model.PositionIndex = Model.SelectionEnd;
            (Model.LineIndex, Model.ColumnIndex) = Model.GetLineColumnIndices(Model.PositionIndex);

            StateHasChanged();
        }
#if DEBUG
        else
        {
            throw new NotImplementedException();
        }
#endif
    }

    [JSInvokable]
    public void OnMouseMove(
        double relativeX,
        double relativeY,
        bool shiftKey,
        int detailRank)
    {
        if (detailRank == 1)
        {
            (Model.LineIndex, Model.ColumnIndex) = GetRelativeIndicesYFirst(relativeY, relativeX);
            Model.PositionIndex = Model.GetPositionIndex(Model.LineIndex, Model.ColumnIndex);
            Model.SelectionEnd = Model.PositionIndex;
            StateHasChanged();
        }
        else if (detailRank == 2)
        {
            var (lineIndex, columnIndex) = GetRelativeIndicesYFirst(relativeY, relativeX);
            var positionIndex = Model.GetPositionIndex(lineIndex, columnIndex);
            
            bool anchorIsLessThanEnd = Model.SelectionAnchor < Model.SelectionEnd
                ? true
                : false;

            if (positionIndex > Model.SelectionAnchor && !anchorIsLessThanEnd)
            {
                Model.SelectionAnchor = OnMouseDown_Detail_Bounds.Small;
                anchorIsLessThanEnd = !anchorIsLessThanEnd;
            }
            else if (positionIndex < Model.SelectionAnchor && anchorIsLessThanEnd)
            {
                Model.SelectionAnchor = OnMouseDown_Detail_Bounds.Large;
                anchorIsLessThanEnd = !anchorIsLessThanEnd;
            }

            if ((anchorIsLessThanEnd && (positionIndex >= OnMouseDown_Detail_Bounds.Large)) ||
                (!anchorIsLessThanEnd && (positionIndex <= OnMouseDown_Detail_Bounds.Small)))
            {
                Model.SelectionEnd = positionIndex;
                Model.PositionIndex = positionIndex;
                (Model.LineIndex, Model.ColumnIndex) = (lineIndex, columnIndex);
            }

            OnMouseMove_DetailRank2_ExpansionStep(anchorIsLessThanEnd);

            StateHasChanged();
        }
        else if (detailRank == 3)
        {

        }
#if DEBUG
        else
        {
            throw new NotImplementedException();
        }
#endif
    }

    private void OnMouseMove_DetailRank2_ExpansionStep(bool anchorIsLessThanEnd)
    {
        var leftCharacterKind = CharacterKind.None;
        var rightCharacterKind = CharacterKind.None;

        var (lineIndex, linePosStart, linePosEnd) = Model.GetLineInformationExcludingLineEndingCharacterByPositionIndex(Model.PositionIndex);

        if (Model.ColumnIndex > 0)
        {
            leftCharacterKind = Model.GetCharacterKind(Model[Model.PositionIndex - 1]);
        }

        var lastValidColumnIndex = Model.GetLastValidColumnIndex(lineIndex);
        if (Model.ColumnIndex < lastValidColumnIndex)
        {
            rightCharacterKind = Model.GetCharacterKind(Model[Model.PositionIndex]);
        }

        if (anchorIsLessThanEnd && rightCharacterKind != CharacterKind.None)
        {
            ExpandSelectionRight(rightCharacterKind, lastValidColumnIndex);
        }
        else if (!anchorIsLessThanEnd && leftCharacterKind != CharacterKind.None)
        {
            var localPositionIndex = Model.PositionIndex;
            var localColumnIndex = Model.ColumnIndex;
            var count = 2;
            var originalCharacterKind = leftCharacterKind;

            while (localColumnIndex - count > -1)
            {
                if (Model.GetCharacterKind(Model[localPositionIndex - count]) == originalCharacterKind)
                {
                    ++count;
                }
                else
                {
                    --count;
                    break;
                }
            }

            if (localColumnIndex - count <= -1)
            {
                --count;
            }

            Model.SelectionEnd = Model.PositionIndex - count;
            Model.PositionIndex = Model.SelectionEnd;
            (Model.LineIndex, Model.ColumnIndex) = Model.GetLineColumnIndices(Model.PositionIndex);
        }
    }

    [JSInvokable]
    public void ReceiveTooltip(double clientX, double clientY, double scrolledClientX, double scrolledClientY)
    {
        if (Model.TooltipList is null)
            return;
    
        var (lineIndex, columnIndex) = GetRelativeIndicesYFirst(scrolledClientY, scrolledClientX);
        var positionIndex = Model.GetPositionIndex(lineIndex, columnIndex);

        var tooltipFound = false;
        
        foreach (var tooltip in Model.TooltipList)
        {
            if (tooltip.StartPositionIndex <= positionIndex && tooltip.EndPositionIndex > positionIndex)
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
    /// Be very careful with this method, the Y axis comes first because it is mirroring "line, column"
    /// </summary>
    private (int lineIndex, int characterIndex) GetRelativeIndicesYFirst(double rY, double rX)
    {
        // rX => relativeX
        // rY => relativeY
        //double rX;
        //double rY;
        
        //rX = relativeX - Model.Measurements.EditorLeft;
        //rY = relativeY - Model.Measurements.EditorTop;
        
        if (rX < 0) rX = 0;
        if (rY < 0) rY = 0;

        var lineIndex = (int)(rY / Model.Measurements.LineHeight);
        if (lineIndex > Model.LineBreakPositionList.Count)
            lineIndex = Model.LineBreakPositionList.Count;

        var columnIndexDouble = rX / Model.Measurements.CharacterWidth;
        var supposedColumnIndex = (int)Math.Round(columnIndexDouble, MidpointRounding.AwayFromZero);

        var (xlineIndex, xlinePosStart, xlinePosEnd) = Model.GetLineInformationExcludingLineEndingCharacterByPositionIndex(Model.GetPositionIndex(lineIndex, supposedColumnIndex));
        var visualColumn = 0;
        var characterColumn = 0;
        var previousWidth = 1;
        for (int j = 0; j < supposedColumnIndex; j++)
        {
            if (xlinePosStart + j >= Model.Length)
            {
                break;
            }

            if (Model[xlinePosStart + j] == '\t')
            {
                previousWidth = 4;
                visualColumn += 4;
            }
            else
            {
                previousWidth = 1;
                visualColumn += 1;
            }
            ++characterColumn;

            if (visualColumn >= supposedColumnIndex)
            {
                break;
            }


            
        }

        if (columnIndexDouble - (visualColumn - previousWidth) < visualColumn - columnIndexDouble)
        {
            supposedColumnIndex = characterColumn - 1;
        }
        else
        {
            supposedColumnIndex = characterColumn;
        }

        var lastValidColumnIndex = Model.GetLastValidColumnIndex(lineIndex);
        if (supposedColumnIndex > lastValidColumnIndex)
            supposedColumnIndex = lastValidColumnIndex;

        return
            (
                lineIndex,
                supposedColumnIndex
            );
    }
    
    public void Dispose()
    {
        _dotNetHelper?.Dispose();
    }
}
