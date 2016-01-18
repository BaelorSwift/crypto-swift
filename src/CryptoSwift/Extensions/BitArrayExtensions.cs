using System;
using System.Collections;

namespace CryptoSwift.Extensions
{
	public static class BitArrayExtensions
	{
		/// <summary>
		/// Converts a bit array to a 16 bit integer.
		/// </summary>
		/// <param name="bitArray">The bit array containing all the bits.</param>
		public static int ToInt16(this BitArray bitArray)
		{
			int value = 0;

			for (int i = 0; i < bitArray.Length; i++)
				if (bitArray[i])
					value += Convert.ToInt16(Math.Pow(2, i));

			return value;
		}

		/// <summary>
		/// Converts a bit array to a byte array.
		/// </summary>
		/// <param name="bitArray">The bit array containing all the bits.</param>
		public static byte[] ToByteArray(this BitArray bitArray)
		{
			var bytes = new byte[bitArray.Length / 8];
			for (var i = 0; i < bitArray.Length; i += 8)
			{
				byte b = 0x00;
				for (var y = 0; y < 8; y++)
					if (bitArray[i + y])
						b |= (byte)(1 << y);
				
				bytes[i == 0 ? 0 : (i / 8)] = b;
			}

			return bytes;
		}
	}
}
