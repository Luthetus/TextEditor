using System.Text;

namespace TextEditor;

public class TextEditorModel
{
    public StringBuilder Content = new("test");
    
    public int PositionIndex { get; set; }
    
    public TextEditorMeasurements Measurements { get; set; }
    
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
