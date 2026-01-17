using System.Text;

namespace TextEditor;

public partial class TextEditorModel
{
    // bug likely 'case H' start
    public override string ToString()
    {
        return _textBuilder.ToString();
    }
    // bug likely 'case H' end

    // bug likely 'case G' start
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
                    if (index < EditPosition)
                    {
                        return _textBuilder[index];
                    }
                    else if (index >= EditPosition && index < EditPosition + EditLength)
                    {
                        return _textBuilder[index + EditLength];
                    }
                    else
                    {
                        return _textBuilder[index + EditLength];
                    }
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
    // bug likely 'case G' end

    // bug likely 'case I' start
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
                    return _textBuilder.Length - EditLength;
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
    // bug likely 'case I' end

    /// <summary>
    /// This method inserts at the provided positionIndex, and if the positionIndex is <= the user's position index, then the user's position index is increased by the amount of text inserted
    /// (note that the text ultimately inserted might not be equal to the text parameter because line endings are always inserted as '\n' then upon saving the file they are written out as the desired line ending)
    /// 
    /// <see cref="InsertText(string)"/> can be used to insert text at the user's current position if that is the desired insertion point.
    /// 
    /// 'bool __unsafe__insertDirectly' should remain as the default false value 99.9% of the time, it is intended for internal use only.
    /// </summary>
    public void InsertTextAtPosition(ReadOnlySpan<char> text, int positionIndex, bool shouldMakeEditHistory = true, bool __unsafe__insertDirectly = false)
    {
        var insertionIndex = positionIndex;

        bool batchEdits = false;

        // bug likely 'case A' start
        if (shouldMakeEditHistory)
        {
            if (!EditIsUndone && (EditKind == EditKind.InsertLtr && EditPosition + EditLength == positionIndex))
            {
                batchEdits = true;
            }
            else
            {
                SquashEdits();
                EditPosition = positionIndex;
            }
        }
        // bug likely 'case A' end

        StringBuilder stringBuilder;
        if (__unsafe__insertDirectly)
        {
            stringBuilder = _textBuilder;
        }
        else
        {
            stringBuilder = _gapBuffer;
            insertionIndex -= EditPosition;
        }

        var entryPositionIndex = positionIndex;
        
        var lineBreakInsertedIndex = -1;
        var lineBreakInsertedCount = 0;

        var tabInsertedIndex = -1;
        var tabInsertedCount = 0;

        var shouldMoveCursor = positionIndex <= PositionIndex;

        for (int i = 0; i < text.Length; i++)
        {
            var character = text[i];

            // always insert '\n' for line endings, and then track separately the desired line end. upon saving, create a string that has the '\n' included as the desired line end.
            if (character == '\n')
            {
                stringBuilder.Insert(insertionIndex, '\n');
                InsertTextAtPosition_InsertLineBreak(ref lineBreakInsertedIndex, ref lineBreakInsertedCount, entryPositionIndex, positionIndex);
            }
            else if (character == '\r')
            {
                if (i < text.Length - 1 && text[i + 1] == '\n')
                    ++i;
                stringBuilder.Insert(insertionIndex, '\n');
                InsertTextAtPosition_InsertLineBreak(ref lineBreakInsertedIndex, ref lineBreakInsertedCount, entryPositionIndex, positionIndex);
            }
            else if (character == '\t')
            {
                stringBuilder.Insert(insertionIndex, '\t');
                InsertTextAtPosition_InsertTab(ref tabInsertedIndex, ref tabInsertedCount, entryPositionIndex, positionIndex);
            }
            else
            {
                stringBuilder.Insert(insertionIndex, character);
            }

            ++insertionIndex;
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
            if (batchEdits)
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

    /// <summary>This method ignores the selection</summary>
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
                    // bug likely 'case B' start
                    History_EnsureCapacity(EditLength + count);
                    Array.Copy(_editedTextHistory, 0, _editedTextHistory, count, EditedTextHistoryCount);
                    for (int editHistoryIndex = 0, i = positionIndex; editHistoryIndex < count; editHistoryIndex++, i++)
                    {
                        _editedTextHistory[editHistoryIndex] = this[i];
                    }
                    EditLength += count;
                    EditPosition = positionIndex;
                    EditedTextHistoryCount += count;
                    // bug likely 'case B' end
                }
                else
                {
                    // bug likely 'case C' start
                    SquashEdits();
                    EditedTextHistoryCount = 0;
                    EditKind = EditKind.RemoveBackspaceRtl;
                    EditPosition = positionIndex;
                    History_EnsureCapacity(EditLength = count);
                    EditedTextHistoryCount = EditLength;
                    for (int editHistoryIndex = 0, i = EditPosition; editHistoryIndex < EditLength; editHistoryIndex++, i++)
                    {
                        // squash then update edit then try to read index => exception
                        _editedTextHistory[editHistoryIndex] = _textBuilder[i];
                    }
                    // bug likely 'case C' end
                }
            }
            else if (removeKind == RemoveKind.DeleteLtr)
            {
                if (Validate_BatchRemoveDeleteLtr(editWasUndone, positionIndex, count))
                {
                    // bug likely 'case D' start
                    History_EnsureCapacity(EditLength + count);
                    for (int editHistoryIndex = EditedTextHistoryCount, i = EditPosition; editHistoryIndex < EditLength; editHistoryIndex++, i++)
                    {
                        _editedTextHistory[editHistoryIndex] = this[i];
                    }
                    EditLength += count;
                    EditedTextHistoryCount = EditLength;
                    // bug likely 'case D' end
                }
                else
                {
                    // bug likely 'case E' start
                    SquashEdits();
                    History_EnsureCapacity(count);
                    for (int editHistoryIndex = 0, i = EditPosition; editHistoryIndex < EditLength; editHistoryIndex++, i++)
                    {
                        // squash then update edit then try to read index => exception
                        _editedTextHistory[editHistoryIndex] = _textBuilder[i];
                    }
                    EditedTextHistoryCount = 0;
                    EditKind = EditKind.RemoveDeleteLtr;
                    EditPosition = positionIndex;
                    EditLength = count;
                    EditedTextHistoryCount = EditLength;
                    // bug likely 'case E' end
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

    public void SquashEdits()
    {
        // bug likely 'case F' start
        switch (EditKind)
        {
            case EditKind.None:
                break;
            case EditKind.InsertLtr:
                _textBuilder.Insert(EditPosition, _gapBuffer);
                _gapBuffer.Clear();
                break;
            case EditKind.RemoveBackspaceRtl:
            case EditKind.RemoveDeleteLtr:
                _textBuilder.Remove(EditPosition, EditLength);
                break;
            default:
#if DEBUG
                throw new NotImplementedException();
#else
                SomethingBadHappenedButDontCrashSomeonesApp();
                return;
#endif
        }

        // Perhaps long term these will be redundant.
        // But you probably wanna ensure the state is being cleared when you squash
        // lest you have some odd bug and it turns out some random value wasn't cleared
        // and you're going down some rabit hole to figure out what's happening.
        //
        // (redundant because the next edit will set these properties)
        //
        EditedTextHistoryCount = 0;
        EditIsUndone = false;
        EditPosition = 0;
        EditLength = 0;
        EditKind = EditKind.None;
        // bug likely 'case F' end
    }

    /// <summary>This method will respect the selection if it exists</summary>
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
                            ++count;
                        else
                            break;
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

    /// <summary>Source code from List was copy, pasted, modified into this method</summary>
    public void History_EnsureCapacity(int totalEditLength)
    {
        if (EditedTextHistoryCapacity >= totalEditLength)
            return;
        
        int newCapacity = EditedTextHistoryCapacity * 2;
        // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
        // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
        if (newCapacity < totalEditLength) newCapacity = totalEditLength;

        var newArray = new char[newCapacity];
        Array.Copy(_editedTextHistory, 0, newArray, 0, EditedTextHistoryCount);
        _editedTextHistory = newArray;
    }
}
