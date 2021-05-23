using System;
using BenchmarkDotNet.Attributes;

namespace FunnyDB.Bench
{
    [SimpleJob]
    public class CachedQueryVsGeneratedQuery
    {
        [GlobalSetup]
        public void Setup()
        {
        }
        
        // ReSharper disable once InconsistentNaming
        private static Func<T1, SqlQuery> reusable<T1>(Func<Func<T1>, SqlQuery> builder)
        {
            T1 p1Holder = default;
            SqlQuery query = null;

            var f = new Func<T1, SqlQuery>(p1 =>
            {
                if (query == null)
                {
                    query = builder(() => p1Holder);
                }

                p1Holder = p1; 
                return query;
            });

            return f;
        }
    }
}