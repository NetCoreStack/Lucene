using Lucene.Net.Documents;
using System.Globalization;

namespace NetCoreStack.Lucene.Test
{
    public static class LuceneSearchIndexItemExtensions
    {
        public static Document ToLuceneDocument(this SampleModel item)
        {
            var id = new Field(
                "id",
                item.Id.ToString(CultureInfo.InvariantCulture),
                Field.Store.YES,
                Field.Index.NOT_ANALYZED,
                Field.TermVector.NO);

            var text = new Field(
                "text",
                (item.Text ?? string.Empty).Replace(".", " "),
                Field.Store.YES,
                Field.Index.ANALYZED,
                Field.TermVector.YES);

            var doc = new Document();
            doc.Add(id);
            doc.Add(text);

            return doc;
        }
    }
}
