using System;
using System.Dynamic;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using TGZG.CSharpScript.Running;
using static TGZG.CSharpScript.Running.CSharpScriptHinter;

namespace TestConsole {
	public static class ExtensionMethods {
		public static void Log(this object message) {
			Console.WriteLine(message);
		}
	}
	public class Program {
		//宿主字段
		public static class 你好 {
			public static double 数据 = 100000;
		}
		public class Globals {
			public double 俯仰;
			public double 偏航;
			public double 滚转;
		}
		public static void Main(string[] args) {
			//File Running Example
			var 脚本路径 = Path.Combine(AppContext.BaseDirectory, "ExampleFile.cssc");
			while (true) {
				CSharpScriptRunning.RunFileAsAction(脚本路径, true);
				//CSharpScriptRunning.CompileFile(脚本路径, true);
				//CSharpScriptRunning.RunFileAsFunc<double>(脚本路径, true);
				$"请输入匹配字符".Log();
				var 匹配字符 = Console.ReadLine();
				$"====所有命名空间====".Log();
				var 所有命名空间 = 匹配命名空间(匹配字符);
				所有命名空间.Log(t => t);
				$"====TestConsole命名空间 的 所有类型====".Log();
				var 所有类型 = 匹配类型(匹配字符, "TestConsole");
				所有类型.Log(t => {
					var sb = new StringBuilder();

					if (t.IsPublic) sb.Append("public ");
					//怎么判断是不是static？
					if (t.IsSealed && t.IsAbstract) sb.Append("static ");
					else if (t.IsAbstract) sb.Append("abstract ");

					if (t.IsClass) sb.Append("class ");
					else if (t.IsEnum) sb.Append("enum ");
					else if (t.IsInterface) sb.Append("interface ");
					else if (t.IsValueType) sb.Append("struct ");

					if (t.IsNested) sb.Append(t.DeclaringType.Name + ".");

					if (t.IsGenericType) {
						sb.Append(t.Name.Split('`')[0] + "<");
						sb.Append(string.Join(", ", t.GetGenericArguments().Select(t => t.Name)));
						sb.Append(">");
					}

					if (t.IsArray) {
						sb.Append(t.GetElementType().Name);
						sb.Append("[");
						sb.Append(t.GetArrayRank());
						sb.Append("]");
					}
					sb.Append(t.Name);
					return sb.ToString();


				});
			}
			//获取当前程序集
			//三个脚本方式：
			//1.脚本操作宿主
			//CSharpScriptRunning.RunFileAsAction(脚本路径, true);
			//CSharpScriptRunning.RunFileAsFunc<T>(脚本路径, true);
			//2.宿主的外接模块
			//CSharpScriptRunning.RunFileAsAction(脚本路径, false);
			//CSharpScriptRunning.RunFileAsFunc<T>(脚本路径, false);
			//3.完整.cs代码文件自动解析为脚本
			//TODO：待写
		}
	}
}
