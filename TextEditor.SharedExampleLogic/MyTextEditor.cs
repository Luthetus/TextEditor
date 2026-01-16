namespace TextEditor.SharedExampleLogic;

public sealed class MyTextEditor : TextEditorModel
{
    public LanguageKind LanguageKind { get; set; } = LanguageKind.CSharp;

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

        // I'm not sure how to do this.
        // I don't know how to do much of anything actually.
        // But... this is the most optimized way I was able to support keywords.
        //
        // You sum the characters of the identifier to act as a heuristic.
        // Thus you can make an educated guess on whether it could match.
        //
        // Then based on your educated guess you decide to take the time to iterate each character
        // and check them one by one.
        //
        // IO'm coughuiong aup a long rn omg
        //
        switch (characterIntSum)
        {
            // NonContextualKeywords-NonControl
            // ================================
            case 852: // abstract
                if (textSpanLength == 8 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'b' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 't')
                {
                    return new SyntaxToken(
                        SyntaxKind.AbstractTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 212: // as
                if (textSpanLength == 2 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 's')
                {
                    return new SyntaxToken(
                        SyntaxKind.AsTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 411: // base
                if (textSpanLength == 4 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'b' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e')
                {
                    return new SyntaxToken(
                        SyntaxKind.BaseTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 428: // bool
                if (textSpanLength == 4 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'b' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'l')
                {
                    return new SyntaxToken(
                        SyntaxKind.BoolTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 436: // !! DUPLICATES !!

                if (textSpanLength != 4)
                    goto default;

                if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'b' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'y' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e')
                {
                    // byte
                    return new SyntaxToken(
                        SyntaxKind.ByteTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }
                else if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'f' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[1] == 'r' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[2] == 'o' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[3] == 'm')
                {
                    // from
                    return new SyntaxToken(
                        SyntaxKind.FromTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }
                else if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'i' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[1] == 'n' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[2] == 'i' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[3] == 't')
                {
                    // init
                    return new SyntaxToken(
                        SyntaxKind.InitTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 515: // catch
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'h')
                {
                    return new SyntaxToken(
                        SyntaxKind.CatchTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 414: // char
                if (textSpanLength == 4 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'h' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'r')
                {
                    return new SyntaxToken(
                        SyntaxKind.CharTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 711: // checked
                if (textSpanLength == 7 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'h' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'k' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'd')
                {
                    return new SyntaxToken(
                        SyntaxKind.CheckedTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 534: // !! DUPLICATES !!

                if (textSpanLength != 5)
                    goto default;

                if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 's')
                {
                    // class
                    return new SyntaxToken(
                        SyntaxKind.ClassTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }
                else if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'f' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[1] == 'l' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[2] == 'o' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[3] == 'a' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[4] == 't')
                {
                    // float
                    return new SyntaxToken(
                        SyntaxKind.FloatTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }
                else if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'a' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[1] == 'w' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[2] == 'a' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[3] == 'i' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[4] == 't')
                {
                    // await
                    return new SyntaxToken(
                        SyntaxKind.AwaitTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 551: // !! DUPLICATES !!

                if (textSpanLength != 5)
                    goto default;

                if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 't')
                {
                    // const
                    return new SyntaxToken(
                        SyntaxKind.ConstTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }
                else if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 's' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[1] == 'b' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[2] == 'y' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[3] == 't' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[4] == 'e')
                {
                    // sbyte
                    return new SyntaxToken(
                        SyntaxKind.SbyteTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 719: // decimal
                if (textSpanLength == 7 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'd' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'm' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'l')
                {
                    return new SyntaxToken(
                        SyntaxKind.DecimalTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 741: // !! DUPLICATES !!

                if (textSpanLength != 7)
                    goto default;

                if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'd' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'f' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 't')
                {
                    // default
                    return new SyntaxToken(
                        SyntaxKind.DefaultTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }
                else if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'd' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[1] == 'y' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[2] == 'n' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[3] == 'a' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[4] == 'm' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[5] == 'i' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[6] == 'c')
                {
                    // dynamic
                    return new SyntaxToken(
                        SyntaxKind.DynamicTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 827: // delegate
                if (textSpanLength == 8 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'd' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'g' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 'e')
                {
                    return new SyntaxToken(
                        SyntaxKind.DelegateTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 635: // double
                if (textSpanLength == 6 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'd' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'b' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'e')
                {
                    return new SyntaxToken(
                        SyntaxKind.DoubleTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 437: // enum
                if (textSpanLength == 4 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'm')
                {
                    return new SyntaxToken(
                        SyntaxKind.EnumTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 546: // event
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'v' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 't')
                {
                    return new SyntaxToken(
                        SyntaxKind.EventTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 866: // explicit
                if (textSpanLength == 8 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'x' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'p' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 't')
                {
                    return new SyntaxToken(
                        SyntaxKind.ExplicitTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 662: // extern
                if (textSpanLength == 6 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'x' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'n')
                {
                    return new SyntaxToken(
                        SyntaxKind.ExternTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 523: // false
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'f' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'e')
                {
                    return new SyntaxToken(
                        SyntaxKind.FalseTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 751: // finally
                if (textSpanLength == 7 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'f' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'y')
                {
                    return new SyntaxToken(
                        SyntaxKind.FinallyTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 528: // fixed
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'f' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'x' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'd')
                {
                    return new SyntaxToken(
                        SyntaxKind.FixedTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 859: // implicit
                if (textSpanLength == 8 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'm' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'p' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 't')
                {
                    return new SyntaxToken(
                        SyntaxKind.ImplicitTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 215: // in
                if (textSpanLength == 2 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'n')
                {
                    return new SyntaxToken(
                        SyntaxKind.InTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 331: // int
                if (textSpanLength == 3 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 't')
                {
                    return new SyntaxToken(
                        SyntaxKind.IntTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 945: // interface
                if (textSpanLength == 9 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'f' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[8] == 'e')
                {
                    return new SyntaxToken(
                        SyntaxKind.InterfaceTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 861: // internal
                if (textSpanLength == 8 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 'l')
                {
                    return new SyntaxToken(
                        SyntaxKind.InternalTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 220: // is
                if (textSpanLength == 2 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 's')
                {
                    return new SyntaxToken(
                        SyntaxKind.IsTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 425: // !! DUPLICATES !!

                if (textSpanLength != 4)
                    goto default;

                if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'k')
                {
                    // lock
                    return new SyntaxToken(
                        SyntaxKind.LockTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }
                else if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'e' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[1] == 'l' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[2] == 's' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e')
                {
                    // else
                    return new SyntaxToken(
                        SyntaxKind.ElseTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.KeywordControl,
                            (ushort)characterIntSum));
                }

                goto default;
            case 432: // !! DUPLICATES !!

                if (textSpanLength != 4)
                    goto default;

                if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'g')
                {
                    // long
                    return new SyntaxToken(
                        SyntaxKind.LongTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }
                else if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'j' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[1] == 'o' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[2] == 'i' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[3] == 'n')
                {
                    // join
                    return new SyntaxToken(
                        SyntaxKind.JoinTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 941: // namespace
                if (textSpanLength == 9 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'm' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'p' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[8] == 'e')
                {
                    /*
                    I changed Person.cs to Person.txt so the parser doesn't hit the file.
                    So now only Program.cs is being hit which is:
                    ```csharp
                    namespace BlazorCrudApp.ServerSide;
                    
                    var person = new Person();
                    
                    ```

                    The first letter in this file is 'n',
                    that triggers LexIdentifierOrKeywordOrKeywordContextual(...),
                    and the CharIntSum of the current word comes out to be 941.

                    The CharIntSum of namespace is also 941, so we end up here.

                    I'm seeing the character start position as 0,
                    and the byte start position as 0.

                    This is wrong, and so the problem has roots here.

                    VS Debugger shows the UTF8 preamble as:
                        ReadOnlySpan<Byte>[3]
                            [0] => 239
                            [1] => 187
                            [2] => 191

                    The BOM only started appearing once I used the StreamWriter.
                     */
                    return new SyntaxToken(
                        SyntaxKind.NamespaceTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 330: // new
                if (textSpanLength == 3 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'w')
                {
                    return new SyntaxToken(
                        SyntaxKind.NewTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 443: // null
                if (textSpanLength == 4 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'l')
                {
                    return new SyntaxToken(
                        SyntaxKind.NullTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 631: // object
                if (textSpanLength == 6 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'b' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'j' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 't')
                {
                    return new SyntaxToken(
                        SyntaxKind.ObjectTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 876: // operator
                if (textSpanLength == 8 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'p' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 'r')
                {
                    return new SyntaxToken(
                        SyntaxKind.OperatorTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 344: // out
                if (textSpanLength == 3 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 't')
                {
                    return new SyntaxToken(
                        SyntaxKind.OutTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 864: // !! DUPLICATES !!

                if (textSpanLength != 8)
                    goto default;

                if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'v' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'd' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 'e')
                {
                    // override
                    return new SyntaxToken(
                        SyntaxKind.OverrideTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }
                else if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'v' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[1] == 'o' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[2] == 'l' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[3] == 'a' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[4] == 't' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[5] == 'i' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[6] == 'l' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[7] == 'e')
                {
                    // volatile
                    return new SyntaxToken(
                        SyntaxKind.VolatileTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 644: // params
                if (textSpanLength == 6 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'p' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'm' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 's')
                {
                    return new SyntaxToken(
                        SyntaxKind.ParamsTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 763: // private
                if (textSpanLength == 7 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'p' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'v' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'e')
                {
                    return new SyntaxToken(
                        SyntaxKind.PrivateTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 970: // protected
                if (textSpanLength == 9 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'p' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[8] == 'd')
                {
                    return new SyntaxToken(
                        SyntaxKind.ProtectedTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 639: // !! DUPLICATES !!

                if (textSpanLength != 6)
                    goto default;

                if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'p' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'b' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'c')
                {
                    // public
                    return new SyntaxToken(
                        SyntaxKind.PublicTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }
                else if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'r' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[1] == 'e' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[2] == 'c' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[3] == 'o' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[4] == 'r' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[5] == 'd')
                {
                    // record
                    return new SyntaxToken(
                        SyntaxKind.RecordTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 862: // readonly
                if (textSpanLength == 8 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'd' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 'y')
                {
                    return new SyntaxToken(
                        SyntaxKind.ReadonlyTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 317: // ref
                if (textSpanLength == 3 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'f')
                {
                    return new SyntaxToken(
                        SyntaxKind.RefTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 622: // sealed
                if (textSpanLength == 6 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'd')
                {
                    return new SyntaxToken(
                        SyntaxKind.SealedTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 560: // short
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'h' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 't')
                {
                    return new SyntaxToken(
                        SyntaxKind.ShortTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 656: // sizeof
                if (textSpanLength == 6 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'z' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'f')
                {
                    return new SyntaxToken(
                        SyntaxKind.SizeofTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 1057: // stackalloc
                if (textSpanLength == 10 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'k' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[8] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[9] == 'c')
                {
                    return new SyntaxToken(
                        SyntaxKind.StackallocTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 648: // static
                if (textSpanLength == 6 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'c')
                {
                    return new SyntaxToken(
                        SyntaxKind.StaticTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 663: // !! DUPLICATES !!

                if (textSpanLength != 6)
                    goto default;

                if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'g')
                {
                    // string
                    return new SyntaxToken(
                        SyntaxKind.StringTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }
                else if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 't' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[1] == 'y' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[2] == 'p' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[4] == 'o' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[5] == 'f')
                {
                    // typeof
                    return new SyntaxToken(
                        SyntaxKind.TypeofTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 677: // !! DUPLICATES !!

                if (textSpanLength != 6)
                    goto default;

                if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 't')
                {
                    // struct
                    return new SyntaxToken(
                        SyntaxKind.StructTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }
                else if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'u' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[1] == 's' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[2] == 'h' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[3] == 'o' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[4] == 'r' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[5] == 't')
                {
                    // ushort
                    return new SyntaxToken(
                        SyntaxKind.UshortTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 440: // this
                if (textSpanLength == 4 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'h' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 's')
                {
                    return new SyntaxToken(
                        SyntaxKind.ThisTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 448: // !! DUPLICATES !!

                if (textSpanLength != 4)
                    goto default;

                if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e')
                {
                    // true
                    return new SyntaxToken(
                        SyntaxKind.TrueTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }
                else if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'u' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[1] == 'i' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[2] == 'n' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[3] == 't')
                {
                    // uint
                    return new SyntaxToken(
                        SyntaxKind.UintTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 351: // try
                if (textSpanLength == 3 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'y')
                {
                    return new SyntaxToken(
                        SyntaxKind.TryTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 549: // ulong
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'g')
                {
                    return new SyntaxToken(
                        SyntaxKind.UlongTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 938: // unchecked
                if (textSpanLength == 9 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'h' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'k' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[8] == 'd')
                {
                    return new SyntaxToken(
                        SyntaxKind.UncheckedTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 642: // unsafe
                if (textSpanLength == 6 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'f' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'e')
                {
                    return new SyntaxToken(
                        SyntaxKind.UnsafeTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 550: // using
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'g')
                {
                    return new SyntaxToken(
                        SyntaxKind.UsingTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 775: // virtual
                if (textSpanLength == 7 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'v' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'l')
                {
                    return new SyntaxToken(
                        SyntaxKind.VirtualTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 434: // !! DUPLICATES !!

                if (textSpanLength != 4)
                    goto default;

                if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'v' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'd')
                {
                    // void
                    return new SyntaxToken(
                        SyntaxKind.VoidTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }
                else if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'w' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[1] == 'h' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[2] == 'e' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[3] == 'n')
                {
                    // when
                    return new SyntaxToken(
                        SyntaxKind.WhenTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 517: // break
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'b' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'k')
                {
                    return new SyntaxToken(
                        SyntaxKind.BreakTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.KeywordControl,
                            (ushort)characterIntSum));
                }

                goto default;
            case 412: // case
                if (textSpanLength == 4 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e')
                {
                    return new SyntaxToken(
                        SyntaxKind.CaseTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.KeywordControl,
                            (ushort)characterIntSum));
                }

                goto default;
            case 869: // continue
                if (textSpanLength == 8 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 'e')
                {
                    return new SyntaxToken(
                        SyntaxKind.ContinueTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.KeywordControl,
                            (ushort)characterIntSum));
                }

                goto default;
            case 211: // do
                if (textSpanLength == 2 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'd' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'o')
                {
                    return new SyntaxToken(
                        SyntaxKind.DoTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.KeywordControl,
                            (ushort)characterIntSum));
                }

                goto default;
            case 327: // for
                if (textSpanLength == 3 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'f' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'r')
                {
                    return new SyntaxToken(
                        SyntaxKind.ForTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.KeywordControl,
                            (ushort)characterIntSum));
                }

                goto default;
            case 728: // foreach
                if (textSpanLength == 7 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'f' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'h')
                {
                    return new SyntaxToken(
                        SyntaxKind.ForeachTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.KeywordControl,
                            (ushort)characterIntSum));
                }

                goto default;
            case 441: // !! DUPLICATES !!

                if (textSpanLength != 4)
                    goto default;

                if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'g' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'o')
                {
                    // goto
                    return new SyntaxToken(
                        SyntaxKind.GotoTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.KeywordControl,
                            (ushort)characterIntSum));
                }
                else if (ideService_CSharpBinder_KeywordCheckBuffer[0] == 'n' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[1] == 'i' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[2] == 'n' &&
                         ideService_CSharpBinder_KeywordCheckBuffer[3] == 't')
                {
                    // nint
                    return new SyntaxToken(
                        SyntaxKind.NintTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 207: // if
                if (textSpanLength == 2 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'f')
                {
                    return new SyntaxToken(
                        SyntaxKind.IfTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.KeywordControl,
                            (ushort)characterIntSum));
                }

                goto default;
            case 672: // return
                if (textSpanLength == 6 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'n')
                {
                    return new SyntaxToken(
                        SyntaxKind.ReturnTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.KeywordControl,
                            (ushort)characterIntSum));
                }

                goto default;
            case 658: // switch
                if (textSpanLength == 6 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'w' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'h')
                {
                    return new SyntaxToken(
                        SyntaxKind.SwitchTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.KeywordControl,
                            (ushort)characterIntSum));
                }

                goto default;
            case 564: // throw
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'h' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'w')
                {
                    return new SyntaxToken(
                        SyntaxKind.ThrowTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.KeywordControl,
                            (ushort)characterIntSum));
                }

                goto default;
            case 537: // while
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'w' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'h' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'e')
                {
                    return new SyntaxToken(
                        SyntaxKind.WhileTokenKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.KeywordControl,
                            (ushort)characterIntSum));
                }

                goto default;
            case 297: // add
                if (textSpanLength == 3 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'd' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'd')
                {
                    return new SyntaxToken(
                        SyntaxKind.AddTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 307: // and
                if (textSpanLength == 3 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'd')
                {
                    return new SyntaxToken(
                        SyntaxKind.AndTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 522: // alias
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 's')
                {
                    return new SyntaxToken(
                        SyntaxKind.AliasTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 940: // ascending
                if (textSpanLength == 9 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'd' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[8] == 'g')
                {
                    return new SyntaxToken(
                        SyntaxKind.AscendingTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 429: // args
                if (textSpanLength == 4 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'g' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 's')
                {
                    return new SyntaxToken(
                        SyntaxKind.ArgsTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 542: // async
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'y' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'c')
                {
                    return new SyntaxToken(
                        SyntaxKind.AsyncTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 219: // by
                if (textSpanLength == 2 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'b' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'y')
                {
                    return new SyntaxToken(
                        SyntaxKind.ByTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 1044: // descending
                if (textSpanLength == 10 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'd' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'd' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[8] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[9] == 'g')
                {
                    return new SyntaxToken(
                        SyntaxKind.DescendingTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 651: // equals
                if (textSpanLength == 6 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'q' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 's')
                {
                    return new SyntaxToken(
                        SyntaxKind.EqualsTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 416: // file
                if (textSpanLength == 4 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'f' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e')
                {
                    return new SyntaxToken(
                        SyntaxKind.FileTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 320: // get
                if (textSpanLength == 3 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'g' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 't')
                {
                    return new SyntaxToken(
                        SyntaxKind.GetTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 625: // global
                if (textSpanLength == 6 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'g' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'b' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'l')
                {
                    return new SyntaxToken(
                        SyntaxKind.GlobalTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 557: // group
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'g' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'p')
                {
                    return new SyntaxToken(
                        SyntaxKind.GroupTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 442: // into
                if (textSpanLength == 4 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'o')
                {
                    return new SyntaxToken(
                        SyntaxKind.IntoTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 325: // let
                if (textSpanLength == 3 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 't')
                {
                    return new SyntaxToken(
                        SyntaxKind.LetTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 717: // managed
                if (textSpanLength == 7 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'm' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'g' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'd')
                {
                    return new SyntaxToken(
                        SyntaxKind.ManagedTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 630: // nameof
                if (textSpanLength == 6 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'm' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'f')
                {
                    return new SyntaxToken(
                        SyntaxKind.NameofTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 337: // not
                if (textSpanLength == 3 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 't')
                {
                    return new SyntaxToken(
                        SyntaxKind.NotTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 780: // notnull
                if (textSpanLength == 7 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'l')
                {
                    return new SyntaxToken(
                        SyntaxKind.NotnullTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 558: // nuint
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 't')
                {
                    return new SyntaxToken(
                        SyntaxKind.NuintTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 221: // on
                if (textSpanLength == 2 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'n')
                {
                    return new SyntaxToken(
                        SyntaxKind.OnTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 225: // or
                if (textSpanLength == 2 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'r')
                {
                    return new SyntaxToken(
                        SyntaxKind.OrTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 759: // orderby
                if (textSpanLength == 7 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'd' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'b' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'y')
                {
                    return new SyntaxToken(
                        SyntaxKind.OrderbyTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 749: // partial
                if (textSpanLength == 7 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'p' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'l')
                {
                    return new SyntaxToken(
                        SyntaxKind.PartialTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 654: // remove
                if (textSpanLength == 6 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'm' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'v' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'e')
                {
                    return new SyntaxToken(
                        SyntaxKind.RemoveTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 865: // required
                if (textSpanLength == 8 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'q' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 'd')
                {
                    return new SyntaxToken(
                        SyntaxKind.RequiredTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 638: // scoped
                if (textSpanLength == 6 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'o' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'p' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'd')
                {
                    return new SyntaxToken(
                        SyntaxKind.ScopedTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 640: // select
                if (textSpanLength == 6 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'c' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 't')
                {
                    return new SyntaxToken(
                        SyntaxKind.SelectTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 332: // set
                if (textSpanLength == 3 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 's' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 't')
                {
                    return new SyntaxToken(
                        SyntaxKind.SetTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 944: // unmanaged
                if (textSpanLength == 9 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'm' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'n' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[5] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[6] == 'g' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[7] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[8] == 'd')
                {
                    return new SyntaxToken(
                        SyntaxKind.UnmanagedTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 541: // value
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'v' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'u' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'e')
                {
                    return new SyntaxToken(
                        SyntaxKind.ValueTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 329: // var
                if (textSpanLength == 3 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'v' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'a' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'r')
                {
                    return new SyntaxToken(
                        SyntaxKind.VarTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 539: // where
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'w' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'h' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'r' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'e')
                {
                    return new SyntaxToken(
                        SyntaxKind.WhereTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 444: // with
                if (textSpanLength == 4 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'w' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 't' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'h')
                {
                    return new SyntaxToken(
                        SyntaxKind.WithTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.Keyword,
                            (ushort)characterIntSum));
                }

                goto default;
            case 535: // yield
                if (textSpanLength == 5 &&
                    ideService_CSharpBinder_KeywordCheckBuffer[0] == 'y' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[1] == 'i' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[2] == 'e' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[3] == 'l' &&
                    ideService_CSharpBinder_KeywordCheckBuffer[4] == 'd')
                {
                    return new SyntaxToken(
                        SyntaxKind.YieldTokenContextualKeyword,
                        new TextEditorTextSpan(
                            entryPositionIndex,
                            streamReaderWrap.PositionIndex,
                            (byte)GenericDecorationKind.KeywordControl,
                            (ushort)characterIntSum));
                }

                goto default;
            default:
                return new SyntaxToken(
                    SyntaxKind.IdentifierToken,
                    new TextEditorTextSpan(
                        entryPositionIndex,
                        streamReaderWrap.PositionIndex,
                        (byte)GenericDecorationKind.None,
                        (ushort)characterIntSum));
        }
    }
}
