using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzeThis
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<TOut> WhereIs<TIn, TOut>(this IEnumerable<TIn> items)
        {
            return items.Where(item => item is TOut)
                .Cast<TOut>();
        }
    }
}
