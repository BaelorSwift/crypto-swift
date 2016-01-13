﻿using System;
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
			await FingerprintManager.GenerateFingerprint(apiKey);
		}
	}
}
