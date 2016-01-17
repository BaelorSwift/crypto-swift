/*
*  Parts of this file are lifted from the following gist
*  made by user @jbtule - https://gist.github.com/jbtule/4336842#file-aesthenhmac-cs
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using CryptoSwift.Extensions;
using CryptoSwift.Models;

namespace CryptoSwift
{
	public class CryptographyManager
	{
		private readonly RandomNumberGenerator _randomNumberGenerator = RandomNumberGenerator.Create();
		private readonly FingerprintManager _fingerprintManager;

		// Preconfigured Encryption Parameters
		public static readonly int BlockBitSize = 128;
		public static readonly int KeyBitSize = 256;

		// Preconfigured Password Key Derivation Parameters
		public static readonly int SaltBitSize = 64;
		public static readonly int Iterations = 10000;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fingerprintManager"></param>
		public CryptographyManager(FingerprintManager fingerprintManager)
		{
			_fingerprintManager = fingerprintManager;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public IEnumerable<Fingerprint> Encrypt(byte[] data, string key)
		{
			// Encrypt data with AES
			var encryptedData = EncryptPayload(data, key);

			// Calculate padding length needed for the encrypted data
			var length = encryptedData.Length;
			var paddingLength = 10 - (length % 10);
			if (paddingLength <= 0) paddingLength = 10;
			Array.Resize(ref encryptedData, length + paddingLength);

			// Create cryptographically random padding
			var padding = new byte[paddingLength];
			_randomNumberGenerator.GetBytes(padding);
			padding[paddingLength - 1] = (byte) paddingLength;
			Array.Copy(padding, 0, encryptedData, length, padding.Length);

			// Create BitArray of the encrypted data
			var encryptedDataBitArray = new BitArray(encryptedData);
			var fingerprints = new List<Fingerprint>();

			// Check to make sure the number of bits in the file is divisible by 10
			if (encryptedDataBitArray.Length % 10 != 0)
				throw new InvalidOperationException();

			for (var i = 0; i < encryptedDataBitArray.Length;)
			{
				var tbA = new BitArray(new bool[]
				{
					encryptedDataBitArray.Get(i++), // 0x00
					encryptedDataBitArray.Get(i++), // 0x01
					encryptedDataBitArray.Get(i++), // 0x02
					encryptedDataBitArray.Get(i++), // 0x03
					encryptedDataBitArray.Get(i++), // 0x04
					encryptedDataBitArray.Get(i++), // 0x05
					encryptedDataBitArray.Get(i++), // 0x06
					encryptedDataBitArray.Get(i++), // 0x07
					encryptedDataBitArray.Get(i++), // 0x08
					encryptedDataBitArray.Get(i++)  // 0x09
				});

				fingerprints.Add(
					_fingerprintManager.GetFromIndex(tbA.ToInt16()));
			}

			return fingerprints;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="encryptedData"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public byte[] Decrypt(string encryptedData, string key)
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
			return new List<byte>(originalData).Take(originalData.Length - paddingLength).ToArray();
		}

		/// <summary>
		/// Helper that generates a random key on each call.
		/// </summary>
		private byte[] NewKey()
		{
			var key = new byte[KeyBitSize / 8];
			_randomNumberGenerator.GetBytes(key);
			return key;
		}

		private byte[] EncryptPayload(byte[] secretMessage, string key)
		{
			byte[] cryptKey;
			byte[] iv;

			// Generate cryptkey
			using (var generator = new Rfc2898DeriveBytes(key, SaltBitSize / 8, Iterations))
			{
				var salt = generator.Salt;
				cryptKey = generator.GetBytes(KeyBitSize / 8);
			}
			
			using (var aes = Aes.Create())
			{
				aes.BlockSize = BlockBitSize;
				aes.KeySize = KeyBitSize;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;

				aes.GenerateIV();
				iv = aes.IV;

				using (var encrypter = aes.CreateEncryptor(cryptKey, iv))
				using (var cipherStream = new MemoryStream())
				{
					using (var cryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
					using (var binaryWriter = new BinaryWriter(cryptoStream))
					{
						binaryWriter.Write(secretMessage);
					}

					return cipherStream.ToArray();
				}
			}
		}
	}
}
