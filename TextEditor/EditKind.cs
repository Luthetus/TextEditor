namespace TextEditor;

/// <summary>
/// ltr => left  to right
/// rtl => right to left
/// </summary>
public enum EditKind
{
    None = 0,
    InsertLtr,
    RemoveBackspaceRtl,
    RemoveDeleteLtr,
}
