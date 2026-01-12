namespace TextEditor;

/// <summary>All measurements are in pixels unless otherwise stated</summary>
public struct TextEditorMeasurements
{
    public double CharacterWidth { get; set; }
    public int LineHeight { get; set; }
    public int EditorWidth { get; set; }
    public int EditorHeight { get; set; }
    public int EditorLeft { get; set; }
    public int EditorTop { get; set; }
    /// <summary>"literal" as opposed to the "scrollHeight", this is the amount of width the y-axis scrollbar measures</summary>
    public double ScrollbarLiteralWidth { get; set; }
    /// <summary>"literal" as opposed to the "scrollWidth", this is the amount of height the x-axis scrollbar measures</summary>
	public double ScrollbarLiteralHeight { get; set; }
}
