using Newtonsoft.Json;
using System;

namespace CryptoSwift.Models
{
	public class Fingerprint
	{
		[JsonProperty("song_slug")]
		public string SongSlug { get; set; }

		[JsonProperty("lyric")]
		public string Lyric { get; set; }

		[JsonProperty("time_code_start")]
		public TimeSpan TimeCodeStart { get; set; }

		[JsonProperty("time_code_end")]
		public TimeSpan TimeCodeEnd { get; set; }
	}
}
