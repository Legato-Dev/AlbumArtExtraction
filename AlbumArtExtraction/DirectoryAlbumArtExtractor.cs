using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AlbumArtExtraction {
	/// <summary>
	/// ディレクトリからアルバムアートを抽出する機能を表します
	/// </summary>
	public class DirectoryAlbumArtExtractor : IAlbumArtExtractor {
		/// <summary>
		/// アルバムアートのようなファイル名のFileInfo一覧を取得します
		/// </summary>
		/// <param name="directory"></param>
		private IEnumerable<FileInfo> _GetFilesLikeAlbumArts(DirectoryInfo directory) =>
			from i in directory.GetFiles()
			where i.Extension == ".png" || i.Extension == ".jpeg" || i.Extension == ".jpg" || i.Extension == ".bmp"
			where
				i.Name.IndexOf(Path.GetFileNameWithoutExtension(i.Name)) != -1 ||
				i.Name.IndexOf("folder") != -1 ||
				i.Name.IndexOf("front") != -1 ||
				i.Name.IndexOf("cover") != -1 ||
				i.Name.IndexOf("album") != -1
			orderby i.Length descending
			select i;

		/// <summary>
		/// 対象のファイルが形式と一致しているかを判別します
		/// </summary>
		public bool CheckType(string filePath) {
			var fileCount = new FileInfo(filePath).Directory.EnumerateFiles().Count();
			var albumArtCount = _GetFilesLikeAlbumArts(new FileInfo(filePath).Directory).Count();

			// ディレクトリのファイル数が50個以下(間違ったアルバムアートが設定されることへの防止) & アルバムアートの画像ファイルがディレクトリにある
			return fileCount <= 50 && albumArtCount > 0;
		}

		/// <summary>
		/// アルバムアートを抽出します
		/// </summary>
		/// <exception cref="FileNotFoundException" />
		public Stream Extract(string filePath) {
			if (!File.Exists(filePath))
				throw new FileNotFoundException("指定されたファイルは存在しません");

			var fileInfo = _GetFilesLikeAlbumArts(new FileInfo(filePath).Directory).ElementAt(0);

			using (var file = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
			{
				var buf = new byte[file.Length];
				file.Read(buf, 0, buf.Length);
				return new MemoryStream(buf);
			}
		}
	}
}
