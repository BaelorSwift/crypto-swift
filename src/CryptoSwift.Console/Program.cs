using System;

namespace CryptoSwift.Console
{
	public class Program
	{
		public void Main(string[] args)
		{
			var apiKey = Environment.GetEnvironmentVariable("baelor-test-apikey");
			var fingerprints = FingerprintManager.GenerateFingerprint(apiKey).Result;
			var cryptoManager = new CryptographyManager(new FingerprintManager(fingerprints));
			
			System.Console.Write("Enter Text to be encrypted: ");
			var input = System.Console.ReadLine();
			var encryptedFingerprintData = cryptoManager.EncryptString(input, "123");

			var output = "";
			foreach (var fingerprint in encryptedFingerprintData)
				output += fingerprint.Lyric + "\n";

			System.Console.WriteLine(output);
			System.Console.ReadLine();
		}
	}
}
