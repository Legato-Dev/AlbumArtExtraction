﻿using System.Drawing;

namespace AlbumArtExtraction {
	/// <summary>
	/// アルバムアートを抽出するために必要となるメンバを公開します
	/// </summary>
	public interface IAlbumArtExtractor {
		bool CheckType(string filePath);
		Image Extract(string filePath);
	}
}
