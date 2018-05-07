// dnlib: See LICENSE.txt for more info

using System;
using System.Diagnostics;
using System.Text;
using dnlib.IO;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD {
	/// <summary>
	/// A metadata stream header
	/// </summary>
	[DebuggerDisplay("O:{offset} L:{streamSize} {name}")]
	public sealed class StreamHeader : FileSection {
		readonly uint offset;
		readonly uint streamSize;
		readonly string name;

		/// <summary>
		/// The offset of the stream relative to the start of the metadata header
		/// </summary>
		public uint Offset { get { return offset; } }

		/// <summary>
		/// The size of the stream
		/// </summary>
		public uint StreamSize { get { return streamSize; } }

		/// <summary>
		/// The name of the stream
		/// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reader">PE file reader pointing to the start of this section</param>
        /// <param name="verify">Verify section</param>
        /// <param name="failedVerification">the status of Verification failed</param>
        /// <exception cref="BadImageFormatException">Thrown if verification fails</exception>
        public StreamHeader(ref DataReader reader, bool verify, [Optional] out bool failedVerification)
			: this(ref reader, verify, verify, out failedVerification) {
		}

		internal StreamHeader(ref DataReader reader, bool throwOnError, bool verify, out bool failedVerification) {
			failedVerification = false;
			SetStartOffset(ref reader);
			offset = reader.ReadUInt32();
			streamSize = reader.ReadUInt32();
			name = ReadString(ref reader, 32, verify, ref failedVerification);
			SetEndoffset(ref reader);
			if (verify && offset + size < offset)
				failedVerification = true;
			if (throwOnError && failedVerification)
				throw new BadImageFormatException("Invalid stream header");
		}

		internal StreamHeader(uint offset, uint streamSize, string name) {
			this.offset = offset;
			this.streamSize = streamSize;
            if (name != null) this.name = name; else throw new ArgumentNullException("name");
		}

		static string ReadString(ref DataReader reader, int maxLen, bool verify, ref bool failedVerification) {
			var origPos = reader.Position;
			var sb = new StringBuilder(maxLen);
			int i;
			for (i = 0; i < maxLen; i++) {
				byte b = reader.ReadByte();
				if (b == 0)
					break;
				sb.Append((char)b);
			}
			if (verify && i == maxLen)
				failedVerification = true;
			if (i != maxLen)
				reader.Position = origPos + (((uint)i + 1 + 3) & ~3U);
			return sb.ToString();
		}
	}
}
