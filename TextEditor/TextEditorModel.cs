using System.Text;

namespace TextEditor;

public class TextEditorModel
{
    private readonly StringBuilder _textBuilder = new();

    public char this[int key]
    {
        get => _textBuilder[key];
    }

    public override string ToString()
    {
        return _textBuilder.ToString();
    }

    public int Length => _textBuilder.Length;

    /// <summary>
    /// You'd only need to store either PositionIndex or both LineIndex and ColumnIndex
    /// since one can calculate the other.
    /// 
    /// But I feel just keeping both representations up to date is best from a usability standpoint.
    /// Otherwise you'd have to ask for one or the other by accessing mutable state that might change out from under you during the calculation.
    /// </summary>
    public int PositionIndex { get; set; }
    public int LineIndex { get; set; }
    public int ColumnIndex { get; set; }
    
    public TextEditorMeasurements Measurements { get; set; }

    public int _editedTextHistoryCapacity => _editedTextHistory.Length;
    public int _editedTextHistoryCount;
    public char[] _editedTextHistory = new char[4];
    public bool EditIsUndone;
    public int EditPosition;
    public int EditLength;
    public EditKind EditKind = EditKind.None;
    
    /// <summary>
    /// You can keep this feature disabled by leaving the property null (the default).
    /// Otherwise, you need to instantiate the list and begin populating it.
    ///
    /// This list is NOT expected to be sorted.
    /// A tooltip event will perform a linear search through this list to find the matching position index.
    /// </summary>
    public List<TextEditorTooltip>? TooltipList { get; set; } = null;
    public const byte CharacterTooltipByteKind = 0;
    public const byte TextTooltipByteKind = 1;

    /// <summary>
    /// This tracks in particular LineBreaks, thus the count of lines is this list's count + 1.
    /// 
    /// always insert '\n' for line endings, and then track separately the desired line end.
    /// upon saving, create a string that has the '\n' included as the desired line end.
    /// </summary>
    public List<int> LineBreakPositionList { get; set; } = new();

    public List<int> TabPositionList { get; set; } = new();

    public int LineCount => LineBreakPositionList.Count + 1;

    public bool HasSelection => SelectionAnchor != -1 && SelectionAnchor != SelectionEnd;

    /// <summary>
    /// The position index
    /// </summary>
    public int SelectionAnchor { get; set; }
    /// <summary>
    /// The position index
    /// </summary>
    public int SelectionEnd { get; set; }

    /// <summary>
    /// You can keep this feature disabled by leaving the property null (the default).
    /// Otherwise, you need to instantiate the list by invoking "EnableDecorations()" and begin populating the method "Decorate(...)".
    ///
    /// This list is expected to be sorted.
    /// </summary>
    public byte[]? DecorationArray => _decorationArray;
    private byte[]? _decorationArray = null;
    private int DecorationArrayCapacity => _decorationArrayCapacity;
    private int _decorationArrayCapacity = 0;
    public const byte NoneDecorationByte = 0;
    public const byte KeywordDecorationByte = 1;
    
    private const int _defaultCapacity = 4;

    public virtual int GetTabCountOnSameLinePriorToCursor()
    {
        var (lineIndex, linePosStart, linePosEnd) = GetLineInformationExcludingLineEndingCharacterByPositionIndex(PositionIndex);

        // you don't need to know the end of the line, just read backwards until a '\n' or startOfFile, then jump back to starting position and go forward until
        // either a '\n' or EOF
        int count = 0;
        for (int i = linePosStart; i < PositionIndex; i++)
        {
            if (_textBuilder[i] == '\n')
                break;
            if (_textBuilder[i] == '\t')
                count++;
        }

        return count;
    }

    /// <summary>
    /// If you return 'null', then the tooltip is essentially "cancelled" as if the event never occurred.
    /// </summary>
    public virtual string? GetTooltipText(TextEditorTooltip tooltip)
    {
        switch (tooltip.ByteKind)
        {
            case CharacterTooltipByteKind:
                if (_textBuilder.Length > tooltip.StartPositionIndex)
                {
                    return _textBuilder[tooltip.StartPositionIndex].ToString();
                }
                break;
            case TextTooltipByteKind:
                if (_textBuilder.Length > tooltip.StartPositionIndex && _textBuilder.Length > tooltip.EndPositionIndex - 1)
                {
                    return _textBuilder.ToString(tooltip.StartPositionIndex, tooltip.EndPositionIndex - tooltip.StartPositionIndex);
                }
                break;
        }
        return null;
    }

    public virtual void MoveCursor(MoveCursorKind moveCursorKind, bool shiftKey, bool ctrlKey)
    {
        var entryPosition = PositionIndex;
        var entryHasSelection = HasSelection;

        switch (moveCursorKind)
        {
            case MoveCursorKind.ArrowLeft:
                if (!shiftKey && entryHasSelection)
                {
                    if (SelectionAnchor < SelectionEnd)
                        PositionIndex = SelectionAnchor;
                    else
                        PositionIndex = SelectionEnd;

                    (LineIndex, ColumnIndex) = GetLineColumnIndices(PositionIndex);
                }
                else if (ColumnIndex > 0)
                {
                    --ColumnIndex;
                    --PositionIndex;
                    if (ctrlKey)
                    {
                        var originalCharacterKind = GetCharacterKind(_textBuilder[PositionIndex]);
                        var localPositionIndex = PositionIndex;
                        var localColumnIndex = ColumnIndex;
                        while (localColumnIndex - 1 > -1)
                        {
                            if (GetCharacterKind(_textBuilder[localPositionIndex - 1]) == originalCharacterKind)
                            {
                                --localColumnIndex;
                                --localPositionIndex;
                            }
                            else
                            {
                                break;
                            }
                        }
                        PositionIndex = localPositionIndex;
                        ColumnIndex = localColumnIndex;
                    }
                }
                else if (LineIndex > 0)
                {
                    --LineIndex;
                    ColumnIndex = GetLastValidColumnIndex(LineIndex);
                    PositionIndex = GetPositionIndex(LineIndex, ColumnIndex);
                }
                break;
            case MoveCursorKind.ArrowDown:
                if (LineIndex < LineBreakPositionList.Count)
                {
                    ++LineIndex;
                    var lastValidColumnIndex = GetLastValidColumnIndex(LineIndex);
                    if (ColumnIndex > lastValidColumnIndex)
                        ColumnIndex = lastValidColumnIndex;
                    PositionIndex = GetPositionIndex(LineIndex, ColumnIndex);
                }
                break;
            case MoveCursorKind.ArrowUp:
                if (LineIndex > 0)
                {
                    --LineIndex;
                    var lastValidColumnIndex = GetLastValidColumnIndex(LineIndex);
                    if (ColumnIndex > lastValidColumnIndex)
                        ColumnIndex = lastValidColumnIndex;
                    PositionIndex = GetPositionIndex(LineIndex, ColumnIndex);
                }
                break;
            case MoveCursorKind.ArrowRight:
                if (!shiftKey && entryHasSelection)
                {
                    if (SelectionAnchor < SelectionEnd)
                        PositionIndex = SelectionEnd;
                    else
                        PositionIndex = SelectionAnchor;

                    (LineIndex, ColumnIndex) = GetLineColumnIndices(PositionIndex);
                }
                else
                {
                    var lastValidColumnIndex = GetLastValidColumnIndex(LineIndex);
                    if (ColumnIndex < lastValidColumnIndex)
                    {
                        ++ColumnIndex;
                        ++PositionIndex;
                        if (ctrlKey)
                        {
                            var originalCharacterKind = GetCharacterKind(_textBuilder[PositionIndex - 1]);
                            var localPositionIndex = PositionIndex;
                            var localColumnIndex = ColumnIndex;
                            while (localColumnIndex < lastValidColumnIndex)
                            {
                                if (GetCharacterKind(_textBuilder[localPositionIndex]) == originalCharacterKind)
                                {
                                    ++localColumnIndex;
                                    ++localPositionIndex;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            PositionIndex = localPositionIndex;
                            ColumnIndex = localColumnIndex;
                        }
                    }
                    else if (LineIndex < LineBreakPositionList.Count)
                    {
                        ++LineIndex;
                        ColumnIndex = 0;
                        PositionIndex = GetPositionIndex(LineIndex, ColumnIndex);
                    }
                }
                
                break;
            case MoveCursorKind.Home:
                if (ctrlKey)
                {
                    PositionIndex = 0;
                    LineIndex = 0;
                    ColumnIndex = 0;
                }
                else
                {
                    ColumnIndex = 0;
                    PositionIndex = GetPositionIndex(LineIndex, ColumnIndex);
                }
                break;
            case MoveCursorKind.End:
                if (ctrlKey)
                {
                    PositionIndex = Length;
                    (LineIndex, ColumnIndex) = GetLineColumnIndices(PositionIndex);
                }
                else
                {
                    (int _ /*lineIndex*/, int _ /*linePosStart*/, PositionIndex /*linePosEnd*/) = GetLineInformationExcludingLineEndingCharacterByPositionIndex(PositionIndex);
                    (LineIndex, ColumnIndex) = GetLineColumnIndices(PositionIndex);
                }
                break;
        }

        if (shiftKey)
        {
            if (!entryHasSelection)
                SelectionAnchor = entryPosition;

            SelectionEnd = PositionIndex;
        }
        else
        {
            SelectionAnchor = -1;
            SelectionEnd = -1;
        }
    }

    public void SetText(string text)
    {
        InsertTextAtPosition(text, 0, shouldMakeEditHistory: false);
    }

    public void SelectAll()
    {
        SelectionAnchor = 0;
        SelectionEnd = Length;
    }

    /// <summary>
    /// This method uses the user's current position as the insertion point.
    /// and if the positionIndex is <= the user's position index,
    /// then the user's position index is increased by the amount of text inserted
    /// (note that the text ultimately inserted might not be equal to the text parameter
    ///  because line endings are always inserted as '\n' then upon saving the file
    ///  they are written out as the desired line ending)
    ///  
    /// This method internally invokes 'InsertTextAtPosition(text, PositionIndex);'
    ///  
    /// <see cref="InsertTextAtPosition(string, int)"/> and <see cref="InsertTextAtLineColumn(string, int, int)"/>
    /// are alternative methods that one can use to insert at a position that isn't the user's cursor.
    /// 
    /// If "\n", "\r", or "\r\n" appear in the text, "\n" will be inserted in place of it because:
    /// always insert '\n' for line endings, and then track separately the desired line end.
    /// upon saving, create a string that has the '\n' included as the desired line end.
    /// </summary>
    public void InsertText(ReadOnlySpan<char> text)
    {
        if (HasSelection)
            RemoveTextAtPositionByCursor(RemoveKind.DeleteLtr, ctrlKey: false);

        InsertTextAtPosition(text, PositionIndex);
    }

    /// <summary>
    /// This method inserts at the provided lineIndex and columnIndex,
    /// and if the 'calculated positionIndex' is <= the user's position index,
    /// then the user's position index is increased by the amount of text inserted
    /// (note that the text ultimately inserted might not be equal to the text parameter
    ///  because line endings are always inserted as '\n' then upon saving the file
    ///  they are written out as the desired line ending)
    ///  
    /// This method internally invokes 'InsertTextAtPosition(text, GetPositionIndex(lineIndex, columnIndex));'
    /// 
    /// <see cref="InsertText(string)"/> can be used to insert text at the user's current position
    /// if that is the desired insertion point.
    /// </summary>
    public void InsertTextAtLineColumn(string text, int lineIndex, int columnIndex) =>
        InsertTextAtPosition(text, GetPositionIndex(lineIndex, columnIndex));

    private void InsertTextAtPosition_InsertLineBreak(ref int lineBreakInsertedIndex, ref int lineBreakInsertedCount, int entryPositionIndex, int positionIndex)
    {
        if (lineBreakInsertedIndex == -1)
        {
            if (LineBreakPositionList.Count == 0)
            {
                lineBreakInsertedIndex = 0;
            }
            else
            {
                for (int lineBreakIndex = 0; lineBreakIndex < LineBreakPositionList.Count; lineBreakIndex++)
                {
                    if (LineBreakPositionList[lineBreakIndex] >= entryPositionIndex)
                    {
                        lineBreakInsertedIndex = lineBreakIndex;
                        break;
                    }
                }
                if (lineBreakInsertedIndex == -1)
                {
                    lineBreakInsertedIndex = LineBreakPositionList.Count;
                }
            }
        }

        LineBreakPositionList.Insert(lineBreakInsertedIndex + lineBreakInsertedCount++, positionIndex);
    }
    
    private void InsertTextAtPosition_InsertTab(ref int tabInsertedIndex, ref int tabInsertedCount, int entryPositionIndex, int positionIndex)
    {
        if (tabInsertedIndex == -1)
        {
            if (TabPositionList.Count == 0)
            {
                tabInsertedIndex = 0;
            }
            else
            {
                for (int tabIndex = 0; tabIndex < TabPositionList.Count; tabIndex++)
                {
                    if (TabPositionList[tabIndex] >= entryPositionIndex)
                    {
                        tabInsertedIndex = tabIndex;
                        break;
                    }
                }
                if (tabInsertedIndex == -1)
                {
                    tabInsertedIndex = TabPositionList.Count;
                }
            }
        }

        TabPositionList.Insert(tabInsertedIndex + tabInsertedCount++, positionIndex);
    }

    /// <summary>
    /// This method inserts at the provided positionIndex,
    /// and if the positionIndex is <= the user's position index,
    /// then the user's position index is increased by the amount of text inserted
    /// (note that the text ultimately inserted might not be equal to the text parameter
    ///  because line endings are always inserted as '\n' then upon saving the file
    ///  they are written out as the desired line ending)
    /// 
    /// <see cref="InsertText(string)"/> can be used to insert text at the user's current position
    /// if that is the desired insertion point.
    /// </summary>
    public void InsertTextAtPosition(ReadOnlySpan<char> text, int positionIndex, bool shouldMakeEditHistory = true)
    {
        var entryPositionIndex = positionIndex;

        var lineBreakInsertedIndex = -1;
        var lineBreakInsertedCount = 0;

        var tabInsertedIndex = -1;
        var tabInsertedCount = 0;

        var shouldMoveCursor = positionIndex <= PositionIndex;

        for (int i = 0; i < text.Length; i++)
        {
            var character = text[i];

            // always insert '\n' for line endings, and then track separately the desired line end.
            // upon saving, create a string that has the '\n' included as the desired line end.
            //
            // this logic is duplicated in:
            // - SetText(...)
            // - InsertTextAtPosition()
            // - InsertCharacterAtPosition() // only partially duplicated here since it is a char insertion
            //
            if (character == '\n')
            {
                _textBuilder.Insert(positionIndex, '\n');
                InsertTextAtPosition_InsertLineBreak(ref lineBreakInsertedIndex, ref lineBreakInsertedCount, entryPositionIndex, positionIndex);
            }
            else if (character == '\r')
            {
                if (i < text.Length - 1 && text[i + 1] == '\n')
                    ++i;
                _textBuilder.Insert(positionIndex, '\n');
                InsertTextAtPosition_InsertLineBreak(ref lineBreakInsertedIndex, ref lineBreakInsertedCount, entryPositionIndex, positionIndex);
            }
            else if (character == '\t')
            {
                _textBuilder.Insert(positionIndex, '\t');
                InsertTextAtPosition_InsertTab(ref tabInsertedIndex, ref tabInsertedCount, entryPositionIndex, positionIndex);
            }
            else
            {
                _textBuilder.Insert(positionIndex, character);
            }

            ++positionIndex;
        }

        int lineBreakStartIndex;
        if (lineBreakInsertedIndex == -1)
            lineBreakStartIndex = 0;
        else
            lineBreakStartIndex = lineBreakInsertedIndex + lineBreakInsertedCount;
        for (int i = lineBreakStartIndex; i < LineBreakPositionList.Count; i++)
        {
            if (LineBreakPositionList[i] >= entryPositionIndex)
                LineBreakPositionList[i] += positionIndex - entryPositionIndex;
        }

        int tabStartIndex;
        if (tabInsertedIndex == -1)
            tabStartIndex = 0;
        else
            tabStartIndex = tabInsertedIndex + tabInsertedCount;
        for (int i = tabStartIndex; i < TabPositionList.Count; i++)
        {
            if (TabPositionList[i] >= entryPositionIndex)
                TabPositionList[i] += positionIndex - entryPositionIndex;
        }

        if (shouldMoveCursor)
        {
            PositionIndex += positionIndex - entryPositionIndex;
            (LineIndex, ColumnIndex) = GetLineColumnIndices(PositionIndex);
        }

        if (shouldMakeEditHistory)
        {
            var editWasUndone = EditIsUndone;
            EditIsUndone = false;
            if (!editWasUndone && (EditKind == EditKind.InsertLtr && EditPosition + EditLength == entryPositionIndex))
            {
                EditLength += positionIndex - entryPositionIndex;
            }
            else
            {
                EditKind = EditKind.InsertLtr;
                EditPosition = entryPositionIndex;
                EditLength = positionIndex - entryPositionIndex;
            }
        }
    }

    /// <summary>
    /// This method will respect the selection if it exists
    /// </summary>
    public virtual void RemoveTextAtPositionByCursor(RemoveKind removeKind, bool ctrlKey)
    {
        if (HasSelection)
        {
            EditIsUndone = false;

            var start = SelectionAnchor;
            var end = SelectionEnd;

            if (SelectionEnd < SelectionAnchor)
            {
                removeKind = RemoveKind.DeleteLtr;
                start = SelectionEnd;
                end = SelectionAnchor;
            }
            else
            {
                removeKind = RemoveKind.BackspaceRtl;
            }

            SelectionAnchor = -1;
            SelectionEnd = -1;

            RemoveTextAtPositionByRandomAccess(start, end - start, removeKind);
            return;
        }

        if (removeKind == RemoveKind.DeleteLtr)
        {
            if (ctrlKey)
            {
                var count = 1;
                if (ctrlKey)
                {
                    var originalCharacterKind = GetCharacterKind(_textBuilder[PositionIndex]);
                    var (_, _, linePosEnd) = GetLineInformationExcludingLineEndingCharacterByPositionIndex(PositionIndex);
                    while (PositionIndex + count < linePosEnd)
                    {
                        if (GetCharacterKind(_textBuilder[PositionIndex + count]) == originalCharacterKind)
                        {
                            ++count;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                RemoveTextAtPositionByRandomAccess(PositionIndex, count, RemoveKind.DeleteLtr);
            }
            else
            {
                RemoveTextAtPositionByRandomAccess(PositionIndex, 1, RemoveKind.DeleteLtr);
            }
        }
        else if (removeKind == RemoveKind.BackspaceRtl)
        {
            var count = 1;
            if (ctrlKey && ColumnIndex > 0)
            {
                var originalCharacterKind = GetCharacterKind(_textBuilder[PositionIndex - count]);
                var (_, linePosStart, _) = GetLineInformationExcludingLineEndingCharacterByPositionIndex(PositionIndex);
                if (PositionIndex - count > linePosStart)
                {
                    ++count;
                    while (PositionIndex - count >= linePosStart)
                    {
                        if (GetCharacterKind(_textBuilder[PositionIndex - count]) != originalCharacterKind)
                        {
                            --count;
                            break;
                        }
                        else
                        {
                            ++count;
                        }
                    }
                    if (PositionIndex - count < linePosStart)
                    {
                        --count;
                    }
                }
                
                RemoveTextAtPositionByRandomAccess(PositionIndex - count, count, RemoveKind.BackspaceRtl);
            }
            else
            {
                RemoveTextAtPositionByRandomAccess(PositionIndex - 1, 1, RemoveKind.BackspaceRtl);
            }
        }
#if DEBUG
        else
        {
            throw new NotImplementedException();
        }
#endif
    }

    private bool Validate_BatchRemoveBackspaceRtl(bool editWasUndone, int positionIndex, int count)
    {
        return EditKind == EditKind.RemoveBackspaceRtl && !editWasUndone && (positionIndex + count == EditPosition);
    }

    private bool Validate_BatchRemoveDeleteLtr(bool editWasUndone, int positionIndex, int count)
    {
        return EditKind == EditKind.RemoveDeleteLtr && !editWasUndone && EditPosition == positionIndex;
    }

    public void History_EnsureCapacity(int totalEditLength)
    {
        if (_editedTextHistoryCapacity >= totalEditLength)
            return;
        
        int newCapacity = _editedTextHistoryCapacity * 2;
        // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
        // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
        if (newCapacity < totalEditLength) newCapacity = totalEditLength;

        var newArray = new char[newCapacity];
        Array.Copy(_editedTextHistory, 0, newArray, 0, _editedTextHistoryCount);
        _editedTextHistory = newArray;
    }

    /// <summary>
    /// This method ignores the selection
    /// </summary>
    public virtual void RemoveTextAtPositionByRandomAccess(int positionIndex, int count, RemoveKind removeKind, bool shouldMakeEditHistory = true)
    {
        if (positionIndex < 0)
            return;
        if (positionIndex >= Length)
            return;

        if (shouldMakeEditHistory)
        {
            var editWasUndone = EditIsUndone;
            EditIsUndone = false;
            if (removeKind == RemoveKind.BackspaceRtl)
            {
                if (Validate_BatchRemoveBackspaceRtl(editWasUndone, positionIndex, count))
                {
                    EditPosition = positionIndex;
                    History_EnsureCapacity(EditLength += count);
                    Array.Copy(_editedTextHistory, 0, _editedTextHistory, count, _editedTextHistoryCount);
                    _editedTextHistoryCount += count;
                    for (int editHistoryIndex = 0, i = positionIndex; editHistoryIndex < count; editHistoryIndex++, i++)
                    {
                        _editedTextHistory[editHistoryIndex] = this[i];
                    }
                }
                else
                {
                    _editedTextHistoryCount = 0;
                    EditKind = EditKind.RemoveBackspaceRtl;
                    EditPosition = positionIndex;
                    History_EnsureCapacity(EditLength = count);
                    _editedTextHistoryCount = EditLength;
                    for (int editHistoryIndex = 0, i = EditPosition; editHistoryIndex < EditLength; editHistoryIndex++, i++)
                    {
                        _editedTextHistory[editHistoryIndex] = this[i];
                    }
                }
            }
            else if (removeKind == RemoveKind.DeleteLtr)
            {
                if (Validate_BatchRemoveDeleteLtr(editWasUndone, positionIndex, count))
                {
                    History_EnsureCapacity(EditLength += count);
                    for (int editHistoryIndex = _editedTextHistoryCount, i = EditPosition; editHistoryIndex < EditLength; editHistoryIndex++, i++)
                    {
                        _editedTextHistory[editHistoryIndex] = this[i];
                    }
                    _editedTextHistoryCount = EditLength;
                }
                else
                {
                    _editedTextHistoryCount = 0;
                    EditKind = EditKind.RemoveDeleteLtr;
                    EditPosition = positionIndex;
                    History_EnsureCapacity(EditLength = count);
                    _editedTextHistoryCount = EditLength;
                    for (int editHistoryIndex = 0, i = EditPosition; editHistoryIndex < EditLength; editHistoryIndex++, i++)
                    {
                        _editedTextHistory[editHistoryIndex] = this[i];
                    }
                }
            }
        }

        var start = positionIndex;
        var end = positionIndex + count;

        _textBuilder.Remove(positionIndex, count);

        var lineBreakOriginalCount = LineBreakPositionList.Count;

        for (var i = LineBreakPositionList.Count - 1; i >= 0; i--)
        {
            if (LineBreakPositionList[i] >= end)
                LineBreakPositionList[i] -= count;
            else if (LineBreakPositionList[i] >= start)
                LineBreakPositionList.RemoveAt(i);
        }
        
        for (var i = TabPositionList.Count - 1; i >= 0; i--)
        {
            if (TabPositionList[i] >= end)
                TabPositionList[i] -= count;
            else if (TabPositionList[i] >= start)
                TabPositionList.RemoveAt(i);
        }

        if (PositionIndex > start)
        {
            PositionIndex -= count;
            // If a selection leaves the cursor as is (such as the current implementation of Ctrl + A)
            // then this logic gives a negative value.
            if (PositionIndex < 0)
                PositionIndex = 0;
            (LineIndex, ColumnIndex) = GetLineColumnIndices(PositionIndex);
        }
    }

    public virtual (int lineIndex, int linePosStart, int linePosEnd) GetLineInformationExcludingLineEndingCharacterByPositionIndex(int positionIndex)
    {
        if (LineBreakPositionList.Count == 0)
            return (0, 0, GetLastValidColumnIndex(0));

        for (int i = 0; i < LineBreakPositionList.Count; i++)
        {
            if (LineBreakPositionList[i] >= positionIndex)
            {
                if (i == 0)
                    return (0, 0, GetLastValidColumnIndex(0));
                else
                    return (i, LineBreakPositionList[i - 1] + 1, LineBreakPositionList[i]);
            }
        }

        return (LineBreakPositionList.Count, LineBreakPositionList[^1] + 1, Length);
    }

    public (int lineIndex, int columnIndex) GetLineColumnIndices(int positionIndex)
    {
        int lastValidColumnIndex;
        int lineIndex;
        int columnIndex;

        if (LineBreakPositionList.Count == 0)
        {
            lastValidColumnIndex = GetLastValidColumnIndex(0);
            return positionIndex > lastValidColumnIndex
                ? (0, lastValidColumnIndex)
                : (0, positionIndex);
        }

        for (int i = 0; i < LineBreakPositionList.Count; i++)
        {
            if (LineBreakPositionList[i] >= positionIndex)
            {
                if (i == 0)
                {
                    lastValidColumnIndex = GetLastValidColumnIndex(0);
                    return positionIndex > lastValidColumnIndex
                        ? (0, lastValidColumnIndex)
                        : (0, positionIndex);
                }
                else
                {
                    lineIndex = i;
                    columnIndex = positionIndex - (LineBreakPositionList[i - 1] + 1);
                    lastValidColumnIndex = GetLastValidColumnIndex(lineIndex);
                    if (columnIndex > lastValidColumnIndex)
                        columnIndex = lastValidColumnIndex;
                    return (lineIndex, columnIndex);
                }
            }
        }

        lineIndex = LineBreakPositionList.Count;
        columnIndex = positionIndex - (LineBreakPositionList[^1] + 1);
        lastValidColumnIndex = GetLastValidColumnIndex(lineIndex);
        if (columnIndex > lastValidColumnIndex)
            columnIndex = lastValidColumnIndex;
        return (lineIndex, columnIndex);
    }

    /// <summary>
    /// If provided an invalid lineIndex or columnIndex this method will re-invoke itself
    /// with the closest valid lineIndex, and columnIndex.
    /// 
    /// The method validates lineIndex first, then checks if the columnIndex provided
    /// is valid for the validated lineIndex.
    /// 
    /// If the provided columnIndex exists on the validated lineIndex then the columnIndex stays the same.
    /// Otherwise the columnIndex is then changed to the closest valid columnIndex for the given line.
    /// </summary>
    public int GetPositionIndex(int lineIndex, int columnIndex)
    {
        int lastValidColumnIndex;

        if (lineIndex == 0)
        {
            lastValidColumnIndex = GetLastValidColumnIndex(lineIndex);
            if (columnIndex > lastValidColumnIndex)
                columnIndex = lastValidColumnIndex;
            return columnIndex;
        }

        for (int i = 0; i < LineBreakPositionList.Count; i++)
        {
            if (i + 1 == lineIndex)
            {
                lastValidColumnIndex = GetLastValidColumnIndex(lineIndex);
                if (columnIndex > lastValidColumnIndex)
                    columnIndex = lastValidColumnIndex;
                return LineBreakPositionList[i] + 1 + columnIndex;
            }
        }

        if (LineBreakPositionList.Count < lineIndex)
            lineIndex = LineBreakPositionList.Count;

        lastValidColumnIndex = GetLastValidColumnIndex(lineIndex);
        if (columnIndex > lastValidColumnIndex)
            columnIndex = lastValidColumnIndex;

        return GetPositionIndex(lineIndex, columnIndex);
    }

    /// <summary>
    /// If there is no valid column index then '-1' is returned.
    /// 
    /// This may seem slightly contrary to the <see cref="GetPositionIndex(int, int)"/> and <see cref="TryGetPositionIndex(int, int, out int)"/>
    /// pattern. But this method checks for a valid column index specifically, so the only option is to return a valid columnIndex or -1.
    /// </summary>
    public int GetLastValidColumnIndex(int lineIndex)
    {
        if (lineIndex == 0)
        {
            if (LineBreakPositionList.Count == 0)
            {
                return Length;
            }
            else
            {
                return LineBreakPositionList[lineIndex];
            }
        }
        else if (LineBreakPositionList.Count > lineIndex)
        {
            return LineBreakPositionList[lineIndex] - (LineBreakPositionList[lineIndex - 1] + 1);
        }
        else if (LineBreakPositionList.Count == lineIndex)
        {
            return Length - (LineBreakPositionList[LineBreakPositionList.Count - 1] + 1);
        }
        else
        {
            return -1;
        }
    }

    public virtual CharacterKind GetCharacterKind(char character)
    {
        // I considered using ASCII codes but I think the switch is faster and it won't take that long.
        switch (character)
        {
            /* Lowercase Letters */
            case 'a':
            case 'b':
            case 'c':
            case 'd':
            case 'e':
            case 'f':
            case 'g':
            case 'h':
            case 'i':
            case 'j':
            case 'k':
            case 'l':
            case 'm':
            case 'n':
            case 'o':
            case 'p':
            case 'q':
            case 'r':
            case 's':
            case 't':
            case 'u':
            case 'v':
            case 'w':
            case 'x':
            case 'y':
            case 'z':
            /* Uppercase Letters */
            case 'A':
            case 'B':
            case 'C':
            case 'D':
            case 'E':
            case 'F':
            case 'G':
            case 'H':
            case 'I':
            case 'J':
            case 'K':
            case 'L':
            case 'M':
            case 'N':
            case 'O':
            case 'P':
            case 'Q':
            case 'R':
            case 'S':
            case 'T':
            case 'U':
            case 'V':
            case 'W':
            case 'X':
            case 'Y':
            case 'Z':
            /* Underscore */
            case '_':
            /* Digits */
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                return CharacterKind.LetterOrDigit;
            case ' ':
            case '\t':
            case '\r':
            case '\n':
                return CharacterKind.Whitespace;
            default:
                return CharacterKind.Punctuation;
        }
    }

    /// <summary>
    /// Maybe this is more a remark...
    /// but something to keep in mind when wanting plain text MIGHT be:
    ///
    /// Returning 'null' avoids the HTML attribute name 'class' from being written.
    /// Whereas 'string.Empty' will still write the HTML attribute name 'class'.
    ///
    ///
    /// But, if you ever wanted to change the color of plain text you'd be in trouble
    /// without a CSS class to target.
    /// </summary>
    public virtual string? DecorationMapToCssClass(byte decorationByte)
    {
        switch (decorationByte)
        {
            case NoneDecorationByte:
                return null;
            case KeywordDecorationByte:
                return "te_k";
            default:
                return null;
        }
    }

    public virtual string? GetSelection()
    {
        if (!HasSelection)
            return null;

        var start = SelectionAnchor;
        var end = SelectionEnd;

        if (SelectionEnd < SelectionAnchor)
        {
            var temp = start;
            start = end;
            end = temp;
        }

        var selectionBuilder = new StringBuilder(capacity: end - start);
        for (int i = start; i < end; i++)
        {
            selectionBuilder.Append(_textBuilder[i]);
        }

        return selectionBuilder.ToString();
    }

    public virtual void ReceiveKeyboardDebounce()
    {
        if (TooltipList is not null)
            TooltipList.Clear();
        Decorate(0, _textBuilder.Length, NoneDecorationByte);

        int position = 0;
        while (position < _textBuilder.Length)
        {
            switch (_textBuilder[position])
            {
                /* Lowercase Letters */
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                /* Uppercase Letters */
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                /* Underscore */
                case '_':
                    LexIdentifierOrKeywordOrKeywordContextual(ref position);
                    break;
            }

            ++position;
        }
    }

    private void LexIdentifierOrKeywordOrKeywordContextual(ref int position)
    {
        var entryPosition = position;
        int characterIntSum = 0;

        while (position < _textBuilder.Length)
        {
            if (!char.IsLetterOrDigit(_textBuilder[position]) &&
                _textBuilder[position] != '_')
            {
                break;
            }

            characterIntSum += _textBuilder[position];
            ++position;
        }

        var textSpanLength = position - entryPosition;

        // t 116
        // e 101
        // s 115
        // t 116
        // =====
        //   448

        switch (characterIntSum)
        {
            case 448: // test
                if (textSpanLength == 4 &&
                    _textBuilder[entryPosition + 0] == 't' &&
                    _textBuilder[entryPosition + 1] == 'e' &&
                    _textBuilder[entryPosition + 2] == 's' &&
                    _textBuilder[entryPosition + 3] == 't')
                {
                    if (DecorationArray is not null)
                    {
                        Decorate(entryPosition, position, KeywordDecorationByte);
                    }

                    if (TooltipList is not null)
                    {
                        TooltipList.Add(new TextEditorTooltip(
                            entryPosition,
                            position,
                            foreignKey: 0,
                            byteKind: TextTooltipByteKind));
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 'startPosition' is inclusive
    /// 'endPosition' is exclusive
    /// </summary>
    public void Decorate(int startPosition, int endPosition, byte decorationByte)
    {
        if (DecorationArray is null)
            return;

        DecorateEnsureCapacityWritable();

        // trim excess when removing large amounts of text?

        for (int i = startPosition; i < endPosition; i++)
        {
            DecorationArray[i] = decorationByte;
        }
    }

    /// <summary>Various parts of the List.cs source code were pasted/modified in here</summary>
    public void DecorateEnsureCapacityWritable()
    {
        if (_decorationArrayCapacity < _textBuilder.Length)
        {
            int newCapacity = _textBuilder.Length == 0 ? _defaultCapacity : _decorationArrayCapacity * 2;
            // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
            if (newCapacity < _textBuilder.Length) newCapacity = _textBuilder.Length;

            var newArray = new byte[newCapacity];
            Array.Copy(DecorationArray, 0, newArray, 0, _decorationArrayCapacity);

            _decorationArrayCapacity = newCapacity;
            _decorationArray = newArray;
        }
    }

    /// <summary>Various parts of the List.cs source code were pasted/modified in here</summary>
    public void EnableDecorations()
    {
        if (DecorationArray is not null)
            return;

        int newCapacity;

        if (_textBuilder.Length == 0)
        {
            newCapacity = _defaultCapacity;
        }
        else
        {
            newCapacity = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)_textBuilder.Length);
            // Why does my IDE code say '< -1'???
            if (newCapacity <= 0)
            {
                newCapacity = _defaultCapacity;
            }
            else
            {
                if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
                if (newCapacity < _textBuilder.Length) newCapacity = _textBuilder.Length;
            }
        }

        _decorationArrayCapacity = newCapacity;
        _decorationArray = new byte[_decorationArrayCapacity];
    }

    /*
     /// <summary>
    /// If provided an invalid lineIndex or columnIndex, this method will return false and set the out int index to -1.
    /// </summary>
    public bool TryGetPositionIndex(int lineIndex, int columnIndex, out int index)
    {
        if (lineIndex == 0)
        {
            index = columnIndex;
            return true;
        }

        for (int i = 0; i < LineBreakPositionList.Count; i++)
        {
            if (i + 1 == lineIndex)
            {
                index = LineBreakPositionList[i] + 1 + columnIndex;
                return true;
            }
        }

        index = -1;
        return false;
    }
     */

    /*
    /// <summary>
    /// See <see cref="InsertText(string)"/> for explanation, this method is the same but with a char.
    /// </summary>
    public void InsertCharacter(char character) => InsertCharacterAtPosition(character, PositionIndex);

    /// <summary>
    /// See <see cref="InsertTextAtLineColumn(string, int, int)"/> for explanation, this method is the same but with a char.
    /// </summary>
    public void InsertCharacterAtLineColumn(char character, int lineIndex, int columnIndex) =>
        InsertCharacterAtPosition(character, GetPositionIndex(lineIndex, columnIndex));

    /// <summary>
    /// See <see cref="InsertTextAtPosition(string, int)"/> for explanation, this method is the same but with a char.
    /// </summary>
    public void InsertCharacterAtPosition(char character, int positionIndex)
    {
        // always insert '\n' for line endings, and then track separately the desired line end.
        // upon saving, create a string that has the '\n' included as the desired line end.
        //
        // this logic is duplicated in:
        // - SetText(...)
        // - InsertTextAtPosition()
        // - InsertCharacterAtPosition() // only partially duplicated here since it is a char insertion
        //
        if (character == '\n')
        {
            _textBuilder.Insert(positionIndex, '\n');
        }
        else if (character == '\r')
        {
            _textBuilder.Insert(positionIndex, '\n');
            LineBreakPositionList.Add(positionIndex);
        }
        else
        {
            _textBuilder.Append(character);
        }

        _textBuilder.Insert(positionIndex, character);
    }
    */
}
