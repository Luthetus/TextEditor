using System.Text;

namespace TextEditor;

public class TextEditorModel
{
    public StringBuilder Content = new("test");
    
    public int PositionIndex { get; set; }
    
    public TextEditorMeasurements Measurements { get; set; }
    
    /// <summary>
    /// You can keep this feature disabled by leaving the property null (the default).
    /// Otherwise, you need to instantiate the list and begin populating it.
    /// </summary>
    public List<TextEditorTooltip>? TooltipList { get; set; } = null;
    
    public void MoveCursor(MoveCursorKind moveCursorKind)
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
    }
}
