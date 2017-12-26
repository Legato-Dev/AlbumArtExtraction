using System;
using System.Collections.Generic;
using System.IO;

namespace AlbumArtExtraction {
	public class Selector {
		public Selector() { }

		/// <summary>
		/// 対象のファイルパスに対して利用可能な AlbumArtExtractor を選択して取得します
		/// </summary>
		/// <param name="filePath">アルバムアート抽出の対象ファイル</param>
		/// <exception cref="FileNotFoundException" />
		/// <exception cref="NotSupportedException" />
		public IAlbumArtExtractor SelectAlbumArtExtractor(string filePath) {
			//if (!File.Exists(filePath))
			//	throw new ArgumentException("指定されたファイルパスは無効です", "filePath");
			if (!File.Exists(filePath)) {
				throw new FileNotFoundException("指定されたファイルは存在しません");
			}

			var extractors = new List<IAlbumArtExtractor> {
				new FlacAlbumArtExtractor(),
				// new ID3v23AlbumArtExtractor(),
				new DirectoryAlbumArtExtractor()
			};
			var extractor = extractors.Find(i => i.CheckType(filePath));

			if (extractor == null) {
				throw new NotSupportedException("CurrentTrackからAlbumArtを抽出する方法が定義されていません");
			}

			return extractor;
		}
	}
}
