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
			var isLastAndMetaDataType = Helper.ReadAsByte(stream);

			// これが最後のメタデータブロックかどうかを示す値
			var isLast = (isLastAndMetaDataType & 0x80U) != 0;

			var metaDataType = (isLastAndMetaDataType & 0x7FU);

			var metaDataLength = Helper.ReadAsUInt(stream, 3);
			if (metaDataLength == 0) {
				throw new InvalidDataException("metaDataLength が不正です");
			}

			// Pictureタイプ以外はストリームから読み取らずにスキップする
			List<byte> metaData;
			if (metaDataType == 6) {
				metaData = Helper.ReadAsByteList(stream, (int)metaDataLength);
			}
			else {
				metaData = null;
				Helper.Skip(stream, (int)metaDataLength);
			}

			return new MetaData((MetaDataType)metaDataType, isLast, metaData);
		}

		/// <summary>
		/// PICTUREタイプのメタデータから Image を取り出します
		/// </summary>
		private Image _ParsePictureMetaData(MetaData pictureMetaData) {
			if (pictureMetaData.Type != MetaDataType.PICTURE) {
				throw new ArgumentException("このメタデータはPICTUREタイプではありません");
			}

			List<byte> imageSource;
			using (var memory = new MemoryStream()) {
				memory.Write(pictureMetaData.Data.ToArray(), 0, pictureMetaData.Data.Count);
				memory.Seek(4, SeekOrigin.Begin);

				var mimeTypeLength = Helper.ReadAsUInt(memory);
				if (mimeTypeLength > 128) {
					throw new InvalidDataException("mimeTypeLength が不正な値です");
				}

				var explanationLength = Helper.ReadAsUInt(memory, skip: (int)mimeTypeLength);

				var imageSourceSize = Helper.ReadAsUInt(memory, skip: (int)explanationLength + 4 * 4);
				imageSource = Helper.ReadAsByteList(memory, (int)imageSourceSize);
			}

			using (var memory = new MemoryStream()) {
				memory.Write(imageSource.ToArray(), 0, imageSource.Count);

				using (var image = Image.FromStream(memory)) {
					return new Bitmap(image);
				}
			}
		}

		/// <summary>
		/// 対象のファイルが形式と一致しているかを判別します
		/// </summary>
		public bool CheckType(string filePath) {
			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
				var fileType = Helper.ReadAsAsciiString(file, 4);

				if (fileType != "fLaC")
					return false;
			}

			try {
				if (Extract(filePath) != null)
					return true;
			}
			catch { }

			return false;
		}

		/// <summary>
		/// アルバムアートを抽出します
		/// </summary>
		/// <exception cref="FileNotFoundException" />
		/// <exception cref="InvalidDataException" />
		public Image Extract(string filePath) {
			if (!File.Exists(filePath)) {
				throw new FileNotFoundException("指定されたファイルは存在しません");
			}

			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
				Helper.Skip(file, 4);

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
