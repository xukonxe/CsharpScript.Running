using System;
using System.Dynamic;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using TGZG.CSharpScript.Running;

namespace TestConsole {
    public static class ExtensionMethods {
        public static void Log(this object message) {
            Console.WriteLine(message);
        }
    }
    public class Program {
        public class Globals {
            public string name = "undefined";
            public int age = 0;
        }
        public static void Main(string[] args) {
            new CSharpScriptRunning()
                .SetGlobals(new Globals())
                .Import("System.Linq")
                .Reference(typeof(Enumerable).Assembly)
                .Run<int>("age = 10; return age;")
                .Log();
        }
    }
}
