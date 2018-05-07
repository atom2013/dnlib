// dnlib: See LICENSE.txt for more info

using System.IO;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Managed {
	/// <summary>
	/// Creates a <see cref="SymbolReader"/> instance
	/// </summary>
	static class SymbolReaderFactory {
		/// <summary>
		/// Creates a new <see cref="SymbolReader"/> instance
		/// </summary>
		/// <param name="pdbContext">PDB context</param>
		/// <param name="pdbStream">PDB file stream which is now owned by this method</param>
		/// <returns>A new <see cref="SymbolReader"/> instance or <c>null</c>.</returns>
		public static SymbolReader Create(PdbReaderContext pdbContext, DataReaderFactory pdbStream) {
			if (pdbStream == null)
				return null;
			try {
				var debugDir = pdbContext.CodeViewDebugDirectory;
				if (debugDir == null)
					return null;
                System.Guid pdbGuid;
                uint age;
				if (!pdbContext.TryGetCodeViewData(out pdbGuid, out age))
					return null;

				var pdbReader = new PdbReader(pdbGuid, age);
				pdbReader.Read(pdbStream.CreateReader());
				if (pdbReader.MatchesModule)
					return pdbReader;
				return null;
			}
			catch (PdbException) {
			}
			catch (IOException) {
			}
			finally {
				if (pdbStream != null) pdbStream.Dispose();
			}
			return null;
		}
	}
}
