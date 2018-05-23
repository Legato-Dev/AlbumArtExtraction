using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace AlbumArtExtraction {
	/// <summary>
	/// FLAC形式のファイルからアルバムアートを抽出する機能を表します
	/// </summary>
	public class FlacAlbumArtExtractor : IAlbumArtExtractor {
		/// <summary>
		/// メタデータブロックを読み取ります
		/// </summary>
		/// <param name="stream">対象の Stream</param>
		private MetaData _ReadMetaDataBlock(Stream stream) {
			var isLastAndMetaDataType = stream.ReadAsByte();

			// これが最後のメタデータブロックかどうかを示す値
			var isLast = (isLastAndMetaDataType & 0x80U) != 0;

			var metaDataType = (isLastAndMetaDataType & 0x7FU);

			var metaDataLength = stream.ReadAsUInt(3);
			if (metaDataLength == 0)
				throw new InvalidDataException("metaDataLength が不正です");

			// Pictureタイプ以外はストリームから読み取らずにスキップする
			List<byte> metaData;
			if (metaDataType == 6) {
				metaData = stream.ReadAsByteList((int)metaDataLength);
			}
			else {
				metaData = null;
				stream.Skip((int)metaDataLength);
			}

			return new MetaData((MetaDataType)metaDataType, isLast, metaData);
		}

		/// <summary>
		/// PICTUREタイプのメタデータから Image を取り出します
		/// </summary>
		private Image _ParsePictureMetaData(MetaData pictureMetaData) {
			if (pictureMetaData.Type != MetaDataType.PICTURE)
				throw new ArgumentException("このメタデータはPICTUREタイプではありません");

			List<byte> imageSource;
			using (var memory = new MemoryStream(pictureMetaData.Data.ToArray())) {
				memory.Skip(4);
				var mimeTypeLength = memory.ReadAsUInt();
				if (mimeTypeLength > 128)
					throw new InvalidDataException("mimeTypeLength が不正な値です");

				memory.Skip((int)mimeTypeLength);
				var explanationLength = memory.ReadAsUInt();

				memory.Skip((int)explanationLength + 4 * 4);
				var imageSourceSize = memory.ReadAsUInt();
				imageSource = memory.ReadAsByteList((int)imageSourceSize);
			}

			using (var memory = new MemoryStream(imageSource.ToArray()))
				using (var image = Image.FromStream(memory))
					return new Bitmap(image);
		}

		/// <summary>
		/// 対象のファイルが形式と一致しているかを判別します
		/// </summary>
		public bool CheckType(string filePath) {
			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
				var formatId = file.ReadAsAsciiString(4);

				if (formatId != "fLaC")
					return false;
			}

			try
			{
				if (Extract(filePath) != null)
					return true;
			}
			catch
			{
				// noop
			}

			return false;
		}

		/// <summary>
		/// アルバムアートを抽出します
		/// </summary>
		/// <exception cref="FileNotFoundException" />
		/// <exception cref="InvalidDataException" />
		public Image Extract(string filePath) {
			if (!File.Exists(filePath))
				throw new FileNotFoundException("指定されたファイルは存在しません");

			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
				file.Skip(4);

				var metaDataList = new List<MetaData>();
				MetaData metaData = null;
				do {
					metaDataList.Add(metaData = _ReadMetaDataBlock(file));
				}
				while (!metaData.IsLast && metaDataList.Count < 64);

				if (metaDataList.Count >= 64)
					throw new InvalidDataException("メタデータの個数が異常です");

				var picture = metaDataList.Find(i => i.Type == MetaDataType.PICTURE);

				return (picture != null) ? _ParsePictureMetaData(picture) : null;
			}
		}

		public class MetaData {
			public MetaData(MetaDataType type, bool isLast, List<byte> data) {
				Type = type;
				IsLast = isLast;
				Data = data;
			}

			public MetaDataType Type { get; set; }

			public bool IsLast { get; set; }

			public List<byte> Data { get; set; }

			public override string ToString() => $"FlacMetaData {{ Type = {Type}, IsLast = {IsLast}, DataSize = {Data.Count} }}";
		}

		public enum MetaDataType {
			STREAMINFO = 0,
			PADDING = 1,
			APPLICATION = 2,
			SEEKTABLE = 3,
			VORBIS_COMMENT = 4,
			CUESHEET = 5,
			PICTURE = 6
		}
	}
}
