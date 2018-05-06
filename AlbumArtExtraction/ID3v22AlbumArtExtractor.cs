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
		private Image _ParsePictureID3v22Tag(string filePath)
		{
			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				// ID3 Header 
				Helper.Skip(file, 6);

				// 各バイトの最上位ビットを無視した 28 ビット を算出
				var ID3Size = (Helper.ReadAsByte(file) << 21) + (Helper.ReadAsByte(file) << 14) + (Helper.ReadAsByte(file) << 7) + Helper.ReadAsByte(file);

				// v2.2 に関しては、Extreme Header は存在しない模様。

				// Frame Header
				var count = 0;
				while (count++ < 70)
				{
					// フレーム名
					var format = Helper.ReadAsAsciiString(file, 3);

					// フレーム終端情報の検査用
					var reg = Regex.IsMatch(format, "^[A-Z0-9]+$");

					// 読んだデータがフレームの終端であれば、ループ終了
					if (!reg) break;

					Debug.WriteLine($"format = {format}");
					var frameSize = Helper.ReadAsUInt(file, 3);

					// フォーマット判定
					if (format == "PIC")
					{
						// 1Byte: 文字コード, 3Byte: フォーマット, 1Byte: 種別は必ず存在する為、読み飛ばす
						Helper.Skip(file, 5);

						// 説明文を読み飛ばす (終端文字含む)
						var length = 1;
						while ((Helper.ReadAsByte(file) != 0x00U))
							length++;

						// byte データを画像として変換する
						var memory = new MemoryStream(Helper.ReadAsByteList(file, (int)frameSize - (5 + length)).ToArray());
						return Image.FromStream(memory);
					}

					// PIC でないフレームのため、フレーム自体を読み飛ばす
					Helper.Skip(file, (int)frameSize);
				}

				return null;
			}
		}

		#endregion

		#region Interface - IAlbumArtExtractor 

		/// <summary>
		/// 対象のファイルが形式と一致しているかを判別します
		/// </summary>
		public bool CheckType(string filePath)
		{
			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				return (Helper.ReadAsAsciiString(file, 3) == "ID3" &&
					Helper.ReadAsUShort(file) == 0x0200U) ? true : false;
		}

		/// <summary>
		/// アルバムアートを抽出します
		/// </summary>
		public Image Extract(string filePath)
		{
			return _ParsePictureID3v22Tag(filePath);
		}

		#endregion

	}
}
