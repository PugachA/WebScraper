using System;
using System.Collections.Generic;

namespace WebScraper.Core.Extensions
{
    public static class LINQExtension
    {
        public static IEnumerable<T> OfTypes<T>(this IEnumerable<T> source, Type[] wantedTypes)
        {
            if (wantedTypes == null)
                throw new ArgumentNullException($"{nameof(wantedTypes)} can not be null");

            if (source == null)
                throw new ArgumentNullException("Source can not be null");

            foreach (object obj in source)
                foreach (var type in wantedTypes)
                    if (type.IsAssignableFrom(obj.GetType()))
                        yield return (T)obj;
        }
    }
}
