using System;

namespace NetCoreStack.Lucene
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class IndexNameAttribute : Attribute
    {
        public string Name { get; }
        public IndexNameAttribute(string name)
        {
            Name = name;
        }
    }
}
