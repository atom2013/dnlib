// dnlib: See LICENSE.txt for more info

using System.Collections.Generic;
using System.Diagnostics;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Pdb {
	/// <summary>
	/// A PDB scope
	/// </summary>
	[DebuggerDisplay("{Start} - {End}")]
	public sealed class PdbScope : IHasCustomDebugInformation {
		readonly IList<PdbScope> scopes = new List<PdbScope>();
		readonly IList<PdbLocal> locals = new List<PdbLocal>();
		readonly IList<string> namespaces = new List<string>();
		readonly IList<PdbConstant> constants = new List<PdbConstant>();

		/// <summary>
		/// Constructor
		/// </summary>
		public PdbScope() {
		}

		/// <summary>
		/// Gets/sets the first instruction
		/// </summary>
		public Instruction Start { get; set; }

		/// <summary>
		/// Gets/sets the last instruction. It's <c>null</c> if it ends at the end of the method.
		/// </summary>
		public Instruction End { get; set; }

		/// <summary>
		/// Gets all child scopes
		/// </summary>
		public IList<PdbScope> Scopes { get { return scopes; } }

		/// <summary>
		/// <c>true</c> if <see cref="Scopes"/> is not empty
		/// </summary>
		public bool HasScopes { get { return scopes.Count > 0; } }

		/// <summary>
		/// Gets all locals in this scope
		/// </summary>
		public IList<PdbLocal> Variables { get { return locals; } }

		/// <summary>
		/// <c>true</c> if <see cref="Variables"/> is not empty
		/// </summary>
		public bool HasVariables { get { return locals.Count > 0; } }

		/// <summary>
		/// Gets all namespaces (Windows PDBs). Portable PDBs use <see cref="ImportScope"/>
		/// </summary>
		public IList<string> Namespaces { get { return namespaces; } }

		/// <summary>
		/// <c>true</c> if <see cref="Namespaces"/> is not empty
		/// </summary>
		public bool HasNamespaces { get { return namespaces.Count > 0; } }

		/// <summary>
		/// Gets/sets the import scope (Portable PDBs). Windows PDBs use <see cref="Namespaces"/>
		/// </summary>
		public PdbImportScope ImportScope { get; set; }

		/// <summary>
		/// Gets all constants
		/// </summary>
		public IList<PdbConstant> Constants { get { return constants; } }

		/// <summary>
		/// <c>true</c> if <see cref="Constants"/> is not empty
		/// </summary>
		public bool HasConstants { get { return constants.Count > 0; } }

		/// <inheritdoc/>
		public int HasCustomDebugInformationTag { get { return 23; } }

		/// <inheritdoc/>
		public bool HasCustomDebugInfos { get { return CustomDebugInfos.Count > 0; } }

		/// <summary>
		/// Gets all custom debug infos
		/// </summary>
        public IList<PdbCustomDebugInfo> CustomDebugInfos { get { return customDebugInfos; } }
		readonly IList<PdbCustomDebugInfo> customDebugInfos = new List<PdbCustomDebugInfo>();
	}
}
