using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace dnlib.Examples
{
    /// <summary>
    /// This example shows how to add byte array to the current assembly, and then save the assembly to disk.
    /// To make things simpler, I decided not to create a holder class (named <PrivateImplementationDetails>{E21EC13E-4669-42C8-B7A5-2EE7FBD85904} in the example)
    /// and put everything in global module instead.
    /// </summary>
    class Example7
    {
        public static void Run()
        {
            // First, we need to add the class with layout. It's called '__StaticArrayInitTypeSize=5' in the example.
            ModuleDefMD mod = ModuleDefMD.Load(typeof(Example2).Module);
            Importer importer = new Importer(mod);
            ITypeDefOrRef valueTypeRef = importer.Import(typeof(System.ValueType));
            TypeDef classWithLayout = new TypeDefUser("'__StaticArrayInitTypeSize=5'", valueTypeRef);
            classWithLayout.Attributes |= TypeAttributes.Sealed | TypeAttributes.ExplicitLayout;
            classWithLayout.ClassLayout = new ClassLayoutUser(1, 5);
            mod.Types.Add(classWithLayout);

            // Now we need to add the static field with data, called '$$method0x6000003-1'.
            FieldDef fieldWithRVA = new FieldDefUser("'$$method0x6000003-1'", new FieldSig(classWithLayout.ToTypeSig(true)), FieldAttributes.Static | FieldAttributes.Assembly | FieldAttributes.HasFieldRVA);
            fieldWithRVA.InitialValue = new byte[] { 1, 2, 3, 4, 5 };
            mod.GlobalType.Fields.Add(fieldWithRVA);

            // Once that is done, we can add our byte array field, called bla in the example.
            ITypeDefOrRef byteArrayRef = importer.Import(typeof(System.Byte[]));
            FieldDef fieldInjectedArray = new FieldDefUser("bla", new FieldSig(byteArrayRef.ToTypeSig(true)), FieldAttributes.Static | FieldAttributes.Public);
            mod.GlobalType.Fields.Add(fieldInjectedArray);

            // That's it, we have all the fields. Now we need to add code to global .cctor to initialize the array properly.
            ITypeDefOrRef systemByte = importer.Import(typeof(System.Byte));
            ITypeDefOrRef runtimeHelpers = importer.Import(typeof(System.Runtime.CompilerServices.RuntimeHelpers));
            IMethod initArray = importer.Import(typeof(System.Runtime.CompilerServices.RuntimeHelpers).GetMethod("InitializeArray", new Type[] { typeof(System.Array), typeof(System.RuntimeFieldHandle) }));

            MethodDef cctor = mod.GlobalType.FindOrCreateStaticConstructor();
            IList<Instruction> instrs = cctor.Body.Instructions;
            instrs.Insert(0, new Instruction(OpCodes.Ldc_I4, 5));
            instrs.Insert(1, new Instruction(OpCodes.Newarr, systemByte));
            instrs.Insert(2, new Instruction(OpCodes.Dup));
            instrs.Insert(3, new Instruction(OpCodes.Ldtoken, fieldWithRVA));
            instrs.Insert(4, new Instruction(OpCodes.Call, initArray));
            instrs.Insert(5, new Instruction(OpCodes.Stsfld, fieldInjectedArray));

            // Save the assembly to a file on disk
            mod.Write(@"D:\saved-assembly.exe");
        }
    }
}
