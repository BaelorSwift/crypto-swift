using System;
using System.Collections;
using System.Collections.Generic;

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

		public static byte[] ToByteArray(this BitArray bitArray)
		{
			var bytes = new List<byte>();
			for (var i = 0; i < bitArray.Length; i += 8)
			{
				byte b = 0x00;
				for (var y = 0; y < 8; y++)
					if (bitArray[i + y])
						b |= (byte)(1 << y);

				bytes.Add(b);
			}

			return bytes.ToArray();
		}
	}
}
