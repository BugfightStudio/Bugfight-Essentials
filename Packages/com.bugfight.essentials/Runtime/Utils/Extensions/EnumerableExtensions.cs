using System;

namespace Bugfight.Utils.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this T[] arr, Action<T> action) => Array.ForEach(arr, action);
    }
}
