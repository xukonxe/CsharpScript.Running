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
	public partial class CSharpScriptRunning {
		/// <summary>
		/// Executes a script and returns the result as type <typeparamref name="TOUT"/>.
		/// If <paramref name="操作宿主"/> is true, the calling assembly of the method will be automatically added to <paramref name="ImportAssemblies"/>.
		/// If the script throws an exception, the method will return the default value of type <typeparamref name="TOUT"/>.
		/// </summary>
		/// <typeparam name="TOUT">The return type of the script execution.</typeparam>
		/// <param name="脚本内容">The script content to be executed.</param>
		/// <param name="操作宿主">Indicates whether the calling assembly should be added to the imported assemblies.</param>
		/// <param name="globals">An optional global object for the script, can be null.</param>
		/// <param name="ImportAssemblies">An array of assemblies to import for the script execution.</param>
		/// <returns>The result of the script execution, or default of <typeparamref name="TOUT"/> if an exception occurs.</returns>

		public static TOUT RunScriptAsFunc<TOUT>(string 脚本内容, bool 操作宿主 = false, object globals = null, params Assembly[] ImportAssemblies) {
			Assembly[] 加入程序集 = 操作宿主 ? ImportAssemblies.Concat([Assembly.GetCallingAssembly()]).ToArray() : ImportAssemblies;
			var 脚本引擎 = new CSharpScriptRunning()
				.Reference(加入程序集);
			if (globals != null) 脚本引擎.SetGlobals(globals);
			try {
				return 脚本引擎.Run<TOUT>(脚本内容);
			} catch (Exception e) {
				e.Message.LogError();
				return default;
			}
		}
		/// <summary>
		/// Executes a script and returns the result as type <typeparamref name="TOUT"/>.
		/// If <paramref name="操作宿主"/> is true, the calling assembly of the method will be automatically added to <paramref name="ImportAssemblies"/>.
		/// If the script throws an exception, the method will return the default value of type <typeparamref name="TOUT"/>.
		/// </summary>
		/// <typeparam name="TOUT">The return type of the script execution.</typeparam>
		/// <param name="脚本内容">The script content to be executed.</param>
		/// <param name="操作宿主">Indicates whether the calling assembly should be added to the imported assemblies.</param>
		/// <param name="globals">An optional global object for the script, can be null.</param>
		/// <param name="ImportAssemblies">An array of assemblies to import for the script execution.</param>
		/// <returns>The result of the script execution, or default of <typeparamref name="TOUT"/> if an exception occurs.</returns>

		public static TOUT RunFileAsFunc<TOUT>(string 路径, bool 操作宿主 = false, object globals = null, params Assembly[] ImportAssemblies) {
			return RunScriptAsFunc<TOUT>(File.ReadAllText(路径), 操作宿主, globals, ImportAssemblies);
		}
		/// <summary>
		/// Executes a script and returns the result as type <typeparamref name="TOUT"/>.
		/// If <paramref name="操作宿主"/> is true, the calling assembly of the method will be automatically added to <paramref name="ImportAssemblies"/>.
		/// If the script throws an exception, the method will return the default value of type <typeparamref name="TOUT"/>.
		/// </summary>
		/// <typeparam name="TOUT">The return type of the script execution.</typeparam>
		/// <param name="脚本内容">The script content to be executed.</param>
		/// <param name="操作宿主">Indicates whether the calling assembly should be added to the imported assemblies.</param>
		/// <param name="globals">An optional global object for the script, can be null.</param>
		/// <param name="ImportAssemblies">An array of assemblies to import for the script execution.</param>
		/// <returns>The result of the script execution, or default of <typeparamref name="TOUT"/> if an exception occurs.</returns>

		public static void RunScriptAsAction(string 脚本内容, bool 操作宿主 = false, object globals = null, params Assembly[] ImportAssemblies) {
			Assembly[] 加入程序集 = 操作宿主 ? ImportAssemblies.Concat([Assembly.GetCallingAssembly()]).ToArray() : ImportAssemblies;
			var 脚本引擎 = new CSharpScriptRunning()
				.Reference(加入程序集);
			if (globals != null) 脚本引擎.SetGlobals(globals);
			try {
				脚本引擎.Run(脚本内容);
			} catch (Exception e) {
				e.Message.LogError();
			}
		}
		/// <summary>
		/// Executes a script and returns the result as type <typeparamref name="TOUT"/>.
		/// If <paramref name="操作宿主"/> is true, the calling assembly of the method will be automatically added to <paramref name="ImportAssemblies"/>.
		/// If the script throws an exception, the method will return the default value of type <typeparamref name="TOUT"/>.
		/// </summary>
		/// <typeparam name="TOUT">The return type of the script execution.</typeparam>
		/// <param name="脚本内容">The script content to be executed.</param>
		/// <param name="操作宿主">Indicates whether the calling assembly should be added to the imported assemblies.</param>
		/// <param name="globals">An optional global object for the script, can be null.</param>
		/// <param name="ImportAssemblies">An array of assemblies to import for the script execution.</param>
		/// <returns>The result of the script execution, or default of <typeparamref name="TOUT"/> if an exception occurs.</returns>
		public static void RunFileAsAction(string 路径, bool 操作宿主 = false, object globals = null, params Assembly[] ImportAssemblies) {
			RunScriptAsAction(File.ReadAllText(路径), 操作宿主, globals, ImportAssemblies);
		}
	}
	public partial class CSharpScriptRunning {
		//Options:before Run
		public object? globals { get; private set; }
		public ScriptOptions options => ScriptOptions.Default
				.WithEmitDebugInformation(true)
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
		public CSharpScriptRunning Reference(params Assembly[] assembly) {
			references.AddRange(assembly);
			return this;
		}
		public CSharpScriptRunning SetGlobals(object globals) {
			this.globals = globals;
			return this;
		}
		//Inner Running Example
		//new CSharpScriptRunning()
		//    .SetGlobals(new Globals())
		//    .Import("System.Linq")
		//    .Reference(typeof(Enumerable).Assembly)
		//    .Run("age = 10; ")
		//    .Log();
		public void Run(string script) {
			if (!RunChecks()) {
				return;
			}
			Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.EvaluateAsync(script, options, globals).Wait();
		}
		//Inner Running Example
		//new CSharpScriptRunning()
		//    .SetGlobals(new Globals())
		//    .Import("System.Linq")
		//    .Reference(typeof(Enumerable).Assembly)
		//    .Run<int>("age = 10; return age;")
		//    .Log();
		public TOUT Run<TOUT>(string script) {
			if (!RunChecks()) {
				throw new Exception("ScriptRunningError: Cannot Run<TOUT> with wrong globals settings.Check inner exception for more details.");
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
				$"ScriptRunningError: Your globals type '{type.Name}' is not a reference type. ".LogError();
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