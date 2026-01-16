namespace TextEditor.SharedExampleLogic;

public sealed class MyTextEditor : TextEditorModel
{
    public LanguageKind LanguageKind { get; set; }

    public override void ReceiveKeyboardDebounce()
    {
        switch (LanguageKind)
        {
            case LanguageKind.CSharp:
                LexCSharp();
                break;
            case LanguageKind.Razor:
                LexRazor();
                break;
        }
    }

    public void LexCSharp()
    {
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
    
    public void LexRazor()
    {
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
        protected void LexIdentifierOrKeywordOrKeywordContextual(ref int position)
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
}
