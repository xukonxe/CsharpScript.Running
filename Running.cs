using Microsoft.AspNetCore.Builder;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace TGZG.CSharpScript.Running {
	static class LogSystem {
		private static Action<object> LogAct => System.Console.WriteLine;

		public static void Log(this object message, ConsoleColor? color = null) {
			if (color.HasValue) {
				System.Console.ForegroundColor = color.Value;
			}
			LogAct?.Invoke(message);
			if (color.HasValue) {
				System.Console.ResetColor();
			}
		}
		public static void Log<T>(this IEnumerable<T> obj, Func<T, string> formatter) {
			foreach (var item in obj) {
				Console.WriteLine(formatter(item));
			}
		}
		public static void LogWarring(this object message) {
			message.Log(ConsoleColor.Yellow);
		}
		public static void LogError(this object message) {
			message.Log(ConsoleColor.Red);
		}
	}
	public static class CSharpScriptEditorAPIs {
		private static WebApplication API;
		public static void StartEditorAPIServer(int port) {
			if (API != null) throw new InvalidOperationException("API already started.");
			API = WebApplication.Create();
			API.MapGet("/", () => "Hello World!");
			API.MapGet("Compile", (string text) => "");
			API.MapGet("GetNamespaces", (string Keys) => "");
			API.MapGet("GetTypes", (string Keys, string? Namespace) => "");
			API.MapGet("GetObjectVars", (string ObjectName, string? Keys) => "");
			API.MapGet("GetObjectMethods", (string ObjectName, string? Keys) => "");
			API.Run("http://localhost:" + port);
		}
	}
	public static class CSharpScriptHinter {
		public static string[] 匹配命名空间(string 匹配字符) {
			匹配字符 = 匹配字符.ToLower();
			Assembly assembly = Assembly.GetCallingAssembly();
			return assembly.GetTypes().Select(t => t.Namespace).Distinct().Where(n =>
				n != null
				&&
				n.ToLower().Contains(匹配字符)
				)
				.Select(t => t.ToString()).ToArray();
		}
		public static Type[] 匹配类型(string 匹配字符, string? 命名空间 = null) {
			匹配字符 = 匹配字符.ToLower();
			Assembly assembly = Assembly.GetExecutingAssembly();
			return assembly.GetTypes().Where(t =>
				t.IsClass
				&&
				(string.IsNullOrEmpty(命名空间) || t.Namespace == 命名空间)
				&&
				t.Name.ToLower().Contains(匹配字符)
				&&
				!t.Name.StartsWith('<')
				&&
				!t.FullName.StartsWith('<')
				).ToArray();
		}
		public static (string 字段名, Type 类型, bool GET, bool SET)[] 匹配类字段(this Type 类型, string 匹配字符 = "") {
			// 获取所有字段
			var fields = 类型.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
							 .Where(f => string.IsNullOrEmpty(匹配字符) || f.Name.Contains(匹配字符))
							 .Select(f => (f.Name, f.FieldType, true, !f.IsInitOnly))
							 .ToList();
			// 获取所有属性
			var properties = 类型.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
								 .Where(p => string.IsNullOrEmpty(匹配字符) || p.Name.Contains(匹配字符))
								 .Select(p => (p.Name, p.PropertyType, p.CanRead, p.CanWrite))
								 .ToList();
			// 合并字段和属性
			return fields.Concat(properties).ToArray();
		}
		public static (string 方法名, (string? 参数名, Type 类型)[] 参数类型, Type 返回类型)[] 匹配类方法(this Type 类型, string 匹配字符 = "") {
			// 获取所有非object类方法，获取所有非IL方法
			var methods = 类型.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(m => m.DeclaringType != typeof(object) &&
				 m.IsSpecialName == false &&
				 m.IsGenericMethod == false &&
				 !m.Name.Contains("<")) // 排除包含"<"的编译器生成的匿名方法
				.ToList();
			return methods
				.Select(m => (m.Name, m.GetParameters().Select(p => (p.Name, p.ParameterType)).ToArray(), m.ReturnType))
				.Where(t => t.Item1.Contains(匹配字符)).ToArray();
		}
		public static (string 字段名, Type 类型, bool GET, bool SET, object? 值)[] 匹配实例字段(this object 实例, string 匹配字符 = "") {
			// 获取所有字段
			var fields = 实例.GetType()
							 .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
							 .Where(f => string.IsNullOrEmpty(匹配字符) || f.Name.Contains(匹配字符))
							 .Select(f => (f.Name, f.FieldType, true, !f.IsInitOnly, f.GetValue(实例)))
							 .ToList();

			// 获取所有属性
			var properties = 实例.GetType()
								 .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
								 .Where(p => string.IsNullOrEmpty(匹配字符) || p.Name.Contains(匹配字符))
								 .Select(p => (p.Name, p.PropertyType, p.CanRead, p.CanWrite, p.CanRead ? p.GetValue(实例) : null))
								 .ToList();

			// 合并字段和属性
			return fields.Concat(properties).ToArray();
		}
		public static (string 方法名, (string? 参数名, Type 类型)[] 参数类型, Type 返回类型)[] 匹配实例方法(this object 实例, string 匹配字符 = "") {
			return 实例.GetType().匹配类方法(匹配字符);
		}
		public static void 输出属性方法信息(this object 实例, [CallerArgumentExpression("实例")] string 实例名称 = null) {
			var type = 实例.GetType();
			$"=== {type.Name} {实例名称} 的内部属性 ===".LogWarring();
			实例.匹配实例字段().Log(t => {
				var sb = new StringBuilder();
				//$"{t.字段名} = {t.值?.ToString() ?? "null"} ({t.类型.Name})"
				sb.Append($"{t.字段名}");
				sb.Append(" { ");
				if (t.GET) sb.Append("get; ");
				if (t.SET) sb.Append("set; ");
				sb.Append("} = ");
				sb.Append(t.值?.ToString() ?? "null");
				sb.Append($" ({t.类型.Name})");
				return sb.ToString();
			});
			$"=== {type.Name} {实例名称} 的内部方法 ===".LogWarring();
			实例.匹配实例方法().Log(t => $"{t.返回类型.Name} {t.方法名}({string.Join(",", t.参数类型.Select(p => $"{p.类型.Name} {p.参数名}"))});");
			$"======".LogWarring();
		}
	}
	public partial class CSharpScriptRunning {
		public dynamic[] FindKeys(string 匹配字符) {
			return CSharpScriptHinter.匹配类型(匹配字符);
		}
	}

	//static maethods for fast access
	public partial class CSharpScriptRunning {
		/// <summary>
		/// Compiles a script WITHOUT running it. log errors. Return true if syntax is correct, false otherwise.
		/// If <paramref name="操作宿主"/> is true, the calling assembly of the method will be automatically added to <paramref name="ImportAssemblies"/>.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="操作宿主"></param>
		/// <param name="globals"></param>
		/// <param name="ImportAssemblies"></param>
		/// <returns></returns>
		public static bool CompileFile(string path, bool 操作宿主 = false, object globals = null, params Assembly[] ImportAssemblies) {
			var text = File.ReadAllText(path);
			Assembly[] 加入程序集 = 操作宿主 ? ImportAssemblies.Concat([Assembly.GetCallingAssembly()]).ToArray() : ImportAssemblies;
			var 脚本引擎 = new CSharpScriptRunning()
				.Reference(加入程序集);
			if (globals != null) 脚本引擎.SetGlobals(globals);
			return 脚本引擎.CheckSyntax(text);
		}
		public static ImmutableArray<Diagnostic> CompileFile_Details(string path, bool 操作宿主 = false, object globals = null, params Assembly[] ImportAssemblies) {
			var text = File.ReadAllText(path);
			Assembly[] 加入程序集 = 操作宿主 ? ImportAssemblies.Concat([Assembly.GetCallingAssembly()]).ToArray() : ImportAssemblies;
			var 脚本引擎 = new CSharpScriptRunning()
				.Reference(加入程序集);
			if (globals != null) 脚本引擎.SetGlobals(globals);
			return 脚本引擎.CheckSyntax_Details(text);
		}
		/// <summary>
		/// Executes a script and returns the result as type <typeparamref name="TOUT"/>.
		/// If <paramref name="操作宿主"/> is true, the calling assembly of the method will be automatically added to <paramref name="ImportAssemblies"/>.
		/// If the script throws an exception, the method will return the default value of type <typeparamref name="TOUT"/>.
		/// </summary>
		/// <typeparam name="TOUT">The return type of the script execution.</typeparam>
		/// <param name="text">The script content to be executed.</param>
		/// <param name="操作宿主">Indicates whether the calling assembly should be added to the imported assemblies.</param>
		/// <param name="globals">An optional global object for the script, can be null.</param>
		/// <param name="ImportAssemblies">An array of assemblies to import for the script execution.</param>
		/// <returns>The result of the script execution, or default of <typeparamref name="TOUT"/> if an exception occurs.</returns>

		public static TOUT RunScriptAsFunc<TOUT>(string text, bool 操作宿主 = false, object globals = null, params Assembly[] ImportAssemblies) {
			Assembly[] 加入程序集 = 操作宿主 ? ImportAssemblies.Concat([Assembly.GetCallingAssembly()]).ToArray() : ImportAssemblies;
			var 脚本引擎 = new CSharpScriptRunning()
				.Reference(加入程序集);
			if (globals != null) 脚本引擎.SetGlobals(globals);
			if (!脚本引擎.CheckSyntax(text)) return default;
			try {
				return 脚本引擎.Run<TOUT>(text);
			} catch (Exception e) {
				e.LogError();
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

		public static TOUT RunFileAsFunc<TOUT>(string path, bool 操作宿主 = false, object globals = null, params Assembly[] ImportAssemblies) {
			return RunScriptAsFunc<TOUT>(File.ReadAllText(path), 操作宿主, globals, ImportAssemblies);
		}
		/// <summary>
		/// Executes a script and returns the result as type <typeparamref name="TOUT"/>.
		/// If <paramref name="操作宿主"/> is true, the calling assembly of the method will be automatically added to <paramref name="ImportAssemblies"/>.
		/// If the script throws an exception, the method will return the default value of type <typeparamref name="TOUT"/>.
		/// </summary>
		/// <typeparam name="TOUT">The return type of the script execution.</typeparam>
		/// <param name="text">The script content to be executed.</param>
		/// <param name="操作宿主">Indicates whether the calling assembly should be added to the imported assemblies.</param>
		/// <param name="globals">An optional global object for the script, can be null.</param>
		/// <param name="ImportAssemblies">An array of assemblies to import for the script execution.</param>
		/// <returns>The result of the script execution, or default of <typeparamref name="TOUT"/> if an exception occurs.</returns>

		public static void RunScriptAsAction(string text, bool 操作宿主 = false, object globals = null, params Assembly[] ImportAssemblies) {
			//Add assemblys and (or not) calling assembly.
			Assembly[] 加入程序集 = 操作宿主 ? ImportAssemblies.Concat([Assembly.GetCallingAssembly()]).ToArray() : ImportAssemblies;
			var 脚本引擎 = new CSharpScriptRunning().Reference(加入程序集);
			//Set global variables
			//Example:
			//=========
			//var globals = new { age = 10, name = "John" };
			//=========
			//
			//Script:
			//=========
			//age = 10;
			//name = "John";
			//=========
			if (globals != null) 脚本引擎.SetGlobals(globals);
			//check syntax and output errors if any
			if (!脚本引擎.CheckSyntax(text)) return;
			//run and check Thrown Exception
			try {
				脚本引擎.Run(text);
			} catch (Exception e) {
				e.LogError();
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
		public static void RunFileAsAction(string path, bool 操作宿主 = false, object globals = null, params Assembly[] ImportAssemblies) {
			RunScriptAsAction(File.ReadAllText(path), 操作宿主, globals, ImportAssemblies);
		}
	}
	//main class for running scripts
	public partial class CSharpScriptRunning {
		public object? globals { get; private set; }
		//Options:before Run
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
		public ImmutableArray<Diagnostic> CheckSyntax_Details(string scriptContent) {
			return Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.Create(scriptContent, options).Compile();
		}
		//Check syntax without running
		public bool CheckSyntax(string scriptContent) {
			var sb = new StringBuilder();
			// 定义脚本选项 (假设 options 在方法外定义，如果不是，可以在此定义)
			// 编译脚本以检查语法
			var diagnostics = CheckSyntax_Details(scriptContent);
			if (diagnostics.Length > 0) {
				sb.AppendLine($"[Compile Failure]: {diagnostics.Length} errors found.");
				sb.AppendLine("==== Error Details ====");
				foreach (var diagnostic in diagnostics) {
					var line = diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1;
					sb.AppendLine($"Line {line}: {diagnostic.GetMessage()}");
				}
				sb.AppendLine("=======================");
				sb.LogError(); // 假设 sb.LogError() 会将错误输出到日志
				return false;
			} else {
				sb.AppendLine("[Compile Success]: 0 errors found.");
				sb.Log(); // 假设 sb.Log() 会将成功信息输出到日志
				return true;
			}
		}

		bool RunChecks() {
			if (globals == null) return true;
			var type = globals.GetType();
			if (usings.Contains("System.Linq") && !references.Contains(typeof(Enumerable).Assembly)) {
				$"Warning: Cannot import 'System.Linq' without adding 'typeof(Enumerable).Assembly' Assembly to References.".LogWarring();
			}
			if (type.IsValueType) {
				$"ScriptRunningError: Your globals type '{type.Name}' is not a reference type. ".LogError();
				return false;
			}
			if (!type.IsVisible) {
				$"Warning: Cannot access '{type.Name}'. Make it public and not internal or it cannot be used.".LogWarring();
			}
			foreach (var field in type.GetFields()) {
				if (!field.IsPublic) {
					$"Warning: Cannot access '{field.Name}'. Make it Public or it cannot be used.".LogWarring();
				}
			}
			return true;
		}
	}
}