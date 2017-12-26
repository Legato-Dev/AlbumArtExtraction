namespace AlbumArtExtraction.Mp3
{
	/// <summary>
	/// ID3 v1, v2 タグに埋め込まれているメタデータを管理します。
	/// </summary>
	public class MetaDataType
	{
		/// <summary>
		/// ID3 v1, v2 タグに埋め込まれているファイル名。
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// ID3 v1, v2 タグに埋め込まれている曲の長さ(ms)。
		/// </summary>
		public ulong Length { get; set; }

		/// <summary>
		/// ID3 v1, v2 タグに埋め込まれている曲名。
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// ID3 v1, v2 タグに埋め込まれているアーティスト名。
		/// </summary>
		public string Artist { get; set; }

		/// <summary>
		/// ID3 v1, v2 タグに埋め込まれているリリース年。
		/// </summary>
		public string Year { get; set; }

		/// <summary>
		/// ID3 v1, v2 タグに埋め込まれているコメント。
		/// </summary>
		public string Comment { get; set; }

		/// <summary>
		/// ID3 v1, v2 タグに埋め込まれている歌詞。
		/// </summary>
		public string Lyrics { get; set; }

		/// <summary>
		/// ID3 v1, v2 タグに埋め込まれているトラック番号。
		/// </summary>
		public uint TrackNumber { get; set; }

		/// <summary>
		/// ID3 v1, v2 タグに埋め込まれているジャンル。
		/// </summary>
		public string Genre { get; set; }

		/// <summary>
		/// ID3 v1, v2 タグに埋め込まれている著作権者。
		/// </summary>
		public string Copyright { get; set; }

		/// <summary>
		/// ID3 v1, v2 タグに埋め込まれているディスク番号。
		/// </summary>
		public string DiscNumber { get; set; }

		/// <summary>
		/// ID3 v1, v2 タグに埋め込まれている作曲者。
		/// </summary>
		public string  Composer { get; set; }

		/// <summary>
		/// ID3 v1, v2 タグに埋め込まれているアルバムアーティスト名。
		/// </summary>
		public string Orchestra { get; set; }

		/// <summary>
		/// ID3 v1, v2 タグに埋め込まれている発行者。
		/// </summary>
		public string Publisher { get; set; }

		/// <summary>
		/// ID3 v1, v2 タグに埋め込まれているエンコード情報。
		/// </summary>
		public string EncodedInfo { get; set; }

	}
}
