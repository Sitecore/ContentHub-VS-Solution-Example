using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.CH.Base.Features.Base.Extensions
{
    public static class CollectionExtensions
    {
        public static IEnumerable<IEnumerable<T>> SplitIntoBatches<T>(this IEnumerable<T> input, int batchSize)
        {
            var arr = input as T[] ?? input.ToArray();
            for (var i = 0; i < arr.Length; i += batchSize)
            {
                var actualBatchLength = i + batchSize < arr.Length ? batchSize : arr.Length - i;
                yield return new ArraySegment<T>(arr, i, actualBatchLength);
            }
        }

        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
