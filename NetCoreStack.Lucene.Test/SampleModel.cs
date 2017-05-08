using System;

namespace NetCoreStack.Lucene.Test
{
    [IndexName("SampleIndex")]
    public class SampleModel : ISearchIndexItem
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
    }
}
