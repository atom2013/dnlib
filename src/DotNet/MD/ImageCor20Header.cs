// dnlib: See LICENSE.txt for more info

using System;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.MD {
	/// <summary>
	/// Represents the IMAGE_COR20_HEADER structure
	/// </summary>
	public sealed class ImageCor20Header : FileSection {
		readonly uint cb;
		readonly ushort majorRuntimeVersion;
		readonly ushort minorRuntimeVersion;
		readonly ImageDataDirectory metadata;
		readonly ComImageFlags flags;
		readonly uint entryPointToken_or_RVA;
		readonly ImageDataDirectory resources;
		readonly ImageDataDirectory strongNameSignature;
		readonly ImageDataDirectory codeManagerTable;
		readonly ImageDataDirectory vtableFixups;
		readonly ImageDataDirectory exportAddressTableJumps;
		readonly ImageDataDirectory managedNativeHeader;

		/// <summary>
		/// Returns <c>true</c> if it has a native header
		/// </summary>
		public bool HasNativeHeader { get { return (flags & ComImageFlags.ILLibrary) != 0; } }

		/// <summary>
		/// Returns the IMAGE_COR20_HEADER.cb field
		/// </summary>
		public uint CB { get { return cb; } }

		/// <summary>
		/// Returns the IMAGE_COR20_HEADER.MajorRuntimeVersion field
		/// </summary>
		public ushort MajorRuntimeVersion { get { return majorRuntimeVersion; } }

		/// <summary>
		/// Returns the IMAGE_COR20_HEADER.MinorRuntimeVersion field
		/// </summary>
		public ushort MinorRuntimeVersion { get { return minorRuntimeVersion; } }

		/// <summary>
		/// Returns the IMAGE_COR20_HEADER.Metadata field
		/// </summary>
		public ImageDataDirectory Metadata { get { return metadata; } }

		/// <summary>
		/// Returns the IMAGE_COR20_HEADER.Flags field
		/// </summary>
		public ComImageFlags Flags { get { return flags; } }

		/// <summary>
		/// Returns the IMAGE_COR20_HEADER.EntryPointToken/EntryPointTokenRVA field
		/// </summary>
		public uint EntryPointToken_or_RVA { get { return entryPointToken_or_RVA; } }

		/// <summary>
		/// Returns the IMAGE_COR20_HEADER.Resources field
		/// </summary>
		public ImageDataDirectory Resources { get { return resources; } }

		/// <summary>
		/// Returns the IMAGE_COR20_HEADER.StrongNameSignature field
		/// </summary>
		public ImageDataDirectory StrongNameSignature { get { return strongNameSignature; } }

		/// <summary>
		/// Returns the IMAGE_COR20_HEADER.CodeManagerTable field
		/// </summary>
		public ImageDataDirectory CodeManagerTable { get { return codeManagerTable; } }

		/// <summary>
		/// Returns the IMAGE_COR20_HEADER.VTableFixups field
		/// </summary>
		public ImageDataDirectory VTableFixups { get { return vtableFixups; } }

		/// <summary>
		/// Returns the IMAGE_COR20_HEADER.ExportAddressTableJumps field
		/// </summary>
		public ImageDataDirectory ExportAddressTableJumps { get { return exportAddressTableJumps; } }

		/// <summary>
		/// Returns the IMAGE_COR20_HEADER.ManagedNativeHeader field
		/// </summary>
        public ImageDataDirectory ManagedNativeHeader { get { return managedNativeHeader; } }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="reader">PE file reader pointing to the start of this section</param>
		/// <param name="verify">Verify section</param>
		/// <exception cref="BadImageFormatException">Thrown if verification fails</exception>
		public ImageCor20Header(ref DataReader reader, bool verify) {
			SetStartOffset(ref reader);
			cb = reader.ReadUInt32();
			if (verify && cb < 0x48)
				throw new BadImageFormatException("Invalid IMAGE_COR20_HEADER.cb value");
			majorRuntimeVersion = reader.ReadUInt16();
			minorRuntimeVersion = reader.ReadUInt16();
			metadata = new ImageDataDirectory(ref reader, verify);
			flags = (ComImageFlags)reader.ReadUInt32();
			entryPointToken_or_RVA = reader.ReadUInt32();
			resources = new ImageDataDirectory(ref reader, verify);
			strongNameSignature = new ImageDataDirectory(ref reader, verify);
			codeManagerTable = new ImageDataDirectory(ref reader, verify);
			vtableFixups = new ImageDataDirectory(ref reader, verify);
			exportAddressTableJumps = new ImageDataDirectory(ref reader, verify);
			managedNativeHeader = new ImageDataDirectory(ref reader, verify);
			SetEndoffset(ref reader);
		}
	}
}
