using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace AlbumArtExtraction
{
	/// <summary>
	/// mp3形式(ID3v2.3/2.4) のファイルからアルバムアートを抽出する機能を表します
	/// <para>拡張ヘッダーのあるファイルには利用されません</para>
	/// </summary>
	public class ID3v2AlbumArtExtractor : IAlbumArtExtractor
	{

		#region Constractor 

		public ID3v2AlbumArtExtractor() { }

		#endregion

		#region Parsing picture for ID3Tag.

		/// <summary>
		/// ID3v2.3/2.4 タグから Image を取り出します
		/// </summary>
		private Image _ReadPictureInFrameHeaders(Stream file)
		{
			var count = 1;

			while (true)
			{
				// Frame Name
				var frameName = file.ReadAsAsciiString(4);

				// 有効な Frame Name であるかどうかを示す
				var validName = Regex.IsMatch(frameName, "^[A-Z0-9]+$");

				// 無効な Frame Name であれば、ループ終了
				if (!validName) break;

				Debug.WriteLine($"frameName = {frameName}");
				var frameSize = file.ReadAsUInt();

				// フラグ読み飛ばし
				file.Skip(2);

				// APIC Frame の判定
				if (frameName == "APIC")
				{
					var removeCount = 0;

					file.Skip(1);
					removeCount += 1;

					while (file.ReadAsByte() != 0x00U) removeCount++;

					file.Skip(1);
					removeCount += 1;

					while (file.ReadAsByte() != 0x00U) removeCount++;

					var imageSource = file.ReadAsByteList((int)frameSize - removeCount);

					// byte データを画像として変換する
					using (var memory = new MemoryStream())
					{
						memory.Write(imageSource.ToArray(), 0, imageSource.Count);
						using (var image = Image.FromStream(memory))
							return new Bitmap(image);
					}
				}

				// PIC Frame でないため、フレーム自体を読み飛ばす
				file.Skip((int)frameSize);

				if (count > 74)
					throw new InvalidDataException("フレーム数が正常な範囲を超えました");

				count++;
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
			{
				var formatId = file.ReadAsAsciiString(3);
				var version = file.ReadAsUShort();
				var headerFlag = file.ReadAsByte();

				// フォーマット判定
				if (!(formatId == "ID3" && (version == 0x0300U || version == 0x0400U)))
					return false;

				// extended header が無い場合のみ一致と判定
				return (headerFlag & 0x0040U) == 0;
			}
		}

		/// <summary>
		/// アルバムアートを抽出します
		/// </summary>
		public Image Extract(string filePath)
		{
			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				// ID3v2 Header 読み飛ばし
				file.Skip(10);

				// Frame Headers
				return _ReadPictureInFrameHeaders(file);
			}
		}

		#endregion

	}
}
