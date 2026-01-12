namespace TextEditor;

/// <summary>All measurements are in pixels unless otherwise stated</summary>
public struct TextEditorMeasurements
{
    public double CharacterWidth { get; set; }
    public double LineHeight { get; set; }
    public double EditorWidth { get; set; }
    public double EditorHeight { get; set; }
    public double EditorLeft { get; set; }
    public double EditorTop { get; set; }
    /// <summary>"literal" as opposed to the "scrollHeight", this is the amount of width the y-axis scrollbar measures</summary>
    public double ScrollbarLiteralWidth { get; set; }
    /// <summary>"literal" as opposed to the "scrollWidth", this is the amount of height the x-axis scrollbar measures</summary>
	public double ScrollbarLiteralHeight { get; set; }
}
