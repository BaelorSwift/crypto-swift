using System;
using System.Collections.Generic;

namespace CryptoSwift.Extentions
{
	public static class ListExtentions
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
	}
}
