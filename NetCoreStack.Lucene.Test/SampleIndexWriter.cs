using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using NetCoreStack.Lucene;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Version = Lucene.Net.Util.Version;

namespace NetCoreStack.Lucene.Test
{
    public class SampleIndexWriter : LuceneIndexWriterFactory<SampleIndexWriter, SampleModel>
    {
        static SampleIndexWriter()
        {
            AnalyzerFactory = new AnalyzerFactoryDelegate(CustomAnalyzerFactory);
            UpdateIndexForItemAction = new Action<SampleModel>(UpdateIndexForItem);
        }

        private static Analyzer CustomAnalyzerFactory()
        {
            return new StandardAnalyzer(Version.LUCENE_30);
        }

        private static string ReplaceTurkishCharacters(string text)
        {
            return
                text.Replace("Ğ", "G")
                    .Replace("İ", "I")
                    .Replace("Ü", "U")
                    .Replace("Ö", "O")
                    .Replace("Ş", "S")
                    .Replace("Ç", "C")

                    .Replace("ğ", "g")
                    .Replace("ı", "i")
                    .Replace("ü", "u")
                    .Replace("ö", "o")
                    .Replace("ş", "s")
                    .Replace("ç", "c");
        }

        private static readonly List<char> TurkishCharacters = new List<char>
        {
            'Ğ', 'İ', 'Ü', 'Ö', 'Ş', 'Ç', 'ğ', 'ı', 'ü', 'ö', 'ş', 'ç'
        };

        public static Task<IEnumerable<SearchResultItem>> SearchAsync(string searchText, int n)
        {
            return Task.Run(() =>
            {
                var searcher = new IndexSearcher(LuceneDirectory, true);
                var parser = new QueryParser(Version.LUCENE_30, "text", CustomAnalyzerFactory());
                var query = parser.Parse(searchText);
                ScoreDoc[] hits = searcher.Search(query, n).ScoreDocs;
                return hits.Select(d =>
                {
                    var document = searcher.Doc(d.Doc);
                    return new SearchResultItem
                    {
                        Id = int.Parse(document.Get("id")),
                        SearchScore = d.Score
                    };
                });
            });
        }

        public static void UpdateIndexForItem(SampleModel item)
        {
            Writer.UpdateDocument(new Term("id", item.Id.ToString(CultureInfo.InvariantCulture)), item.ToLuceneDocument());
        }
    }
}
