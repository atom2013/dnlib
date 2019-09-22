// dnlib: See LICENSE.txt for more info

using System;
using System.Collections.Generic;
using System.Diagnostics;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Portable {
	sealed class SymbolScopeImpl : SymbolScope {
		readonly PortablePdbReader owner;
		internal SymbolMethod method;
		readonly SymbolScopeImpl parent;
		readonly int startOffset;
		readonly int endOffset;
		internal readonly List<SymbolScope> childrenList;
		internal readonly List<SymbolVariable> localsList;
		internal PdbImportScope importScope;
		readonly PdbCustomDebugInfo[] customDebugInfos;

		public override SymbolMethod Method {
			get {
				if (!(method is null))
					return method;
				var p = parent;
				if (p is null)
					return method;
				for (;;) {
					if (p.parent is null)
						return method = p.method;
					p = p.parent;
				}
			}
		}

		public override SymbolScope Parent { get { return parent; } }
		public override int StartOffset { get { return startOffset; } }
		public override int EndOffset { get { return endOffset; } }
		public override IList<SymbolScope> Children { get { return childrenList; } }
		public override IList<SymbolVariable> Locals { get { return localsList; } }
		public override IList<SymbolNamespace> Namespaces { get { return Array2.Empty<SymbolNamespace>(); } }
		public override IList<PdbCustomDebugInfo> CustomDebugInfos { get { return customDebugInfos; } }
		public override PdbImportScope ImportScope { get { return importScope; } }

		public SymbolScopeImpl(PortablePdbReader owner, SymbolScopeImpl parent, int startOffset, int endOffset, PdbCustomDebugInfo[] customDebugInfos) {
			this.owner = owner;
			method = null;
			this.parent = parent;
			this.startOffset = startOffset;
			this.endOffset = endOffset;
			childrenList = new List<SymbolScope>();
			localsList = new List<SymbolVariable>();
			this.customDebugInfos = customDebugInfos;
		}

		Metadata constantsMetadata;
		uint constantList;
		uint constantListEnd;

		internal void SetConstants(Metadata metadata, uint constantList, uint constantListEnd) {
			constantsMetadata = metadata;
			this.constantList = constantList;
			this.constantListEnd = constantListEnd;
		}

		public override IList<PdbConstant> GetConstants(ModuleDef module, GenericParamContext gpContext) {
			if (constantList >= constantListEnd)
				return Array2.Empty<PdbConstant>();
			Debug.Assert(!(constantsMetadata is null));

			var res = new PdbConstant[constantListEnd - constantList];
			int w = 0;
			for (int i = 0; i < res.Length; i++) {
				uint rid = constantList + (uint)i;
                RawLocalConstantRow row;
                bool b = constantsMetadata.TablesStream.TryReadLocalConstantRow(rid, out row);
				Debug.Assert(b);
				var name = constantsMetadata.StringsStream.Read(row.Name);
                DataReader reader;
                if (!constantsMetadata.BlobStream.TryCreateReader(row.Signature, out reader))
					continue;
				var localConstantSigBlobReader = new LocalConstantSigBlobReader(module, ref reader, gpContext);
                TypeSig type;
                object value;
				b = localConstantSigBlobReader.Read( out type, out value );
				Debug.Assert(b);
				if (b) {
					var pdbConstant = new PdbConstant(name, type, value);
					int token = new MDToken(Table.LocalConstant, rid).ToInt32();
					owner.GetCustomDebugInfos(token, gpContext, pdbConstant.CustomDebugInfos);
					res[w++] = pdbConstant;
				}
			}
			if (res.Length != w)
				Array.Resize(ref res, w);
			return res;
		}
	}
}
