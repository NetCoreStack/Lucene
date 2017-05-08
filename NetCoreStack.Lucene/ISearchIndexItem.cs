namespace NetCoreStack.Lucene
{
    public interface ISearchIndexItem
    {
        long Id { get; set; }
        string Text { get; set; }
    }
}