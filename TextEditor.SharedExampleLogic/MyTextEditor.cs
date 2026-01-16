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
    
    private void SuccessFoundKeyword(int entryPosition, int position)
    {
        if (DecorationArray is not null)
            Decorate(entryPosition, position, KeywordDecorationByte);

        if (TooltipList is not null)
        {
            TooltipList.Add(new TextEditorTooltip(
                entryPosition,
                position,
                foreignKey: 0,
                byteKind: TextTooltipByteKind));
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
                    SuccessFoundKeyword(entryPosition, position);
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
                    _textBuilder[entryPosition + 0] == 'a' &&
                    _textBuilder[entryPosition + 1] == 'b' &&
                    _textBuilder[entryPosition + 2] == 's' &&
                    _textBuilder[entryPosition + 3] == 't' &&
                    _textBuilder[entryPosition + 4] == 'r' &&
                    _textBuilder[entryPosition + 5] == 'a' &&
                    _textBuilder[entryPosition + 6] == 'c' &&
                    _textBuilder[entryPosition + 7] == 't')
                {
                    SuccessFoundKeyword(entryPosition, position);
                }

                goto default;
            case 212: // as
                if (textSpanLength == 2 &&
                    _textBuilder[entryPosition + 0] == 'a' &&
                    _textBuilder[entryPosition + 1] == 's')
                {
                    SuccessFoundKeyword(entryPosition, position);
                }

                goto default;
            case 411: // base
                if (textSpanLength == 4 &&
                    _textBuilder[entryPosition + 0] == 'b' &&
                    _textBuilder[entryPosition + 1] == 'a' &&
                    _textBuilder[entryPosition + 2] == 's' &&
                    _textBuilder[entryPosition + 3] == 'e')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 428: // bool
                if (textSpanLength == 4 &&
                    _textBuilder[entryPosition + 0] == 'b' &&
                    _textBuilder[entryPosition + 1] == 'o' &&
                    _textBuilder[entryPosition + 2] == 'o' &&
                    _textBuilder[entryPosition + 3] == 'l')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 436: // !! DUPLICATES !!

                if (textSpanLength != 4)
                    goto default;

                if (_textBuilder[entryPosition + 0] == 'b' &&
                    _textBuilder[entryPosition + 1] == 'y' &&
                    _textBuilder[entryPosition + 2] == 't' &&
                    _textBuilder[entryPosition + 3] == 'e')
                {
                    // byte
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }
                else if (_textBuilder[entryPosition + 0] == 'f' &&
                         _textBuilder[entryPosition + 1] == 'r' &&
                         _textBuilder[entryPosition + 2] == 'o' &&
                         _textBuilder[entryPosition + 3] == 'm')
                {
                    // from
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }
                else if (_textBuilder[entryPosition + 0] == 'i' &&
                         _textBuilder[entryPosition + 1] == 'n' &&
                         _textBuilder[entryPosition + 2] == 'i' &&
                         _textBuilder[entryPosition + 3] == 't')
                {
                    // init
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 515: // catch
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 'c' &&
                    _textBuilder[entryPosition + 1] == 'a' &&
                    _textBuilder[entryPosition + 2] == 't' &&
                    _textBuilder[entryPosition + 3] == 'c' &&
                    _textBuilder[entryPosition + 4] == 'h')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 414: // char
                if (textSpanLength == 4 &&
                    _textBuilder[entryPosition + 0] == 'c' &&
                    _textBuilder[entryPosition + 1] == 'h' &&
                    _textBuilder[entryPosition + 2] == 'a' &&
                    _textBuilder[entryPosition + 3] == 'r')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 711: // checked
                if (textSpanLength == 7 &&
                    _textBuilder[entryPosition + 0] == 'c' &&
                    _textBuilder[entryPosition + 1] == 'h' &&
                    _textBuilder[entryPosition + 2] == 'e' &&
                    _textBuilder[entryPosition + 3] == 'c' &&
                    _textBuilder[entryPosition + 4] == 'k' &&
                    _textBuilder[entryPosition + 5] == 'e' &&
                    _textBuilder[entryPosition + 6] == 'd')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 534: // !! DUPLICATES !!

                if (textSpanLength != 5)
                    goto default;

                if (_textBuilder[entryPosition + 0] == 'c' &&
                    _textBuilder[entryPosition + 1] == 'l' &&
                    _textBuilder[entryPosition + 2] == 'a' &&
                    _textBuilder[entryPosition + 3] == 's' &&
                    _textBuilder[entryPosition + 4] == 's')
                {
                    // class
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }
                else if (_textBuilder[entryPosition + 0] == 'f' &&
                         _textBuilder[entryPosition + 1] == 'l' &&
                         _textBuilder[entryPosition + 2] == 'o' &&
                         _textBuilder[entryPosition + 3] == 'a' &&
                         _textBuilder[entryPosition + 4] == 't')
                {
                    // float
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }
                else if (_textBuilder[entryPosition + 0] == 'a' &&
                         _textBuilder[entryPosition + 1] == 'w' &&
                         _textBuilder[entryPosition + 2] == 'a' &&
                         _textBuilder[entryPosition + 3] == 'i' &&
                         _textBuilder[entryPosition + 4] == 't')
                {
                    // await
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 551: // !! DUPLICATES !!

                if (textSpanLength != 5)
                    goto default;

                if (_textBuilder[entryPosition + 0] == 'c' &&
                    _textBuilder[entryPosition + 1] == 'o' &&
                    _textBuilder[entryPosition + 2] == 'n' &&
                    _textBuilder[entryPosition + 3] == 's' &&
                    _textBuilder[entryPosition + 4] == 't')
                {
                    // const
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }
                else if (_textBuilder[entryPosition + 0] == 's' &&
                         _textBuilder[entryPosition + 1] == 'b' &&
                         _textBuilder[entryPosition + 2] == 'y' &&
                         _textBuilder[entryPosition + 3] == 't' &&
                         _textBuilder[entryPosition + 4] == 'e')
                {
                    // sbyte
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 719: // decimal
                if (textSpanLength == 7 &&
                    _textBuilder[entryPosition + 0] == 'd' &&
                    _textBuilder[entryPosition + 1] == 'e' &&
                    _textBuilder[entryPosition + 2] == 'c' &&
                    _textBuilder[entryPosition + 3] == 'i' &&
                    _textBuilder[entryPosition + 4] == 'm' &&
                    _textBuilder[entryPosition + 5] == 'a' &&
                    _textBuilder[entryPosition + 6] == 'l')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 741: // !! DUPLICATES !!

                if (textSpanLength != 7)
                    goto default;

                if (_textBuilder[entryPosition + 0] == 'd' &&
                    _textBuilder[entryPosition + 1] == 'e' &&
                    _textBuilder[entryPosition + 2] == 'f' &&
                    _textBuilder[entryPosition + 3] == 'a' &&
                    _textBuilder[entryPosition + 4] == 'u' &&
                    _textBuilder[entryPosition + 5] == 'l' &&
                    _textBuilder[entryPosition + 6] == 't')
                {
                    // default
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }
                else if (_textBuilder[entryPosition + 0] == 'd' &&
                         _textBuilder[entryPosition + 1] == 'y' &&
                         _textBuilder[entryPosition + 2] == 'n' &&
                         _textBuilder[entryPosition + 3] == 'a' &&
                         _textBuilder[entryPosition + 4] == 'm' &&
                         _textBuilder[entryPosition + 5] == 'i' &&
                         _textBuilder[entryPosition + 6] == 'c')
                {
                    // dynamic
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 827: // delegate
                if (textSpanLength == 8 &&
                    _textBuilder[entryPosition + 0] == 'd' &&
                    _textBuilder[entryPosition + 1] == 'e' &&
                    _textBuilder[entryPosition + 2] == 'l' &&
                    _textBuilder[entryPosition + 3] == 'e' &&
                    _textBuilder[entryPosition + 4] == 'g' &&
                    _textBuilder[entryPosition + 5] == 'a' &&
                    _textBuilder[entryPosition + 6] == 't' &&
                    _textBuilder[entryPosition + 7] == 'e')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 635: // double
                if (textSpanLength == 6 &&
                    _textBuilder[entryPosition + 0] == 'd' &&
                    _textBuilder[entryPosition + 1] == 'o' &&
                    _textBuilder[entryPosition + 2] == 'u' &&
                    _textBuilder[entryPosition + 3] == 'b' &&
                    _textBuilder[entryPosition + 4] == 'l' &&
                    _textBuilder[entryPosition + 5] == 'e')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 437: // enum
                if (textSpanLength == 4 &&
                    _textBuilder[entryPosition + 0] == 'e' &&
                    _textBuilder[entryPosition + 1] == 'n' &&
                    _textBuilder[entryPosition + 2] == 'u' &&
                    _textBuilder[entryPosition + 3] == 'm')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 546: // event
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 'e' &&
                    _textBuilder[entryPosition + 1] == 'v' &&
                    _textBuilder[entryPosition + 2] == 'e' &&
                    _textBuilder[entryPosition + 3] == 'n' &&
                    _textBuilder[entryPosition + 4] == 't')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 866: // explicit
                if (textSpanLength == 8 &&
                    _textBuilder[entryPosition + 0] == 'e' &&
                    _textBuilder[entryPosition + 1] == 'x' &&
                    _textBuilder[entryPosition + 2] == 'p' &&
                    _textBuilder[entryPosition + 3] == 'l' &&
                    _textBuilder[entryPosition + 4] == 'i' &&
                    _textBuilder[entryPosition + 5] == 'c' &&
                    _textBuilder[entryPosition + 6] == 'i' &&
                    _textBuilder[entryPosition + 7] == 't')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 662: // extern
                if (textSpanLength == 6 &&
                    _textBuilder[entryPosition + 0] == 'e' &&
                    _textBuilder[entryPosition + 1] == 'x' &&
                    _textBuilder[entryPosition + 2] == 't' &&
                    _textBuilder[entryPosition + 3] == 'e' &&
                    _textBuilder[entryPosition + 4] == 'r' &&
                    _textBuilder[entryPosition + 5] == 'n')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 523: // false
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 'f' &&
                    _textBuilder[entryPosition + 1] == 'a' &&
                    _textBuilder[entryPosition + 2] == 'l' &&
                    _textBuilder[entryPosition + 3] == 's' &&
                    _textBuilder[entryPosition + 4] == 'e')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 751: // finally
                if (textSpanLength == 7 &&
                    _textBuilder[entryPosition + 0] == 'f' &&
                    _textBuilder[entryPosition + 1] == 'i' &&
                    _textBuilder[entryPosition + 2] == 'n' &&
                    _textBuilder[entryPosition + 3] == 'a' &&
                    _textBuilder[entryPosition + 4] == 'l' &&
                    _textBuilder[entryPosition + 5] == 'l' &&
                    _textBuilder[entryPosition + 6] == 'y')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 528: // fixed
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 'f' &&
                    _textBuilder[entryPosition + 1] == 'i' &&
                    _textBuilder[entryPosition + 2] == 'x' &&
                    _textBuilder[entryPosition + 3] == 'e' &&
                    _textBuilder[entryPosition + 4] == 'd')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 859: // implicit
                if (textSpanLength == 8 &&
                    _textBuilder[entryPosition + 0] == 'i' &&
                    _textBuilder[entryPosition + 1] == 'm' &&
                    _textBuilder[entryPosition + 2] == 'p' &&
                    _textBuilder[entryPosition + 3] == 'l' &&
                    _textBuilder[entryPosition + 4] == 'i' &&
                    _textBuilder[entryPosition + 5] == 'c' &&
                    _textBuilder[entryPosition + 6] == 'i' &&
                    _textBuilder[entryPosition + 7] == 't')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 215: // in
                if (textSpanLength == 2 &&
                    _textBuilder[entryPosition + 0] == 'i' &&
                    _textBuilder[entryPosition + 1] == 'n')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 331: // int
                if (textSpanLength == 3 &&
                    _textBuilder[entryPosition + 0] == 'i' &&
                    _textBuilder[entryPosition + 1] == 'n' &&
                    _textBuilder[entryPosition + 2] == 't')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 945: // interface
                if (textSpanLength == 9 &&
                    _textBuilder[entryPosition + 0] == 'i' &&
                    _textBuilder[entryPosition + 1] == 'n' &&
                    _textBuilder[entryPosition + 2] == 't' &&
                    _textBuilder[entryPosition + 3] == 'e' &&
                    _textBuilder[entryPosition + 4] == 'r' &&
                    _textBuilder[entryPosition + 5] == 'f' &&
                    _textBuilder[entryPosition + 6] == 'a' &&
                    _textBuilder[entryPosition + 7] == 'c' &&
                    _textBuilder[entryPosition + 8] == 'e')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 861: // internal
                if (textSpanLength == 8 &&
                    _textBuilder[entryPosition + 0] == 'i' &&
                    _textBuilder[entryPosition + 1] == 'n' &&
                    _textBuilder[entryPosition + 2] == 't' &&
                    _textBuilder[entryPosition + 3] == 'e' &&
                    _textBuilder[entryPosition + 4] == 'r' &&
                    _textBuilder[entryPosition + 5] == 'n' &&
                    _textBuilder[entryPosition + 6] == 'a' &&
                    _textBuilder[entryPosition + 7] == 'l')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 220: // is
                if (textSpanLength == 2 &&
                    _textBuilder[entryPosition + 0] == 'i' &&
                    _textBuilder[entryPosition + 1] == 's')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 425: // !! DUPLICATES !!

                if (textSpanLength != 4)
                    goto default;

                if (_textBuilder[entryPosition + 0] == 'l' &&
                    _textBuilder[entryPosition + 1] == 'o' &&
                    _textBuilder[entryPosition + 2] == 'c' &&
                    _textBuilder[entryPosition + 3] == 'k')
                {
                    // lock
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }
                else if (_textBuilder[entryPosition + 0] == 'e' &&
                         _textBuilder[entryPosition + 1] == 'l' &&
                         _textBuilder[entryPosition + 2] == 's' &&
                         _textBuilder[entryPosition + 3] == 'e')
                {
                    // else
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 432: // !! DUPLICATES !!

                if (textSpanLength != 4)
                    goto default;

                if (_textBuilder[entryPosition + 0] == 'l' &&
                    _textBuilder[entryPosition + 1] == 'o' &&
                    _textBuilder[entryPosition + 2] == 'n' &&
                    _textBuilder[entryPosition + 3] == 'g')
                {
                    // long
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }
                else if (_textBuilder[entryPosition + 0] == 'j' &&
                         _textBuilder[entryPosition + 1] == 'o' &&
                         _textBuilder[entryPosition + 2] == 'i' &&
                         _textBuilder[entryPosition + 3] == 'n')
                {
                    // join
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 941: // namespace
                if (textSpanLength == 9 &&
                    _textBuilder[entryPosition + 0] == 'n' &&
                    _textBuilder[entryPosition + 1] == 'a' &&
                    _textBuilder[entryPosition + 2] == 'm' &&
                    _textBuilder[entryPosition + 3] == 'e' &&
                    _textBuilder[entryPosition + 4] == 's' &&
                    _textBuilder[entryPosition + 5] == 'p' &&
                    _textBuilder[entryPosition + 6] == 'a' &&
                    _textBuilder[entryPosition + 7] == 'c' &&
                    _textBuilder[entryPosition + 8] == 'e')
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
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 330: // new
                if (textSpanLength == 3 &&
                    _textBuilder[entryPosition + 0] == 'n' &&
                    _textBuilder[entryPosition + 1] == 'e' &&
                    _textBuilder[entryPosition + 2] == 'w')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 443: // null
                if (textSpanLength == 4 &&
                    _textBuilder[entryPosition + 0] == 'n' &&
                    _textBuilder[entryPosition + 1] == 'u' &&
                    _textBuilder[entryPosition + 2] == 'l' &&
                    _textBuilder[entryPosition + 3] == 'l')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 631: // object
                if (textSpanLength == 6 &&
                    _textBuilder[entryPosition + 0] == 'o' &&
                    _textBuilder[entryPosition + 1] == 'b' &&
                    _textBuilder[entryPosition + 2] == 'j' &&
                    _textBuilder[entryPosition + 3] == 'e' &&
                    _textBuilder[entryPosition + 4] == 'c' &&
                    _textBuilder[entryPosition + 5] == 't')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 876: // operator
                if (textSpanLength == 8 &&
                    _textBuilder[entryPosition + 0] == 'o' &&
                    _textBuilder[entryPosition + 1] == 'p' &&
                    _textBuilder[entryPosition + 2] == 'e' &&
                    _textBuilder[entryPosition + 3] == 'r' &&
                    _textBuilder[entryPosition + 4] == 'a' &&
                    _textBuilder[entryPosition + 5] == 't' &&
                    _textBuilder[entryPosition + 6] == 'o' &&
                    _textBuilder[entryPosition + 7] == 'r')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 344: // out
                if (textSpanLength == 3 &&
                    _textBuilder[entryPosition + 0] == 'o' &&
                    _textBuilder[entryPosition + 1] == 'u' &&
                    _textBuilder[entryPosition + 2] == 't')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 864: // !! DUPLICATES !!

                if (textSpanLength != 8)
                    goto default;

                if (_textBuilder[entryPosition + 0] == 'o' &&
                    _textBuilder[entryPosition + 1] == 'v' &&
                    _textBuilder[entryPosition + 2] == 'e' &&
                    _textBuilder[entryPosition + 3] == 'r' &&
                    _textBuilder[entryPosition + 4] == 'r' &&
                    _textBuilder[entryPosition + 5] == 'i' &&
                    _textBuilder[entryPosition + 6] == 'd' &&
                    _textBuilder[entryPosition + 7] == 'e')
                {
                    // override
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }
                else if (_textBuilder[entryPosition + 0] == 'v' &&
                         _textBuilder[entryPosition + 1] == 'o' &&
                         _textBuilder[entryPosition + 2] == 'l' &&
                         _textBuilder[entryPosition + 3] == 'a' &&
                         _textBuilder[entryPosition + 4] == 't' &&
                         _textBuilder[entryPosition + 5] == 'i' &&
                         _textBuilder[entryPosition + 6] == 'l' &&
                         _textBuilder[entryPosition + 7] == 'e')
                {
                    // volatile
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 644: // params
                if (textSpanLength == 6 &&
                    _textBuilder[entryPosition + 0] == 'p' &&
                    _textBuilder[entryPosition + 1] == 'a' &&
                    _textBuilder[entryPosition + 2] == 'r' &&
                    _textBuilder[entryPosition + 3] == 'a' &&
                    _textBuilder[entryPosition + 4] == 'm' &&
                    _textBuilder[entryPosition + 5] == 's')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 763: // private
                if (textSpanLength == 7 &&
                    _textBuilder[entryPosition + 0] == 'p' &&
                    _textBuilder[entryPosition + 1] == 'r' &&
                    _textBuilder[entryPosition + 2] == 'i' &&
                    _textBuilder[entryPosition + 3] == 'v' &&
                    _textBuilder[entryPosition + 4] == 'a' &&
                    _textBuilder[entryPosition + 5] == 't' &&
                    _textBuilder[entryPosition + 6] == 'e')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 970: // protected
                if (textSpanLength == 9 &&
                    _textBuilder[entryPosition + 0] == 'p' &&
                    _textBuilder[entryPosition + 1] == 'r' &&
                    _textBuilder[entryPosition + 2] == 'o' &&
                    _textBuilder[entryPosition + 3] == 't' &&
                    _textBuilder[entryPosition + 4] == 'e' &&
                    _textBuilder[entryPosition + 5] == 'c' &&
                    _textBuilder[entryPosition + 6] == 't' &&
                    _textBuilder[entryPosition + 7] == 'e' &&
                    _textBuilder[entryPosition + 8] == 'd')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 639: // !! DUPLICATES !!

                if (textSpanLength != 6)
                    goto default;

                if (_textBuilder[entryPosition + 0] == 'p' &&
                    _textBuilder[entryPosition + 1] == 'u' &&
                    _textBuilder[entryPosition + 2] == 'b' &&
                    _textBuilder[entryPosition + 3] == 'l' &&
                    _textBuilder[entryPosition + 4] == 'i' &&
                    _textBuilder[entryPosition + 5] == 'c')
                {
                    // public
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }
                else if (_textBuilder[entryPosition + 0] == 'r' &&
                         _textBuilder[entryPosition + 1] == 'e' &&
                         _textBuilder[entryPosition + 2] == 'c' &&
                         _textBuilder[entryPosition + 3] == 'o' &&
                         _textBuilder[entryPosition + 4] == 'r' &&
                         _textBuilder[entryPosition + 5] == 'd')
                {
                    // record
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 862: // readonly
                if (textSpanLength == 8 &&
                    _textBuilder[entryPosition + 0] == 'r' &&
                    _textBuilder[entryPosition + 1] == 'e' &&
                    _textBuilder[entryPosition + 2] == 'a' &&
                    _textBuilder[entryPosition + 3] == 'd' &&
                    _textBuilder[entryPosition + 4] == 'o' &&
                    _textBuilder[entryPosition + 5] == 'n' &&
                    _textBuilder[entryPosition + 6] == 'l' &&
                    _textBuilder[entryPosition + 7] == 'y')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 317: // ref
                if (textSpanLength == 3 &&
                    _textBuilder[entryPosition + 0] == 'r' &&
                    _textBuilder[entryPosition + 1] == 'e' &&
                    _textBuilder[entryPosition + 2] == 'f')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 622: // sealed
                if (textSpanLength == 6 &&
                    _textBuilder[entryPosition + 0] == 's' &&
                    _textBuilder[entryPosition + 1] == 'e' &&
                    _textBuilder[entryPosition + 2] == 'a' &&
                    _textBuilder[entryPosition + 3] == 'l' &&
                    _textBuilder[entryPosition + 4] == 'e' &&
                    _textBuilder[entryPosition + 5] == 'd')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 560: // short
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 's' &&
                    _textBuilder[entryPosition + 1] == 'h' &&
                    _textBuilder[entryPosition + 2] == 'o' &&
                    _textBuilder[entryPosition + 3] == 'r' &&
                    _textBuilder[entryPosition + 4] == 't')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 656: // sizeof
                if (textSpanLength == 6 &&
                    _textBuilder[entryPosition + 0] == 's' &&
                    _textBuilder[entryPosition + 1] == 'i' &&
                    _textBuilder[entryPosition + 2] == 'z' &&
                    _textBuilder[entryPosition + 3] == 'e' &&
                    _textBuilder[entryPosition + 4] == 'o' &&
                    _textBuilder[entryPosition + 5] == 'f')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 1057: // stackalloc
                if (textSpanLength == 10 &&
                    _textBuilder[entryPosition + 0] == 's' &&
                    _textBuilder[entryPosition + 1] == 't' &&
                    _textBuilder[entryPosition + 2] == 'a' &&
                    _textBuilder[entryPosition + 3] == 'c' &&
                    _textBuilder[entryPosition + 4] == 'k' &&
                    _textBuilder[entryPosition + 5] == 'a' &&
                    _textBuilder[entryPosition + 6] == 'l' &&
                    _textBuilder[entryPosition + 7] == 'l' &&
                    _textBuilder[entryPosition + 8] == 'o' &&
                    _textBuilder[entryPosition + 9] == 'c')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 648: // static
                if (textSpanLength == 6 &&
                    _textBuilder[entryPosition + 0] == 's' &&
                    _textBuilder[entryPosition + 1] == 't' &&
                    _textBuilder[entryPosition + 2] == 'a' &&
                    _textBuilder[entryPosition + 3] == 't' &&
                    _textBuilder[entryPosition + 4] == 'i' &&
                    _textBuilder[entryPosition + 5] == 'c')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 663: // !! DUPLICATES !!

                if (textSpanLength != 6)
                    goto default;

                if (_textBuilder[entryPosition + 0] == 's' &&
                    _textBuilder[entryPosition + 1] == 't' &&
                    _textBuilder[entryPosition + 2] == 'r' &&
                    _textBuilder[entryPosition + 3] == 'i' &&
                    _textBuilder[entryPosition + 4] == 'n' &&
                    _textBuilder[entryPosition + 5] == 'g')
                {
                    // string
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }
                else if (_textBuilder[entryPosition + 0] == 't' &&
                         _textBuilder[entryPosition + 1] == 'y' &&
                         _textBuilder[entryPosition + 2] == 'p' &&
                         _textBuilder[entryPosition + 3] == 'e' &&
                         _textBuilder[entryPosition + 4] == 'o' &&
                         _textBuilder[entryPosition + 5] == 'f')
                {
                    // typeof
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 677: // !! DUPLICATES !!

                if (textSpanLength != 6)
                    goto default;

                if (_textBuilder[entryPosition + 0] == 's' &&
                    _textBuilder[entryPosition + 1] == 't' &&
                    _textBuilder[entryPosition + 2] == 'r' &&
                    _textBuilder[entryPosition + 3] == 'u' &&
                    _textBuilder[entryPosition + 4] == 'c' &&
                    _textBuilder[entryPosition + 5] == 't')
                {
                    // struct
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }
                else if (_textBuilder[entryPosition + 0] == 'u' &&
                         _textBuilder[entryPosition + 1] == 's' &&
                         _textBuilder[entryPosition + 2] == 'h' &&
                         _textBuilder[entryPosition + 3] == 'o' &&
                         _textBuilder[entryPosition + 4] == 'r' &&
                         _textBuilder[entryPosition + 5] == 't')
                {
                    // ushort
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 440: // this
                if (textSpanLength == 4 &&
                    _textBuilder[entryPosition + 0] == 't' &&
                    _textBuilder[entryPosition + 1] == 'h' &&
                    _textBuilder[entryPosition + 2] == 'i' &&
                    _textBuilder[entryPosition + 3] == 's')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 448: // !! DUPLICATES !!

                if (textSpanLength != 4)
                    goto default;

                if (_textBuilder[entryPosition + 0] == 't' &&
                    _textBuilder[entryPosition + 1] == 'r' &&
                    _textBuilder[entryPosition + 2] == 'u' &&
                    _textBuilder[entryPosition + 3] == 'e')
                {
                    // true
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }
                else if (_textBuilder[entryPosition + 0] == 'u' &&
                         _textBuilder[entryPosition + 1] == 'i' &&
                         _textBuilder[entryPosition + 2] == 'n' &&
                         _textBuilder[entryPosition + 3] == 't')
                {
                    // uint
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 351: // try
                if (textSpanLength == 3 &&
                    _textBuilder[entryPosition + 0] == 't' &&
                    _textBuilder[entryPosition + 1] == 'r' &&
                    _textBuilder[entryPosition + 2] == 'y')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 549: // ulong
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 'u' &&
                    _textBuilder[entryPosition + 1] == 'l' &&
                    _textBuilder[entryPosition + 2] == 'o' &&
                    _textBuilder[entryPosition + 3] == 'n' &&
                    _textBuilder[entryPosition + 4] == 'g')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 938: // unchecked
                if (textSpanLength == 9 &&
                    _textBuilder[entryPosition + 0] == 'u' &&
                    _textBuilder[entryPosition + 1] == 'n' &&
                    _textBuilder[entryPosition + 2] == 'c' &&
                    _textBuilder[entryPosition + 3] == 'h' &&
                    _textBuilder[entryPosition + 4] == 'e' &&
                    _textBuilder[entryPosition + 5] == 'c' &&
                    _textBuilder[entryPosition + 6] == 'k' &&
                    _textBuilder[entryPosition + 7] == 'e' &&
                    _textBuilder[entryPosition + 8] == 'd')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 642: // unsafe
                if (textSpanLength == 6 &&
                    _textBuilder[entryPosition + 0] == 'u' &&
                    _textBuilder[entryPosition + 1] == 'n' &&
                    _textBuilder[entryPosition + 2] == 's' &&
                    _textBuilder[entryPosition + 3] == 'a' &&
                    _textBuilder[entryPosition + 4] == 'f' &&
                    _textBuilder[entryPosition + 5] == 'e')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 550: // using
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 'u' &&
                    _textBuilder[entryPosition + 1] == 's' &&
                    _textBuilder[entryPosition + 2] == 'i' &&
                    _textBuilder[entryPosition + 3] == 'n' &&
                    _textBuilder[entryPosition + 4] == 'g')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 775: // virtual
                if (textSpanLength == 7 &&
                    _textBuilder[entryPosition + 0] == 'v' &&
                    _textBuilder[entryPosition + 1] == 'i' &&
                    _textBuilder[entryPosition + 2] == 'r' &&
                    _textBuilder[entryPosition + 3] == 't' &&
                    _textBuilder[entryPosition + 4] == 'u' &&
                    _textBuilder[entryPosition + 5] == 'a' &&
                    _textBuilder[entryPosition + 6] == 'l')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 434: // !! DUPLICATES !!

                if (textSpanLength != 4)
                    goto default;

                if (_textBuilder[entryPosition + 0] == 'v' &&
                    _textBuilder[entryPosition + 1] == 'o' &&
                    _textBuilder[entryPosition + 2] == 'i' &&
                    _textBuilder[entryPosition + 3] == 'd')
                {
                    // void
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }
                else if (_textBuilder[entryPosition + 0] == 'w' &&
                         _textBuilder[entryPosition + 1] == 'h' &&
                         _textBuilder[entryPosition + 2] == 'e' &&
                         _textBuilder[entryPosition + 3] == 'n')
                {
                    // when
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 517: // break
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 'b' &&
                    _textBuilder[entryPosition + 1] == 'r' &&
                    _textBuilder[entryPosition + 2] == 'e' &&
                    _textBuilder[entryPosition + 3] == 'a' &&
                    _textBuilder[entryPosition + 4] == 'k')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 412: // case
                if (textSpanLength == 4 &&
                    _textBuilder[entryPosition + 0] == 'c' &&
                    _textBuilder[entryPosition + 1] == 'a' &&
                    _textBuilder[entryPosition + 2] == 's' &&
                    _textBuilder[entryPosition + 3] == 'e')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 869: // continue
                if (textSpanLength == 8 &&
                    _textBuilder[entryPosition + 0] == 'c' &&
                    _textBuilder[entryPosition + 1] == 'o' &&
                    _textBuilder[entryPosition + 2] == 'n' &&
                    _textBuilder[entryPosition + 3] == 't' &&
                    _textBuilder[entryPosition + 4] == 'i' &&
                    _textBuilder[entryPosition + 5] == 'n' &&
                    _textBuilder[entryPosition + 6] == 'u' &&
                    _textBuilder[entryPosition + 7] == 'e')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 211: // do
                if (textSpanLength == 2 &&
                    _textBuilder[entryPosition + 0] == 'd' &&
                    _textBuilder[entryPosition + 1] == 'o')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 327: // for
                if (textSpanLength == 3 &&
                    _textBuilder[entryPosition + 0] == 'f' &&
                    _textBuilder[entryPosition + 1] == 'o' &&
                    _textBuilder[entryPosition + 2] == 'r')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 728: // foreach
                if (textSpanLength == 7 &&
                    _textBuilder[entryPosition + 0] == 'f' &&
                    _textBuilder[entryPosition + 1] == 'o' &&
                    _textBuilder[entryPosition + 2] == 'r' &&
                    _textBuilder[entryPosition + 3] == 'e' &&
                    _textBuilder[entryPosition + 4] == 'a' &&
                    _textBuilder[entryPosition + 5] == 'c' &&
                    _textBuilder[entryPosition + 6] == 'h')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 441: // !! DUPLICATES !!

                if (textSpanLength != 4)
                    goto default;

                if (_textBuilder[entryPosition + 0] == 'g' &&
                    _textBuilder[entryPosition + 1] == 'o' &&
                    _textBuilder[entryPosition + 2] == 't' &&
                    _textBuilder[entryPosition + 3] == 'o')
                {
                    // goto
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }
                else if (_textBuilder[entryPosition + 0] == 'n' &&
                         _textBuilder[entryPosition + 1] == 'i' &&
                         _textBuilder[entryPosition + 2] == 'n' &&
                         _textBuilder[entryPosition + 3] == 't')
                {
                    // nint
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 207: // if
                if (textSpanLength == 2 &&
                    _textBuilder[entryPosition + 0] == 'i' &&
                    _textBuilder[entryPosition + 1] == 'f')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 672: // return
                if (textSpanLength == 6 &&
                    _textBuilder[entryPosition + 0] == 'r' &&
                    _textBuilder[entryPosition + 1] == 'e' &&
                    _textBuilder[entryPosition + 2] == 't' &&
                    _textBuilder[entryPosition + 3] == 'u' &&
                    _textBuilder[entryPosition + 4] == 'r' &&
                    _textBuilder[entryPosition + 5] == 'n')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 658: // switch
                if (textSpanLength == 6 &&
                    _textBuilder[entryPosition + 0] == 's' &&
                    _textBuilder[entryPosition + 1] == 'w' &&
                    _textBuilder[entryPosition + 2] == 'i' &&
                    _textBuilder[entryPosition + 3] == 't' &&
                    _textBuilder[entryPosition + 4] == 'c' &&
                    _textBuilder[entryPosition + 5] == 'h')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 564: // throw
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 't' &&
                    _textBuilder[entryPosition + 1] == 'h' &&
                    _textBuilder[entryPosition + 2] == 'r' &&
                    _textBuilder[entryPosition + 3] == 'o' &&
                    _textBuilder[entryPosition + 4] == 'w')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 537: // while
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 'w' &&
                    _textBuilder[entryPosition + 1] == 'h' &&
                    _textBuilder[entryPosition + 2] == 'i' &&
                    _textBuilder[entryPosition + 3] == 'l' &&
                    _textBuilder[entryPosition + 4] == 'e')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 297: // add
                if (textSpanLength == 3 &&
                    _textBuilder[entryPosition + 0] == 'a' &&
                    _textBuilder[entryPosition + 1] == 'd' &&
                    _textBuilder[entryPosition + 2] == 'd')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 307: // and
                if (textSpanLength == 3 &&
                    _textBuilder[entryPosition + 0] == 'a' &&
                    _textBuilder[entryPosition + 1] == 'n' &&
                    _textBuilder[entryPosition + 2] == 'd')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 522: // alias
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 'a' &&
                    _textBuilder[entryPosition + 1] == 'l' &&
                    _textBuilder[entryPosition + 2] == 'i' &&
                    _textBuilder[entryPosition + 3] == 'a' &&
                    _textBuilder[entryPosition + 4] == 's')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 940: // ascending
                if (textSpanLength == 9 &&
                    _textBuilder[entryPosition + 0] == 'a' &&
                    _textBuilder[entryPosition + 1] == 's' &&
                    _textBuilder[entryPosition + 2] == 'c' &&
                    _textBuilder[entryPosition + 3] == 'e' &&
                    _textBuilder[entryPosition + 4] == 'n' &&
                    _textBuilder[entryPosition + 5] == 'd' &&
                    _textBuilder[entryPosition + 6] == 'i' &&
                    _textBuilder[entryPosition + 7] == 'n' &&
                    _textBuilder[entryPosition + 8] == 'g')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 429: // args
                if (textSpanLength == 4 &&
                    _textBuilder[entryPosition + 0] == 'a' &&
                    _textBuilder[entryPosition + 1] == 'r' &&
                    _textBuilder[entryPosition + 2] == 'g' &&
                    _textBuilder[entryPosition + 3] == 's')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 542: // async
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 'a' &&
                    _textBuilder[entryPosition + 1] == 's' &&
                    _textBuilder[entryPosition + 2] == 'y' &&
                    _textBuilder[entryPosition + 3] == 'n' &&
                    _textBuilder[entryPosition + 4] == 'c')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 219: // by
                if (textSpanLength == 2 &&
                    _textBuilder[entryPosition + 0] == 'b' &&
                    _textBuilder[entryPosition + 1] == 'y')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 1044: // descending
                if (textSpanLength == 10 &&
                    _textBuilder[entryPosition + 0] == 'd' &&
                    _textBuilder[entryPosition + 1] == 'e' &&
                    _textBuilder[entryPosition + 2] == 's' &&
                    _textBuilder[entryPosition + 3] == 'c' &&
                    _textBuilder[entryPosition + 4] == 'e' &&
                    _textBuilder[entryPosition + 5] == 'n' &&
                    _textBuilder[entryPosition + 6] == 'd' &&
                    _textBuilder[entryPosition + 7] == 'i' &&
                    _textBuilder[entryPosition + 8] == 'n' &&
                    _textBuilder[entryPosition + 9] == 'g')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 651: // equals
                if (textSpanLength == 6 &&
                    _textBuilder[entryPosition + 0] == 'e' &&
                    _textBuilder[entryPosition + 1] == 'q' &&
                    _textBuilder[entryPosition + 2] == 'u' &&
                    _textBuilder[entryPosition + 3] == 'a' &&
                    _textBuilder[entryPosition + 4] == 'l' &&
                    _textBuilder[entryPosition + 5] == 's')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 416: // file
                if (textSpanLength == 4 &&
                    _textBuilder[entryPosition + 0] == 'f' &&
                    _textBuilder[entryPosition + 1] == 'i' &&
                    _textBuilder[entryPosition + 2] == 'l' &&
                    _textBuilder[entryPosition + 3] == 'e')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 320: // get
                if (textSpanLength == 3 &&
                    _textBuilder[entryPosition + 0] == 'g' &&
                    _textBuilder[entryPosition + 1] == 'e' &&
                    _textBuilder[entryPosition + 2] == 't')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 625: // global
                if (textSpanLength == 6 &&
                    _textBuilder[entryPosition + 0] == 'g' &&
                    _textBuilder[entryPosition + 1] == 'l' &&
                    _textBuilder[entryPosition + 2] == 'o' &&
                    _textBuilder[entryPosition + 3] == 'b' &&
                    _textBuilder[entryPosition + 4] == 'a' &&
                    _textBuilder[entryPosition + 5] == 'l')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 557: // group
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 'g' &&
                    _textBuilder[entryPosition + 1] == 'r' &&
                    _textBuilder[entryPosition + 2] == 'o' &&
                    _textBuilder[entryPosition + 3] == 'u' &&
                    _textBuilder[entryPosition + 4] == 'p')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 442: // into
                if (textSpanLength == 4 &&
                    _textBuilder[entryPosition + 0] == 'i' &&
                    _textBuilder[entryPosition + 1] == 'n' &&
                    _textBuilder[entryPosition + 2] == 't' &&
                    _textBuilder[entryPosition + 3] == 'o')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 325: // let
                if (textSpanLength == 3 &&
                    _textBuilder[entryPosition + 0] == 'l' &&
                    _textBuilder[entryPosition + 1] == 'e' &&
                    _textBuilder[entryPosition + 2] == 't')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 717: // managed
                if (textSpanLength == 7 &&
                    _textBuilder[entryPosition + 0] == 'm' &&
                    _textBuilder[entryPosition + 1] == 'a' &&
                    _textBuilder[entryPosition + 2] == 'n' &&
                    _textBuilder[entryPosition + 3] == 'a' &&
                    _textBuilder[entryPosition + 4] == 'g' &&
                    _textBuilder[entryPosition + 5] == 'e' &&
                    _textBuilder[entryPosition + 6] == 'd')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 630: // nameof
                if (textSpanLength == 6 &&
                    _textBuilder[entryPosition + 0] == 'n' &&
                    _textBuilder[entryPosition + 1] == 'a' &&
                    _textBuilder[entryPosition + 2] == 'm' &&
                    _textBuilder[entryPosition + 3] == 'e' &&
                    _textBuilder[entryPosition + 4] == 'o' &&
                    _textBuilder[entryPosition + 5] == 'f')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 337: // not
                if (textSpanLength == 3 &&
                    _textBuilder[entryPosition + 0] == 'n' &&
                    _textBuilder[entryPosition + 1] == 'o' &&
                    _textBuilder[entryPosition + 2] == 't')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 780: // notnull
                if (textSpanLength == 7 &&
                    _textBuilder[entryPosition + 0] == 'n' &&
                    _textBuilder[entryPosition + 1] == 'o' &&
                    _textBuilder[entryPosition + 2] == 't' &&
                    _textBuilder[entryPosition + 3] == 'n' &&
                    _textBuilder[entryPosition + 4] == 'u' &&
                    _textBuilder[entryPosition + 5] == 'l' &&
                    _textBuilder[entryPosition + 6] == 'l')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 558: // nuint
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 'n' &&
                    _textBuilder[entryPosition + 1] == 'u' &&
                    _textBuilder[entryPosition + 2] == 'i' &&
                    _textBuilder[entryPosition + 3] == 'n' &&
                    _textBuilder[entryPosition + 4] == 't')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 221: // on
                if (textSpanLength == 2 &&
                    _textBuilder[entryPosition + 0] == 'o' &&
                    _textBuilder[entryPosition + 1] == 'n')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 225: // or
                if (textSpanLength == 2 &&
                    _textBuilder[entryPosition + 0] == 'o' &&
                    _textBuilder[entryPosition + 1] == 'r')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 759: // orderby
                if (textSpanLength == 7 &&
                    _textBuilder[entryPosition + 0] == 'o' &&
                    _textBuilder[entryPosition + 1] == 'r' &&
                    _textBuilder[entryPosition + 2] == 'd' &&
                    _textBuilder[entryPosition + 3] == 'e' &&
                    _textBuilder[entryPosition + 4] == 'r' &&
                    _textBuilder[entryPosition + 5] == 'b' &&
                    _textBuilder[entryPosition + 6] == 'y')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 749: // partial
                if (textSpanLength == 7 &&
                    _textBuilder[entryPosition + 0] == 'p' &&
                    _textBuilder[entryPosition + 1] == 'a' &&
                    _textBuilder[entryPosition + 2] == 'r' &&
                    _textBuilder[entryPosition + 3] == 't' &&
                    _textBuilder[entryPosition + 4] == 'i' &&
                    _textBuilder[entryPosition + 5] == 'a' &&
                    _textBuilder[entryPosition + 6] == 'l')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 654: // remove
                if (textSpanLength == 6 &&
                    _textBuilder[entryPosition + 0] == 'r' &&
                    _textBuilder[entryPosition + 1] == 'e' &&
                    _textBuilder[entryPosition + 2] == 'm' &&
                    _textBuilder[entryPosition + 3] == 'o' &&
                    _textBuilder[entryPosition + 4] == 'v' &&
                    _textBuilder[entryPosition + 5] == 'e')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 865: // required
                if (textSpanLength == 8 &&
                    _textBuilder[entryPosition + 0] == 'r' &&
                    _textBuilder[entryPosition + 1] == 'e' &&
                    _textBuilder[entryPosition + 2] == 'q' &&
                    _textBuilder[entryPosition + 3] == 'u' &&
                    _textBuilder[entryPosition + 4] == 'i' &&
                    _textBuilder[entryPosition + 5] == 'r' &&
                    _textBuilder[entryPosition + 6] == 'e' &&
                    _textBuilder[entryPosition + 7] == 'd')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 638: // scoped
                if (textSpanLength == 6 &&
                    _textBuilder[entryPosition + 0] == 's' &&
                    _textBuilder[entryPosition + 1] == 'c' &&
                    _textBuilder[entryPosition + 2] == 'o' &&
                    _textBuilder[entryPosition + 3] == 'p' &&
                    _textBuilder[entryPosition + 4] == 'e' &&
                    _textBuilder[entryPosition + 5] == 'd')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 640: // select
                if (textSpanLength == 6 &&
                    _textBuilder[entryPosition + 0] == 's' &&
                    _textBuilder[entryPosition + 1] == 'e' &&
                    _textBuilder[entryPosition + 2] == 'l' &&
                    _textBuilder[entryPosition + 3] == 'e' &&
                    _textBuilder[entryPosition + 4] == 'c' &&
                    _textBuilder[entryPosition + 5] == 't')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 332: // set
                if (textSpanLength == 3 &&
                    _textBuilder[entryPosition + 0] == 's' &&
                    _textBuilder[entryPosition + 1] == 'e' &&
                    _textBuilder[entryPosition + 2] == 't')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 944: // unmanaged
                if (textSpanLength == 9 &&
                    _textBuilder[entryPosition + 0] == 'u' &&
                    _textBuilder[entryPosition + 1] == 'n' &&
                    _textBuilder[entryPosition + 2] == 'm' &&
                    _textBuilder[entryPosition + 3] == 'a' &&
                    _textBuilder[entryPosition + 4] == 'n' &&
                    _textBuilder[entryPosition + 5] == 'a' &&
                    _textBuilder[entryPosition + 6] == 'g' &&
                    _textBuilder[entryPosition + 7] == 'e' &&
                    _textBuilder[entryPosition + 8] == 'd')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 541: // value
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 'v' &&
                    _textBuilder[entryPosition + 1] == 'a' &&
                    _textBuilder[entryPosition + 2] == 'l' &&
                    _textBuilder[entryPosition + 3] == 'u' &&
                    _textBuilder[entryPosition + 4] == 'e')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 329: // var
                if (textSpanLength == 3 &&
                    _textBuilder[entryPosition + 0] == 'v' &&
                    _textBuilder[entryPosition + 1] == 'a' &&
                    _textBuilder[entryPosition + 2] == 'r')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 539: // where
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 'w' &&
                    _textBuilder[entryPosition + 1] == 'h' &&
                    _textBuilder[entryPosition + 2] == 'e' &&
                    _textBuilder[entryPosition + 3] == 'r' &&
                    _textBuilder[entryPosition + 4] == 'e')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 444: // with
                if (textSpanLength == 4 &&
                    _textBuilder[entryPosition + 0] == 'w' &&
                    _textBuilder[entryPosition + 1] == 'i' &&
                    _textBuilder[entryPosition + 2] == 't' &&
                    _textBuilder[entryPosition + 3] == 'h')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            case 535: // yield
                if (textSpanLength == 5 &&
                    _textBuilder[entryPosition + 0] == 'y' &&
                    _textBuilder[entryPosition + 1] == 'i' &&
                    _textBuilder[entryPosition + 2] == 'e' &&
                    _textBuilder[entryPosition + 3] == 'l' &&
                    _textBuilder[entryPosition + 4] == 'd')
                {
                    SuccessFoundKeyword(entryPosition, position);
                    return;
                }

                goto default;
            default:
                return;
        }
    }
}
