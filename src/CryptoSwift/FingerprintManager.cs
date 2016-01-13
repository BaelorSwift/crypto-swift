using BaelorNet;
using CryptoSwift.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using CryptoSwift.Extentions;
using System.IO;
using Newtonsoft.Json;

namespace CryptoSwift
{
	public class FingerprintManager
	{
		private IList<Fingerprint> _fingerprintData;

		public FingerprintManager(string filePath)
		{
			var fingerprintText = File.ReadAllText(filePath);
			_fingerprintData = JsonConvert.DeserializeObject<List<Fingerprint>>(fingerprintText);
		}

		public FingerprintManager(List<Fingerprint> fingerprintData)
		{
			_fingerprintData = fingerprintData;
		}

		public static async Task<IEnumerable<Fingerprint>> GenerateFingerprint(string baelorApiKey)
		{
			var baelorClient = new BaelorClient(baelorApiKey);
			var songs = await baelorClient.Songs();
			var fingerprints = new List<Fingerprint>();

			foreach (var song in songs)
			{
				var index = 0;
				foreach (var lyric in song.Lyrics.Where(l => !string.IsNullOrWhiteSpace(l.Content)))
				{
					fingerprints.Add(new Fingerprint
					{
						SongSlug = song.Slug,
						Lyric = lyric.Content,
						TimeCodeStart = lyric.TimeCode,
						TimeCodeEnd = (index == song.Lyrics.Count() - 1) ? song.Length : song.Lyrics.ElementAt(index + 1).TimeCode
					});

					index++;
				}
			}

			fingerprints.Shuffle();
			return fingerprints.Distinct().Take(1024);
		}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(_fingerprintData);
		}
	}
}
