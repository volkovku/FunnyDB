using System;
using System.Data;

namespace FunnyDB
{
    /// <summary>
    /// Represents a platform independent SQL query parameter. 
    /// </summary>
    public abstract class SqlQueryParameter
    {
        private static readonly string[] IndexName;
        internal const int PinnedNameIndex = -1;

        static SqlQueryParameter()
        {
            var indexName = new string[1000];
            for (var i = 0; i < indexName.Length; i++)
            {
                indexName[i] = $"p_{i}_";
            }

            IndexName = indexName;
        }

        protected SqlQueryParameter(int index)
            : this(index, GetIndexName(index))
        {
        }

        protected SqlQueryParameter(string name)
            : this(PinnedNameIndex, name)
        {
        }

        private SqlQueryParameter(int index, string name)
        {
            Index = index;
            Name = name;
        }

        public readonly int Index;
        public readonly string Name;
        public abstract Func<object> Value { get; }

        public abstract SqlQueryParameter ChangeIndex(int index);
        public abstract void AddToCommand(IDbCommand command);

        private static string GetIndexName(int index)
        {
            var indexName = IndexName;
            if (index >= 0 && index < indexName.Length)
            {
                return indexName[index];
            }

            return $"p_{index}_";
        }
    }
}