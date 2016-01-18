using System;
using System.Collections.Generic;
using System.Linq;
using CryptoSwift.Helpers;

namespace CryptoSwift.Extensions
{
	public static class ListExtensions
	{
		/// <summary>
		/// Shuffles the contents of a <see cref="List<T>"/>.
		/// </summary>
		/// <typeparam name="T">The type of list.</typeparam>
		/// <param name="list">The list to shuffle.</param>
		public static void Shuffle<T>(this List<T> list)
		{
			for (var i = 0; i < RandomHelper.GenerateRandom(1, 10, 40).First(); i++)
			{
				int n = list.Count;
				while (n > 1)
				{
					n--;
					int k = RandomHelper.GenerateRandom(1, n + 1).First();
					T value = list[k];
					list[k] = list[n];
					list[n] = value;
				}
			}
		}

		/// <summary>
		/// Removes any duplicates from a list based off of a property within the object.
		/// </summary>
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
