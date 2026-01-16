using TextEditor;

namespace TextEditorTests;

public class UnitTest1
{
    /// <summary>
    /// Ensure SetText can be invoked multiple times to re-use the same TextEditorModel instance.
    /// (this currently fails but it is a very important detail that needs to be made true, and stay true)
    /// </summary>
    [Fact]
    public void ModelSetsContent()
    {
        var model = new TextEditorModel();
        model.SetText("abc\n\n\t\t");
        Assert.Equal(2, model.LineBreakPositionList.Count);
        Assert.Equal(2, model.TabPositionList.Count);

        model.SetText("abc\n\t");
        Assert.Single(model.LineBreakPositionList);
        Assert.Single(model.TabPositionList);
    }
}
