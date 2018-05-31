using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AlbumArtExtraction
{
	public static class StreamExtensions
	{
		/// <summary>
		/// 指定したバイト数のデータを読み飛ばします
		/// </summary>
		/// <param name="skip">読み飛ばすバイト数</param>
		public static void Skip(this Stream stream, int skip)
		{
			if (skip > 0)
				stream.Seek(skip, SeekOrigin.Current);
		}

		/// <summary>
		/// 指定した長さのデータを List&lt;byte&gt; として読み取ります
		/// </summary>
		/// <param name="count">読み取るデータの長さ(バイト数)</param>
		public static List<byte> ReadAsByteList(this Stream stream, int count)
		{
			var buf = new byte[count];
			stream.Read(buf, 0, count);

			return new List<byte>(buf);
		}

		/// <summary>
		/// byte(1 Bytes) として読み取ります
		/// </summary>
		public static byte ReadAsByte(this Stream stream) =>
			stream.ReadAsByteList(1)[0];

		private static byte[] _ReadAsUIntXBase(this Stream stream, int returnSize, int count)
		{
			if (count < 1 || count > returnSize)
				throw new ArgumentOutOfRangeException("count");

			var buf = stream.ReadAsByteList(count);

			// 配列にゼロパディングを追加
			foreach (var i in Enumerable.Range(0, returnSize - count))
				buf.Insert(0, 0);

			buf.Reverse();

			return buf.ToArray();
		}

		/// <summary>
		/// 指定した長さのデータを ushort(2 Bytes) として読み取ります(ビッグエンディアン)
		/// </summary>
		/// <param name="count">読み取るデータの長さ(バイト数) 範囲: 1-2</param>
		/// <exception cref="ArgumentOutOfRangeException" />
		public static ushort ReadAsUShort(this Stream stream, int count = 2) =>
			BitConverter.ToUInt16(stream._ReadAsUIntXBase(2, count), 0);

		/// <summary>
		/// 指定した長さのデータを uint(4 Bytes) として読み取ります(ビッグエンディアン)
		/// </summary>
		/// <param name="count">読み取るデータの長さ(バイト数) 範囲: 1-4</param>
		/// <exception cref="ArgumentOutOfRangeException" />
		public static uint ReadAsUInt(this Stream stream, int count = 4) =>
			BitConverter.ToUInt32(stream._ReadAsUIntXBase(4, count), 0);

		/// <summary>
		/// ASCII文字列として指定されたカウント数だけ読み取ります
		/// </summary>
		public static string ReadAsAsciiString(this Stream stream, int count)
		{
			var bytes = stream.ReadAsByteList(count).ToArray();

			return new string(Encoding.ASCII.GetChars(bytes));
		}
	}
}
