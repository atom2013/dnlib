// dnlib: See LICENSE.txt for more info

namespace dnlib.DotNet.Pdb {
	static class PdbUtils {
        public static bool IsEndInclusive(PdbFileKind pdbFileKind, Compiler compiler) {
            return pdbFileKind == PdbFileKind.WindowsPDB && compiler == Compiler.VisualBasic;
        }
	}
}
