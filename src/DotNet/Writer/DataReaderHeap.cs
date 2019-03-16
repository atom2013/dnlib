// dnlib: See LICENSE.txt for more info

using System;
using dnlib.DotNet.MD;
using dnlib.IO;
using System.Runtime.CompilerServices;

namespace dnlib.DotNet.Writer {
	/// <summary>
	/// Copies existing data to a new metadata heap
	/// </summary>
	public sealed class DataReaderHeap : HeapBase {
        [CompilerGenerated]
        private readonly string Name__BackingField;
        [CompilerGenerated]
        private readonly DotNetStream OptionalOriginalStream__BackingField;
		/// <summary>
		/// Gets the name of the heap
		/// </summary>
		public override string Name { get { return Name__BackingField; } }

		internal DotNetStream OptionalOriginalStream { get { return OptionalOriginalStream__BackingField; } }

		readonly DataReader heapReader;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="stream">The stream whose data will be copied to the new metadata file</param>
		public DataReaderHeap(DotNetStream stream) {
            if (stream != null) OptionalOriginalStream__BackingField = stream; else throw new ArgumentNullException("stream");
			heapReader = stream.CreateReader();
            Name__BackingField = stream.Name;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Heap name</param>
		/// <param name="heapReader">Heap content</param>
		public DataReaderHeap(string name, DataReader heapReader) {
			this.heapReader = heapReader;
			this.heapReader.Position = 0;
            if (name != null) Name__BackingField = name; else throw new ArgumentNullException("name");
		}

		/// <inheritdoc/>
		public override uint GetRawLength() { return heapReader.Length; }

		/// <inheritdoc/>
        protected override void WriteToImpl(DataWriter writer) { heapReader.CopyTo(writer); }
	}
}
