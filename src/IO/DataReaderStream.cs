// dnlib: See LICENSE.txt for more info

using System;
using System.IO;

namespace dnlib.IO {
	sealed class DataReaderStream : Stream {
		public override bool CanRead { get { return  true; } }
		public override bool CanSeek { get { return true; } }
		public override bool CanWrite { get { return  false; } }
		public override long Length { get { return reader.Length; } }

		public override long Position {
			get { return position; }
			set { position = value; }
		}

		DataReader reader;
		long position;

		public DataReaderStream(ref DataReader reader) {
			this.reader = reader;
			position = reader.Position;
		}

		public override void Flush() { }

		bool CheckAndSetPosition() {
			if ((ulong)position > reader.Length)
				return false;
			reader.Position = (uint)position;
			return true;
		}

		public override long Seek(long offset, SeekOrigin origin) {
			switch (origin) {
			case SeekOrigin.Begin:		Position = offset; break;
			case SeekOrigin.Current:	Position += offset; break;
			case SeekOrigin.End:		Position = Length + offset; break;
			}
			return Position;
		}

		public override int Read(byte[] buffer, int offset, int count) {
			if (buffer is null)
				throw new ArgumentNullException("buffer");
			if (offset < 0)
				throw new ArgumentOutOfRangeException("offset");
			if (count < 0)
				throw new ArgumentOutOfRangeException("count");
			if (!CheckAndSetPosition())
				return 0;
			int bytesToRead = (int)Math.Min((uint)count, reader.BytesLeft);
			reader.ReadBytes(buffer, offset, bytesToRead);
			Position += bytesToRead;
			return bytesToRead;
		}

		public override int ReadByte() {
			if (!CheckAndSetPosition() || !reader.CanRead(1U))
				return -1;
			Position++;
			return reader.ReadByte();
		}

		public override void SetLength(long value) { throw new NotSupportedException(); }
        public override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
	}
}
