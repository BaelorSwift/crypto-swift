using System;
using System.Collections;
using System.Collections.Generic;
using CryptoSwift.Extensions;
using CryptoSwift.Models;

namespace CryptoSwift
{
	public class CryptographyManager
	{
		private readonly FingerprintManager _fingerprintManager;

		public CryptographyManager(FingerprintManager fingerprintManager)
		{
			_fingerprintManager = fingerprintManager;
		}

		public IEnumerable<Fingerprint> EncryptString(string data, string key)
		{
			var length = data.Length;
			var paddingLength = (10 % length);
			if (paddingLength == 0) paddingLength = 10;
			var byteData = new byte[length + paddingLength];
			Array.Copy(System.Text.Encoding.ASCII.GetBytes(data), byteData, length);
			byteData[byteData.Length - 1] = (byte)paddingLength;

			// do aes encrypytion

			// do taylor swift encoding
			var bitArray = new BitArray(byteData);
			var x = new List<Fingerprint>();

			for (var i = 0; i < bitArray.Length;)
			{
				var tbA = new BitArray(new bool[]
				{
					bitArray.Get(i++),
					bitArray.Get(i++),
					bitArray.Get(i++),
					bitArray.Get(i++),
					bitArray.Get(i++),
					bitArray.Get(i++),
					bitArray.Get(i++),
					bitArray.Get(i++),
					bitArray.Get(i++),
					bitArray.Get(i++)
				});

				x.Add(_fingerprintManager.GetFromIndex(tbA.ToInt16()));
			}

			return x;
		}
	}
}