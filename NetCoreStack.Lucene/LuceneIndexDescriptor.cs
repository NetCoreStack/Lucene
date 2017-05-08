using System;

namespace NetCoreStack.Lucene
{
    public class LuceneIndexDescriptor
    {
        public long LastIndexId { get; set; }
        public string Metadata { get; set; }
        public DateTime IndexedTime { get; set; }
    }
}
