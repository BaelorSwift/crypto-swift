using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CryptoSwift.Tests
{
	public class FingerprintManagerTests
	{
		[Fact]
		public async Task GenerateFingerprintTest()
		{
			var apiKey = Environment.GetEnvironmentVariable("baelor-test-apikey");
			var fingerprints = await FingerprintManager.GenerateFingerprint(apiKey);

			Assert.True(fingerprints.Count() == FingerprintManager.FingerprintCount);
		}
	}
}
