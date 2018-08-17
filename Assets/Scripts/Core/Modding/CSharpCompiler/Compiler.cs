// Adapted from https://github.com/Interkarma/daggerfall-unity/blob/master/Assets/Game/Addons/CSharpCompiler/Compiler.cs
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MafiaUnity
{
    public class Compiler
    {
        private static Dictionary<string, Assembly> dynamicAssemblyResolver = new Dictionary<string, Assembly>();
        private static CSharpCompiler.CodeCompiler codeCompiler;
        
        public static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public static Assembly CompileSource(string[] sources, bool isSource, bool GenerateInMemory = true)
        {
            if (codeCompiler == null)
                codeCompiler = new CSharpCompiler.CodeCompiler();

            var compilerParams = new CompilerParameters();

            //add all references to assembly - need to use Assembly resolver for Dynamically created
            //assemblies, as assembly.Location will fail for them
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    compilerParams.ReferencedAssemblies.Add(assembly.Location);
                }
                catch
                {
                    if (dynamicAssemblyResolver.ContainsKey(assembly.FullName))
                        compilerParams.ReferencedAssemblies.Add(assembly.GetName().FullName);
                }
            }

            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = GenerateInMemory;

            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                if (dynamicAssemblyResolver.ContainsKey(e.Name))
                {
                    //UnityEngine.Debug.Log("resolved assembly for:" + e.Name);
                    return dynamicAssemblyResolver[e.Name];
                }
                else
                    return null;
            };

            // Compile the source
            CompilerResults result;

            if (isSource)
                result = codeCompiler.CompileAssemblyFromSourceBatch(compilerParams, sources);
            else
                result = codeCompiler.CompileAssemblyFromFileBatch(compilerParams, sources);


            if (result.CompiledAssembly != null)
            {
                if (!dynamicAssemblyResolver.ContainsKey(result.CompiledAssembly.FullName))
                    dynamicAssemblyResolver.Add(result.CompiledAssembly.FullName, result.CompiledAssembly);
            }

            if (result.Errors.Count > 0)
            {
                var msg = new StringBuilder();
                foreach (CompilerError error in result.Errors)
                {
                    msg.AppendFormat("Error ({0}, {2}) in {3} : {1}\n",
                        error.ErrorNumber, error.ErrorText, error.Line, error.FileName);
                }

                throw new Exception(msg.ToString());
            }

            // Return the assembly
            return result.CompiledAssembly;
        }
    }
}
