using System;
using System.Threading.Tasks;
using Xunit;

namespace CryptoSwift.Tests
{
	public class CryptographyManagerTests
	{
		[Fact]
		public async Task Test()
		{
			var apiKey = Environment.GetEnvironmentVariable("baelor-test-apikey");
			var fingerprints = await FingerprintManager.GenerateFingerprint(apiKey);
			var cryptoManager = new CryptographyManager(new FingerprintManager(fingerprints));
			
			cryptoManager.EncryptString("hannah", "123");
		}
	}
}
