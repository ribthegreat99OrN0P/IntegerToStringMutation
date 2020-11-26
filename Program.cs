using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;


namespace IntegerToStringMutation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "IntegerToString Mutations By N0P";

            var asm = AssemblyDef.Load(args[0]);
            var count = 0;
            foreach (var module in asm.Modules)
            {
                foreach (var type in module.GetTypes())
                {
                    foreach (var method in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions))
                    {
                        method.Body.SimplifyMacros(method.Parameters);
                        for (var i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if (method.Body.Instructions[i].OpCode == OpCodes.Ldc_I4)
                            {
                                var operand = method.Body.Instructions[i].GetLdcI4Value();
                                if (operand > 0 | operand <= 0)
                                {
                                    var mainString = RandomString(operand);


                                    var local0 = new Local(module.ImportAsTypeSig(typeof(int)));
                                    var ldlocIns = new Instruction(OpCodes.Ldloc, local0);
                                    method.Body.Variables.Add(local0);
                                    method.Body.Instructions[i].OpCode = OpCodes.Nop;

                                    method.Body.Instructions.Insert(i + 1,
                                        new Instruction(OpCodes.Ldstr, mainString));
                                    method.Body.Instructions.Insert(i + 2,
                                        new Instruction(OpCodes.Callvirt,
                                            module.Import(typeof(String).GetMethod("get_Length"))));
                                    method.Body.Instructions.Insert(i + 3,
                                        new Instruction(OpCodes.Stloc, local0));
                                    method.Body.Instructions.Insert(i + 4,
                                        new Instruction(OpCodes.Br_S, ldlocIns));
                                    method.Body.Instructions.Insert(i + 5, ldlocIns);
                                    i += 5;
                                    count++;
                                }
                            }
                        }
                    }
                }
            }
            var opts = new ModuleWriterOptions(asm.ManifestModule);
            opts.MetadataOptions.Flags |= MetadataFlags.PreserveAll;
            opts.Logger = DummyLogger.NoThrowInstance;

            if(args[0].Contains(".exe"))
                asm.ManifestModule.Write(Path.GetFileNameWithoutExtension(args[0]) + "-mutated.exe", opts);
            else
                asm.ManifestModule.Write(Path.GetFileNameWithoutExtension(args[0]) + "-mutated.dll", opts);

            Console.WriteLine($"Done! Mutated {count} integers");
            Console.ReadKey();
        }

        public static string RandomString(int size)
        {
            var charSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"; ; //chars        

            var chars = charSet.ToCharArray();
            var data = new byte[1];
            var crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            var result = new StringBuilder(size);
            foreach (var b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

    }
}
