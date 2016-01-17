using System;
using System.Text;
using Xunit;

namespace CryptoSwift.Tests
{
	public class CryptographyManagerTests
	{
		private readonly CryptographyManager _cryptoManagaer;

		public CryptographyManagerTests()
		{
			var apiKey = Environment.GetEnvironmentVariable("baelor-test-apikey");
			var fingerprints = FingerprintManager.GenerateFingerprint(apiKey).Result;
			_cryptoManagaer = new CryptographyManager(new FingerprintManager(fingerprints));
		}

		[Fact]
		public void TestKeyGeneration()
		{
			var keyA = _cryptoManagaer.GenerateNewKey();
			var keyB = _cryptoManagaer.GenerateNewKey();

			Assert.False(keyA == keyB);
			Assert.True(keyA.Length * 8 == CryptographyManager.KeyBitSize);
			Assert.True(keyB.Length * 8 == CryptographyManager.KeyBitSize);
		}

		[Fact]
		public void TestIvGeneration()
		{
			var ivA = _cryptoManagaer.GenerateNewIv();
			var ivB = _cryptoManagaer.GenerateNewIv();

			Assert.False(ivA == ivB);
			Assert.True(ivA.Length * 8 == CryptographyManager.BlockBitSize);
			Assert.True(ivB.Length * 8 == CryptographyManager.BlockBitSize);
		}

		[Theory]
		[InlineData("1")]
		[InlineData("11")]
		[InlineData("Sample string to test encryption and decryption")]
		public void TestEncryptionAndDecryption(string data)
		{
			var key = _cryptoManagaer.GenerateNewKey();
			var iv = _cryptoManagaer.GenerateNewIv();

			var encryptedDataFingerprints = _cryptoManagaer.Encrypt(Encoding.ASCII.GetBytes(data), key, iv);
			var formattedEncryptedFingerprints = "";
			foreach (var fingerprint in encryptedDataFingerprints)
				formattedEncryptedFingerprints += fingerprint.Lyric + "\n";
			formattedEncryptedFingerprints = formattedEncryptedFingerprints.TrimEnd('\n');

			var decryptedData = _cryptoManagaer.Decrypt(formattedEncryptedFingerprints, key, iv);
			var decryptedPlainText = Encoding.ASCII.GetString(decryptedData);

			Assert.True(data == decryptedPlainText);
		}
	}
}
