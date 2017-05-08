using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreStack.Lucene.Test
{
    [TestClass]
    public class LuceneTests
    {
        private void Tokenize(LuceneIndexDescriptor descriptor, IEnumerable<SampleModel> searchIndexItems, bool isLastBundle = false)
        {
            var tokenizedSearhIndexItems = new List<SampleModel>();
            try
            {
                foreach (var item in searchIndexItems)
                {
                    if (item.Text == null)
                        continue;

                    tokenizedSearhIndexItems.Add(item);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"===Lucene Task Exception:{ex.Message}===LastIndexId:{descriptor.LastIndexId}");
#endif
            }

            if (tokenizedSearhIndexItems.Any())
            {
                SampleIndexWriter.AddItemsToIndex(tokenizedSearhIndexItems);
                descriptor.Metadata = typeof(SampleModel).FullName;
                descriptor.IndexedTime = DateTime.Now;

                var lastIndex = tokenizedSearhIndexItems.Last();
                if (tokenizedSearhIndexItems.Any())
                {
                    descriptor.LastIndexId = lastIndex.Id;
                }

                if (isLastBundle)
                {
                    SampleIndexWriter.SetSyncIndexFile(descriptor);
                    SampleIndexWriter.RunIndexMaintenance();
                }
                else
                {
                    SampleIndexWriter.SetSyncIndexFile(descriptor);
                }
            }
        }

        private void LuceneConfig()
        {
            // Start Index Maintenance
            SampleIndexWriter.RunIndexMaintenance();

            var descriptor = SampleIndexWriter.GetSyncInfo();
            IEnumerable<SampleModel> searchIndexItems;
            if (descriptor == null)
            {
                descriptor = new LuceneIndexDescriptor();
                searchIndexItems = new List<SampleModel>
                {
                     new SampleModel { Id = 1, Text = "Lorem Ipsum has been the industry's standard dummy text ever since the 1500s", Date = DateTime.Now },
                     new SampleModel { Id = 2, Text = "Contrary to popular belief, Lorem Ipsum is not simply random text. It has roots in a piece of classical Latin literature from 45 BC", Date = DateTime.Now },
                     new SampleModel { Id = 3, Text = "The standard chunk of Lorem Ipsum used since the 1500s is reproduced below for those interested", Date = DateTime.Now },
                     new SampleModel { Id = 4, Text = "Cicero are also reproduced in their exact original form, accompanied by English versions from the 1914 translation by H. Rackham", Date = DateTime.Now }
                };

                Tokenize(descriptor, searchIndexItems, true);
            }
            else
            {
                // Check db latest ID and sync with Where(x => x.Id > descriptor.LastIndexId);
            }
        }

        [TestMethod]
        public void RunIndex()
        {
            LuceneConfig();
        }
    }
}
