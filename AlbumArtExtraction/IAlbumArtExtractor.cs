using System.IO;

namespace AlbumArtExtraction {
	/// <summary>
	/// アルバムアートを抽出するために必要となるメンバを公開します
	/// </summary>
	public interface IAlbumArtExtractor {
		bool CheckType(string filePath);
		Stream Extract(string filePath);
	}
}
