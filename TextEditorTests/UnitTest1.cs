using TextEditor;

namespace TextEditorTests;

public class UnitTest1
{
    /// <summary>
    /// Ensure SetText can be invoked multiple times to re-use the same TextEditorModel instance.
    /// (this currently fails but it is a very important detail that needs to be made true, and stay true)
    /// </summary>
    [Fact]
    void ModelSetsContent()
    {
        var model = new TextEditorModel();
        var input1 = "abc\n\n\t\t";
        model.SetText(input1);
        Assert.Equal(input1, model.ToString());
        Assert.Equal(2, model.LineBreakPositionList.Count);
        Assert.Equal(2, model.TabPositionList.Count);
        Assert.Equal(0, model.PositionIndex);
        Assert.Equal(0, model.LineIndex);
        Assert.Equal(0, model.ColumnIndex);
        Assert.Equal(0, model.SelectionAnchor);
        Assert.Equal(0, model.SelectionEnd);
        Assert.Equal(0, model._editedTextHistoryCount);
        Assert.False(model.EditIsUndone);
        Assert.Equal(0, model.EditPosition);
        Assert.Equal(0, model.EditLength);
        Assert.Equal(EditKind.None, model.EditKind);

        // These are very important to ensure there never is an
        // exception being thrown due to the next 'SetText' invocation
        // having a length < the previous, and thus indices become out of bounds.
        //
        // Decorations don't matter nearly as much because they won't out of bounds,
        // instead they'll just bring syntax highlighting to the next text.
        // Which you may or may not want, but it isn't exception causing.
        //
        // If these are changed without the UI Synchronization Context,
        // then it's undefined behavior and presumed to be capable of causing an exception to be thrown
        // when rendering (but I don't know for certain of an example exception scenario, this is my mindset).
        //
        model.PositionIndex = 1;
        model.LineIndex = 1;
        model.ColumnIndex = 1;
        model.SelectionAnchor = 1;
        model.SelectionEnd = 1;
        model._editedTextHistoryCount = 1;
        model.EditIsUndone = true;
        model.EditPosition = 1;
        model.EditLength = 1;
        model.EditKind = EditKind.RemoveDeleteLtr;

        var input2 = "abc\n\t";
        model.SetText(input2);
        Assert.Equal(input2, model.ToString());
        Assert.Single(model.LineBreakPositionList);
        Assert.Single(model.TabPositionList);
        Assert.Equal(0, model.PositionIndex);
        Assert.Equal(0, model.LineIndex);
        Assert.Equal(0, model.ColumnIndex);
        Assert.Equal(0, model.SelectionAnchor);
        Assert.Equal(0, model.SelectionEnd);
        Assert.Equal(0, model._editedTextHistoryCount);
        Assert.False(model.EditIsUndone);
        Assert.Equal(0, model.EditPosition);
        Assert.Equal(0, model.EditLength);
        Assert.Equal(EditKind.None, model.EditKind);
    }

    /// <summary>
    /// Ensure the editor will put the decoration logic in a GC-collectable state
    /// </summary>
    [Fact]
    public void DisablesDecorations()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Ensure the editor will shrink the size of its internal buffers if asked to do so.
    /// </summary>
    [Fact]
    public void Shrinks()
    {
        throw new NotImplementedException();
    }
}
