using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace NetCoreStack.Lucene
{
    public abstract class LuceneIndexWriterFactory<TInstance, TItem> where TItem : ISearchIndexItem
    {
        private static readonly object _syncObj = new object();
        private static readonly Lazy<IndexWriter> _indexWriter = new Lazy<IndexWriter>(() => InnerFactory());

        protected delegate Analyzer AnalyzerFactoryDelegate();
        protected static AnalyzerFactoryDelegate AnalyzerFactory { get; set; }

        protected delegate void IndexMaintenanceDelegate();
        protected static IndexMaintenanceDelegate IndexMaintenance { get; set; }

        protected static Action<TItem> UpdateIndexForItemAction { get; set; }

        protected static IndexWriter Writer
        {
            get
            {
                return _indexWriter.Value;
            }
        }

        protected static string IndexLocation { get; set; }

        protected static string IndexSyncFileLocation { get; set; }
        
        protected static Directory LuceneDirectory { get; set; }

        // protected to prevent direct instantiation.
        protected LuceneIndexWriterFactory()
        {
            
        }

        static LuceneIndexWriterFactory()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lucene");
            var attr = typeof(TItem).GetCustomAttribute<IndexNameAttribute>();
            if (attr == null)
            {
                throw new ArgumentNullException(nameof(IndexNameAttribute));
            }
            if (string.IsNullOrEmpty(attr.Name))
            {
                throw new ArgumentNullException(nameof(IndexNameAttribute.Name));
            }

            var indexLocation = attr.Name;
            var dataDir = Path.Combine(path.ToString(), indexLocation);
            System.IO.Directory.CreateDirectory(dataDir);
            IndexLocation = dataDir;
            var syncFile = Path.Combine(dataDir, $"{indexLocation}.sync");
            if (!File.Exists(syncFile))
            {
                using (File.Create(syncFile)) { }
            }
            IndexSyncFileLocation = syncFile;
            Activator.CreateInstance(typeof(TInstance));
        }

        private static IndexWriter InnerFactory()
        {
            LuceneDirectory = FSDirectory.Open(IndexLocation);
            if (IndexWriter.IsLocked(LuceneDirectory))
            {
                IndexWriter.Unlock(LuceneDirectory);
            }
            return new IndexWriter(FSDirectory.Open(IndexLocation), CreateAnalyzerFactory(), IndexWriter.MaxFieldLength.UNLIMITED);
        }

        private static Analyzer CreateAnalyzerFactory()
        {
            if (AnalyzerFactory != null)
                return AnalyzerFactory();

            return new StandardAnalyzer(Version.LUCENE_30);
        }

        public static void RunIndexMaintenance()
        {
            if (IndexMaintenance != null)
            {
                IndexMaintenance();
                return;
            }

            Writer.Optimize();
            Writer.Commit();
        }

        public static void AddItemsToIndex(IEnumerable<TItem> items)
        {
            try
            {
                foreach (var item in items)
                {
                    UpdateIndexForItemAction(item);
                }
                Writer.Commit();
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"===Lucene Index Exception: {ex.Message}");
#endif
            }
        }

        public static bool WriterIsLocked
        {
            get
            {
                return IndexWriter.IsLocked(LuceneDirectory);
            }
        }

        public static LuceneIndexDescriptor GetSyncInfo()
        {
            LuceneIndexDescriptor syncInfo = null;
            var content = File.ReadAllText(IndexSyncFileLocation);
            if (!string.IsNullOrEmpty(content))
            {
                try
                {
                    syncInfo = JsonConvert.DeserializeObject<LuceneIndexDescriptor>(content);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return syncInfo;
        }

        public static void SetSyncIndexFile(LuceneIndexDescriptor descriptor)
        {
            lock (_syncObj)
            {
                var content = JsonConvert.SerializeObject(descriptor);
                File.WriteAllText(IndexSyncFileLocation, content);
            }
        }
    }
}