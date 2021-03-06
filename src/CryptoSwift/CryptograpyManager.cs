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
		/// <summary>
		/// The Random Number Generator to use for cryptographic randomness.
		/// </summary>
		private readonly RandomNumberGenerator _randomNumberGenerator = RandomNumberGenerator.Create();

		/// <summary>
		/// The Fingerprint Manager to use for Encryption and Decryption.
		/// </summary>
		private readonly FingerprintManager _fingerprintManager;
		
		/// <summary>
		/// The bit size of the block cipher used in AES.
		/// </summary>
		public const int BlockBitSize = 128;

		/// <summary>
		/// The bit size of the key to use in AES.
		/// </summary>
		public const int KeyBitSize = 256;

		/// <summary>
		/// Creates a new Cryptography Manager based off of a Fingerprint Manager.
		/// </summary>
		/// <param name="fingerprintManager">The Fingerprint Manager to create a Cryptography Manager</param>
		public CryptographyManager(FingerprintManager fingerprintManager)
		{
			_fingerprintManager = fingerprintManager;
		}

		/// <summary>
		/// Encrypts data into an <see cref="IEnumerable"/> of <see cref="Fingerprint"/>'s.
		/// </summary>
		/// <param name="data">The data you want to encrypt.</param>
		/// <param name="key">A 256 bit random Key to encrypt the data. You can use the <see cref="GenerateNewKey"/> method to generate one.</param>
		/// <param name="iv">A 128 bit random IV. You can use the <see cref="GenerateNewIv"/> method to generate one.</param>
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
		/// Decrypts a set of Taylor Swift lyrics into the original byte data.
		/// </summary>
		/// <param name="data">The string representation of the Taylor Swift lyrics to decrypt.</param>
		/// <param name="key">The 256 bit Key used to encrypt the data.</param>
		/// <param name="iv">The 128 bit IV used to encrypt the data.</param>
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
		/// Encrypts byte data using the AES algorithm.
		/// </summary>
		/// <param name="secretMessage">The byte data to encrypt.</param>
		/// <param name="key">The 256 bit key to encrypt the data with.</param>
		/// <param name="iv">The 128 bit IV.</param>
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
		/// Decrypts byte data using the AES algorithm.
		/// </summary>
		/// <param name="secretMessage">The byte data to decrypt.</param>
		/// <param name="key">The 256 bit key used to encrypt the data.</param>
		/// <param name="iv">The 128 bit IV used to encrypt the data.</param>
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
