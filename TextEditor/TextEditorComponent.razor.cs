using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;

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
        _textEditorHeight = _measurements.EditorHeight;
        _failedToInitialize = _measurements.IsDefault();
    }
    
    public async Task TakeMeasurements()
    {
        _measurements = await JsRuntime.InvokeAsync<TextEditorMeasurements>("textEditor.takeMeasurements");
        _textEditorHeight = _measurements.EditorHeight;
    }

    private double _scrollTop;
    private double _textEditorHeight;

    [JSInvokable]
    public void OnScroll(double scrollTop, double textEditorHeight)
    {
        _scrollTop = scrollTop;
        _textEditorHeight = textEditorHeight;
        StateHasChanged();
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
        _showContextMenu = false;
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
            case "x":
                {
                    var selectedText = Model.GetSelection();
                    if (selectedText is not null)
                    {
                        await JsRuntime.InvokeVoidAsync("textEditor.setClipboard", selectedText);
                        Model.RemoveTextAtPositionByCursor(RemoveKind.DeleteLtr, false);
                        StateHasChanged();
                    }
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
        else
        {
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
                    Model.RemoveTextAtPositionByCursor(RemoveKind.DeleteLtr, ctrlKey: ctrlKey);
                    break;
                case "Backspace":
                    Model.RemoveTextAtPositionByCursor(RemoveKind.BackspaceRtl, ctrlKey: ctrlKey);
                    break;
            }
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
        _showContextMenu = false;
        if (detailRank == 1)
        {
            if (shiftKey)
                Model.SelectionAnchor = Model.PositionIndex;
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

            var oneBeyondLinePosEnd = linePosEnd + 1;
            if (oneBeyondLinePosEnd >= Model.Length)
                oneBeyondLinePosEnd -= 1;

            OnMouseDown_Detail_Bounds = (linePosStart, oneBeyondLinePosEnd);
            OnMouseDown_DetailRank3_OriginalLineIndex = lineIndex;

            Model.SelectionAnchor = oneBeyondLinePosEnd;
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
            var (_lineIndex, columnIndex) = GetRelativeIndicesYFirst(relativeY, relativeX);
            var positionIndex = Model.GetPositionIndex(_lineIndex, columnIndex);

            var (lineIndex, linePosStart, linePosEnd) = Model.GetLineInformationExcludingLineEndingCharacterByPositionIndex(positionIndex);
            
            var oneBeyondLinePosEnd = linePosEnd + 1;
            if (oneBeyondLinePosEnd >= Model.Length)
                oneBeyondLinePosEnd -= 1;

            if (lineIndex <= OnMouseDown_DetailRank3_OriginalLineIndex)
            {
                Model.SelectionAnchor = OnMouseDown_Detail_Bounds.Large;
                Model.SelectionEnd = linePosStart;
            }
            else
            {
                Model.SelectionAnchor = OnMouseDown_Detail_Bounds.Small;
                Model.SelectionEnd = oneBeyondLinePosEnd;
            }

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
    public void OnUndo()
    {
        // If you do the gap buffer optimization you can only do it for the most recent edit and then have to move the text still if you support more than one edit?
        //
        // you only support 1 edit so

        // insert thenctrlzyoucanjustholdthegapbufferanduseitforctrly

        // delete whenyoudeletedontmarkas'\0'thenyoucanrestorethetextthatway

        // anythingbeyondthemostrecenteditifyouhavemorethan1historyyoubrpprobabkiwenantgineoijaoiewfbibuputindibndidndindbd dndnbndnbndbnsejinosEGojisegoijegsijo;gjoig

        if (Model.EditKind != EditKind.None && !Model.EditIsUndone)
        {
            Model.EditIsUndone = true;
            if (Model.EditKind == EditKind.InsertLtr)
            {
                Model.EditedTextHistoryCount = 0;
                Model.History_EnsureCapacity(Model.EditLength);
                Model.EditedTextHistoryCount = Model.EditLength;
                for (int editHistoryIndex = 0, i = Model.EditPosition; editHistoryIndex < Model.EditLength; editHistoryIndex++, i++)
                {
                    Model._editedTextHistory[editHistoryIndex] = Model[i];
                }
                Model.RemoveTextAtPositionByRandomAccess(positionIndex: Model.EditPosition, count: Model.EditLength, RemoveKind.DeleteLtr, shouldMakeEditHistory: false);
                Model.PositionIndex = Model.EditPosition;
                (Model.LineIndex, Model.ColumnIndex) = Model.GetLineColumnIndices(Model.PositionIndex);
            }
            else if (Model.EditKind == EditKind.RemoveBackspaceRtl)
            {
                Model.InsertTextAtPosition(new ReadOnlySpan<char>(Model._editedTextHistory, 0, Model.EditedTextHistoryCount), Model.EditPosition, shouldMakeEditHistory: false);
                Model.PositionIndex = Model.EditPosition + Model.EditLength;
                (Model.LineIndex, Model.ColumnIndex) = Model.GetLineColumnIndices(Model.PositionIndex);
            }
            else if (Model.EditKind == EditKind.RemoveDeleteLtr)
            {
                Model.InsertTextAtPosition(new ReadOnlySpan<char>(Model._editedTextHistory, 0, Model.EditedTextHistoryCount), Model.EditPosition, shouldMakeEditHistory: false);
                Model.PositionIndex = Model.EditPosition;
                (Model.LineIndex, Model.ColumnIndex) = Model.GetLineColumnIndices(Model.PositionIndex);
            }
            StateHasChanged();
        }
    }
    
    [JSInvokable]
    public void OnRedo()
    {
        // TODO: Keep this commented out until non CtrlZ/CtrlY edits work properly
        /*
        if (Model.EditKind != EditKind.None && Model.EditIsUndone)
        {
            Model.EditIsUndone = false;
            if (Model.EditKind == EditKind.InsertLtr)
            {
                Model.InsertTextAtPosition(new ReadOnlySpan<char>(Model._editedTextHistory, 0, Model.EditedTextHistoryCount), Model.EditPosition, shouldMakeEditHistory: false);
                Model.PositionIndex = Model.EditPosition + Model.EditLength;
                (Model.LineIndex, Model.ColumnIndex) = Model.GetLineColumnIndices(Model.PositionIndex);
            }
            else if (Model.EditKind == EditKind.RemoveBackspaceRtl)
            {
                Model.RemoveTextAtPositionByRandomAccess(positionIndex: Model.EditPosition, count: Model.EditLength, RemoveKind.DeleteLtr, shouldMakeEditHistory: false);
                Model.PositionIndex = Model.EditPosition;
                (Model.LineIndex, Model.ColumnIndex) = Model.GetLineColumnIndices(Model.PositionIndex);
            }
            else if (Model.EditKind == EditKind.RemoveDeleteLtr)
            {
                Model.RemoveTextAtPositionByRandomAccess(positionIndex: Model.EditPosition, count: Model.EditLength, RemoveKind.DeleteLtr, shouldMakeEditHistory: false);
                Model.PositionIndex = Model.EditPosition;
                (Model.LineIndex, Model.ColumnIndex) = Model.GetLineColumnIndices(Model.PositionIndex);
            }
            StateHasChanged();
        }
        */
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

    private double _dropdownClientX;
    private double _dropdownClientY;
    private bool _showContextMenu;

    [JSInvokable]
    public void ReceiveContextMenu(double clientX, double clientY, double scrolledClientX, double scrolledClientY)
    {
        _dropdownClientX = clientX;
        _dropdownClientY = clientY;
        _showContextMenu = true;
        StateHasChanged();
    }

    /// <summary>
    /// Be very careful with this method, the Y axis comes first because it is mirroring "line, column"
    /// </summary>
    private (int lineIndex, int characterIndex) GetRelativeIndicesYFirst(double rY, double rX)
    {
        // rX => relativeX
        // rY => relativeY
        
        if (rX < 0) rX = 0;
        if (rY < 0) rY = 0;

        var lineIndex = (int)(rY / Measurements.LineHeight);
        if (lineIndex > Model.LineBreakPositionList.Count)
            lineIndex = Model.LineBreakPositionList.Count;

        var columnIndexDouble = rX / Measurements.CharacterWidth;
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

    private string GetTotalHeightStyle(StringBuilder stringBuilder)
    {
        stringBuilder.Append("height:");
        stringBuilder.Append(((Model.LineBreakPositionList.Count + 1) * _measurements.LineHeight).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        stringBuilder.Append("px;");
        var totalHeightStyle = stringBuilder.ToString();
        stringBuilder.Clear();
        return totalHeightStyle;
    }

    private string GetLargeRectangleToOffsetLinesStyle(StringBuilder stringBuilder, int startLineIndex)
    {
        stringBuilder.Append("height:");
        stringBuilder.Append((startLineIndex * _measurements.LineHeight).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        stringBuilder.Append("px;");
        var largeRectangleToOffsetLinesStyle = stringBuilder.ToString();
        stringBuilder.Clear();
        return largeRectangleToOffsetLinesStyle;
    }

    private string GetSelectionStyle(StringBuilder stringBuilder, ref int pos, int end)
    {
        var (lineIndex, linePosStart, linePosEnd) = Model.GetLineInformationExcludingLineEndingCharacterByPositionIndex(pos);

        int count = 0;
        for (int j = linePosStart; j < pos; j++)
        {
            if (Model[j] == '\t')
                count++;
        }
        // tabCount == 4, extra is 4 - 1 => 3
        var xleftExtraFromTabs = count * 3 * Measurements.CharacterWidth;

        stringBuilder.Append("left:");
        stringBuilder.Append((Measurements.CharacterWidth * (pos - linePosStart) + xleftExtraFromTabs).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        stringBuilder.Append("px;");
        stringBuilder.Append("top:");
        stringBuilder.Append((Measurements.LineHeight * lineIndex).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        stringBuilder.Append("px;");
        stringBuilder.Append("width:");

        var lineSegmentStart = pos;
        var lineSegmentEnd = linePosEnd < end ? linePosEnd : end;
        int widthCount = lineSegmentEnd - lineSegmentStart;
        if (linePosEnd < end)
        {
            ++widthCount;
        }

        count = 0;
        for (int j = lineSegmentStart; j < lineSegmentEnd; j++)
        {
            if (Model[j] == '\t')
                count++;
        }
        // tabCount == 4, extra is 4 - 1 => 3
        xleftExtraFromTabs = count * 3 * Measurements.CharacterWidth;

        stringBuilder.Append((Measurements.CharacterWidth * widthCount + xleftExtraFromTabs).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        stringBuilder.Append("px;");
        var selectStyle = stringBuilder.ToString();
        stringBuilder.Clear();

        pos = linePosEnd;

        return selectStyle;
    }

    private (int startLineIndex, int endLineIndex) GetStartEndLineIndices()
    {
        int startLineIndex;
        if (_measurements.LineHeight != 0)
            startLineIndex = ((int)(_scrollTop / _measurements.LineHeight) - 1);
        else
            startLineIndex = 0;
        if (startLineIndex < 0) startLineIndex = 0;

        int endLineIndex = startLineIndex + ((int)(_textEditorHeight / _measurements.LineHeight) + 1);
        if (endLineIndex > Model.LineBreakPositionList.Count + 1) endLineIndex = Model.LineBreakPositionList.Count + 1;

        return (startLineIndex, endLineIndex);
    }

    /// <summary>
    /// This is likely a massive performance cost to be checking the if statement for every single decoration byte.
    /// But the gap buffer isn't working perfectly yet, so abstracting this into a costly bounds check is 100% worth it for now to reduce noise.
    /// </summary>
    private byte GetDecorationByte(int i)
    {
        if (Model.DecorationArray is null || i >= Model.DecorationArray.Length)
            return 0;
        return Model.DecorationArray[i];
    }

    private string GetTooltipStyle(StringBuilder stringBuilder)
    {
        stringBuilder.Append("left:");
        stringBuilder.Append(Math.Max(0, _tooltipClientX - 2).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        stringBuilder.Append("px;");
        stringBuilder.Append("top:");
        stringBuilder.Append(Math.Max(0, _tooltipClientY - 2).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        stringBuilder.Append("px;");
        var tooltipStyle = stringBuilder.ToString();
        stringBuilder.Clear();
        return tooltipStyle;
    }

    private string GetDropdownStyle(StringBuilder stringBuilder)
    {
        stringBuilder.Append("left:");
        stringBuilder.Append(Math.Max(0, _dropdownClientX - 2).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        stringBuilder.Append("px;");
        stringBuilder.Append("top:");
        stringBuilder.Append(Math.Max(0, _dropdownClientY - 2).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        stringBuilder.Append("px;");
        var dropdownStyle = stringBuilder.ToString();
        stringBuilder.Clear();
        return dropdownStyle;
    }

    private string GetCursorStyle(StringBuilder stringBuilder)
    {
        var tabCountOnSameLinePriorToCursor = Model.GetTabCountOnSameLinePriorToCursor();
        // tabCount == 4, extra is 4 - 1 => 3
        var leftExtraFromTabs = tabCountOnSameLinePriorToCursor * 3 * Measurements.CharacterWidth;

        stringBuilder.Append("left:");
        //
        // Two decimal places for the double values avoids excessive information being sent when the visual difference is negligible.
        //
        // Avoid ',' in css when user's culture would default to using that in place of '.'
        // with System.Globalization.CultureInfo.InvariantCulture
        //
        stringBuilder.Append((Measurements.CharacterWidth * Model.ColumnIndex + leftExtraFromTabs).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        stringBuilder.Append("px;");

        stringBuilder.Append("top:");
        stringBuilder.Append((Measurements.LineHeight * Model.LineIndex).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        stringBuilder.Append("px;");

        var cursorStyle = stringBuilder.ToString();
        stringBuilder.Clear();

        return cursorStyle;
    }

    /// <summary>
    /// (in the case that this won't inline, it still doesn't matter the gap buffer doesn't work, rip out all the noise, if this matters then look at it later when the gap buffer works).
    /// </summary>
    private void AppendEscapedCharacter(StringBuilder stringBuilder, char character)
    {
        switch (character)
        {
            case '\t':
                stringBuilder.Append("&nbsp;&nbsp;&nbsp;&nbsp;");
                break;
            case '\0':
                stringBuilder.Append("0");
                break;
            case ' ':
                stringBuilder.Append("&nbsp;");
                break;
            case '<':
                stringBuilder.Append("&lt;");
                break;
            case '>':
                stringBuilder.Append("&gt;");
                break;
            case '"':
                stringBuilder.Append("&quot;");
                break;
            case '\'':
                stringBuilder.Append("&#39;");
                break;
            case '&':
                stringBuilder.Append("&amp;");
                break;
            default:
                stringBuilder.Append(character);
                break;
        }
    }

    private (int startPosition, int endPosition) GetStartEndPositions(int startLineIndex, int endLineIndex)
    {
        int startPosition = 0;
        if (startLineIndex == 0)
            startPosition = 0;
        else if (startLineIndex - 1 < Model.LineBreakPositionList.Count)
            startPosition = Model.LineBreakPositionList[startLineIndex - 1] + 1; // You get the line ending of the previous line and then + 1 because all line endings are stored as '\n' until saving the file in which they are swapped out for the desired line endings.
        else
            startLineIndex = 0; // I need this so the value is initialized but I TODO: need to look into this case further

        int endPosition;
        if (endLineIndex == 0 || endLineIndex >= Model.LineBreakPositionList.Count)
            endPosition = Model.Length; // This might be wrong. I think it takes an extra line (even beyond the "extra line so the virtualization looks smoother").
        else if (endLineIndex < Model.LineBreakPositionList.Count)
            endPosition = Model.LineBreakPositionList[endLineIndex];
        else
            endPosition = 0; // I need this so the value is initialized but I TODO: need to look into this case further

        return (startPosition, endPosition);
    }

    public void Dispose()
    {
        _dotNetHelper?.Dispose();
    }
}
