/*
*  Parts of this file are lifted from the following gist
*  made by user @jbtule - https://gist.github.com/jbtule/4336842#file-aesthenhmac-cs
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
		/// <param name="iv"></param>
		/// <returns></returns>
		public IEnumerable<Fingerprint> Encrypt(byte[] data, byte[] key, byte[] iv)
		{
			// Validate data parameter
			if (data == null)
				throw new ArgumentNullException("data");
			if (data.Length == 0)
				return new List<Fingerprint>();

			// Validate key parameter
			if (key == null)
				throw new ArgumentNullException("key");
			if (key.Length == 0)
				throw new ArgumentException("The key parameter can not be empty.", "key");
			if (key.Length * 8 != KeyBitSize)
				throw new ArgumentException($"The key parameter must be {KeyBitSize} bits long.", "key");

			// Validate iv parameter
			if (iv == null)
				throw new ArgumentNullException("iv");
			if (iv.Length == 0)
				throw new ArgumentException("The iv parameter can not be empty.", "iv");
			if (iv.Length * 8 != BlockBitSize)
				throw new ArgumentException($"The iv parameter must be {BlockBitSize} bits long.", "iv");

			// Encrypt data with AES
			var encryptedData = EncryptPayload(data, key, iv);

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
				var blockBitArray = new BitArray(new bool[]
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
					_fingerprintManager.GetFromIndex(blockBitArray.ToInt16()));
			}

			return fingerprints;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <param name="iv"></param>
		/// <returns></returns>
		public byte[] Decrypt(string data, byte[] key, byte[] iv)
		{
			// Validate key parameter
			if (key == null)
				throw new ArgumentNullException("key");
			if (key.Length == 0)
				throw new ArgumentException("The key parameter can not be empty.", "key");
			if (key.Length * 8 != KeyBitSize)
				throw new ArgumentException($"The key parameter must be {KeyBitSize} bits long.", "key");

			// Validate iv parameter
			if (iv == null)
				throw new ArgumentNullException("iv");
			if (iv.Length == 0)
				throw new ArgumentException("The iv parameter can not be empty.", "iv");
			if (iv.Length * 8 != BlockBitSize)
				throw new ArgumentException($"The iv parameter must be {BlockBitSize} bits long.", "iv");

			// Get fingerprint id's from the fingerprint strings
			var fingerprintIds = new List<int>();
			foreach (var lyric in data.Split('\n'))
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

			var encryptedData = bitArray.ToByteArray();

			// Remove Padding from encrypted data
			var paddingLength = encryptedData[encryptedData.Length - 1];
			Array.Resize(ref encryptedData, encryptedData.Length - paddingLength);

			// Decrypt Data
			var decryptedData = DecryptPayload(encryptedData, key, iv);
			return decryptedData;
		}

		/// <summary>
		/// Helper that generates a random key on each call.
		/// </summary>
		public byte[] GenerateNewKey()
		{
			var key = new byte[KeyBitSize / 8];
			_randomNumberGenerator.GetBytes(key);
			return key;
		}

		/// <summary>
		/// Helper that generates a random iv on each call.
		/// </summary>
		public byte[] GenerateNewIv()
		{
			using (var aes = Aes.Create())
			{
				aes.GenerateIV();
				return aes.IV;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="secretMessage"></param>
		/// <param name="key"></param>
		/// <param name="iv"></param>
		/// <returns></returns>
		private byte[] EncryptPayload(byte[] secretMessage, byte[] key, byte[] iv)
		{
			using (var aes = Aes.Create())
			{
				aes.BlockSize = BlockBitSize;
				aes.KeySize = KeyBitSize;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;

				using (var encrypter = aes.CreateEncryptor(key, iv))
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="encryptedMessage"></param>
		/// <param name="key"></param>
		/// <param name="iv"></param>
		/// <returns></returns>
		private byte[] DecryptPayload(byte[] encryptedMessage, byte[] key, byte[] iv)
		{
			using (var aes = Aes.Create())
			{
				aes.BlockSize = BlockBitSize;
				aes.KeySize = KeyBitSize;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;

				using (var decrypter = aes.CreateDecryptor(key, iv))
				using (var plainTextStream = new MemoryStream())
				{
					using (var decrypterStream = new CryptoStream(plainTextStream, decrypter, CryptoStreamMode.Write))
					using (var binaryWriter = new BinaryWriter(decrypterStream))
					{
						binaryWriter.Write(encryptedMessage);
					}

					return plainTextStream.ToArray();
				}
			}
		}
	}
}
