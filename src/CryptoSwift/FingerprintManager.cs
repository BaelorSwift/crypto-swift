using BaelorNet;
using CryptoSwift.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoSwift.Extensions;
using System.IO;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace CryptoSwift
{
	public class FingerprintManager
	{
		/// <summary>
		/// The collection fingerprints loaded in by the constructor.
		/// </summary>
		private Collection<Fingerprint> _fingerprintData;

		/// <summary>
		/// The number of Fingerprints to generate.
		/// </summary>
		public const int FingerprintCount = 1024;

		/// <summary>
		/// Creates a new Fingerprint Manager based off of a fingerprint from a local file.
		/// </summary>
		/// <param name="filePath">The file path of the fingerprint data.</param>
		public FingerprintManager(string filePath)
		{
			var fingerprintText = File.ReadAllText(filePath);
			_fingerprintData = JsonConvert.DeserializeObject<Collection<Fingerprint>>(fingerprintText);
		}

		/// <summary>
		/// Creates a new Fingerprint Manager based off of a collection of fingerprint objects.
		/// </summary>
		/// <param name="fingerprintData">The collection of fingerprint objects.</param>
		public FingerprintManager(IEnumerable<Fingerprint> fingerprintData)
		{
			_fingerprintData = new Collection<Fingerprint>(fingerprintData.ToList());
		}

		/// <summary>
		/// Returns the fingerprint at a certain index within the collection.
		/// </summary>
		/// <param name="index">The index of the desired fingerprint.</param>
		public Fingerprint GetFromIndex(int index)
		{
			return _fingerprintData.ElementAt(index);
		}

		/// <summary>
		/// Returns the fingerprint based off of a lyric. Guaranteed to be unique, as distinct is run in the fingerprint generation.
		/// </summary>
		/// <param name="lyric">The lyric specified to find the relevant fingerprint.</param>
		public Fingerprint GetFromLyric(string lyric)
		{
			return _fingerprintData.Single(f => f.Lyric.ToLowerInvariant() == lyric.ToLowerInvariant());
		}

		/// <summary>
		/// Returns the index in the collection of a fingerprint.
		/// </summary>
		/// <param name="fingerprint">The fingerprint specified to find the relevant index.</param>
		public int GetIndexOfFingerprint(Fingerprint fingerprint)
		{
			return _fingerprintData.IndexOf(fingerprint);
		}

		/// <summary>
		/// Returns a JSON string representation of the fingerprint collection.
		/// </summary>
		public string ToJson()
		{
			return JsonConvert.SerializeObject(_fingerprintData);
		}
		
		/// <summary>
		/// Generates a cryptographically random and unique collection of fingerprints.
		/// </summary>
		/// <param name="baelorApiKey">An Api Key for the Baelor Api (https://baelor.io)</param>
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
						AlbumSlug = song.Album.Slug,
						Lyric = lyric.Content,
						TimeCodeStart = lyric.TimeCode,
						TimeCodeEnd = (index == song.Lyrics.Count() - 1) ? song.Length : song.Lyrics.ElementAt(index + 1).TimeCode
					});

					index++;
				}
			}

			fingerprints.Shuffle();
			return fingerprints.DistinctBy(f => f.Lyric.Trim().ToLowerInvariant()).Take(FingerprintCount);
		}
	}
}
