using System.Collections.Generic;
using System.Text;

namespace TextEditor;

public class TextEditorModel
{
    private void SetContent(string content)
    {
        for (int i = 0; i < content.Length; i++)
        {
            var character = content[i];

            // always insert '\n' for line endings, and then track separately the desired line end.
            // upon saving, create a string that has the '\n' included as the desired line end.
            //
            if (character == '\n')
            {
                if (i < content.Length - 1 && content[i + 1] == '\r')
                    ++character;
                _content.Append('\n');
            }
            else if (character == '\r')
            {
                _content.Append('\n');
            }

            _content.Append(character);
        }
    }

    private StringBuilder _content = new();
    
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

    public List<int> LineEndList { get; set; } = new();

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
                if (_content.Length > tooltip.StartPositionIndex)
                {
                    return _content[tooltip.StartPositionIndex].ToString();
                }
                break;
            case TextTooltipByteKind:
                if (_content.Length > tooltip.StartPositionIndex && _content.Length > tooltip.EndPositionIndex - 1)
                {
                    return _content.ToString(tooltip.StartPositionIndex, tooltip.EndPositionIndex - tooltip.StartPositionIndex);
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
        
        if (PositionIndex > _content.Length)
            PositionIndex = _content.Length;
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
        Decorate(0, _content.Length, NoneDecorationByte);
        
        int position = 0;
        while (position < _content.Length)
        {
            switch (_content[position])
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
    
        while (position < _content.Length)
        {
            if (!char.IsLetterOrDigit(_content[position]) &&
                _content[position] != '_')
            {
                break;
            }

            characterIntSum += _content[position];
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
                    _content[entryPosition + 0] == 't' &&
                    _content[entryPosition + 1] == 'e' &&
                    _content[entryPosition + 2] == 's' &&
                    _content[entryPosition + 3] == 't')
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
        if (_decorationArrayCapacity < _content.Length) {
            int newCapacity = _content.Length == 0? _defaultCapacity : _decorationArrayCapacity * 2;
            // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
            if (newCapacity < _content.Length) newCapacity = _content.Length;
            
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
        
        if (_content.Length == 0)
        {
            newCapacity = _defaultCapacity;
        }
        else
        {
            newCapacity = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)_content.Length);
            // Why does my IDE code say '< -1'???
            if (newCapacity <= 0)
            {
                newCapacity = _defaultCapacity;
            }
            else
            {
                if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
                if (newCapacity < _content.Length) newCapacity = _content.Length;
            }
        }
        
        _decorationArrayCapacity = newCapacity;
        _decorationArray = new byte[_decorationArrayCapacity];
    }
}
