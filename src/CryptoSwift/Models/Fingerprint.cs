using Newtonsoft.Json;
using System;

namespace CryptoSwift.Models
{
	public class Fingerprint
	{
		/// <summary>
		/// The slug of the song the fingerprint is generated from.
		/// </summary>
		[JsonProperty("song_slug")]
		public string SongSlug { get; set; }

		/// <summary>
		/// The slug of the album the song belongs to.
		/// </summary>
		[JsonProperty("album_slug")]
		public string AlbumSlug { get; set; }
		
		/// <summary>
		/// The lyric of the song.
		/// </summary>
		[JsonProperty("lyric")]
		public string Lyric { get; set; }

		/// <summary>
		/// Where in the song the lyric starts.
		/// </summary>
		[JsonProperty("time_code_start")]
		public TimeSpan TimeCodeStart { get; set; }

		/// <summary>
		/// Where in the song the lyric ends.
		/// </summary>
		[JsonProperty("time_code_end")]
		public TimeSpan TimeCodeEnd { get; set; }
	}
}
