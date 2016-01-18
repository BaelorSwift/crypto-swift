using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CryptoSwift.Tests
{
	public class CryptographyManagerTests
	{
		//private readonly CryptographyManager _cryptoManagaer;

		public CryptographyManagerTests()
		{
		//	var apiKey = Environment.GetEnvironmentVariable("baelor-test-apikey");
		//	var fingerprints = FingerprintManager.GenerateFingerprint(apiKey).Result;
		//	_cryptoManagaer = new CryptographyManager(new FingerprintManager(fingerprints));
		}

		[Fact]
		public async Task TestKeyGeneration()
		{
			var apiKey = Environment.GetEnvironmentVariable("baelor-test-apikey");
			var fingerprints = await FingerprintManager.GenerateFingerprint(apiKey);
			var cryptoManagaer = new CryptographyManager(new FingerprintManager(fingerprints));

			var keyA = cryptoManagaer.GenerateNewKey();
			var keyB = cryptoManagaer.GenerateNewKey();

			Assert.False(keyA == keyB);
			Assert.True(keyA.Length * 8 == CryptographyManager.KeyBitSize);
			Assert.True(keyB.Length * 8 == CryptographyManager.KeyBitSize);
		}

		[Fact]
		public async Task TestIvGeneration()
		{
			var apiKey = Environment.GetEnvironmentVariable("baelor-test-apikey");
			var fingerprints = await FingerprintManager.GenerateFingerprint(apiKey);
			var cryptoManagaer = new CryptographyManager(new FingerprintManager(fingerprints));

			var ivA = cryptoManagaer.GenerateNewIv();
			var ivB = cryptoManagaer.GenerateNewIv();

			Assert.False(ivA == ivB);
			Assert.True(ivA.Length * 8 == CryptographyManager.BlockBitSize);
			Assert.True(ivB.Length * 8 == CryptographyManager.BlockBitSize);
		}

		[Theory]
		[InlineData("1")]
		[InlineData("11")]
		[InlineData("Sample string to test encryption and decryption")]
		public async Task TestEncryptionAndDecryption(string data)
		{
			var apiKey = Environment.GetEnvironmentVariable("baelor-test-apikey");
			var fingerprints = await FingerprintManager.GenerateFingerprint(apiKey);
			var cryptoManagaer = new CryptographyManager(new FingerprintManager(fingerprints));

			var key = cryptoManagaer.GenerateNewKey();
			var iv = cryptoManagaer.GenerateNewIv();

			var encryptedDataFingerprints = cryptoManagaer.Encrypt(Encoding.ASCII.GetBytes(data), key, iv);
			var formattedEncryptedFingerprints = "";
			foreach (var fingerprint in encryptedDataFingerprints)
				formattedEncryptedFingerprints += fingerprint.Lyric + "\n";
			formattedEncryptedFingerprints = formattedEncryptedFingerprints.TrimEnd('\n');

			var decryptedData = cryptoManagaer.Decrypt(formattedEncryptedFingerprints, key, iv);
			var decryptedPlainText = Encoding.ASCII.GetString(decryptedData);

			Assert.True(data == decryptedPlainText);
		}
	}
}
