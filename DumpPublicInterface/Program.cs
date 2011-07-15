using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.IO;

namespace DumpPublicInterface
{
    class Program
    {
        static string[] namespacePrefixed = 
                    {
                        "TradingTechnologies."
                    };

        static void Main(string[] args)
        {
            string assemblyPath = @"C:\build\main\i386\debug\bin\TradingTechnologies.TTAPI.dll";
            string outputPath = @"C:\build\main\i386\debug\bin\TradingTechnologies.TTAPI.dll.public_interface.txt";

            var decompilationOptions = new ICSharpCode.ILSpy.DecompilationOptions()
            {
                DecompilerSettings = new ICSharpCode.Decompiler.DecompilerSettings()
                {
                    ShowXmlDocumentation = false,
                    FullyQualifyAmbiguousTypeNames = true,
                    OnlyPublicAndProtected = true,
                }
            };

            var assemblyResolver = new MyAssemblyResolver(assemblyPath);
            var asm = assemblyResolver.ReadAssembly(assemblyPath);

            var csharpLang = new ICSharpCode.ILSpy.CSharpLanguage();

            using(var writer = new System.IO.StreamWriter(outputPath))
            {
                var output = new ICSharpCode.Decompiler.PlainTextOutput(writer);

                foreach (var module in asm.Modules.OrderBy(m => m.Name))
                {
                    foreach (var type in module.Types.Where(t => isValidNamespace(t) && t.IsPublic))
                    {
                        output.WriteLine();
                        output.Write("//====================================================================");
                        output.WriteLine();
                        output.Write("// ");
                        output.Write(type.FullName);
                        output.WriteLine();
                        output.WriteLine();

                        csharpLang.DecompileType(type, output, decompilationOptions);
                        output.WriteLine();
                    }
                }

                
                //csharpLang.DecompileAssembly(asm, output, decompilationOptions);
            }
        }

        static bool isValidNamespace(Mono.Cecil.TypeDefinition t)
        {
            bool found = false;

            foreach (string prefix in namespacePrefixed)
            {
                if (t.Namespace.StartsWith(prefix))
                {
                    found = true;
                    break;
                }

            }

            return found;
        }

    }

    sealed class MyAssemblyResolver : IAssemblyResolver
    {
        string fileName;
        Dictionary<string, AssemblyDefinition> assemblies = new Dictionary<string, AssemblyDefinition>();

        public MyAssemblyResolver(string mainAssemblyPath)
        {
            this.fileName = mainAssemblyPath;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return lookupReferencedAssembly(name.FullName);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            return lookupReferencedAssembly(name.FullName);
        }

        public AssemblyDefinition Resolve(string fullName)
        {
            return lookupReferencedAssembly(fullName);
        }

        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            return lookupReferencedAssembly(fullName);
        }

        public AssemblyDefinition ReadAssembly(string assemblyPath)
        {
            var asm = Mono.Cecil.AssemblyDefinition.ReadAssembly(
                assemblyPath,
                new ReaderParameters()
                {
                     AssemblyResolver = this,
                });

            if (asm != null)
            {
                assemblies[asm.FullName] = asm;
            }

            return asm;
        }


        private AssemblyDefinition lookupReferencedAssembly(string fullName)
        {
            AssemblyDefinition asm;
            if (assemblies.TryGetValue(fullName, out asm))
            {
                return asm;
            }

            var name = AssemblyNameReference.Parse(fullName);
            string file = ICSharpCode.ILSpy.GacInterop.FindAssemblyInNetGac(name);
            if (file == null)
            {
                string dir = Path.GetDirectoryName(this.fileName);
                if (File.Exists(Path.Combine(dir, name.Name + ".dll")))
                    file = Path.Combine(dir, name.Name + ".dll");
                else if (File.Exists(Path.Combine(dir, name.Name + ".exe")))
                    file = Path.Combine(dir, name.Name + ".exe");
            }
            if (file != null)
            {
                return this.ReadAssembly(file);
            }
            else
            {
                return null;
            }
        }
    }
}
