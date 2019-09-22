// dnlib: See LICENSE.txt for more info

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.DotNet.Pdb.WindowsPdb;

namespace dnlib.DotNet.Pdb.Dss {
	sealed class SymbolReaderImpl : SymbolReader {
		ModuleDef module;
		ISymUnmanagedReader reader;
		object[] objsToKeepAlive;

		const int E_FAIL = unchecked((int)0x80004005);

		public SymbolReaderImpl(ISymUnmanagedReader reader, object[] objsToKeepAlive) {
			if (reader != null) this.reader = reader; else throw new ArgumentNullException("reader");
			if (objsToKeepAlive != null) this.objsToKeepAlive = objsToKeepAlive; else throw new ArgumentNullException("objsToKeepAlive");
		}

		~SymbolReaderImpl() { Dispose(false); }

		public override PdbFileKind PdbFileKind { get { return PdbFileKind.WindowsPDB; } }

		public override int UserEntryPoint {
			get {
                uint token;
				int hr = reader.GetUserEntryPoint(out token);
				if (hr == E_FAIL)
					token = 0;
				else
					Marshal.ThrowExceptionForHR(hr);
				return (int)token;
			}
		}

		public override IList<SymbolDocument> Documents {
			get {
				if (documents is null) {
                    uint numDocs;
					reader.GetDocuments(0, out numDocs, null);
					var unDocs = new ISymUnmanagedDocument[numDocs];
					reader.GetDocuments((uint)unDocs.Length, out numDocs, unDocs);
					var docs = new SymbolDocument[numDocs];
					for (uint i = 0; i < numDocs; i++)
						docs[i] = new SymbolDocumentImpl(unDocs[i]);
					documents = docs;
				}
				return documents;
			}
		}
		volatile SymbolDocument[] documents;

		public override void Initialize(ModuleDef module) { this.module = module; }

		public override SymbolMethod GetMethod(MethodDef method, int version) {
            ISymUnmanagedMethod unMethod;
			int hr = reader.GetMethodByVersion(method.MDToken.Raw, version, out unMethod);
			if (hr == E_FAIL)
				return null;
			Marshal.ThrowExceptionForHR(hr);
			return unMethod is null ? null : new SymbolMethodImpl(this, unMethod);
		}

		internal void GetCustomDebugInfos(SymbolMethodImpl symMethod, MethodDef method, CilBody body, IList<PdbCustomDebugInfo> result) {
			var asyncMethod = PseudoCustomDebugInfoFactory.TryCreateAsyncMethod(method.Module, method, body, symMethod.AsyncKickoffMethod, symMethod.AsyncStepInfos, symMethod.AsyncCatchHandlerILOffset);
			if (!(asyncMethod is null))
				result.Add(asyncMethod);

			const string CDI_NAME = "MD2";
            uint bufSize;
			reader.GetSymAttribute(method.MDToken.Raw, CDI_NAME, 0, out bufSize, null);
			if (bufSize == 0)
				return;
			var cdiData = new byte[bufSize];
			reader.GetSymAttribute(method.MDToken.Raw, CDI_NAME, (uint)cdiData.Length, out bufSize, cdiData);
			PdbCustomDebugInfoReader.Read(method, body, result, cdiData);
		}

		public override void GetCustomDebugInfos(int token, GenericParamContext gpContext, IList<PdbCustomDebugInfo> result) {
			if (token == 0x00000001)
				GetCustomDebugInfos_ModuleDef(result);
		}

		void GetCustomDebugInfos_ModuleDef(IList<PdbCustomDebugInfo> result) {
			var sourceLinkData = GetSourceLinkData();
			if (!(sourceLinkData is null))
				result.Add(new PdbSourceLinkCustomDebugInfo(sourceLinkData));
			var sourceServerData = GetSourceServerData();
			if (!(sourceServerData is null))
				result.Add(new PdbSourceServerCustomDebugInfo(sourceServerData));
		}

		byte[] GetSourceLinkData() {
            ISymUnmanagedReader4 reader4;
			if ((reader4 = reader as ISymUnmanagedReader4) != null) {
				// It returns data that it owns. The data is freed once its Destroy() method is called
				Debug.Assert(reader is ISymUnmanagedDispose);
				// Despite its name, it seems to only return source link data, and not source server data
                int sizeData;
                IntPtr srcLinkData;
				if (reader4.GetSourceServerData(out srcLinkData, out sizeData) == 0) {
					if (sizeData == 0)
						return Array2.Empty<byte>();
					var data = new byte[sizeData];
					Marshal.Copy(srcLinkData, data, 0, data.Length);
					return data;
				}
			}
			return null;
		}

		byte[] GetSourceServerData() {
            ISymUnmanagedSourceServerModule srcSrvModule;
			if ((srcSrvModule = reader as ISymUnmanagedSourceServerModule) != null) {
				var srcSrvData = IntPtr.Zero;
				try {
					// This method only returns source server data, not source link data
                    int sizeData;
					if (srcSrvModule.GetSourceServerData(out sizeData, out srcSrvData) == 0) {
						if (sizeData == 0)
							return Array2.Empty<byte>();
						var data = new byte[sizeData];
						Marshal.Copy(srcSrvData, data, 0, data.Length);
						return data;
					}
				}
				finally {
					if (srcSrvData != IntPtr.Zero)
						Marshal.FreeCoTaskMem(srcSrvData);
				}
			}
			return null;
		}

		public override void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing) {
            ISymUnmanagedDispose symUnmanagedDispose;
			if ((symUnmanagedDispose = reader as ISymUnmanagedDispose) != null) symUnmanagedDispose.Destroy();
			var o = objsToKeepAlive;
			if (!(o is null)) {
				foreach (var obj in o)
                { IDisposable disposable; if ((disposable = obj as IDisposable) != null) disposable.Dispose(); }
			}
			module = null;
			reader = null;
			objsToKeepAlive = null;
		}

		public bool MatchesModule(Guid pdbId, uint stamp, uint age) {
            ISymUnmanagedReader4 reader4;
            if ((reader4 = reader as ISymUnmanagedReader4) != null) {
                bool result;
                int hr = reader4.MatchesModule(pdbId, stamp, age, out result);
				if (hr < 0)
					return false;
				return result;
			}

			// There seems to be no other method that can verify that we opened the correct PDB, so return true
			return true;
		}
	}
}
