using System;
using System.Text;

namespace TextEditor;

public class TextEditorModel
{
    private readonly StringBuilder _textBuilder = new();
    /// <summary>
    /// The gap buffer is being added, and presumably you wouldn't want
    /// such a "clean/natural" way to get the _textBuilder.
    /// 
    /// So wrapped the protected access to it in a method makes it "feel awkward"
    /// and thus notifies the person that they might be going down the wrong route,
    /// i.e.: that they want to say something along the lines of 'this[i]'
    /// </summary>
    protected StringBuilder GetTextBuilder() => _textBuilder;

    /// <summary>
    /// An insertion gap buffer is a single '\0' in the _textBuilder.
    /// 
    /// When deleting, the deleted text is replaced with '\0'
    /// 
    /// When squashing the insertion gap buffer the singular '\0' is replaced with the text in the gap buffer.
    /// 
    /// When squashing the deleting, the contiguous '\0' are removed from the _textBuilder in bulk.
    /// 
    /// You cannot differentiate an insertion vs deleting gap buffer scenario by the single '\0' alone,
    /// because perhaps you deleted a single character.
    /// 
    /// So you need to look at the current EditKind as well.
    /// 
    /// ===========
    /// 3 case
    /// - prior to the gap buffer
    ///     - "nothing" needs to be done
    /// - within the gap buffer
    ///     - reading from the gap buffer
    /// - after the gap buffer
    ///     - offset by the length of the gap buffer
    ///     
    /// ============
    /// 
    /// I think I'm gonna keep the linend positions and tabs
    /// up to date at all times.
    /// 
    /// Because they theoretically are much smaller lists than the content itself.
    /// 
    /// (although
    ///     1: this isn't necessarily the case,
    ///     2: they're still a bit large and it isn't desired to keep them up to date I imagine)
    ///    
    /// But I gotta do one thing at time.
    /// </summary>
    protected readonly StringBuilder _gapBuffer = new();

    public bool GapBufferIsEmpty => _gapBuffer.Length == 0;
    public void __DEBUG_AppendGapBuffer(char character)
    {
        _gapBuffer.Append(character);
    }

    public void SomethingBadHappenedButDontCrashSomeonesApp()
    {

    }

    public char this[int index]
    {
        get
        {
            switch (EditKind)
            {
                case EditKind.None:
                    return _textBuilder[index];
                case EditKind.InsertLtr:
                    if (index < EditPosition)
                    {
                        return _textBuilder[index];
                    }
                    else if (index >= EditPosition && index <= EditPosition + _gapBuffer.Length - 1)
                    {
                        return _gapBuffer[index - EditPosition];
                    }
                    else 
                    {
                        return _textBuilder[index - _gapBuffer.Length];
                    }
                case EditKind.RemoveDeleteLtr:
                case EditKind.RemoveBackspaceRtl:
                    throw new NotImplementedException();
                default:
#if DEBUG
                    throw new NotImplementedException();
#else
                    // ???
                    SomethingBadHappenedButDontCrashSomeonesApp();
                    return 'a';
#endif

            }
        }
    }

    public override string ToString()
    {
        return _textBuilder.ToString();
    }

    public int Length
    {
        get
        {
            switch (EditKind)
            {
                case EditKind.None:
                    return _textBuilder.Length;
                case EditKind.InsertLtr:
                    // -1 for the '\0' that represents the inserted gap buffer
                    return _textBuilder.Length - 1 + _gapBuffer.Length;
                case EditKind.RemoveDeleteLtr:
                case EditKind.RemoveBackspaceRtl:
                    throw new NotImplementedException();
                default:
#if DEBUG
                    throw new NotImplementedException();
#else
                    // ???
                    SomethingBadHappenedButDontCrashSomeonesApp();
                    return 'a';
#endif
            }
        }
    }

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

    public bool HasSelection => SelectionAnchor != SelectionEnd;

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
    protected byte[]? _decorationArray = null;
    protected int DecorationArrayCapacity => _decorationArrayCapacity;
    protected int _decorationArrayCapacity = 0;
    public const byte NoneDecorationByte = 0;
    public const byte KeywordDecorationByte = 1;

    protected const int _defaultCapacity = 4;

    public virtual int GetTabCountOnSameLinePriorToCursor()
    {
        var (lineIndex, linePosStart, linePosEnd) = GetLineInformationExcludingLineEndingCharacterByPositionIndex(PositionIndex);

        // you don't need to know the end of the line, just read backwards until a '\n' or startOfFile, then jump back to starting position and go forward until
        // either a '\n' or EOF
        int count = 0;
        for (int i = linePosStart; i < PositionIndex; i++)
        {
            if (this[i] == '\n')
                break;
            if (this[i] == '\t')
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
                if (this.Length > tooltip.StartPositionIndex)
                {
                    return this[tooltip.StartPositionIndex].ToString();
                }
                break;
            case TextTooltipByteKind:
                if (this.Length > tooltip.StartPositionIndex && this.Length > tooltip.EndPositionIndex - 1)
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
                        var originalCharacterKind = GetCharacterKind(this[PositionIndex]);
                        var localPositionIndex = PositionIndex;
                        var localColumnIndex = ColumnIndex;
                        while (localColumnIndex - 1 > -1)
                        {
                            if (GetCharacterKind(this[localPositionIndex - 1]) == originalCharacterKind)
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
                            var originalCharacterKind = GetCharacterKind(this[PositionIndex - 1]);
                            var localPositionIndex = PositionIndex;
                            var localColumnIndex = ColumnIndex;
                            while (localColumnIndex < lastValidColumnIndex)
                            {
                                if (GetCharacterKind(this[localPositionIndex]) == originalCharacterKind)
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
            SelectionAnchor = 0;
            SelectionEnd = 0;
        }
    }

    public void Clear()
    {
        _textBuilder.Clear();
        _gapBuffer.Clear();
        LineBreakPositionList.Clear();
        TabPositionList.Clear();
        PositionIndex = 0;
        LineIndex = 0;
        ColumnIndex = 0;
        SelectionAnchor = 0;
        SelectionEnd = 0;
        _editedTextHistoryCount = 0;
        EditIsUndone = false;
        EditPosition = 0;
        EditLength = 0;
        EditKind = EditKind.None;
    }

    public void SetText(string text)
    {
        Clear();

        InsertTextAtPosition(text, 0, stringBuilder: _textBuilder, shouldMakeEditHistory: false);
        PositionIndex = 0;
        LineIndex = 0;
        ColumnIndex = 0;
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

    protected void InsertTextAtPosition_InsertLineBreak(ref int lineBreakInsertedIndex, ref int lineBreakInsertedCount, int entryPositionIndex, int positionIndex)
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

    protected void InsertTextAtPosition_InsertTab(ref int tabInsertedIndex, ref int tabInsertedCount, int entryPositionIndex, int positionIndex)
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
    public void InsertTextAtPosition(ReadOnlySpan<char> text, int positionIndex, StringBuilder? stringBuilder = null, bool shouldMakeEditHistory = true)
    {
        stringBuilder ??= _gapBuffer;

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
                stringBuilder.Insert(positionIndex, '\n');
                InsertTextAtPosition_InsertLineBreak(ref lineBreakInsertedIndex, ref lineBreakInsertedCount, entryPositionIndex, positionIndex);
            }
            else if (character == '\r')
            {
                if (i < text.Length - 1 && text[i + 1] == '\n')
                    ++i;
                stringBuilder.Insert(positionIndex, '\n');
                InsertTextAtPosition_InsertLineBreak(ref lineBreakInsertedIndex, ref lineBreakInsertedCount, entryPositionIndex, positionIndex);
            }
            else if (character == '\t')
            {
                stringBuilder.Insert(positionIndex, '\t');
                InsertTextAtPosition_InsertTab(ref tabInsertedIndex, ref tabInsertedCount, entryPositionIndex, positionIndex);
            }
            else
            {
                stringBuilder.Insert(positionIndex, character);
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
                SquashEdits();

                EditKind = EditKind.InsertLtr;
                EditPosition = entryPositionIndex;
                EditLength = positionIndex - entryPositionIndex;
            }
        }
    }

    /// <summary>
    /// The decorations can be shifted by the current contiguous edit.
    /// 
    /// Anytime the contiguous edit moves, you need to make sure the previous edit
    /// modifies the decorations for good so the shift is persisted.
    /// </summary>
    public void SquashEdits()
    {
        /*
        if (DecorationArray is null)
            return;

        DecorateEnsureCapacityWritable();

        // hmm
        Array.Copy(DecorationArray, EditPosition, DecorationArray, EditPosition + EditLength, Length - EditPosition);
        */
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

            SelectionAnchor = 0;
            SelectionEnd = 0;

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
                    var originalCharacterKind = GetCharacterKind(this[PositionIndex]);
                    var (_, _, linePosEnd) = GetLineInformationExcludingLineEndingCharacterByPositionIndex(PositionIndex);
                    while (PositionIndex + count < linePosEnd)
                    {
                        if (GetCharacterKind(this[PositionIndex + count]) == originalCharacterKind)
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
                var originalCharacterKind = GetCharacterKind(this[PositionIndex - count]);
                var (_, linePosStart, _) = GetLineInformationExcludingLineEndingCharacterByPositionIndex(PositionIndex);
                if (PositionIndex - count > linePosStart)
                {
                    ++count;
                    while (PositionIndex - count >= linePosStart)
                    {
                        if (GetCharacterKind(this[PositionIndex - count]) != originalCharacterKind)
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

    protected bool Validate_BatchRemoveBackspaceRtl(bool editWasUndone, int positionIndex, int count)
    {
        return EditKind == EditKind.RemoveBackspaceRtl && !editWasUndone && (positionIndex + count == EditPosition);
    }

    protected bool Validate_BatchRemoveDeleteLtr(bool editWasUndone, int positionIndex, int count)
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

        // Delete from the gap buffer is another case...
        // unless I write the gap buffer then remove.
        // Honestly it's probably a good idea to do that first since I've never written a gap buffer.
        // It doesn't seem too bad but I don't wanna get too complicated and fall flat on my face either.

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
                    SquashEdits();
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
                    SquashEdits();
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

        // this has a few overloads...:
        // _textBuilder.Replace();
        //
        // gonna just for loop for now
        for (int i = positionIndex; i < positionIndex + count; i++)
        {
            _textBuilder[i] = '\0';
        }

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
            selectionBuilder.Append(this[i]);
        }

        return selectionBuilder.ToString();
    }

    public virtual void ReceiveKeyboardDebounce()
    {
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
        if (_decorationArrayCapacity < this.Length)
        {
            int newCapacity = this.Length == 0 ? _defaultCapacity : _decorationArrayCapacity * 2;
            // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
            if (newCapacity < this.Length) newCapacity = this.Length;

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

        if (this.Length == 0)
        {
            newCapacity = _defaultCapacity;
        }
        else
        {
            newCapacity = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)this.Length);
            // Why does my IDE code say '< -1'???
            if (newCapacity <= 0)
            {
                newCapacity = _defaultCapacity;
            }
            else
            {
                if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
                if (newCapacity < this.Length) newCapacity = this.Length;
            }
        }

        _decorationArrayCapacity = newCapacity;
        _decorationArray = new byte[_decorationArrayCapacity];
    }

    /*
     * I'm actually not too sure how I would do this yet so ima comment this out.
     * 
    /// <summary>
    /// Ensure the editor will put the decoration logic in a GC-collectable state
    /// </summary>
    public void DisableDecorations()
    {
        _decorationArray = null;
        _decorationArrayCapacity = 0;
    }

    /// <summary>
    /// Copy, paste, modify; of List.TrimExcess() source code.
    /// 
    /// Ensure the editor will shrink the size of its internal buffers if asked to do so,
    /// and it is possible to do so.
    /// 
    /// Returns true if any buffers were successfully shrunk.
    /// Otherwise, returns false if no buffers were able to be shrunk.
    /// 
    /// List source code has this comment (the indented text):
    ///     To completely clear a list and
    ///     release all memory referenced by the list, execute the following
    ///     statements:
    ///     
    ///     list.Clear();
    ///     list.TrimExcess();
    ///     
    /// And I'll support that as well.
    /// </summary>
    public void TrimExcess()
    {
        /*
        readonly StringBuilder _textBuilder = new();
        char[] _editedTextHistory = new char[4];
        List<TextEditorTooltip>? TooltipList { get; set; } = null;
        List<int> LineBreakPositionList { get; set; } = new();
        List<int> TabPositionList { get; set; } = new();
        byte[]? DecorationArray => _decorationArray;
        byte[]? _decorationArray = null;
        *//*
        _textBuilder.

        int threshold = (int)(((double)_items.Length) * 0.9);
        if (_size < threshold)
        {
            Capacity = _size;
        }
    }
    */
}
