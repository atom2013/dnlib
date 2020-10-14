// dnlib: See LICENSE.txt for more info

using System.Collections.Generic;

namespace dnlib.DotNet {
	/// <summary>
	/// Various helper methods for <see cref="IType"/> classes to prevent infinite recursion
	/// </summary>
	struct TypeHelper {
		RecursionCounter recursionCounter;

		internal static bool ContainsGenericParameter(StandAloneSig ss) { return ss != null && TypeHelper.ContainsGenericParameter(ss.Signature); }
		internal static bool ContainsGenericParameter(InterfaceImpl ii) { return ii != null && TypeHelper.ContainsGenericParameter(ii.Interface); }
		internal static bool ContainsGenericParameter(GenericParamConstraint gpc) { return gpc != null && ContainsGenericParameter(gpc.Constraint); }

		internal static bool ContainsGenericParameter(MethodSpec ms) {
			if (ms == null)
				return false;

			// A normal MethodSpec should always contain generic arguments and thus
			// its MethodDef is always a generic method with generic parameters.
			return true;
		}

		internal static bool ContainsGenericParameter(MemberRef mr) {
			if (mr == null)
				return false;

			if (ContainsGenericParameter(mr.Signature))
				return true;

			var cl = mr.Class;

            ITypeDefOrRef tdr;
            if ((tdr = cl as ITypeDefOrRef) != null)
				return tdr.ContainsGenericParameter;

            MethodDef md;
            if ((md = cl as MethodDef) != null)
				return TypeHelper.ContainsGenericParameter(md.Signature);

			return false;
		}

		/// <summary>
		/// Checks whether <paramref name="callConv"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.
		/// </summary>
		/// <param name="callConv">Calling convention signature</param>
		/// <returns><c>true</c> if <paramref name="callConv"/> contains a <see cref="GenericVar"/>
		/// or a <see cref="GenericMVar"/>.</returns>
		public static bool ContainsGenericParameter(CallingConventionSig callConv) {
            FieldSig fs;
            if ((fs = callConv as FieldSig) != null)
				return ContainsGenericParameter(fs);

            MethodBaseSig mbs;
            if ((mbs = callConv as MethodBaseSig) != null)
				return ContainsGenericParameter(mbs);

            LocalSig ls;
            if ((ls = callConv as LocalSig) != null)
				return ContainsGenericParameter(ls);

            GenericInstMethodSig gim;
            if ((gim = callConv as GenericInstMethodSig) != null)
				return ContainsGenericParameter(gim);

			return false;
		}

		/// <summary>
		/// Checks whether <paramref name="fieldSig"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.
		/// </summary>
		/// <param name="fieldSig">Field signature</param>
		/// <returns><c>true</c> if <paramref name="fieldSig"/> contains a <see cref="GenericVar"/>
		/// or a <see cref="GenericMVar"/>.</returns>
		public static bool ContainsGenericParameter(FieldSig fieldSig) { return new TypeHelper().ContainsGenericParameterInternal(fieldSig); }

		/// <summary>
		/// Checks whether <paramref name="methodSig"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.
		/// </summary>
		/// <param name="methodSig">Method or property signature</param>
		/// <returns><c>true</c> if <paramref name="methodSig"/> contains a <see cref="GenericVar"/>
		/// or a <see cref="GenericMVar"/>.</returns>
		public static bool ContainsGenericParameter(MethodBaseSig methodSig) { return new TypeHelper().ContainsGenericParameterInternal(methodSig); }

		/// <summary>
		/// Checks whether <paramref name="localSig"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.
		/// </summary>
		/// <param name="localSig">Local signature</param>
		/// <returns><c>true</c> if <paramref name="localSig"/> contains a <see cref="GenericVar"/>
		/// or a <see cref="GenericMVar"/>.</returns>
		public static bool ContainsGenericParameter(LocalSig localSig) { return new TypeHelper().ContainsGenericParameterInternal(localSig); }

		/// <summary>
		/// Checks whether <paramref name="gim"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.
		/// </summary>
		/// <param name="gim">Generic method signature</param>
		/// <returns><c>true</c> if <paramref name="gim"/> contains a <see cref="GenericVar"/>
		/// or a <see cref="GenericMVar"/>.</returns>
		public static bool ContainsGenericParameter(GenericInstMethodSig gim) { return new TypeHelper().ContainsGenericParameterInternal(gim); }

		/// <summary>
		/// Checks whether <paramref name="type"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.
		/// </summary>
		/// <param name="type">Type</param>
		/// <returns><c>true</c> if <paramref name="type"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.</returns>
		public static bool ContainsGenericParameter(IType type) {
            TypeDef td;
            if ((td = type as TypeDef) != null)
				return ContainsGenericParameter(td);

            TypeRef tr;
            if ((tr = type as TypeRef) != null)
				return ContainsGenericParameter(tr);

            TypeSpec ts;
            if ((ts = type as TypeSpec) != null)
				return ContainsGenericParameter(ts);

            TypeSig sig;
            if ((sig = type as TypeSig) != null)
				return ContainsGenericParameter(sig);

            ExportedType et;
            if ((et = type as ExportedType) != null)
				return ContainsGenericParameter(et);

			return false;
		}

		/// <summary>
		/// Checks whether <paramref name="type"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.
		/// </summary>
		/// <param name="type">Type</param>
		/// <returns><c>true</c> if <paramref name="type"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.</returns>
		public static bool ContainsGenericParameter(TypeDef type) { return new TypeHelper().ContainsGenericParameterInternal(type); }

		/// <summary>
		/// Checks whether <paramref name="type"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.
		/// </summary>
		/// <param name="type">Type</param>
		/// <returns><c>true</c> if <paramref name="type"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.</returns>
		public static bool ContainsGenericParameter(TypeRef type) { return new TypeHelper().ContainsGenericParameterInternal(type); }

		/// <summary>
		/// Checks whether <paramref name="type"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.
		/// </summary>
		/// <param name="type">Type</param>
		/// <returns><c>true</c> if <paramref name="type"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.</returns>
		public static bool ContainsGenericParameter(TypeSpec type) { return new TypeHelper().ContainsGenericParameterInternal(type); }

		/// <summary>
		/// Checks whether <paramref name="type"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.
		/// </summary>
		/// <param name="type">Type</param>
		/// <returns><c>true</c> if <paramref name="type"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.</returns>
		public static bool ContainsGenericParameter(TypeSig type) { return new TypeHelper().ContainsGenericParameterInternal(type); }

		/// <summary>
		/// Checks whether <paramref name="type"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.
		/// </summary>
		/// <param name="type">Type</param>
		/// <returns><c>true</c> if <paramref name="type"/> contains a <see cref="GenericVar"/> or a
		/// <see cref="GenericMVar"/>.</returns>
		public static bool ContainsGenericParameter(ExportedType type) { return new TypeHelper().ContainsGenericParameterInternal(type); }

		bool ContainsGenericParameterInternal(TypeDef type) { return false; }

		bool ContainsGenericParameterInternal(TypeRef type) { return false; }

		bool ContainsGenericParameterInternal(TypeSpec type) {
			if (type == null)
				return false;
			if (!recursionCounter.Increment())
				return false;

			bool res = ContainsGenericParameterInternal(type.TypeSig);

			recursionCounter.Decrement();
			return res;
		}

		bool ContainsGenericParameterInternal(ITypeDefOrRef tdr) {
			if (tdr == null)
				return false;
			// TypeDef and TypeRef contain no generic parameters
			return ContainsGenericParameterInternal(tdr as TypeSpec);
		}

		bool ContainsGenericParameterInternal(TypeSig type) {
			if (type == null)
				return false;
			if (!recursionCounter.Increment())
				return false;

			bool res;
			switch (type.ElementType) {
			case ElementType.Void:
			case ElementType.Boolean:
			case ElementType.Char:
			case ElementType.I1:
			case ElementType.U1:
			case ElementType.I2:
			case ElementType.U2:
			case ElementType.I4:
			case ElementType.U4:
			case ElementType.I8:
			case ElementType.U8:
			case ElementType.R4:
			case ElementType.R8:
			case ElementType.String:
			case ElementType.ValueType:
			case ElementType.Class:
			case ElementType.TypedByRef:
			case ElementType.I:
			case ElementType.U:
			case ElementType.Object:
				res = ContainsGenericParameterInternal((type as TypeDefOrRefSig).TypeDefOrRef);
				break;

			case ElementType.Var:
			case ElementType.MVar:
				res = true;
				break;

			case ElementType.FnPtr:
				res = ContainsGenericParameterInternal((type as FnPtrSig).Signature);
				break;

			case ElementType.GenericInst:
				var gi = (GenericInstSig)type;
				res = ContainsGenericParameterInternal(gi.GenericType) ||
					ContainsGenericParameter(gi.GenericArguments);
				break;

			case ElementType.Ptr:
			case ElementType.ByRef:
			case ElementType.Array:
			case ElementType.SZArray:
			case ElementType.Pinned:
			case ElementType.ValueArray:
			case ElementType.Module:
				res = ContainsGenericParameterInternal((type as NonLeafSig).Next);
				break;

			case ElementType.CModReqd:
			case ElementType.CModOpt:
				res = ContainsGenericParameterInternal((type as ModifierSig).Modifier) ||
					ContainsGenericParameterInternal((type as NonLeafSig).Next);
				break;

			case ElementType.End:
			case ElementType.R:
			case ElementType.Internal:
			case ElementType.Sentinel:
			default:
				res = false;
				break;
			}

			recursionCounter.Decrement();
			return res;
		}

        bool ContainsGenericParameterInternal(ExportedType type) { return false; }

		bool ContainsGenericParameterInternal(CallingConventionSig callConv) {
            FieldSig fs;
            if ((fs = callConv as FieldSig) != null)
				return ContainsGenericParameterInternal(fs);

            MethodBaseSig mbs;
            if ((mbs = callConv as MethodBaseSig) != null)
				return ContainsGenericParameterInternal(mbs);

            LocalSig ls;
            if ((ls = callConv as LocalSig) != null)
				return ContainsGenericParameterInternal(ls);

            GenericInstMethodSig gim;
            if ((gim = callConv as GenericInstMethodSig) != null)
				return ContainsGenericParameterInternal(gim);

			return false;
		}

		bool ContainsGenericParameterInternal(FieldSig fs) {
			if (fs == null)
				return false;
			if (!recursionCounter.Increment())
				return false;

			bool res = ContainsGenericParameterInternal(fs.Type);

			recursionCounter.Decrement();
			return res;
		}

		bool ContainsGenericParameterInternal(MethodBaseSig mbs) {
			if (mbs == null)
				return false;
			if (!recursionCounter.Increment())
				return false;

			bool res = ContainsGenericParameterInternal(mbs.RetType) ||
				ContainsGenericParameter(mbs.Params) ||
				ContainsGenericParameter(mbs.ParamsAfterSentinel);

			recursionCounter.Decrement();
			return res;
		}

		bool ContainsGenericParameterInternal(LocalSig ls) {
			if (ls == null)
				return false;
			if (!recursionCounter.Increment())
				return false;

			bool res = ContainsGenericParameter(ls.Locals);

			recursionCounter.Decrement();
			return res;
		}

		bool ContainsGenericParameterInternal(GenericInstMethodSig gim) {
			if (gim == null)
				return false;
			if (!recursionCounter.Increment())
				return false;

			bool res = ContainsGenericParameter(gim.GenericArguments);

			recursionCounter.Decrement();
			return res;
		}

		bool ContainsGenericParameter(IList<TypeSig> types) {
			if (types == null)
				return false;
			if (!recursionCounter.Increment())
				return false;

			bool res = false;
			int count = types.Count;
			for (int i = 0; i < count; i++) {
				var type = types[i];
				if (ContainsGenericParameter(type)) {
					res = true;
					break;
				}
			}
			recursionCounter.Decrement();
			return res;
		}
	}
}
