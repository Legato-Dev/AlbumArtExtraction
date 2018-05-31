using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace AlbumArtExtraction
{
	/// <summary>
	/// mp3形式(ID3v2.2) のファイルからアルバムアートを抽出する機能を表します
	/// </summary>
	public class ID3v22AlbumArtExtractor : IAlbumArtExtractor
	{

		#region Constractor 

		public ID3v22AlbumArtExtractor() { }

		#endregion

		#region Parsing picture for ID3Tag.

		/// <summary>
		/// ID3v2.2 タグから画像データの Stream を取り出します
		/// </summary>
		private Stream _ReadPictureInFrameHeaders(Stream file)
		{
			var count = 0;
			while (count++ < 63)
			{
				// Frame Name
				var frameName = file.ReadAsAsciiString(3);

				// 有効な Frame Name であるかどうかを示す
				var validName = Regex.IsMatch(frameName, "^[A-Z0-9]+$");

				// 無効な Frame Name であれば、ループ終了
				if (!validName) break;

				Debug.WriteLine($"frameName = {frameName}");
				var frameSize = file.ReadAsUInt(3);

				// PIC Frame の判定
				if (frameName == "PIC")
				{
					// 1Byte: 文字コード, 3Byte: フォーマット, 1Byte: 種別は必ず存在する為、読み飛ばす
					file.Skip(5);

					// 説明文を読み飛ばす (終端文字含む)
					var length = 1;
					while ((file.ReadAsByte() != 0x00U))
						length++;

					var imageSource = file.ReadAsByteList((int)frameSize - (5 + length));

					return new MemoryStream(imageSource.ToArray());
				}

				// PIC Frame でないため、フレーム自体を読み飛ばす
				file.Skip((int)frameSize);
			}

			return null;
		}

		#endregion

		#region Interface - IAlbumArtExtractor 

		/// <summary>
		/// 対象のファイルが形式と一致しているかを判別します
		/// </summary>
		public bool CheckType(string filePath)
		{
			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				return (file.ReadAsAsciiString(3) == "ID3" && file.ReadAsUShort() == 0x0200U);
		}

		/// <summary>
		/// アルバムアートを抽出します
		/// </summary>
		public Stream Extract(string filePath)
		{
			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				// ID3v2 Header 読み飛ばし
				file.Skip(10);

				// v2.2 に関しては、ID3 Extended Header や、そのフラグは存在しない模様。

				// Frame Headers
				return _ReadPictureInFrameHeaders(file);
			}
		}

		#endregion

	}
}
