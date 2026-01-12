using System.Text;

namespace TextEditor;

public class TextEditorModel
{
    public StringBuilder Content = new("test");
    
    public int PositionIndex { get; set; }
    
    public TextEditorMeasurements Measurements { get; set; }
}
