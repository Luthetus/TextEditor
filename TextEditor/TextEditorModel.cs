using System.Text;

namespace TextEditor;

public class TextEditorModel
{
    public const byte CharacterTooltipByteKind = 0;

    public StringBuilder Content = new("test");
    
    public int PositionIndex { get; set; }
    
    public TextEditorMeasurements Measurements { get; set; }
    
    /// <summary>
    /// You can keep this feature disabled by leaving the property null (the default).
    /// Otherwise, you need to instantiate the list and begin populating it.
    ///
    /// This list is not expected to be sorted.
    /// A tooltip event will perform a linear search through this list to find the matching position index.
    /// </summary>
    public List<TextEditorTooltip>? TooltipList { get; set; } = null;
    
    /// <summary>
    /// If you return 'null', then the tooltip is essentially "cancelled" as if the event never occurred.
    /// </summary>
    public virtual string? GetTooltipText(TextEditorTooltip tooltip)
    {
        switch (tooltip.ByteKind)
        {
            case CharacterTooltipByteKind:
                if (Content.Length > tooltip.StartPositionIndex)
                {
                    return Content[tooltip.StartPositionIndex].ToString();
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
        
        if (PositionIndex > Content.Length)
            PositionIndex = Content.Length;
    }
}
