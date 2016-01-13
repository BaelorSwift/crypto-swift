using System;
using System.Collections;

namespace CryptoSwift.Extensions
{
	public static class BitArrayExtensions
	{
		public static int ToInt16(this BitArray bitArray)
		{
			int value = 0;

			for (int i = 0; i < bitArray.Length; i++)
				if (bitArray[i])
					value += Convert.ToInt16(Math.Pow(2, i));

			return value;
		}
	}
}
