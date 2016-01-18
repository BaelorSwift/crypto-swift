using System.Security.Cryptography;

namespace CryptoSwift.Helpers
{
	public static class RandomHelper
	{
		/// <summary>
		/// Generates a set of non-negative cryptographically random integers.
		/// </summary>
		/// <param name="length">The number of random integers to generate</param>
		/// <param name="maxExclusive">The exclusive upper bound of possible integers.</param>
		public unsafe static int[] GenerateRandom(int length, int maxExclusive)
		{
			return GenerateRandom(length, 0, maxExclusive);
		}

		/// <summary>
		/// Generates a set of cryptographically random integers.
		/// </summary>
		/// <param name="length">The number of random integers to generate</param>
		/// <param name="minInclusive">The inclusive lower bound of possible integers.</param>
		/// <param name="maxExclusive">The exclusive upper bound of possible integers.</param>
		public unsafe static int[] GenerateRandom(int length, int minInclusive, int maxExclusive)
		{
			var bytes = new byte[length * 4];
			var ints = new int[length];

			var ratio = uint.MaxValue / (double)(maxExclusive - minInclusive);

			using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
			{
				generator.GetBytes(bytes);
				fixed (byte* b = bytes)
				{
					uint* i = (uint*)b;
					for (int j = 0; j < length; j++, i++)
					{
						ints[j] = minInclusive + (int)(*i / ratio);
					}
				}
			}

			return ints;
		}
	}
}
