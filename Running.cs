using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;

namespace TGZG.CSharpScript.Running {
    static class ExtensionMethods {
        private static Action<object> LogAct => System.Console.WriteLine;
        private static Action<object> LogWarningAct => t => {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine(t);
            System.Console.ResetColor();
        };
        private static Action<object> LogErrorAct = t => {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine(t);
            System.Console.ResetColor();
        };
        public static void Log(this object message) {
            LogAct?.Invoke(message);
        }
        public static void LogWarning(this object message) {
            LogWarningAct?.Invoke(message);
        }
        public static void LogError(this object message) {
            LogErrorAct?.Invoke(message);
        }
    }
    public class CSharpScriptRunning {
        //Options:before Run
        public object? globals { get; private set; }
        public ScriptOptions options => ScriptOptions.Default
                .WithImports(usings)
                .WithReferences(references);
        List<string> usings = [];
        List<Assembly> references = [];
        public CSharpScriptRunning() { }
        public CSharpScriptRunning(object globals) {
            this.globals = globals;
        }
        public CSharpScriptRunning Import(string namespaceName) {
            usings.Add(namespaceName);
            return this;
        }
        public CSharpScriptRunning Reference(Assembly assembly) {
            references.Add(assembly);
            return this;
        }
        public CSharpScriptRunning SetGlobals(object globals) {
            this.globals = globals;
            return this;
        }
        public void Run(string script) {
            if (!RunChecks()) {
                return;
            }
            Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.EvaluateAsync(script, options, globals).Wait();
        }
        public TOUT Run<TOUT>(string script) {
            if (!RunChecks()) {
                return default;
            }
            return Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.EvaluateAsync<TOUT>(script, options, globals).Result;
        }
        bool RunChecks() {
            if (globals == null) return true;
            var type = globals.GetType();
            if (usings.Contains("System.Linq") && !references.Contains(typeof(Enumerable).Assembly)) {
                $"Warning: Cannot import 'System.Linq' without adding 'typeof(Enumerable).Assembly' Assembly to References.".LogWarning();
            }
            if (type.IsValueType) {
                $"Error: '{type.Name}' Cannot be value type.".LogError();
                return false;
            }
            if (!type.IsVisible) {
                $"Warning: Cannot access '{type.Name}'. Make it public and not internal or it cannot be used.".LogWarning();
            }
            foreach (var field in type.GetFields()) {
                if (!field.IsPublic) {
                    $"Warning: Cannot access '{field.Name}'. Make it Public or it cannot be used.".LogWarning();
                }
            }
            return true;
        }
    }
}