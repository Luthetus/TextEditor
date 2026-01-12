namespace TextEditor;

public struct TextEditorTooltip
{
    /// <summary>
    /// 'startPositionIndex' is inclusive;
    /// 'endPositionIndex' is exclusive;
    /// </summary>
    public TextEditorTooltip(int startPositionIndex, int endPositionIndex, int foreignKey, byte byteKind)
    {
        StartPositionIndex = startPositionIndex;
        EndPositionIndex = endPositionIndex;
        ForeignKey = foreignKey;
        ByteKind = byteKind;
    }

    /// <summary>
    /// Inclusive
    /// </summary>
    public int StartPositionIndex { get; set; }
    /// <summary>
    /// Exclusive
    /// </summary>
    public int EndPositionIndex { get; set; }
    /// <summary>
    /// Rather than duplicating your data, this is a means of on-demand retrieving the data.
    /// This can be whatever is useful to you.
    /// Perhaps you store the index within some other list that your data resides at.
    /// Or you could store the "id" of some data in order to look it up.
    /// </summary>
    public int ForeignKey { get; set; }
    /// <summary>
    /// The combination of <see cref="ByteKind"/> and <see cref="ForeignKey"/>
    /// is intended to permit endless customization.
    ///
    /// Because now the <see cref="ByteKind"/> will let you specify what the <see cref="ForeignKey"/> represents.
    /// In the case that you have many varying representations for the <see cref="ForeignKey"/> within the tooltip list.
    /// </summary>
    public byte ByteKind { get; set; }
}
