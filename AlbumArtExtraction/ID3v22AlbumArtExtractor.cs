using System.Diagnostics;
using System.Drawing;
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
		/// ID3v2.2 タグから Image を取り出します
		/// </summary>
		private Image _ReadPictureInFrameHeaders(Stream file)
		{
			var count = 0;
			while (count++ < 63)
			{
				// Frame Name
				var frameName = Helper.ReadAsAsciiString(file, 3);

				// 有効な Frame Name であるかどうかを示す
				var validName = Regex.IsMatch(frameName, "^[A-Z0-9]+$");

				// 無効な Frame Name であれば、ループ終了
				if (!validName) break;

				Debug.WriteLine($"frameName = {frameName}");
				var frameSize = Helper.ReadAsUInt(file, 3);

				// PIC Frame の判定
				if (frameName == "PIC")
				{
					// 1Byte: 文字コード, 3Byte: フォーマット, 1Byte: 種別は必ず存在する為、読み飛ばす
					Helper.Skip(file, 5);

					// 説明文を読み飛ばす (終端文字含む)
					var length = 1;
					while ((Helper.ReadAsByte(file) != 0x00U))
						length++;

					var imageSource = Helper.ReadAsByteList(file, (int)frameSize - (5 + length));

					// byte データを画像として変換する
					using (var memory = new MemoryStream())
					{
						memory.Write(imageSource.ToArray(), 0, imageSource.Count);

						using (var image = Image.FromStream(memory))
						{
							return new Bitmap(image);
						}
					}
				}

				// PIC Frame でないため、フレーム自体を読み飛ばす
				Helper.Skip(file, (int)frameSize);
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
				return (Helper.ReadAsAsciiString(file, 3) == "ID3" && Helper.ReadAsUShort(file) == 0x0200U);
		}

		/// <summary>
		/// アルバムアートを抽出します
		/// </summary>
		public Image Extract(string filePath)
		{
			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				// ID3v2 Header 読み飛ばし
				Helper.Skip(file, 10);

				// v2.2 に関しては、ID3 Extended Header や、そのフラグは存在しない模様。

				// Frame Headers
				return _ReadPictureInFrameHeaders(file);
			}
		}

		#endregion

	}
}
