using System.Text;

namespace CryptoSwift.Console
{
	public class Program
	{
		public void Main(string[] args)
		{
			var fingerprintManager = new FingerprintManager("fingerprints.json");
			var cryptoManager = new CryptographyManager(fingerprintManager);
			var key = cryptoManager.GenerateNewKey();
			var iv = cryptoManager.GenerateNewIv();

			System.Console.Write("Enter Text to be encrypted: ");
			var input = System.Console.ReadLine();
			var encryptedFingerprintData = cryptoManager.Encrypt(Encoding.ASCII.GetBytes(input), key, iv);

			var output = "";
			foreach (var fingerprint in encryptedFingerprintData)
				output += fingerprint.Lyric + "\n";
			output = output.TrimEnd('\n');

			var decryptedOutput = cryptoManager.Decrypt(output, key, iv);

			System.Console.WriteLine();
			System.Console.WriteLine("=== ENCRYPTED DATA ===");
			System.Console.WriteLine(output);
			System.Console.WriteLine("======================");
			System.Console.WriteLine();
			System.Console.WriteLine("=== DECRYPTED DATA ===");
			System.Console.WriteLine(Encoding.ASCII.GetString(decryptedOutput));
			System.Console.WriteLine("=====================");
			System.Console.ReadLine();
		}
	}
}
