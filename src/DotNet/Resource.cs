// dnlib: See LICENSE.txt for more info

using System;
using dnlib.IO;
using dnlib.DotNet.MD;

namespace dnlib.DotNet {
	/// <summary>
	/// Type of resource
	/// </summary>
	public enum ResourceType {
		/// <summary>
		/// It's a <see cref="EmbeddedResource"/>
		/// </summary>
		Embedded,

		/// <summary>
		/// It's a <see cref="AssemblyLinkedResource"/>
		/// </summary>
		AssemblyLinked,

		/// <summary>
		/// It's a <see cref="LinkedResource"/>
		/// </summary>
		Linked,
	}

	/// <summary>
	/// Resource base class
	/// </summary>
	public abstract class Resource : IMDTokenProvider {
		uint rid;
		uint? offset;
		UTF8String name;
		ManifestResourceAttributes flags;

		/// <inheritdoc/>
		public MDToken MDToken { get { return new MDToken(Table.ManifestResource, rid); } }

		/// <inheritdoc/>
		public uint Rid {
			get { return rid; }
			set { rid = value; }
		}

		/// <summary>
		/// Gets/sets the offset of the resource
		/// </summary>
		public uint? Offset {
			get { return offset; }
			set { offset = value; }
		}

		/// <summary>
		/// Gets/sets the name
		/// </summary>
		public UTF8String Name {
			get { return name; }
			set { name = value; }
		}

		/// <summary>
		/// Gets/sets the flags
		/// </summary>
		public ManifestResourceAttributes Attributes {
			get { return flags; }
			set { flags = value; }
		}

		/// <summary>
		/// Gets the type of resource
		/// </summary>
		public abstract ResourceType ResourceType { get; }

		/// <summary>
		/// Gets/sets the visibility
		/// </summary>
		public ManifestResourceAttributes Visibility {
			get { return flags & ManifestResourceAttributes.VisibilityMask; }
			set { flags = (flags & ~ManifestResourceAttributes.VisibilityMask) | (value & ManifestResourceAttributes.VisibilityMask); }
		}

		/// <summary>
		/// <c>true</c> if <see cref="ManifestResourceAttributes.Public"/> is set
		/// </summary>
		public bool IsPublic { get { return (flags & ManifestResourceAttributes.VisibilityMask) == ManifestResourceAttributes.Public; } }

		/// <summary>
		/// <c>true</c> if <see cref="ManifestResourceAttributes.Private"/> is set
		/// </summary>
		public bool IsPrivate { get { return (flags & ManifestResourceAttributes.VisibilityMask) == ManifestResourceAttributes.Private; } }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="flags">flags</param>
		protected Resource(UTF8String name, ManifestResourceAttributes flags) {
			this.name = name;
			this.flags = flags;
		}
	}

	/// <summary>
	/// A resource that is embedded in a .NET module. This is the most common type of resource.
	/// </summary>
	public sealed class EmbeddedResource : Resource {
		readonly DataReaderFactory dataReaderFactory;
		readonly uint resourceStartOffset;
		readonly uint resourceLength;

		/// <summary>
		/// Gets the length of the data
		/// </summary>
		public uint Length { get { return resourceLength; } }

		/// <inheritdoc/>
		public override ResourceType ResourceType { get { return ResourceType.Embedded; } }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name of resource</param>
		/// <param name="data">Resource data</param>
		/// <param name="flags">Resource flags</param>
		public EmbeddedResource(UTF8String name, byte[] data, ManifestResourceAttributes flags)
			: this(name, ByteArrayDataReaderFactory.Create(data, /* filename: */ null), 0, (uint)data.Length, flags) {
		}

        /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name of resource</param>
		/// <param name="data">Resource data</param>
		public EmbeddedResource(UTF8String name, byte[] data)
			: this(name, ByteArrayDataReaderFactory.Create(data, null), 0, (uint)data.Length, ManifestResourceAttributes.Private) {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name of resource</param>
		/// <param name="dataReaderFactory">Data reader factory</param>
		/// <param name="offset">Offset of resource data</param>
		/// <param name="length">Length of resource data</param>
		/// <param name="flags">Resource flags</param>
		public EmbeddedResource(UTF8String name, DataReaderFactory dataReaderFactory, uint offset, uint length, ManifestResourceAttributes flags)
			: base(name, flags) {
            if (dataReaderFactory != null) this.dataReaderFactory = dataReaderFactory; else throw new ArgumentNullException("dataReaderFactory");
			resourceStartOffset = offset;
			resourceLength = length;
		}

        /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name of resource</param>
		/// <param name="dataReaderFactory">Data reader factory</param>
		/// <param name="offset">Offset of resource data</param>
		/// <param name="length">Length of resource data</param>
		public EmbeddedResource(UTF8String name, DataReaderFactory dataReaderFactory, uint offset, uint length)
			: base(name, ManifestResourceAttributes.Private) {
            if (dataReaderFactory != null) this.dataReaderFactory = dataReaderFactory; else throw new ArgumentNullException("dataReaderFactory");
			resourceStartOffset = offset;
			resourceLength = length;
		}

		/// <summary>
		/// Gets a data reader that can access the resource
		/// </summary>
		/// <returns></returns>
		public DataReader CreateReader() { return dataReaderFactory.CreateReader(resourceStartOffset, resourceLength); }

		/// <inheritdoc/>
		public override string ToString() { return string.Format( "{0} - size: {1}", UTF8String.ToSystemStringOrEmpty(Name), (resourceLength) ); }
	}

	/// <summary>
	/// A reference to a resource in another assembly
	/// </summary>
	public sealed class AssemblyLinkedResource : Resource {
		AssemblyRef asmRef;

		/// <inheritdoc/>
		public override ResourceType ResourceType { get { return ResourceType.AssemblyLinked; } }

		/// <summary>
		/// Gets/sets the assembly reference
		/// </summary>
		public AssemblyRef Assembly {
			get { return asmRef; }
			set { if (value != null) asmRef = value; else throw new ArgumentNullException("value"); }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name of resource</param>
		/// <param name="asmRef">Assembly reference</param>
		/// <param name="flags">Resource flags</param>
		public AssemblyLinkedResource(UTF8String name, AssemblyRef asmRef, ManifestResourceAttributes flags)
			: base(name, flags) { if (asmRef != null) this.asmRef = asmRef; else throw new ArgumentNullException("asmRef"); }

		/// <inheritdoc/>
		public override string ToString() { return string.Format( "{0} - assembly: {1}", UTF8String.ToSystemStringOrEmpty(Name), asmRef.FullName ); }
	}

	/// <summary>
	/// A resource that is stored in a file on disk
	/// </summary>
	public sealed class LinkedResource : Resource {
		FileDef file;

		/// <inheritdoc/>
		public override ResourceType ResourceType { get { return ResourceType.Linked; } }

		/// <summary>
		/// Gets/sets the file
		/// </summary>
		public FileDef File {
			get { return file; }
			set { if (value != null) file = value; else throw new ArgumentNullException("value"); }
		}

		/// <summary>
		/// Gets/sets the hash
		/// </summary>
		public byte[] Hash {
			get { return file.HashValue; }
			set { file.HashValue = value; }
		}

		/// <summary>
		/// Gets/sets the file name
		/// </summary>
		public UTF8String FileName { get { return file == null ? UTF8String.Empty : file.Name; } }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name of resource</param>
		/// <param name="file">The file</param>
		/// <param name="flags">Resource flags</param>
		public LinkedResource(UTF8String name, FileDef file, ManifestResourceAttributes flags)
			: base(name, flags) { this.file = file; }

		/// <inheritdoc/>
		public override string ToString() { return string.Format( "{0} - file: {1}", UTF8String.ToSystemStringOrEmpty(Name), UTF8String.ToSystemStringOrEmpty(FileName) ); }
	}
}
