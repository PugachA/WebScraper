using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace WebScraper.ML.DatasetGenerator
{
    public static class LINQExtension
    {
        public static IEnumerable<T> OfTypes<T>(this IEnumerable<T> collection, Type[] wantedTypes)
        {
            if (wantedTypes == null)
                return null;
            else
                return collection.Where(element => wantedTypes.Contains(element.GetType()));
        }
    }
}
