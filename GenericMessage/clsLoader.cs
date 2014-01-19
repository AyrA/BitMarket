using System;
using System.IO;
using Microsoft.CSharp;
using System.Reflection;
using System.Diagnostics;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace GenericHandler
{
    public static class AddonManager
    {
        private static string AppPath
        {
            get
            {
                string name = Process.GetCurrentProcess().MainModule.FileName;
                return name.Substring(0, name.LastIndexOf('\\'));
            }
        }

        public static List<GenericServer> Servers
        { get; private set; }

        public static List<GenericProcessor> Processors
        { get; private set; }

        /// <summary>
        /// Contains a List of Compiler Errors that may occur on the "Load" Call, if it failed.
        /// </summary>
        public static CompilerErrorCollection LastErrors = null;

        /// <summary>
        /// Loads Source into Memory
        /// </summary>
        /// <param name="Content">Source Code to load</param>
        /// <returns>Server, or null if Error</returns>
        public static GenericServer LoadServer(string Source)
        {
            string[] Lines = File.ReadAllLines(Source);
            //Initialize Compiler
            string retValue = string.Empty;
            string Code = string.Empty;
            CodeDomProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.CompilerOptions = "/target:library /optimize";
            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = true;
            compilerParams.IncludeDebugInformation = false;
            compilerParams.ReferencedAssemblies.Add("mscorlib.dll");
            compilerParams.ReferencedAssemblies.Add("System.dll");
            compilerParams.ReferencedAssemblies.Add(Path.Combine(AppPath, "GenericHandler.dll"));

            foreach (string Line in Lines)
            {
                //Check if Include Statement or Code
                if (Line.Trim().ToLower().StartsWith("#include "))
                {
                    compilerParams.ReferencedAssemblies.Add(Line.Substring(9));
                }
                else if (Line.Trim().Length > 0)
                {
                    Code += Line.Trim() + "\r\n";
                }
            }

            //Compile that shit
            CompilerResults results = codeProvider.CompileAssemblyFromSource(compilerParams, new string[] { Code });

            //Check if Errors
            if (results.Errors.Count > 0)
            {
                LastErrors = results.Errors;
                throw new Exception("Compiler Error");
            }
            return LoadServerAssembly(results.CompiledAssembly);
        }

        /// <summary>
        /// Loads an Assembly ang gets its Name
        /// </summary>
        /// <param name="a">Assembly to load</param>
        /// <returns>Generic Server component</returns>
        private static GenericServer LoadServerAssembly(Assembly a)
        {
            GenericServer retValue = null;

            foreach (Type type in a.GetTypes())
            {
                if (type.IsPublic) // Ruft einen Wert ab, der angibt, ob der Type als öffentlich deklariert ist. 
                {
                    if (!type.IsAbstract)  //nur Assemblys verwenden die nicht Abstrakt sind
                    {
                        // Sucht die Schnittstelle mit dem angegebenen Namen. 
                        Type typeInterface = type.GetInterface(Type.GetType("GenericHandler.GenericServer").ToString(), true);

                        //Make sure the interface we want to use actually exists
                        if (typeInterface != null)
                        {
                            object activedInstance = Activator.CreateInstance(type);
                            if (activedInstance != null)
                            {
                                GenericServer script = (GenericServer)activedInstance;
                                retValue = script;
                                if (Servers == null)
                                {
                                    Servers = new List<GenericServer>();
                                }
                                Servers.Add(retValue);
                                Console.WriteLine("Loaded Server: {0}",retValue);
                            }
                        }

                        typeInterface = null;
                    }
                }
            }
            a = null;
            return retValue;
        }

        /// <summary>
        /// Loads a compiled Script into Memory
        /// </summary>
        /// <param name="Path">DLL File Name</param>
        /// <returns>Generic Server component</returns>
        public static GenericServer LoadServerLib(string Path)
        {
            return LoadServerAssembly(Assembly.LoadFrom(Path));
        }

        /// <summary>
        /// Loads Source into Memory
        /// </summary>
        /// <param name="Content">Source Code to load</param>
        /// <returns>Processor, or null if Error</returns>
        public static GenericProcessor LoadProcessor(string Source)
        {
            string[] Lines = File.ReadAllLines(Source);
            //Initialize Compiler
            string retValue = string.Empty;
            string Code = string.Empty;
            CodeDomProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.CompilerOptions = "/target:library /optimize";
            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = true;
            compilerParams.IncludeDebugInformation = false;
            compilerParams.ReferencedAssemblies.Add("mscorlib.dll");
            compilerParams.ReferencedAssemblies.Add("System.dll");
            compilerParams.ReferencedAssemblies.Add(Path.Combine(AppPath, "GenericHandler.dll"));

            foreach (string Line in Lines)
            {
                //Check if Include Statement or Code
                if (Line.Trim().ToLower().StartsWith("#include "))
                {
                    compilerParams.ReferencedAssemblies.Add(Line.Substring(9));
                }
                else if (Line.Trim().Length > 0)
                {
                    Code += Line.Trim() + "\r\n";
                }
            }

            //Compile that shit
            CompilerResults results = codeProvider.CompileAssemblyFromSource(compilerParams, new string[] { Code });

            //Check if Errors
            if (results.Errors.Count > 0)
            {
                LastErrors = results.Errors;
                throw new Exception("Compiler Error");
            }
            return LoadProcessorAssembly(results.CompiledAssembly);
        }

        /// <summary>
        /// Loads an Assembly ang gets its Name
        /// </summary>
        /// <param name="a">Assembly to load</param>
        /// <returns>Generic Processor component</returns>
        private static GenericProcessor LoadProcessorAssembly(Assembly a)
        {
            GenericProcessor retValue = null;

            foreach (Type type in a.GetTypes())
            {
                if (type.IsPublic) // Ruft einen Wert ab, der angibt, ob der Type als öffentlich deklariert ist. 
                {
                    if (!type.IsAbstract)  //nur Assemblys verwenden die nicht Abstrakt sind
                    {
                        // Sucht die Schnittstelle mit dem angegebenen Namen. 
                        Type typeInterface = type.GetInterface(Type.GetType("GenericHandler.GenericProcessor").ToString(), true);

                        //Make sure the interface we want to use actually exists
                        if (typeInterface != null)
                        {
                            object activedInstance = Activator.CreateInstance(type);
                            if (activedInstance != null)
                            {
                                GenericProcessor script = (GenericProcessor)activedInstance;
                                retValue = script;
                                if (Processors == null)
                                {
                                    Processors = new List<GenericProcessor>();
                                }
                                Processors.Add(retValue);
                                Console.WriteLine("Loaded Processor: {0}", retValue);
                            }
                        }

                        typeInterface = null;
                    }
                }
            }
            a = null;
            return retValue;
        }

        /// <summary>
        /// Loads a compiled Script into Memory
        /// </summary>
        /// <param name="Path">DLL File Name</param>
        /// <returns>Generic Processor component</returns>
        public static GenericProcessor LoadProcessorLib(string Path)
        {
            return LoadProcessorAssembly(Assembly.LoadFrom(Path));
        }

        /// <summary>
        /// Compiles a Script to a DLL File
        /// </summary>
        /// <param name="Content">Source Code</param>
        /// <param name="DestinationDLL">DLL File Name</param>
        public static void Compile(string Content, string DestinationDLL)
        {
            //Initialize Compiler
            string retValue = string.Empty;
            string Code = string.Empty;
            CodeDomProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.CompilerOptions = "/target:library /optimize";
            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = false;
            compilerParams.IncludeDebugInformation = false;
            compilerParams.OutputAssembly = DestinationDLL;
            compilerParams.ReferencedAssemblies.Add("mscorlib.dll");
            compilerParams.ReferencedAssemblies.Add("System.dll");
            compilerParams.ReferencedAssemblies.Add("GenericHandler.dll");

            string[] Lines = Content.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (string Line in Lines)
            {
                //Check if Include Statement or Code
                if (Line.Trim().ToLower().StartsWith("#include "))
                {
                    compilerParams.ReferencedAssemblies.Add(Line.Substring(9));
                }
                else if (Line.Trim().Length > 0)
                {
                    Code += Line.Trim() + "\r\n";
                }
            }

            //Compile that shit
            CompilerResults results = codeProvider.CompileAssemblyFromSource(compilerParams, new string[] { Code });

            //Check if Errors
            if (results.Errors.Count > 0)
            {
                LastErrors = results.Errors;
                throw new Exception("Compiler Error");
            }

        }
    }
}