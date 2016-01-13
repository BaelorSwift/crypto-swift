using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			var paddingLength = 10 - (length % 10);
			if (paddingLength <= 0) paddingLength = 10;
			var byteData = new byte[length + paddingLength];
			Array.Copy(Encoding.ASCII.GetBytes(data), byteData, length);
			byteData[byteData.Length - 1] = (byte)paddingLength;

			// do aes encrypytion

			// do taylor swift encoding
			var bitArray = new BitArray(byteData);
			var x = new List<Fingerprint>();

			if (bitArray.Length % 10 != 0)
				throw new InvalidOperationException();

			for (var i = 0; i < bitArray.Length;)
			{
				var tbA = new BitArray(new bool[]
				{
					bitArray.Get(i++), // 0x00
					bitArray.Get(i++), // 0x01
					bitArray.Get(i++), // 0x02
					bitArray.Get(i++), // 0x03
					bitArray.Get(i++), // 0x04
					bitArray.Get(i++), // 0x05
					bitArray.Get(i++), // 0x06
					bitArray.Get(i++), // 0x07
					bitArray.Get(i++), // 0x08
					bitArray.Get(i++)  // 0x09
				});

				x.Add(_fingerprintManager.GetFromIndex(tbA.ToInt16()));
			}
			
			return x;
		}

		public string Decrypt(string encryptedData, string key)
		{
			// do aes decryption

			var fingerprintIds = new List<int>();
			foreach (var lyric in encryptedData.Split('\n'))
			{
				if (lyric.Trim() == string.Empty)
					continue;

				fingerprintIds.Add(
					_fingerprintManager.GetIndexOfFingerprint(
						_fingerprintManager.GetFromLyric(lyric)));
			}

			var bitArray = new BitArray(fingerprintIds.Count * 10);
			var index = 0;
			foreach (var x in fingerprintIds)
			{
				var arr = new BitArray(new int[] { x });
				bitArray.Set(index++, arr.Get(0));
				bitArray.Set(index++, arr.Get(1));
				bitArray.Set(index++, arr.Get(2));
				bitArray.Set(index++, arr.Get(3));
				bitArray.Set(index++, arr.Get(4));
				bitArray.Set(index++, arr.Get(5));
				bitArray.Set(index++, arr.Get(6));
				bitArray.Set(index++, arr.Get(7));
				bitArray.Set(index++, arr.Get(8));
				bitArray.Set(index++, arr.Get(9));
			}

			var originalData = bitArray.ToByteArray();
			
			var paddingLength = originalData[originalData.Length - 1];
			originalData = new List<byte>(originalData).Take(originalData.Length - paddingLength).ToArray();

			var originalText = Encoding.ASCII.GetString(originalData);

			return originalText;
		}
	}
}