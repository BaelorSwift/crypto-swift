using System;
using System.Collections.Generic;

namespace CryptoSwift.Extensions
{
	public static class ListExtensions
	{
		public static void Shuffle<T>(this List<T> list)
		{
			var rng = new Random();
			for (var i = 0; i < rng.Next(10, 40); i++)
			{
				int n = list.Count;
				while (n > 1)
				{
					n--;
					int k = rng.Next(n + 1);
					T value = list[k];
					list[k] = list[n];
					list[n] = value;
				}
			}
		}

		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this List<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> seenKeys = new HashSet<TKey>();
			foreach (TSource element in source)
			{
				if (seenKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}
	}
}
