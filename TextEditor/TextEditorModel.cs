using System.Text;

namespace TextEditor;

public class TextEditorModel
{
    private readonly StringBuilder _textBuilder = new();

    public int this[int key]
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

    public int LineCount => LineBreakPositionList.Count + 1;

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
    
    public virtual void MoveCursor(MoveCursorKind moveCursorKind)
    {
        switch (moveCursorKind)
        {
            case MoveCursorKind.ArrowLeft:
                if (PositionIndex > 0)
                {
                    --PositionIndex;
                }
                break;
            case MoveCursorKind.ArrowDown:
                break;
            case MoveCursorKind.ArrowUp:
                break;
            case MoveCursorKind.ArrowRight:
                ++PositionIndex;
                break;
        }
        
        if (PositionIndex > _textBuilder.Length)
            PositionIndex = _textBuilder.Length;
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
        if (_decorationArrayCapacity < _textBuilder.Length) {
            int newCapacity = _textBuilder.Length == 0? _defaultCapacity : _decorationArrayCapacity * 2;
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

    public void SetText(string text)
    {
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
                if (i < text.Length - 1 && text[i + 1] == '\r')
                    ++i;
                _textBuilder.Append('\n');
                LineBreakPositionList.Add(i);
            }
            else if (character == '\r')
            {
                _textBuilder.Append('\n');
                LineBreakPositionList.Add(i);
            }
            else
            {
                _textBuilder.Append(character);
            }
        }
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
    public void InsertText(string text) => InsertTextAtPosition(text, PositionIndex);

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
    public void InsertTextAtPosition(string text, int positionIndex)
    {
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
                if (i < text.Length - 1 && text[i + 1] == '\r')
                    ++i;
                _textBuilder.Append('\n');
                LineBreakPositionList.Add(i);
            }
            else if (character == '\r')
            {
                _textBuilder.Append('\n');
                LineBreakPositionList.Add(i);
            }
            else
            {
                _textBuilder.Append(character);
            }
        }
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
        if (lineIndex == 0)
        {
            return columnIndex;
        }

        for (int i = 0; i < LineBreakPositionList.Count; i++)
        {
            if (i + 1 == lineIndex)
            {
                return LineBreakPositionList[i] + 1 + columnIndex;
            }
        }

        if (LineBreakPositionList.Count < lineIndex)
            lineIndex = LineBreakPositionList.Count;

        var lastValidColumnIndex = GetLastValidColumnIndex(lineIndex);
        if (columnIndex > lastValidColumnIndex)
            columnIndex = lastValidColumnIndex;

        return GetPositionIndex(lineIndex, columnIndex);
    }

    public int GetLastValidColumnIndex(int lineIndex)
    {
        if (lineIndex == 0)
        {
            if (LineBreakPositionList.Count == 0)
            {
                return Length;
            }
            else if (LineBreakPositionList.Count == lineIndex)
            {
                return Length - (LineBreakPositionList[^1] + 1);
            }
        }
    }

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
            _textBuilder.Insert('\n');
        }
        else if (character == '\r')
        {
            _textBuilder.Append('\n');
            LineBreakPositionList.Add(i);
        }
        else
        {
            _textBuilder.Append(character);
        }

        _textBuilder.Insert(character);
        for (int i = 0; i < text.Length; i++)
        {
            var character = text[i];


        }
    }
}
